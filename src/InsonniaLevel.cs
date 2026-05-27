using Insonnia.Properties;
using System;
using System.Drawing;

namespace Insonnia
{
    internal class InsonniaLevel
    {
        // I nomi dei livelli sono proprietà lazy per supportare il cambio lingua a runtime
        private static readonly InsonniaLevel Off      = new InsonniaLevel(TimeSpan.Zero,           "Level_Off",       Resources.coffee);
        private static readonly InsonniaLevel Quiet    = new InsonniaLevel(TimeSpan.FromMinutes(30), "Level_Quiet",     Resources.quiet);
        private static readonly InsonniaLevel Happy    = new InsonniaLevel(TimeSpan.FromHours(2),   "Level_Happy",     Resources.happy);
        private static readonly InsonniaLevel Weary    = new InsonniaLevel(TimeSpan.FromHours(4),   "Level_Weary",     Resources.weary);
        private static readonly InsonniaLevel Exhausted= new InsonniaLevel(TimeSpan.MaxValue,       "Level_Exhausted", Resources.exhausted);

        public static readonly InsonniaLevel[] Levels = new InsonniaLevel[] { Off, Quiet, Happy, Weary, Exhausted };

        private readonly TimeSpan _timeKeptAwake;
        private readonly string   _nameKey;   // chiave risorsa
        private readonly Bitmap   _source;    // glyph mood (48x48), condivisa per tutta la vita dell'app
        private readonly Icon     _icon;      // icona "piatta" precalcolata (stato fermo)

        public TimeSpan TimeKeptAwake { get { return _timeKeptAwake; } }
        /// <summary>Nome localizzato del livello — risolto a runtime dalla cultura corrente.</summary>
        public string   Name          { get { return L.Get(_nameKey); } }
        /// <summary>Icona statica del livello, usata a riposo.</summary>
        public Icon     Icon          { get { return _icon; } }
        /// <summary>Bitmap sorgente del mood, base per le icone dinamiche (vedi TrayIconRenderer).</summary>
        public Bitmap   Source        { get { return _source; } }

        private InsonniaLevel(TimeSpan timeKeptAwake, string nameKey, Bitmap source)
        {
            _timeKeptAwake = timeKeptAwake;
            _nameKey       = nameKey;
            _source        = source;
            _icon          = source.ToIcon();
        }

        public static InsonniaLevel GetCurrent(TimeSpan timeAwake)
        {
            foreach (InsonniaLevel level in Levels)
                if (timeAwake <= level._timeKeptAwake)
                    return level;
            return Exhausted;
        }
    }
}
