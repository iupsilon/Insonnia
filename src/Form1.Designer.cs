using System.Drawing;
using Insonnia.Properties;

namespace Insonnia
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon               = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu              = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.startStopMenuItem        = new System.Windows.Forms.ToolStripMenuItem();
            this.durationMenuItem         = new System.Windows.Forms.ToolStripMenuItem();
            this.startWithWindowsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1       = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem    = new System.Windows.Forms.ToolStripMenuItem();

            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle          = new System.Windows.Forms.Label();

            // Riga durata
            this.panelDuration     = new System.Windows.Forms.Panel();
            this.lblDuration       = new System.Windows.Forms.Label();
            this.cmbDuration       = new System.Windows.Forms.ComboBox();
            this.nudCustomMinutes  = new System.Windows.Forms.NumericUpDown();
            this.lblMinutes        = new System.Windows.Forms.Label();

            // Riga status
            this.panelStatus       = new System.Windows.Forms.Panel();
            this.progressTimer     = new System.Windows.Forms.ProgressBar();
            this.lblStatus         = new System.Windows.Forms.Label();
            this.lblVersion        = new System.Windows.Forms.Label();

            // Bottoni
            this.btnStart          = new System.Windows.Forms.Button();
            this.btnStop           = new System.Windows.Forms.Button();

            this.contextMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelDuration.SuspendLayout();
            this.panelStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCustomMinutes)).BeginInit();
            this.SuspendLayout();

            // ── notifyIcon ──────────────────────────────────────────────
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Text             = "Insonnia - prevent sleeping";
            this.notifyIcon.Visible          = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);

            // ── contextMenu ─────────────────────────────────────────────
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.showToolStripMenuItem,
                this.startStopMenuItem,
                this.durationMenuItem,
                this.startWithWindowsMenuItem,
                this.toolStripMenuItem1,
                this.quitToolStripMenuItem });
            this.contextMenu.Name = "contextMenu";

            this.showToolStripMenuItem.Name  = "showToolStripMenuItem";
            this.showToolStripMenuItem.Text  = L.Menu_Show;
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);

            // startStop: testo e handler gestiti da UpdateUI()
            this.startStopMenuItem.Name  = "startStopMenuItem";
            this.startStopMenuItem.Text  = L.Menu_Start;
            this.startStopMenuItem.Font  = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.startStopMenuItem.Click += new System.EventHandler(this.startStopMenuItem_Click);

            // duration: sottomenu popolato a runtime da RebuildDurationMenu()
            this.durationMenuItem.Name = "durationMenuItem";
            this.durationMenuItem.Text = L.Menu_Duration;

            this.startWithWindowsMenuItem.Name  = "startWithWindowsMenuItem";
            this.startWithWindowsMenuItem.Text  = L.Menu_StartWithWindows;
            this.startWithWindowsMenuItem.Click += new System.EventHandler(this.startWithWindowsMenuItem_Click);

            this.toolStripMenuItem1.Name = "toolStripMenuItem1";

            this.quitToolStripMenuItem.Name  = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Text  = L.Menu_Quit;
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);

            // ── tableLayoutPanel1 ────────────────────────────────────────
            this.tableLayoutPanel1.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowCount    = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblTitle,       0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelDuration,  0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelStatus,    0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnStart,       0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnStop,        0, 3);
            this.tableLayoutPanel1.Location  = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name      = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding   = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel1.TabIndex  = 0;

            // ── lblTitle ─────────────────────────────────────────────────
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 102, 204);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Text      = L.UI_Title;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.Padding   = new System.Windows.Forms.Padding(4, 0, 0, 0);

            // ── panelDuration ─────────────────────────────────────────────
            this.panelDuration.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.panelDuration.Name     = "panelDuration";
            this.panelDuration.Controls.Add(this.lblMinutes);
            this.panelDuration.Controls.Add(this.nudCustomMinutes);
            this.panelDuration.Controls.Add(this.cmbDuration);
            this.panelDuration.Controls.Add(this.lblDuration);

            this.lblDuration.AutoSize  = true;
            this.lblDuration.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDuration.Location  = new System.Drawing.Point(4, 12);
            this.lblDuration.Name      = "lblDuration";
            this.lblDuration.Text      = L.UI_LabelDuration;

            this.cmbDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDuration.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDuration.Location      = new System.Drawing.Point(60, 9);
            this.cmbDuration.Name          = "cmbDuration";
            this.cmbDuration.Size          = new System.Drawing.Size(150, 23);
            this.cmbDuration.TabIndex      = 1;
            this.cmbDuration.SelectedIndexChanged += new System.EventHandler(this.cmbDuration_SelectedIndexChanged);

            // ── nudCustomMinutes ──────────────────────────────────────────
            this.nudCustomMinutes.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.nudCustomMinutes.Location = new System.Drawing.Point(218, 9);
            this.nudCustomMinutes.Minimum  = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudCustomMinutes.Maximum  = new decimal(new int[] { 999, 0, 0, 0 });
            this.nudCustomMinutes.Value    = new decimal(new int[] { 45, 0, 0, 0 });
            this.nudCustomMinutes.Width    = 55;
            this.nudCustomMinutes.Name     = "nudCustomMinutes";
            this.nudCustomMinutes.TabIndex = 2;
            this.nudCustomMinutes.Visible  = false;
            this.nudCustomMinutes.ValueChanged += new System.EventHandler(this.nudCustomMinutes_ValueChanged);

            // ── lblMinutes ────────────────────────────────────────────────
            this.lblMinutes.AutoSize  = true;
            this.lblMinutes.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMinutes.Location  = new System.Drawing.Point(277, 12);
            this.lblMinutes.Name      = "lblMinutes";
            this.lblMinutes.Text      = L.UI_LabelMinutes;
            this.lblMinutes.Visible   = false;

            // ── panelStatus ───────────────────────────────────────────────
            this.panelStatus.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.panelStatus.Name     = "panelStatus";
            this.panelStatus.Controls.Add(this.progressTimer);
            this.panelStatus.Controls.Add(this.lblVersion);
            this.panelStatus.Controls.Add(this.lblStatus);

            this.progressTimer.Dock   = System.Windows.Forms.DockStyle.Bottom;
            this.progressTimer.Height = 14;
            this.progressTimer.Name   = "progressTimer";
            this.progressTimer.Style  = System.Windows.Forms.ProgressBarStyle.Continuous;

            this.lblStatus.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Name      = "lblStatus";
            this.lblStatus.Text      = L.UI_StatusPaused;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Padding   = new System.Windows.Forms.Padding(0, 4, 0, 0);

            // ── lblVersion ────────────────────────────────────────────────
            this.lblVersion.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.lblVersion.Font      = new System.Drawing.Font("Segoe UI", 7F);
            this.lblVersion.ForeColor = System.Drawing.Color.Silver;
            this.lblVersion.Height    = 16;
            this.lblVersion.Name      = "lblVersion";
            this.lblVersion.Text      = "v0.0.0";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblVersion.Padding   = new System.Windows.Forms.Padding(0, 0, 4, 0);

            // ── btnStart ──────────────────────────────────────────────────
            this.btnStart.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Font             = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.BackColor        = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnStart.ForeColor        = System.Drawing.Color.White;
            this.btnStart.FlatStyle        = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.Name             = "btnStart";
            this.btnStart.Text             = L.UI_BtnStart;
            this.btnStart.TabIndex         = 2;
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click           += new System.EventHandler(this.btnStart_Click);

            // ── btnStop ───────────────────────────────────────────────────
            this.btnStop.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font             = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.BackColor        = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnStop.ForeColor        = System.Drawing.Color.White;
            this.btnStop.FlatStyle        = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.Name             = "btnStop";
            this.btnStop.Text             = L.UI_BtnStop;
            this.btnStop.TabIndex         = 3;
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click           += new System.EventHandler(this.btnStop_Click);

            // ── Form1 ─────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize          = new System.Drawing.Size(400, 260);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox         = false;
            this.MinimizeBox         = true;
            this.Name                = "Form1";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "Insonnia";
            this.Resize             += new System.EventHandler(this.Form1_Resize);
            this.FormClosing        += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);

            this.contextMenu.ResumeLayout(false);
            this.panelDuration.ResumeLayout(false);
            this.panelDuration.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudCustomMinutes)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startStopMenuItem;
        private System.Windows.Forms.ToolStripMenuItem durationMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startWithWindowsMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelDuration;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.ComboBox cmbDuration;
        private System.Windows.Forms.NumericUpDown nudCustomMinutes;
        private System.Windows.Forms.Label lblMinutes;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.ProgressBar progressTimer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
    }
}

