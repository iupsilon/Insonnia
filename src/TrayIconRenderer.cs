using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Insonnia
{
    /// <summary>
    /// Compone a runtime le icone "dinamiche" del tray a partire dalla glyph mood:
    ///  • Timer    → anello di progresso che cala (verde → ambra → rosso), pulsa nell'urgenza
    ///  • Active   → anello pieno tenue (sempre attivo, nessun conto alla rovescia)
    ///  • IdleSuspended → glyph attenuata + lunetta (PC libero di dormire)
    /// Tutte ritornano una nuova Icon: il chiamante è responsabile del Dispose.
    /// </summary>
    internal static class TrayIconRenderer
    {
        private const int  Size       = 32;     // tela icona
        private const float Stroke    = 3.5f;   // spessore anello
        private const float StartTop  = -90f;   // ore 12

        private static readonly Color Track   = Color.FromArgb(70, 130, 130, 130);
        private static readonly Color Green   = Color.FromArgb(0, 153, 76);
        private static readonly Color Orange  = Color.Orange;
        private static readonly Color Crimson = Color.Crimson;
        private static readonly Color Blue    = Color.FromArgb(150, 0, 120, 204);

        /// <summary>Anello di progresso. <paramref name="elapsedRatio"/> in [0,1]; il riempimento residuo cala.</summary>
        public static Icon Timer(Bitmap source, double elapsedRatio, bool urgent, bool pulseOn)
        {
            double remaining = Math.Max(0.0, Math.Min(1.0, 1.0 - elapsedRatio));
            using (Bitmap bmp = NewCanvas())
            using (Graphics g = Prepare(bmp))
            {
                RectangleF ring = RingRect();
                DrawGlyph(g, source, 1f);
                DrawTrack(g, ring);

                if (urgent)
                {
                    // Allarme: anello intero rosso che pulsa di intensità
                    int alpha = pulseOn ? 255 : 110;
                    DrawArc(g, ring, Color.FromArgb(alpha, Crimson), StartTop, 360f);
                }
                else
                {
                    Color c = elapsedRatio >= 0.75 ? Orange : Green;
                    DrawArc(g, ring, c, StartTop, (float)(remaining * 360.0));
                }
                return bmp.ToIcon();
            }
        }

        /// <summary>Modalità "sempre attivo": glyph + anello pieno tenue, senza conto alla rovescia.</summary>
        public static Icon Active(Bitmap source)
        {
            using (Bitmap bmp = NewCanvas())
            using (Graphics g = Prepare(bmp))
            {
                RectangleF ring = RingRect();
                DrawGlyph(g, source, 1f);
                DrawTrack(g, ring);
                DrawArc(g, ring, Blue, StartTop, 360f);
                return bmp.ToIcon();
            }
        }

        /// <summary>Sospeso per inattività: glyph attenuata + lunetta (il PC può dormire).</summary>
        public static Icon IdleSuspended(Bitmap source)
        {
            using (Bitmap bmp = NewCanvas())
            using (Graphics g = Prepare(bmp))
            {
                DrawGlyph(g, source, 0.40f);
                DrawMoonBadge(g);
                return bmp.ToIcon();
            }
        }

        // ── helpers ────────────────────────────────────────────────────────
        private static Bitmap NewCanvas() { return new Bitmap(Size, Size); }

        private static Graphics Prepare(Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode   = PixelOffsetMode.HighQuality;
            return g;
        }

        private static RectangleF RingRect()
        {
            float pad = Stroke / 2f + 0.5f;
            return new RectangleF(pad, pad, Size - 2 * pad, Size - 2 * pad);
        }

        /// <summary>Disegna la glyph centrata, lasciando spazio all'anello. <paramref name="alpha"/> in [0,1].</summary>
        private static void DrawGlyph(Graphics g, Bitmap source, float alpha)
        {
            // La glyph occupa l'area interna all'anello
            const float inset = 6f;
            RectangleF dest = new RectangleF(inset, inset, Size - 2 * inset, Size - 2 * inset);

            if (alpha >= 1f)
            {
                g.DrawImage(source, dest.X, dest.Y, dest.Width, dest.Height);
                return;
            }
            ColorMatrix cm = new ColorMatrix();
            cm.Matrix33 = alpha; // canale alpha
            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(source,
                    new Rectangle((int)dest.X, (int)dest.Y, (int)dest.Width, (int)dest.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
            }
        }

        private static void DrawTrack(Graphics g, RectangleF ring)
        {
            using (Pen p = new Pen(Track, Stroke))
                g.DrawEllipse(p, ring.X, ring.Y, ring.Width, ring.Height);
        }

        private static void DrawArc(Graphics g, RectangleF ring, Color color, float start, float sweep)
        {
            if (sweep <= 0.01f) return;
            using (Pen p = new Pen(color, Stroke))
            {
                p.StartCap = LineCap.Round;
                p.EndCap   = LineCap.Round;
                g.DrawArc(p, ring.X, ring.Y, ring.Width, ring.Height, start, sweep);
            }
        }

        /// <summary>Lunetta in basso a destra, scolpita per sottrazione su una piccola tela trasparente.</summary>
        private static void DrawMoonBadge(Graphics g)
        {
            const int d = 15;
            using (Bitmap moon = new Bitmap(d, d))
            {
                using (Graphics mg = Graphics.FromImage(moon))
                {
                    mg.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(250, 248, 244, 200)))
                        mg.FillEllipse(b, 1, 1, d - 2, d - 2);
                    // Scolpisce la falce sovrapponendo un cerchio trasparente (SourceCopy)
                    mg.CompositingMode = CompositingMode.SourceCopy;
                    using (SolidBrush clear = new SolidBrush(Color.Transparent))
                        mg.FillEllipse(clear, 5, -2, d - 1, d - 1);
                }
                g.DrawImage(moon, Size - d, Size - d, d, d);
            }
        }
    }
}
