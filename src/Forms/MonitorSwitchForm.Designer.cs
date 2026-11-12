using System.Drawing;
using Insonnia.Properties;
using Insonnia.UI;

namespace Insonnia.Forms
{
    partial class MonitorSwitchForm
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
            this.lblHeader          = new System.Windows.Forms.Label();
            this.lblHeaderSub       = new System.Windows.Forms.Label();
            this.cardUsb            = new ModernCard();
            this.chkEnabled         = new ToggleSwitch();
            this.lblUsb             = new System.Windows.Forms.Label();
            this.cmbUsb             = new System.Windows.Forms.ComboBox();
            this.btnUsbRefresh      = new ModernButton();
            this.cardMonitors       = new ModernCard();
            this.lblMonitors        = new System.Windows.Forms.Label();
            this.lvMonitors         = new System.Windows.Forms.ListView();
            this.colIndex           = new System.Windows.Forms.ColumnHeader();
            this.colName            = new System.Windows.Forms.ColumnHeader();
            this.colSerial          = new System.Windows.Forms.ColumnHeader();
            this.colInput           = new System.Windows.Forms.ColumnHeader();
            this.colInstance        = new System.Windows.Forms.ColumnHeader();
            this.colDisplay         = new System.Windows.Forms.ColumnHeader();
            this.btnMonitorsRefresh = new ModernButton();
            this.btnCopy            = new ModernButton();
            this.cardInputs         = new ModernCard();
            this.lblMonitorId       = new System.Windows.Forms.Label();
            this.txtMonitorId       = new System.Windows.Forms.TextBox();
            this.lblConnect         = new System.Windows.Forms.Label();
            this.cmbConnect         = new System.Windows.Forms.ComboBox();
            this.btnTestConnect     = new ModernButton();
            this.lblDisconnect      = new System.Windows.Forms.Label();
            this.cmbDisconnect      = new System.Windows.Forms.ComboBox();
            this.btnTestDisconnect  = new ModernButton();
            this.lblStatus          = new System.Windows.Forms.Label();
            this.btnSave            = new ModernButton();
            this.btnCancel          = new ModernButton();
            this.cardUsb.SuspendLayout();
            this.cardMonitors.SuspendLayout();
            this.cardInputs.SuspendLayout();
            this.SuspendLayout();

            // ── intestazione ─────────────────────────────────────────────
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font     = Theme.DisplayFont(15F, FontStyle.Bold);
            this.lblHeader.Location = new System.Drawing.Point(22, 16);
            this.lblHeader.Name     = "lblHeader";
            this.lblHeader.Text     = L.Menu_MonitorSwitch;

            this.lblHeaderSub.AutoSize = true;
            this.lblHeaderSub.Font     = Theme.UiFont(9.5F);
            this.lblHeaderSub.Location = new System.Drawing.Point(24, 46);
            this.lblHeaderSub.Name     = "lblHeaderSub";
            this.lblHeaderSub.Tag      = "keepcolor";
            this.lblHeaderSub.Text     = L.MS_Enable;

            // ── card USB ─────────────────────────────────────────────────
            this.cardUsb.Location = new System.Drawing.Point(24, 78);
            this.cardUsb.Name     = "cardUsb";
            this.cardUsb.Size     = new System.Drawing.Size(632, 108);
            this.cardUsb.Controls.Add(this.chkEnabled);
            this.cardUsb.Controls.Add(this.lblUsb);
            this.cardUsb.Controls.Add(this.cmbUsb);
            this.cardUsb.Controls.Add(this.btnUsbRefresh);

            this.chkEnabled.Location = new System.Drawing.Point(16, 12);
            this.chkEnabled.Name     = "chkEnabled";
            this.chkEnabled.Size     = new System.Drawing.Size(420, 32);
            this.chkEnabled.Text     = L.Menu_MonitorSwitchEnabled;
            this.chkEnabled.TabIndex = 0;

            this.lblUsb.AutoSize = true;
            this.lblUsb.Location = new System.Drawing.Point(16, 66);
            this.lblUsb.Name     = "lblUsb";
            this.lblUsb.Text     = L.MS_UsbDevice;

            this.cmbUsb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbUsb.DropDownWidth = 460;
            this.cmbUsb.FlatStyle     = System.Windows.Forms.FlatStyle.Standard;
            this.cmbUsb.Location      = new System.Drawing.Point(236, 62);
            this.cmbUsb.Name          = "cmbUsb";
            this.cmbUsb.Size          = new System.Drawing.Size(266, 25);
            this.cmbUsb.TabIndex      = 1;

            this.btnUsbRefresh.Glyph    = Theme.GlyphRefresh;
            this.btnUsbRefresh.Location = new System.Drawing.Point(510, 58);
            this.btnUsbRefresh.Name     = "btnUsbRefresh";
            this.btnUsbRefresh.Size     = new System.Drawing.Size(106, 32);
            this.btnUsbRefresh.Text     = L.MS_Refresh;
            this.btnUsbRefresh.TabIndex = 2;
            this.btnUsbRefresh.Click   += new System.EventHandler(this.btnUsbRefresh_Click);

            // ── card monitor rilevati ────────────────────────────────────
            this.cardMonitors.Location = new System.Drawing.Point(24, 198);
            this.cardMonitors.Name     = "cardMonitors";
            this.cardMonitors.Size     = new System.Drawing.Size(632, 252);
            this.cardMonitors.Controls.Add(this.lblMonitors);
            this.cardMonitors.Controls.Add(this.lvMonitors);
            this.cardMonitors.Controls.Add(this.btnMonitorsRefresh);
            this.cardMonitors.Controls.Add(this.btnCopy);

            this.lblMonitors.AutoSize = true;
            this.lblMonitors.Font     = Theme.UiFont(10F, FontStyle.Bold);
            this.lblMonitors.Location = new System.Drawing.Point(16, 14);
            this.lblMonitors.Name     = "lblMonitors";
            this.lblMonitors.Text     = L.MS_Monitors;

            this.colIndex.Text     = "#";
            this.colIndex.Width    = 32;
            this.colName.Text      = L.MS_ColName;
            this.colName.Width     = 130;
            this.colSerial.Text    = L.MS_ColSerial;
            this.colSerial.Width   = 110;
            this.colInput.Text     = L.MS_ColInput;
            this.colInput.Width    = 150;
            this.colInstance.Text  = L.MS_ColInstance;
            this.colInstance.Width = 280;
            this.colDisplay.Text   = L.MS_ColDisplay;
            this.colDisplay.Width  = 90;

            this.lvMonitors.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvMonitors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colIndex, this.colName, this.colSerial, this.colInput, this.colInstance, this.colDisplay });
            this.lvMonitors.FullRowSelect    = true;
            this.lvMonitors.HideSelection    = false;
            this.lvMonitors.MultiSelect      = false;
            this.lvMonitors.OwnerDraw        = true;
            this.lvMonitors.ShowItemToolTips = true;
            this.lvMonitors.View             = System.Windows.Forms.View.Details;
            this.lvMonitors.Location         = new System.Drawing.Point(16, 42);
            this.lvMonitors.Name             = "lvMonitors";
            this.lvMonitors.Size             = new System.Drawing.Size(600, 150);
            this.lvMonitors.TabIndex         = 3;
            this.lvMonitors.SelectedIndexChanged += new System.EventHandler(this.lvMonitors_SelectedIndexChanged);
            this.lvMonitors.DrawColumnHeader     += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvMonitors_DrawColumnHeader);
            this.lvMonitors.DrawItem             += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvMonitors_DrawItem);
            this.lvMonitors.DrawSubItem          += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvMonitors_DrawSubItem);

            this.btnMonitorsRefresh.Glyph    = Theme.GlyphRefresh;
            this.btnMonitorsRefresh.Location = new System.Drawing.Point(16, 204);
            this.btnMonitorsRefresh.Name     = "btnMonitorsRefresh";
            this.btnMonitorsRefresh.Size     = new System.Drawing.Size(160, 32);
            this.btnMonitorsRefresh.Text     = L.MS_RefreshMonitors;
            this.btnMonitorsRefresh.TabIndex = 4;
            this.btnMonitorsRefresh.Click   += new System.EventHandler(this.btnMonitorsRefresh_Click);

            this.btnCopy.Glyph    = Theme.GlyphCopy;
            this.btnCopy.Location = new System.Drawing.Point(184, 204);
            this.btnCopy.Name     = "btnCopy";
            this.btnCopy.Size     = new System.Drawing.Size(160, 32);
            this.btnCopy.Text     = L.MS_CopyList;
            this.btnCopy.TabIndex = 5;
            this.btnCopy.Click   += new System.EventHandler(this.btnCopy_Click);

            // ── card ID monitor + ingressi ───────────────────────────────
            this.cardInputs.Location = new System.Drawing.Point(24, 462);
            this.cardInputs.Name     = "cardInputs";
            this.cardInputs.Size     = new System.Drawing.Size(632, 142);
            this.cardInputs.Controls.Add(this.lblMonitorId);
            this.cardInputs.Controls.Add(this.txtMonitorId);
            this.cardInputs.Controls.Add(this.lblConnect);
            this.cardInputs.Controls.Add(this.cmbConnect);
            this.cardInputs.Controls.Add(this.btnTestConnect);
            this.cardInputs.Controls.Add(this.lblDisconnect);
            this.cardInputs.Controls.Add(this.cmbDisconnect);
            this.cardInputs.Controls.Add(this.btnTestDisconnect);

            this.lblMonitorId.AutoSize = true;
            this.lblMonitorId.Location = new System.Drawing.Point(16, 22);
            this.lblMonitorId.Name     = "lblMonitorId";
            this.lblMonitorId.Text     = L.MS_MonitorId;

            this.txtMonitorId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMonitorId.Location    = new System.Drawing.Point(236, 18);
            this.txtMonitorId.Name        = "txtMonitorId";
            this.txtMonitorId.Size        = new System.Drawing.Size(380, 25);
            this.txtMonitorId.TabIndex    = 6;

            this.lblConnect.AutoSize = true;
            this.lblConnect.Location = new System.Drawing.Point(16, 62);
            this.lblConnect.Name     = "lblConnect";
            this.lblConnect.Text     = L.MS_InputOnConnect;

            this.cmbConnect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbConnect.FlatStyle     = System.Windows.Forms.FlatStyle.Standard;
            this.cmbConnect.Location      = new System.Drawing.Point(236, 58);
            this.cmbConnect.Name          = "cmbConnect";
            this.cmbConnect.Size          = new System.Drawing.Size(266, 25);
            this.cmbConnect.TabIndex      = 7;

            this.btnTestConnect.Glyph    = Theme.GlyphFlash;
            this.btnTestConnect.Location = new System.Drawing.Point(510, 54);
            this.btnTestConnect.Name     = "btnTestConnect";
            this.btnTestConnect.Size     = new System.Drawing.Size(106, 32);
            this.btnTestConnect.Text     = L.MS_Test;
            this.btnTestConnect.TabIndex = 8;
            this.btnTestConnect.Click   += new System.EventHandler(this.btnTestConnect_Click);

            this.lblDisconnect.AutoSize = true;
            this.lblDisconnect.Location = new System.Drawing.Point(16, 102);
            this.lblDisconnect.Name     = "lblDisconnect";
            this.lblDisconnect.Text     = L.MS_InputOnDisconnect;

            this.cmbDisconnect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbDisconnect.FlatStyle     = System.Windows.Forms.FlatStyle.Standard;
            this.cmbDisconnect.Location      = new System.Drawing.Point(236, 98);
            this.cmbDisconnect.Name          = "cmbDisconnect";
            this.cmbDisconnect.Size          = new System.Drawing.Size(266, 25);
            this.cmbDisconnect.TabIndex      = 9;

            this.btnTestDisconnect.Glyph    = Theme.GlyphFlash;
            this.btnTestDisconnect.Location = new System.Drawing.Point(510, 94);
            this.btnTestDisconnect.Name     = "btnTestDisconnect";
            this.btnTestDisconnect.Size     = new System.Drawing.Size(106, 32);
            this.btnTestDisconnect.Text     = L.MS_Test;
            this.btnTestDisconnect.TabIndex = 10;
            this.btnTestDisconnect.Click   += new System.EventHandler(this.btnTestDisconnect_Click);

            // ── stato + pulsanti ─────────────────────────────────────────
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Location     = new System.Drawing.Point(24, 620);
            this.lblStatus.Name         = "lblStatus";
            this.lblStatus.Size         = new System.Drawing.Size(400, 36);
            this.lblStatus.Tag          = "keepcolor";
            this.lblStatus.TextAlign    = System.Drawing.ContentAlignment.MiddleLeft;

            this.btnSave.Glyph    = Theme.GlyphSave;
            this.btnSave.Location = new System.Drawing.Point(436, 620);
            this.btnSave.Name     = "btnSave";
            this.btnSave.Size     = new System.Drawing.Size(106, 36);
            this.btnSave.Text     = L.MS_Save;
            this.btnSave.TabIndex = 11;
            this.btnSave.Variant  = ButtonVariant.Primary;
            this.btnSave.Click   += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Location = new System.Drawing.Point(550, 620);
            this.btnCancel.Name     = "btnCancel";
            this.btnCancel.Size     = new System.Drawing.Size(106, 36);
            this.btnCancel.Text     = L.MS_Cancel;
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Click   += new System.EventHandler(this.btnCancel_Click);

            // ── MonitorSwitchForm ────────────────────────────────────────
            this.AcceptButton        = this.btnSave;
            this.CancelButton        = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize          = new System.Drawing.Size(680, 676);
            this.Font                = Theme.UiFont(10F);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblHeaderSub);
            this.Controls.Add(this.cardUsb);
            this.Controls.Add(this.cardMonitors);
            this.Controls.Add(this.cardInputs);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.DoubleBuffered  = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "MonitorSwitchForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = L.Menu_MonitorSwitch;
            this.cardUsb.ResumeLayout(false);
            this.cardUsb.PerformLayout();
            this.cardMonitors.ResumeLayout(false);
            this.cardMonitors.PerformLayout();
            this.cardInputs.ResumeLayout(false);
            this.cardInputs.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label        lblHeader;
        private System.Windows.Forms.Label        lblHeaderSub;
        private ModernCard                        cardUsb;
        private ToggleSwitch                      chkEnabled;
        private System.Windows.Forms.Label        lblUsb;
        private System.Windows.Forms.ComboBox     cmbUsb;
        private ModernButton                      btnUsbRefresh;
        private ModernCard                        cardMonitors;
        private System.Windows.Forms.Label        lblMonitors;
        private System.Windows.Forms.ListView     lvMonitors;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colSerial;
        private System.Windows.Forms.ColumnHeader colInput;
        private System.Windows.Forms.ColumnHeader colInstance;
        private System.Windows.Forms.ColumnHeader colDisplay;
        private ModernButton                      btnMonitorsRefresh;
        private ModernButton                      btnCopy;
        private ModernCard                        cardInputs;
        private System.Windows.Forms.Label        lblMonitorId;
        private System.Windows.Forms.TextBox      txtMonitorId;
        private System.Windows.Forms.Label        lblConnect;
        private System.Windows.Forms.ComboBox     cmbConnect;
        private ModernButton                      btnTestConnect;
        private System.Windows.Forms.Label        lblDisconnect;
        private System.Windows.Forms.ComboBox     cmbDisconnect;
        private ModernButton                      btnTestDisconnect;
        private System.Windows.Forms.Label        lblStatus;
        private ModernButton                      btnSave;
        private ModernButton                      btnCancel;
    }
}
