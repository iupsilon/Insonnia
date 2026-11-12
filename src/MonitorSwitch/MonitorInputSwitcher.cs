using System;
using System.Threading;
using Insonnia.Properties;

namespace Insonnia.MonitorSwitch
{
    /// <summary>Esito di un cambio ingresso, notificato da <see cref="MonitorInputSwitcher.SwitchCompleted"/>.</summary>
    internal sealed class MonitorSwitchEventArgs : EventArgs
    {
        public readonly MonitorSwitchOutcome Outcome;
        public readonly string               MonitorId;
        public readonly int                  Input;
        public readonly bool                 Manual;
        public readonly string               Detail;   // descrizione tecnica dell'esito (es. errore DDC/CI)

        public MonitorSwitchEventArgs(MonitorSwitchOutcome outcome, string monitorId, int input, bool manual, string detail)
        {
            Outcome   = outcome;
            MonitorId = monitorId;
            Input     = input;
            Manual    = manual;
            Detail    = detail;
        }
    }

    /// <summary>
    /// Cambio automatico dell'ingresso monitor al collegamento/scollegamento di un dispositivo USB.
    /// Collega <see cref="UsbDeviceWatcher"/> (eventi) a <see cref="MonitorControl"/> (DDC/CI):
    ///  • debounce: un dispositivo composito o uno switch USB generano raffiche di eventi; si agisce
    ///    solo dopo <see cref="DebounceMs"/> di quiete e in base all'ULTIMO stato ricevuto;
    ///  • edge detection: nessun comando se lo stato finale coincide con l'ultimo già applicato;
    ///  • i comandi DDC/CI girano su un thread del pool, mai sul thread UI;
    ///  • nessuna eccezione esce da qui: gli errori sono riportati come esito in <see cref="SwitchCompleted"/>.
    /// La configurazione è fotografata da <see cref="Restart"/>: dopo una modifica va richiamato.
    /// </summary>
    internal sealed class MonitorInputSwitcher : IDisposable
    {
        private const int DebounceMs = 1500;

        private readonly object                 _sync = new object();
        private readonly System.Threading.Timer _debounce;
        private UsbDeviceWatcher                _watcher;

        // configurazione fotografata all'avvio
        private string _monitorId;
        private int    _inputOnConnect;
        private int    _inputOnDisconnect;

        private bool? _pendingConnected;   // ultimo stato ricevuto, in attesa della fine del debounce
        private bool? _lastConnected;      // ultimo stato per cui è stato inviato un comando
        private bool  _disposed;

        /// <summary>Esito di ogni comando (automatico o manuale). Sollevato su un thread del pool.</summary>
        public event EventHandler<MonitorSwitchEventArgs> SwitchCompleted;

        public MonitorInputSwitcher()
        {
            _debounce = new System.Threading.Timer(DebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool IsRunning
        {
            get { return _watcher != null; }
        }

        /// <summary>
        /// (Ri)avvia il watcher USB secondo le impostazioni correnti. Da chiamare sul thread UI.
        /// Non parte se la funzione è disattivata o l'ID USB non è valido.
        /// </summary>
        public void Restart()
        {
            Stop();

            Settings s = Settings.Default;
            if (!s.MonitorSwitchEnabled) return;

            string usbId = UsbDeviceWatcher.NormalizeUsbId(s.MonitorSwitchUsbId);
            if (usbId == null) return;

            lock (_sync)
            {
                _monitorId         = (s.MonitorSwitchMonitorId ?? "").Trim();
                _inputOnConnect    = s.MonitorSwitchInputOnConnect;
                _inputOnDisconnect = s.MonitorSwitchInputOnDisconnect;
                _pendingConnected  = null;
                _lastConnected     = null;
            }

            UsbDeviceWatcher watcher = new UsbDeviceWatcher(usbId);
            watcher.Connected    += delegate { OnUsbEvent(true); };
            watcher.Disconnected += delegate { OnUsbEvent(false); };
            try
            {
                watcher.Start();
                _watcher = watcher;
            }
            catch (Exception)
            {
                // registrazione delle notifiche USB non riuscita: la funzione resta inattiva
                watcher.Dispose();
            }
        }

        /// <summary>Ferma il watcher USB e annulla un eventuale debounce in corso. Da chiamare sul thread UI.</summary>
        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
            lock (_sync)
            {
                _pendingConnected = null;
                if (!_disposed)
                    _debounce.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Forza subito il cambio verso <paramref name="input"/> sul monitor configurato (menu tray),
        /// anche a funzione disattivata. Asincrono: l'esito arriva da <see cref="SwitchCompleted"/>.
        /// </summary>
        public void SwitchNow(int input)
        {
            string monitorId = (Settings.Default.MonitorSwitchMonitorId ?? "").Trim();
            ThreadPool.QueueUserWorkItem(delegate { Apply(monitorId, input, true); });
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            lock (_sync)
            {
                _disposed = true;
                _debounce.Dispose();
            }
        }

        // ── eventi USB + debounce ────────────────────────────────────────
        private void OnUsbEvent(bool connected)
        {
            lock (_sync)
            {
                if (_disposed) return;
                _pendingConnected = connected;
                _debounce.Change(DebounceMs, Timeout.Infinite);   // ogni evento riavvia l'attesa
            }
        }

        private void DebounceElapsed(object state)
        {
            try
            {
                bool   connected;
                string monitorId;
                int    input;
                lock (_sync)
                {
                    if (_disposed || !_pendingConnected.HasValue) return;
                    connected         = _pendingConnected.Value;
                    _pendingConnected = null;

                    // stato invariato rispetto all'ultimo comando: nessuna azione
                    if (_lastConnected.HasValue && _lastConnected.Value == connected) return;

                    _lastConnected = connected;
                    monitorId      = _monitorId;
                    input          = connected ? _inputOnConnect : _inputOnDisconnect;
                }

                if (input <= 0) return;   // nessun ingresso configurato per questo evento

                if (!Apply(monitorId, input, false))
                {
                    // consente un nuovo tentativo al prossimo evento con lo stesso stato
                    lock (_sync) { _lastConnected = null; }
                }
            }
            catch (Exception)
            {
                // un errore sul thread del timer non deve mai far cadere l'app
            }
        }

        /// <summary>Invia il comando (thread del pool). True se l'ingresso risulta impostato.</summary>
        private bool Apply(string monitorId, int input, bool manual)
        {
            MonitorSwitchOutcome outcome;
            string               detail;
            try
            {
                outcome = MonitorControl.SetInput(monitorId, input, out detail);
            }
            catch (Exception ex)
            {
                outcome = MonitorSwitchOutcome.DdcError;
                detail  = ex.Message;
            }

            EventHandler<MonitorSwitchEventArgs> handler = SwitchCompleted;
            if (handler != null)
            {
                try
                {
                    handler(this, new MonitorSwitchEventArgs(outcome, monitorId, input, manual, detail));
                }
                catch (Exception)
                {
                    // la notifica è accessoria: un errore del gestore non interrompe il cambio
                }
            }
            return outcome == MonitorSwitchOutcome.Switched || outcome == MonitorSwitchOutcome.AlreadyActive;
        }
    }
}
