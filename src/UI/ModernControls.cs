using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Insonnia.UI
{
    internal enum ButtonVariant
    {
        Primary,     // azione principale (ambra)
        Danger,      // azione di arresto (rosso)
        Secondary,   // azione secondaria (superficie + bordo)
        Subtle       // solo testo/icona, sfondo al passaggio del mouse
    }

    /// <summary>Pulsante Fluent: angoli arrotondati, stati hover/pressed, icona opzionale.</summary>
    internal sealed class ModernButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public ButtonVariant Variant  { get; set; }
        public string        Glyph    { get; set; }
        public int           Radius   { get; set; }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor   = false;
            Cursor  = Cursors.Hand;
            Radius  = 8;
            Variant = ButtonVariant.Secondary;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme    t        = Theme.Current;
            Graphics g        = e.Graphics;
            Color    parentBg = Parent != null ? Parent.BackColor : t.Background;
            g.Clear(parentBg);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill, border, text;
            switch (Variant)
            {
                case ButtonVariant.Primary:
                    fill   = !Enabled ? t.Track : _pressed ? t.AccentPressed : _hover ? t.AccentHover : t.Accent;
                    border = fill;
                    text   = Enabled ? t.AccentText : t.TextMuted;
                    break;
                case ButtonVariant.Danger:
                    fill   = !Enabled ? t.Track : _pressed ? t.DangerPressed : _hover ? t.DangerHover : t.Danger;
                    border = fill;
                    text   = Enabled ? t.DangerText : t.TextMuted;
                    break;
                case ButtonVariant.Subtle:
                    fill   = _pressed ? t.Border : _hover ? t.SurfaceAlt : parentBg;
                    border = fill;
                    text   = Enabled ? t.Text : t.TextMuted;
                    break;
                default:
                    Color rest = parentBg == t.Surface ? t.SurfaceAlt : t.Surface;
                    fill   = _pressed ? t.Border : _hover ? Blend(rest, t.Border, 0.5f) : rest;
                    border = t.Border;
                    text   = Enabled ? t.Text : t.TextMuted;
                    break;
            }

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (GraphicsPath path = Theme.RoundedRect(r, Scale(Radius)))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            // contenuto: icona + testo centrati
            const TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding |
                                          TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            Size textSize  = TextRenderer.MeasureText(g, Text, Font, Size.Empty, flags);
            bool hasGlyph  = !string.IsNullOrEmpty(Glyph) && Theme.IconFont(10) != null;
            int  glyphSize = hasGlyph ? Font.Height : 0;
            int  gap       = hasGlyph && Text.Length > 0 ? Scale(8) : 0;
            int  x         = (Width - (glyphSize + gap + textSize.Width)) / 2;

            if (hasGlyph)
            {
                Theme.DrawGlyph(g, Glyph, Font.SizeInPoints, text, new RectangleF(x, 0, glyphSize, Height));
                x += glyphSize + gap;
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, textSize.Width + 2, Height), text, fill, flags);

            if (Focused && ShowFocusCues)
            {
                RectangleF fr = RectangleF.Inflate(r, -2.5f, -2.5f);
                using (GraphicsPath path = Theme.RoundedRect(fr, Math.Max(2, Scale(Radius) - 2)))
                using (Pen pen = new Pen(Color.FromArgb(160, text), 1f))
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawPath(pen, path);
                }
            }
        }

        private int Scale(int px) { return (int)Math.Round(px * DeviceDpi / 96f); }

        internal static Color Blend(Color a, Color b, float amount)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * amount),
                (int)(a.G + (b.G - a.G) * amount),
                (int)(a.B + (b.B - a.B) * amount));
        }
    }

    /// <summary>Interruttore on/off in stile Windows 11 (sostituisce la CheckBox), con breve animazione.</summary>
    internal sealed class ToggleSwitch : CheckBox
    {
        private readonly Timer _anim = new Timer();
        private float _pos;   // 0 = spento, 1 = acceso

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            Cursor   = Cursors.Hand;
            _anim.Interval = 15;
            _anim.Tick    += Anim_Tick;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            if (IsHandleCreated && Visible)
                _anim.Start();
            else
            {
                _pos = Checked ? 1f : 0f;
                Invalidate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _pos = Checked ? 1f : 0f;
        }

        private void Anim_Tick(object sender, EventArgs e)
        {
            float target = Checked ? 1f : 0f;
            float step   = 0.2f;
            if (Math.Abs(target - _pos) <= step)
            {
                _pos = target;
                _anim.Stop();
            }
            else
                _pos += target > _pos ? step : -step;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme    t = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : t.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float s = DeviceDpi / 96f;
            float w = 40 * s, h = 20 * s;
            RectangleF track = new RectangleF(1, (Height - h) / 2f, w, h);

            Color offBorder = Enabled ? t.TextMuted : t.Border;
            Color onFill    = Enabled ? t.Accent : t.Track;

            using (GraphicsPath path = Theme.RoundedRect(track, h / 2f))
            {
                // spento: solo contorno; acceso: pieno colore accento (dissolvenza con _pos)
                using (Pen pen = new Pen(offBorder, 1.2f * s))
                    g.DrawPath(pen, path);
                if (_pos > 0)
                    using (SolidBrush on = new SolidBrush(Color.FromArgb((int)(255 * _pos), onFill)))
                        g.FillPath(on, path);
            }

            float knob = (12 + 2 * _pos) * s;
            float left = track.X + 4 * s;
            float right = track.Right - 4 * s - knob;
            float kx   = left + (right - left) * _pos;
            Color knobColor = _pos > 0.5f ? (Enabled ? t.AccentText : t.TextMuted) : offBorder;
            using (SolidBrush brush = new SolidBrush(knobColor))
                g.FillEllipse(brush, kx, track.Y + (h - knob) / 2f, knob, knob);

            Rectangle textRect = new Rectangle((int)(track.Right + 10 * s), 0, Width - (int)(track.Right + 10 * s), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, Enabled ? t.Text : t.TextMuted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                RectangleF fr = RectangleF.Inflate(track, 2 * s, 2 * s);
                using (GraphicsPath path = Theme.RoundedRect(fr, fr.Height / 2f))
                using (Pen pen = new Pen(Color.FromArgb(140, t.Text), 1f))
                    g.DrawPath(pen, path);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _anim.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Anello di avanzamento con testo grande al centro (tempo) e didascalia (stato).
    /// </summary>
    internal sealed class ProgressRing : Control
    {
        private float  _fraction;
        private Color  _ringColor = Color.Orange;
        private string _caption   = "";
        private bool   _muted;

        public ProgressRing()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Porzione di anello colorata, 0..1.</summary>
        public float Fraction
        {
            get { return _fraction; }
            set { _fraction = Math.Max(0f, Math.Min(1f, value)); Invalidate(); }
        }

        public Color RingColor
        {
            get { return _ringColor; }
            set { _ringColor = value; Invalidate(); }
        }

        public string Caption
        {
            get { return _caption; }
            set { _caption = value ?? ""; Invalidate(); }
        }

        /// <summary>Testo centrale attenuato (sessione ferma).</summary>
        public bool Muted
        {
            get { return _muted; }
            set { _muted = value; Invalidate(); }
        }

        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme    t = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : t.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float s     = DeviceDpi / 96f;
            float thick = 12 * s;
            float side  = Math.Min(Width, Height) - thick - 8 * s;
            RectangleF r = new RectangleF((Width - side) / 2f, (Height - side) / 2f, side, side);

            using (Pen track = new Pen(t.Track, thick))
                g.DrawEllipse(track, r);

            if (_fraction > 0)
            {
                // alone tenue + arco pieno con estremità arrotondate
                using (Pen glow = new Pen(Color.FromArgb(45, _ringColor), thick + 8 * s))
                using (Pen arc  = new Pen(_ringColor, thick))
                {
                    glow.StartCap = glow.EndCap = LineCap.Round;
                    arc.StartCap  = arc.EndCap  = LineCap.Round;
                    if (_fraction >= 0.999f)
                    {
                        g.DrawEllipse(glow, r);
                        g.DrawEllipse(arc, r);
                    }
                    else
                    {
                        float sweep = 360f * _fraction;
                        g.DrawArc(glow, r, -90f, sweep);
                        g.DrawArc(arc,  r, -90f, sweep);
                    }
                }
            }

            // testo centrale (tempo) + didascalia
            Font big   = Theme.DisplayFont(26f);
            Font small = Theme.UiFont(9.5f);
            Size bigSize   = TextRenderer.MeasureText(g, Text, big, Size.Empty, TextFormatFlags.NoPadding);
            Size smallSize = TextRenderer.MeasureText(g, _caption, small, Size.Empty, TextFormatFlags.NoPadding);
            int  top       = (Height - bigSize.Height - smallSize.Height) / 2;

            TextRenderer.DrawText(g, Text, big, new Rectangle(0, top, Width, bigSize.Height),
                _muted ? t.TextMuted : t.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, _caption, small, new Rectangle(0, top + bigSize.Height, Width, smallSize.Height),
                t.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
        }
    }

    /// <summary>
    /// Combo a elenco (DropDownList) disegnato interamente a tema: riquadro arrotondato,
    /// chevron Fluent, voci con evidenziazione tenue. Per i combo editabili si usa quello nativo.
    /// </summary>
    internal sealed class ModernComboBox : ComboBox
    {
        private bool _hover;

        public ModernComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawMode      = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle     = FlatStyle.Flat;
            ItemHeight    = 24;
            Cursor        = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)       { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e)       { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e)         { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e)        { Invalidate(); base.OnLostFocus(e); }
        protected override void OnDropDownClosed(EventArgs e)   { Invalidate(); base.OnDropDownClosed(e); }
        protected override void OnEnabledChanged(EventArgs e)   { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme    t = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : t.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill   = !Enabled ? (Parent != null ? Parent.BackColor : t.Background)
                         : _hover   ? ModernButton.Blend(t.SurfaceAlt, t.Border, 0.5f)
                                    : t.SurfaceAlt;
            Color border = Enabled && (Focused || DroppedDown) ? t.Accent : t.Border;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (GraphicsPath path = Theme.RoundedRect(r, 6 * DeviceDpi / 96f))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            int chevron = (int)(26 * DeviceDpi / 96f);
            string text = SelectedItem != null ? SelectedItem.ToString() : "";
            TextRenderer.DrawText(g, text, Font, new Rectangle(10, 0, Width - chevron - 10, Height),
                Enabled ? t.Text : t.TextMuted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            Theme.DrawGlyph(g, Theme.GlyphChevronDown, 8f, t.TextMuted,
                new RectangleF(Width - chevron, 0, chevron - 4, Height));
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            Theme t        = Theme.Current;
            bool  selected = (e.State & DrawItemState.Selected) != 0;
            using (SolidBrush bg = new SolidBrush(selected ? t.AccentSoft : t.Surface))
                e.Graphics.FillRectangle(bg, e.Bounds);
            Rectangle text = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), Font, text, t.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Contenitore "card": superficie arrotondata con bordo sottile.</summary>
    internal sealed class ModernCard : Panel
    {
        public ModernCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Theme    t = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : t.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (GraphicsPath path = Theme.RoundedRect(r, 10 * DeviceDpi / 96f))
            using (SolidBrush brush = new SolidBrush(t.Surface))
            using (Pen pen = new Pen(t.Border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }
    }

    /// <summary>Linea divisoria orizzontale di 1 px.</summary>
    internal sealed class ModernSeparator : Control
    {
        public ModernSeparator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme t = Theme.Current;
            e.Graphics.Clear(Parent != null ? Parent.BackColor : t.Background);
            using (Pen pen = new Pen(t.Border))
                e.Graphics.DrawLine(pen, 0, Height / 2, Width, Height / 2);
        }
    }

    /// <summary>Riquadro arrotondato con il logo dell'app su una tinta ambra tenue.</summary>
    internal sealed class LogoTile : Control
    {
        public LogoTile()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme    t = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : t.Background);
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            RectangleF r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (GraphicsPath path = Theme.RoundedRect(r, 12 * DeviceDpi / 96f))
            using (SolidBrush brush = new SolidBrush(t.AccentSoft))
                g.FillPath(brush, path);

            float side = Math.Min(Width, Height) * 0.72f;
            g.DrawImage(TrayIconRenderer.Logo, (Width - side) / 2f, (Height - side) / 2f, side, side);
        }
    }

    /// <summary>Renderer del menu del tray: superficie a tema, evidenziazione arrotondata, spunte Fluent.</summary>
    internal sealed class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer()
        {
            RoundedEdges = false;
        }

        private static Color HoverColor(Theme t)
        {
            return t.IsDark ? Color.FromArgb(0x3A, 0x3A, 0x3F) : Color.FromArgb(0xF0, 0xEB, 0xE4);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.Graphics.Clear(Theme.Current.Surface);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Current.Border))
                e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            // nessuna colonna icone separata: superficie uniforme
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected || !e.Item.Enabled) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(4, 1, e.Item.Width - 8, e.Item.Height - 2);
            using (GraphicsPath path = Theme.RoundedRect(r, 4))
            using (SolidBrush brush = new SolidBrush(HoverColor(Theme.Current)))
                g.FillPath(brush, path);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            Theme t = Theme.Current;
            e.TextColor = e.Item.Enabled ? t.Text : t.TextMuted;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.Current.TextMuted;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using (Pen pen = new Pen(Theme.Current.Border))
                e.Graphics.DrawLine(pen, 12, y, e.Item.Width - 12, y);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Theme t = Theme.Current;
            if (Theme.IconFont(9) == null)
            {
                base.OnRenderItemCheck(e);
                return;
            }
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = RectangleF.Inflate(e.ImageRectangle, 1, 1);
            using (GraphicsPath path = Theme.RoundedRect(r, 4))
            using (SolidBrush brush = new SolidBrush(t.AccentSoft))
                g.FillPath(brush, path);
            Theme.DrawGlyph(g, Theme.GlyphCheck, 9f, t.IsDark ? t.Accent : t.AccentPressed, r);
        }
    }
}
