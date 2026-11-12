using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Insonnia.UI
{
    /// <summary>
    /// Tema grafico "Fluent" dell'app: palette chiara/scura che segue l'impostazione di Windows
    /// (Impostazioni → Personalizzazione → Colori), font Segoe UI Variable, icone Segoe Fluent Icons
    /// e cornice della finestra Windows 11 (barra del titolo in tinta, angoli arrotondati).
    /// Nessuna dipendenza esterna: se un font o un attributo DWM non è disponibile si ripiega
    /// sul comportamento standard.
    /// </summary>
    internal sealed class Theme
    {
        // ── palette ──────────────────────────────────────────────────────
        public bool  IsDark;
        public Color Background;     // sfondo finestra
        public Color Surface;        // card
        public Color SurfaceAlt;     // campi di input / hover tenue
        public Color Border;         // bordi card e campi
        public Color Text;
        public Color TextMuted;
        public Color Accent;         // ambra caffè: pulsante principale, anello attivo
        public Color AccentHover;
        public Color AccentPressed;
        public Color AccentText;     // testo sopra Accent
        public Color AccentSoft;     // tinta tenue per sfondi (logo, selezioni)
        public Color Danger;         // pulsante Ferma
        public Color DangerHover;
        public Color DangerPressed;
        public Color DangerText;
        public Color Success;        // timer lontano dalla scadenza
        public Color Warning;        // timer oltre il 75%
        public Color Critical;       // ultimo minuto / errori
        public Color Idle;           // sospeso per inattività
        public Color Track;          // traccia dell'anello e dei toggle spenti

        private static Theme _current;

        /// <summary>Tema in uso. Si aggiorna da solo quando cambia l'impostazione di Windows.</summary>
        public static Theme Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Create(DetectDark());
                    SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                }
                return _current;
            }
        }

        /// <summary>Sollevato (sul thread UI) quando Windows passa da chiaro a scuro o viceversa.</summary>
        public static event EventHandler Changed;

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General) return;
            bool dark = DetectDark();
            if (_current != null && dark == _current.IsDark) return;
            _current = Create(dark);
            EventHandler handler = Changed;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        private static bool DetectDark()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false))
                {
                    object value = key != null ? key.GetValue("AppsUseLightTheme") : null;
                    return value is int && (int)value == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static Theme Create(bool dark)
        {
            Theme t = new Theme();
            t.IsDark = dark;
            if (dark)
            {
                t.Background    = Color.FromArgb(0x1C, 0x1C, 0x1E);
                t.Surface       = Color.FromArgb(0x2A, 0x2A, 0x2D);
                t.SurfaceAlt    = Color.FromArgb(0x33, 0x33, 0x37);
                t.Border        = Color.FromArgb(0x3D, 0x3D, 0x42);
                t.Text          = Color.FromArgb(0xF5, 0xF5, 0xF7);
                t.TextMuted     = Color.FromArgb(0xA1, 0xA1, 0xA8);
                t.Accent        = Color.FromArgb(0xF2, 0xA0, 0x3B);
                t.AccentHover   = Color.FromArgb(0xF5, 0xB0, 0x58);
                t.AccentPressed = Color.FromArgb(0xD9, 0x8C, 0x2C);
                t.AccentText    = Color.FromArgb(0x2B, 0x17, 0x05);
                t.AccentSoft    = Color.FromArgb(0x4A, 0x36, 0x22);
                t.Danger        = Color.FromArgb(0xE5, 0x48, 0x4D);
                t.DangerHover   = Color.FromArgb(0xEC, 0x5D, 0x62);
                t.DangerPressed = Color.FromArgb(0xC9, 0x3A, 0x3F);
                t.DangerText    = Color.White;
                t.Success       = Color.FromArgb(0x5F, 0xD0, 0x8A);
                t.Warning       = Color.FromArgb(0xF7, 0xB2, 0x4B);
                t.Critical      = Color.FromArgb(0xFF, 0x6B, 0x6B);
                t.Idle          = Color.FromArgb(0xA5, 0xAD, 0xE8);
                t.Track         = Color.FromArgb(0x3A, 0x3A, 0x3F);
            }
            else
            {
                t.Background    = Color.FromArgb(0xF4, 0xF2, 0xEF);
                t.Surface       = Color.White;
                t.SurfaceAlt    = Color.FromArgb(0xF7, 0xF5, 0xF2);
                t.Border        = Color.FromArgb(0xE4, 0xE0, 0xDA);
                t.Text          = Color.FromArgb(0x1D, 0x1B, 0x19);
                t.TextMuted     = Color.FromArgb(0x6E, 0x69, 0x63);
                t.Accent        = Color.FromArgb(0xC2, 0x61, 0x1F);
                t.AccentHover   = Color.FromArgb(0xD0, 0x6E, 0x2A);
                t.AccentPressed = Color.FromArgb(0xA9, 0x52, 0x17);
                t.AccentText    = Color.White;
                t.AccentSoft    = Color.FromArgb(0xFB, 0xEB, 0xD9);
                t.Danger        = Color.FromArgb(0xC4, 0x2B, 0x1C);
                t.DangerHover   = Color.FromArgb(0xD1, 0x3A, 0x2B);
                t.DangerPressed = Color.FromArgb(0xA8, 0x22, 0x15);
                t.DangerText    = Color.White;
                t.Success       = Color.FromArgb(0x1E, 0x8E, 0x4E);
                t.Warning       = Color.FromArgb(0xD9, 0x7A, 0x06);
                t.Critical      = Color.FromArgb(0xC4, 0x2B, 0x1C);
                t.Idle          = Color.FromArgb(0x5B, 0x63, 0xB7);
                t.Track         = Color.FromArgb(0xEA, 0xE6, 0xE0);
            }
            return t;
        }

        // ── font ─────────────────────────────────────────────────────────
        private static string _uiFamily;
        private static string _displayFamily;
        private static string _iconFamily;
        private static readonly Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        /// <summary>Font del testo (Segoe UI Variable su Windows 11, Segoe UI altrimenti). Cache condivisa: non liberare.</summary>
        public static Font UiFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (_uiFamily == null) _uiFamily = FirstInstalled("Segoe UI Variable Text", "Segoe UI");
            return GetFont(_uiFamily, size, style);
        }

        /// <summary>Font per numeri/titoli grandi. Cache condivisa: non liberare.</summary>
        public static Font DisplayFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (_displayFamily == null) _displayFamily = FirstInstalled("Segoe UI Variable Display", "Segoe UI Semibold", "Segoe UI");
            return GetFont(_displayFamily, size, style);
        }

        /// <summary>Font delle icone (Segoe Fluent Icons o MDL2 Assets), null se assente.</summary>
        public static Font IconFont(float size)
        {
            if (_iconFamily == null) _iconFamily = FirstInstalled("Segoe Fluent Icons", "Segoe MDL2 Assets") ?? "";
            return _iconFamily.Length > 0 ? GetFont(_iconFamily, size, FontStyle.Regular) : null;
        }

        private static Font GetFont(string family, float size, FontStyle style)
        {
            string key = family + "|" + size + "|" + style;
            Font font;
            if (!_fonts.TryGetValue(key, out font))
            {
                font = new Font(family, size, style, GraphicsUnit.Point);
                _fonts[key] = font;
            }
            return font;
        }

        private static string FirstInstalled(params string[] families)
        {
            using (InstalledFontCollection installed = new InstalledFontCollection())
            {
                foreach (string name in families)
                    foreach (FontFamily family in installed.Families)
                        if (string.Equals(family.Name, name, StringComparison.OrdinalIgnoreCase))
                            return family.Name;
            }
            return null;
        }

        // ── icone (glifi Segoe Fluent Icons) ─────────────────────────────
        public const string GlyphPlay     = "";
        public const string GlyphStop     = "";
        public const string GlyphClock    = "";
        public const string GlyphMoon     = "";
        public const string GlyphMonitor  = "";
        public const string GlyphPower    = "";
        public const string GlyphClose    = "";
        public const string GlyphWindow   = "";
        public const string GlyphSettings = "";
        public const string GlyphRefresh  = "";
        public const string GlyphCopy     = "";
        public const string GlyphSave     = "";
        public const string GlyphFlash    = "";
        public const string GlyphCheck    = "";
        public const string GlyphChevron  = "";
        public const string GlyphChevronDown = "";

        /// <summary>
        /// Toglie i simboli iniziali dai testi localizzati ("▶  Avvia" → "Avvia") dove
        /// l'icona è già disegnata come glifo Fluent.
        /// </summary>
        public static string PlainText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int i = 0;
            while (i < text.Length && !char.IsLetterOrDigit(text[i])) i++;
            return text.Substring(i);
        }
        public const string GlyphUsb      = "";
        public const string GlyphSwitch   = "";

        /// <summary>Disegna un glifo centrato in <paramref name="bounds"/>. Nessun effetto se il font icone manca.</summary>
        public static void DrawGlyph(Graphics g, string glyph, float size, Color color, RectangleF bounds)
        {
            Font font = IconFont(size);
            if (font == null || string.IsNullOrEmpty(glyph)) return;
            using (StringFormat sf = new StringFormat())
            using (SolidBrush brush = new SolidBrush(color))
            {
                sf.Alignment     = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(glyph, font, brush, bounds, sf);
            }
        }

        /// <summary>Icona per le voci di menu (bitmap del glifo). Il chiamante ne è proprietario.</summary>
        public static Bitmap GlyphImage(string glyph, Color color, int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                DrawGlyph(g, glyph, size * 0.55f, color, new RectangleF(0, 0, size, size));
            }
            return bmp;
        }

        // ── geometria ────────────────────────────────────────────────────
        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── applicazione ai controlli standard ───────────────────────────
        /// <summary>
        /// Applica colori e font ai controlli standard (ricorsivo). I controlli "Modern*" si
        /// disegnano da soli leggendo <see cref="Current"/>.
        /// </summary>
        public static void Apply(Control root)
        {
            Theme t = Current;
            foreach (Control c in root.Controls)
            {
                Color surface = c.Parent is ModernCard ? t.Surface : t.Background;

                if (c is ModernComboBox)
                {
                    // si disegna da solo
                }
                else if (c is ComboBox || c is TextBox || c is NumericUpDown)
                {
                    c.BackColor = t.SurfaceAlt;
                    c.ForeColor = t.Text;
                    SetNativeTheme(c, t.IsDark ? "DarkMode_CFD" : "CFD");

                    // NumericUpDown: le frecce native restano chiare anche in modalità scura;
                    // si nascondono (rotellina e tasti freccia continuano a funzionare)
                    NumericUpDown nud = c as NumericUpDown;
                    if (nud != null && nud.Controls.Count > 1)
                    {
                        nud.Controls[0].Visible = false;                    // UpDownButtons
                        nud.Controls[1].Width   = nud.ClientSize.Width;     // il campo occupa lo spazio liberato
                        nud.TextAlign           = HorizontalAlignment.Center;
                    }
                }
                else if (c is ListView)
                {
                    c.BackColor = t.Surface;
                    c.ForeColor = t.Text;
                    SetNativeTheme(c, t.IsDark ? "DarkMode_Explorer" : "Explorer");
                }
                else if (c is Label || c is CheckBox)
                {
                    c.BackColor = surface;
                    if (!(c.Tag is string && (string)c.Tag == "keepcolor"))
                        c.ForeColor = t.Text;
                }
                else if (c is ModernCard)
                {
                    c.BackColor = t.Surface;   // i figli leggono Parent.BackColor
                }
                else if (c is Panel)
                {
                    c.BackColor = surface;
                }

                if (c.HasChildren) Apply(c);
                c.Invalidate();
            }
        }

        /// <summary>Barra del titolo in tinta con lo sfondo e angoli arrotondati (Windows 11).</summary>
        public static void ApplyWindowChrome(Form form)
        {
            if (!form.IsHandleCreated) return;
            Theme t = Current;
            form.BackColor = t.Background;
            form.ForeColor = t.Text;
            try
            {
                int dark = t.IsDark ? 1 : 0;
                DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

                int corners = DWMWCP_ROUND;
                DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corners, sizeof(int));

                int caption = ColorTranslator.ToWin32(t.Background);
                DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref caption, sizeof(int));

                int text = ColorTranslator.ToWin32(t.Text);
                DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref text, sizeof(int));
            }
            catch (Exception)
            {
                // dwmapi non disponibile o attributo non supportato (Windows 10): cornice standard
            }
        }

        /// <summary>Angoli arrotondati per i menu a comparsa (Windows 11).</summary>
        public static void ApplyMenuChrome(ToolStripDropDown dropDown)
        {
            if (!dropDown.IsHandleCreated) return;
            try
            {
                int corners = DWMWCP_ROUNDSMALL;
                DwmSetWindowAttribute(dropDown.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corners, sizeof(int));
            }
            catch (Exception)
            {
                // non supportato: angoli squadrati
            }
        }

        private static void SetNativeTheme(Control c, string theme)
        {
            try
            {
                if (c.IsHandleCreated) SetWindowTheme(c.Handle, theme, null);
                foreach (Control child in c.Controls)
                    if (child.IsHandleCreated) SetWindowTheme(child.Handle, theme, null);
            }
            catch (Exception)
            {
                // uxtheme non disponibile
            }
        }

        // ── P/Invoke ─────────────────────────────────────────────────────
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE  = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_CAPTION_COLOR            = 35;
        private const int DWMWA_TEXT_COLOR               = 36;
        private const int DWMWCP_ROUND                   = 2;
        private const int DWMWCP_ROUNDSMALL              = 3;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string subAppName, string subIdList);
    }
}
