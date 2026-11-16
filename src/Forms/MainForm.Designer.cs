using System.Drawing;
using Insonnia.Properties;
using Insonnia.UI;

namespace Insonnia.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        // NB: layout scritto a mano (testi localizzati via L.*, controlli personalizzati).
        //     Non aprire nel designer di Visual Studio: riserializzerebbe i testi come costanti.
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon               = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu              = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.startStopMenuItem        = new System.Windows.Forms.ToolStripMenuItem();
            this.durationMenuItem         = new System.Windows.Forms.ToolStripMenuItem();
            this.idleMenuItem             = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorSwitchMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.startWithWindowsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1       = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem    = new System.Windows.Forms.ToolStripMenuItem();

            // Testata
            this.picTitle          = new LogoTile();
            this.lblTitle          = new System.Windows.Forms.Label();
            this.lblSubtitle       = new System.Windows.Forms.Label();
            this.lblVersion        = new System.Windows.Forms.Label();

            // Anello di stato
            this.progressTimer     = new ProgressRing();

            // Card impostazioni sessione
            this.cardSession       = new ModernCard();
            this.lblDuration       = new System.Windows.Forms.Label();
            this.cmbDuration       = new ModernComboBox();
            this.nudCustomMinutes  = new System.Windows.Forms.NumericUpDown();
            this.lblMinutes        = new System.Windows.Forms.Label();
            this.sepSession        = new ModernSeparator();
            this.chkIdleSuspend    = new ToggleSwitch();
            this.nudIdleMinutes    = new System.Windows.Forms.NumericUpDown();
            this.lblIdleMinutes    = new System.Windows.Forms.Label();

            // Bottoni
            this.btnStart          = new ModernButton();
            this.btnStop           = new ModernButton();

            this.contextMenu.SuspendLayout();
            this.cardSession.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCustomMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIdleMinutes)).BeginInit();
            this.SuspendLayout();

            // ── notifyIcon ──────────────────────────────────────────────
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Text             = "Insonnia - prevent sleeping";
            this.notifyIcon.Visible          = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);

            // ── contextMenu (renderer, font e icone impostati da ApplyTheme) ──
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(16, 16);
            this.contextMenu.Padding          = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.showToolStripMenuItem,
                this.startStopMenuItem,
                this.durationMenuItem,
                this.idleMenuItem,
                this.monitorSwitchMenuItem,
                this.startWithWindowsMenuItem,
                this.toolStripMenuItem1,
                this.quitToolStripMenuItem });
            this.contextMenu.Name = "contextMenu";

            this.showToolStripMenuItem.Name  = "showToolStripMenuItem";
            this.showToolStripMenuItem.Text  = L.Menu_Show;
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);

            // startStop: testo e icona gestiti da UpdateUI()
            this.startStopMenuItem.Name  = "startStopMenuItem";
            this.startStopMenuItem.Text  = Theme.PlainText(L.Menu_Start);
            this.startStopMenuItem.Click += new System.EventHandler(this.startStopMenuItem_Click);

            // duration: sottomenu popolato a runtime da RebuildDurationMenu()
            this.durationMenuItem.Name = "durationMenuItem";
            this.durationMenuItem.Text = L.Menu_Duration;

            // idle: sottomenu popolato a runtime da RebuildIdleMenu()
            this.idleMenuItem.Name = "idleMenuItem";
            this.idleMenuItem.Text = L.Menu_IdleSuspend;

            // monitor: sottomenu popolato a runtime da RebuildMonitorSwitchMenu()
            this.monitorSwitchMenuItem.Name = "monitorSwitchMenuItem";
            this.monitorSwitchMenuItem.Text = L.Menu_MonitorSwitch;

            this.startWithWindowsMenuItem.Name  = "startWithWindowsMenuItem";
            this.startWithWindowsMenuItem.Text  = L.Menu_StartWithWindows;
            this.startWithWindowsMenuItem.Click += new System.EventHandler(this.startWithWindowsMenuItem_Click);

            this.toolStripMenuItem1.Name = "toolStripMenuItem1";

            this.quitToolStripMenuItem.Name  = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Text  = L.Menu_Quit;
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);

            // ── testata: logo + titolo + sottotitolo + versione ─────────
            this.picTitle.Location = new System.Drawing.Point(24, 22);
            this.picTitle.Name     = "picTitle";
            this.picTitle.Size     = new System.Drawing.Size(46, 46);
            this.picTitle.TabStop  = false;

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font     = Theme.DisplayFont(17F, FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(80, 18);
            this.lblTitle.Name     = "lblTitle";
            this.lblTitle.Text     = L.UI_Title;

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font     = Theme.UiFont(9.5F);
            this.lblSubtitle.Location = new System.Drawing.Point(82, 49);
            this.lblSubtitle.Name     = "lblSubtitle";
            this.lblSubtitle.Tag      = "keepcolor";
            this.lblSubtitle.Text     = L.UI_Subtitle;

            this.lblVersion.Font      = Theme.UiFont(8.5F);
            this.lblVersion.Location  = new System.Drawing.Point(280, 26);
            this.lblVersion.Name      = "lblVersion";
            this.lblVersion.Size      = new System.Drawing.Size(76, 18);
            this.lblVersion.Tag       = "keepcolor";
            this.lblVersion.Text      = "v0.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ── anello di stato (tempo al centro) ───────────────────────
            this.progressTimer.Location = new System.Drawing.Point(90, 88);
            this.progressTimer.Name     = "progressTimer";
            this.progressTimer.Size     = new System.Drawing.Size(200, 200);
            this.progressTimer.TabStop  = false;

            // ── card sessione ───────────────────────────────────────────
            this.cardSession.Location = new System.Drawing.Point(24, 308);
            this.cardSession.Name     = "cardSession";
            this.cardSession.Size     = new System.Drawing.Size(332, 120);
            this.cardSession.Controls.Add(this.lblDuration);
            this.cardSession.Controls.Add(this.cmbDuration);
            this.cardSession.Controls.Add(this.nudCustomMinutes);
            this.cardSession.Controls.Add(this.lblMinutes);
            this.cardSession.Controls.Add(this.sepSession);
            this.cardSession.Controls.Add(this.chkIdleSuspend);
            this.cardSession.Controls.Add(this.nudIdleMinutes);
            this.cardSession.Controls.Add(this.lblIdleMinutes);

            // riga durata
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(16, 21);
            this.lblDuration.Name     = "lblDuration";
            this.lblDuration.Text     = L.UI_LabelDuration;

            this.cmbDuration.Location      = new System.Drawing.Point(100, 14);
            this.cmbDuration.Name          = "cmbDuration";
            this.cmbDuration.Size          = new System.Drawing.Size(142, 30);
            this.cmbDuration.TabIndex      = 1;
            this.cmbDuration.SelectedIndexChanged += new System.EventHandler(this.cmbDuration_SelectedIndexChanged);

            this.nudCustomMinutes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudCustomMinutes.Location    = new System.Drawing.Point(250, 18);
            this.nudCustomMinutes.Minimum     = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudCustomMinutes.Maximum     = new decimal(new int[] { 999, 0, 0, 0 });
            this.nudCustomMinutes.Value       = new decimal(new int[] { 45, 0, 0, 0 });
            this.nudCustomMinutes.Width       = 44;
            this.nudCustomMinutes.Name        = "nudCustomMinutes";
            this.nudCustomMinutes.TabIndex    = 2;
            this.nudCustomMinutes.Visible     = false;
            this.nudCustomMinutes.ValueChanged += new System.EventHandler(this.nudCustomMinutes_ValueChanged);

            this.lblMinutes.AutoSize = true;
            this.lblMinutes.Location = new System.Drawing.Point(298, 21);
            this.lblMinutes.Name     = "lblMinutes";
            this.lblMinutes.Text     = L.UI_LabelMinutes;
            this.lblMinutes.Visible  = false;

            this.sepSession.Location = new System.Drawing.Point(16, 58);
            this.sepSession.Name     = "sepSession";
            this.sepSession.Size     = new System.Drawing.Size(300, 3);
            this.sepSession.TabStop  = false;

            // riga inattività
            this.chkIdleSuspend.Location = new System.Drawing.Point(16, 72);
            this.chkIdleSuspend.Name     = "chkIdleSuspend";
            this.chkIdleSuspend.Size     = new System.Drawing.Size(220, 32);
            this.chkIdleSuspend.Text     = L.UI_IdleSuspendLabel;
            this.chkIdleSuspend.TabIndex = 3;
            this.chkIdleSuspend.CheckedChanged += new System.EventHandler(this.chkIdleSuspend_CheckedChanged);

            this.nudIdleMinutes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudIdleMinutes.Location    = new System.Drawing.Point(250, 76);
            this.nudIdleMinutes.Minimum     = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudIdleMinutes.Maximum     = new decimal(new int[] { 240, 0, 0, 0 });
            this.nudIdleMinutes.Value       = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudIdleMinutes.Width       = 44;
            this.nudIdleMinutes.Name        = "nudIdleMinutes";
            this.nudIdleMinutes.TabIndex    = 4;
            this.nudIdleMinutes.ValueChanged += new System.EventHandler(this.nudIdleMinutes_ValueChanged);

            this.lblIdleMinutes.AutoSize = true;
            this.lblIdleMinutes.Location = new System.Drawing.Point(298, 79);
            this.lblIdleMinutes.Name     = "lblIdleMinutes";
            this.lblIdleMinutes.Text     = L.UI_LabelMinutes;

            // ── btnStart / btnStop (si alternano nella stessa posizione) ─
            this.btnStart.Font     = Theme.UiFont(11F, FontStyle.Bold);
            this.btnStart.Glyph    = Theme.GlyphPlay;
            this.btnStart.Location = new System.Drawing.Point(24, 446);
            this.btnStart.Name     = "btnStart";
            this.btnStart.Radius   = 10;
            this.btnStart.Size     = new System.Drawing.Size(332, 50);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text     = Theme.PlainText(L.UI_BtnStart);
            this.btnStart.Variant  = ButtonVariant.Primary;
            this.btnStart.Click   += new System.EventHandler(this.btnStart_Click);

            this.btnStop.Font     = Theme.UiFont(11F, FontStyle.Bold);
            this.btnStop.Glyph    = Theme.GlyphStop;
            this.btnStop.Location = new System.Drawing.Point(24, 446);
            this.btnStop.Name     = "btnStop";
            this.btnStop.Radius   = 10;
            this.btnStop.Size     = new System.Drawing.Size(332, 50);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text     = Theme.PlainText(L.UI_BtnStop);
            this.btnStop.Variant  = ButtonVariant.Danger;
            this.btnStop.Click   += new System.EventHandler(this.btnStop_Click);

            // ── MainForm ──────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize          = new System.Drawing.Size(380, 520);
            this.Font                = Theme.UiFont(10F);
            this.Controls.Add(this.picTitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.progressTimer);
            this.Controls.Add(this.cardSession);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.DoubleBuffered      = true;
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox         = false;
            this.MinimizeBox         = true;
            this.Name                = "MainForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "Insonnia";
            this.Resize             += new System.EventHandler(this.MainForm_Resize);
            this.FormClosing        += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed         += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);

            this.contextMenu.ResumeLayout(false);
            this.cardSession.ResumeLayout(false);
            this.cardSession.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCustomMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIdleMinutes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.NotifyIcon        notifyIcon;
        private System.Windows.Forms.ContextMenuStrip  contextMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startStopMenuItem;
        private System.Windows.Forms.ToolStripMenuItem durationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem idleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorSwitchMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startWithWindowsMenuItem;
        private LogoTile                               picTitle;
        private System.Windows.Forms.Label             lblTitle;
        private System.Windows.Forms.Label             lblSubtitle;
        private System.Windows.Forms.Label             lblVersion;
        private ProgressRing                           progressTimer;
        private ModernCard                             cardSession;
        private System.Windows.Forms.Label             lblDuration;
        private ModernComboBox                         cmbDuration;
        private System.Windows.Forms.NumericUpDown     nudCustomMinutes;
        private System.Windows.Forms.Label             lblMinutes;
        private ModernSeparator                        sepSession;
        private ToggleSwitch                           chkIdleSuspend;
        private System.Windows.Forms.NumericUpDown     nudIdleMinutes;
        private System.Windows.Forms.Label             lblIdleMinutes;
        private ModernButton                           btnStart;
        private ModernButton                           btnStop;
    }
}
