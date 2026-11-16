using System.Drawing;
using System.Drawing.Drawing2D;

namespace Insonnia.UI
{
    /// <summary>
    /// Disegna a vettore le icone di stato del tray (tazzina di caffè), pensate per essere
    /// nitide e gradevoli a 16px. La tazzina è sempre la stessa; un piccolo overlay stilizzato
    /// comunica la modalità di funzionamento:
    ///  • <see cref="Active"/>        → tazzina calda con vapore, NESSUN overlay (durata illimitata)
    ///  • <see cref="Timer"/>         → tazzina calda + orologino in basso a destra (sessione a tempo)
    ///  • <see cref="IdleSuspended"/> → tazzina spenta + "zzz" in alto a destra (sospeso per inattività)
    ///  • <see cref="Stopped"/>       → tazzina grigia spenta (fermo, nessun overlay)
    /// Nel periodo natalizio (<see cref="Theme.IsFestive"/>) la tazzina porta un rametto di agrifoglio.
    /// Le icone sono precalcolate una sola volta per variante (normale/natalizia) e vivono quanto
    /// l'app (nessun Dispose).
    /// </summary>
    internal static class TrayIconRenderer
    {
        private const int Size = 32;   // tela: resa a 16px dal tray con downscale antialiasing

        // corpo / caffè (tono più scuro dentro il bordo) per stato
        private static readonly Color BodyActive   = Color.FromArgb(0xF2, 0xA0, 0x3B); // ambra calda
        private static readonly Color BrewActive   = Color.FromArgb(0x6B, 0x40, 0x2A); // espresso
        private static readonly Color BodyIdle     = Color.FromArgb(0x8A, 0x93, 0xC2); // indaco tenue
        private static readonly Color BrewIdle     = Color.FromArgb(0x4A, 0x52, 0x86); // indaco scuro
        private static readonly Color BodyStopped  = Color.FromArgb(0xA6, 0xA6, 0xA6); // grigio chiaro
        private static readonly Color BrewStopped  = Color.FromArgb(0x6E, 0x6E, 0x6E); // grigio scuro

        // overlay
        private static readonly Color ClockFace    = Color.FromArgb(0xFA, 0xF4, 0xDE); // quadrante crema
        private static readonly Color ClockInk     = Color.FromArgb(0x3A, 0x2E, 0x22); // bordo/lancette bruno
        private static readonly Color ZzzInk       = Color.FromArgb(0xF6, 0xEF, 0xCB); // crema
        private static readonly Color ZzzEdge      = Color.FromArgb(0x32, 0x38, 0x5E); // contorno indaco scuro

        // agrifoglio (tema natalizio)
        private static readonly Color HollyLeaf    = Color.FromArgb(0x2E, 0x7D, 0x32); // verde agrifoglio
        private static readonly Color HollyVein    = Color.FromArgb(0x1B, 0x5E, 0x20); // nervatura
        private static readonly Color HollyBerry   = Color.FromArgb(0xE5, 0x39, 0x35); // bacca rossa
        private static readonly Color BerryEdge    = Color.FromArgb(0xFF, 0xF8, 0xF6); // contorno: bacche leggibili sulla tazza rossa

        // tazzina natalizia: rossa con bordo bianco "neve" (stati attivo/timer e logo)
        private static readonly Color BodyFestive  = Color.FromArgb(0xD3, 0x2F, 0x2F); // rosso Natale
        private static readonly Color LipFestive   = Color.FromArgb(0xFF, 0xF4, 0xF2); // bordo bianco
        private static readonly Color SteamFestive = Color.FromArgb(0xEF, 0x9A, 0x9A); // vapore rosato

        // cache per variante: indice 0 = normale, 1 = natalizia
        private static readonly Icon[]   _active  = new Icon[2];
        private static readonly Icon[]   _timer   = new Icon[2];
        private static readonly Icon[]   _idle    = new Icon[2];
        private static readonly Icon[]   _stopped = new Icon[2];
        private static readonly Bitmap[] _logo    = new Bitmap[2];

        private static int Variant { get { return Theme.Current.IsFestive ? 1 : 0; } }

        public static Icon Active        { get { int v = Variant; return _active[v]  ?? (_active[v]  = BuildWarm(Overlay.None,  v == 1)); } }
        public static Icon Timer         { get { int v = Variant; return _timer[v]   ?? (_timer[v]   = BuildWarm(Overlay.Clock, v == 1)); } }
        // fermo e sospeso mantengono i colori di stato (grigio/indaco), con l'agrifoglio a Natale
        public static Icon IdleSuspended { get { int v = Variant; return _idle[v]    ?? (_idle[v]    = Build(BodyIdle,    BrewIdle,    BodyIdle,    BodyIdle,    Overlay.Zzz,  false, v == 1)); } }
        public static Icon Stopped       { get { int v = Variant; return _stopped[v] ?? (_stopped[v] = Build(BodyStopped, BrewStopped, BodyStopped, BodyStopped, Overlay.None, false, v == 1)); } }

        /// <summary>Tazzina "calda" (attivo/timer): ambra, oppure rossa con bordo bianco a Natale.</summary>
        private static Icon BuildWarm(Overlay overlay, bool festive)
        {
            return festive
                ? Build(BodyFestive, BrewActive, LipFestive, SteamFestive, overlay, true, true)
                : Build(BodyActive,  BrewActive, BodyActive, BodyActive,   overlay, true, false);
        }

        /// <summary>
        /// Logo "brand" (tazzina ambra calda con vapore) come bitmap a sfondo trasparente,
        /// per la testata della finestra: stessa identità visiva dell'icona del tray.
        /// Precalcolato una volta per variante, vive quanto l'app.
        /// </summary>
        public static Bitmap Logo
        {
            get
            {
                int v = Variant;
                if (_logo[v] == null)
                {
                    Bitmap bmp = new Bitmap(Size, Size);
                    using (Graphics g = Prepare(bmp))
                    {
                        if (v == 1)
                        {
                            DrawSteam(g, SteamFestive);
                            DrawCup(g, BodyFestive, BrewActive, LipFestive);
                            DrawHolly(g);
                        }
                        else
                        {
                            DrawSteam(g, BodyActive);
                            DrawCup(g, BodyActive, BrewActive, BodyActive);
                        }
                    }
                    _logo[v] = bmp;
                }
                return _logo[v];
            }
        }

        // ── icona applicazione (desktop / Esplora risorse / barra applicazioni) ──
        private static readonly Color TileTop    = Color.FromArgb(0xF6, 0xB6, 0x5A); // ambra chiara
        private static readonly Color TileBottom = Color.FromArgb(0xD6, 0x6F, 0x27); // caramello
        private static readonly Color CupCream   = Color.FromArgb(0xFF, 0xF7, 0xEA); // tazzina crema
        private static readonly Color CupBrew    = Color.FromArgb(0x6B, 0x40, 0x2A); // espresso

        /// <summary>
        /// Icona dell'applicazione a <paramref name="size"/> px: riquadro arrotondato con gradiente ambra
        /// e tazzina crema centrata otticamente. Sotto i 32 px il vapore è omesso e la tazzina ingrandita,
        /// per restare nitida. Usata da tools\MakeAppIcon.ps1 per generare insonnia.ico.
        /// </summary>
        public static Bitmap RenderAppIcon(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Prepare(bmp))
            {
                g.Clear(Color.Transparent);

                // riquadro con margine minimo (come le icone di sistema) e angoli ~22%
                float margin = size >= 48 ? size * 0.06f : size >= 24 ? 1f : 0f;
                RectangleF tile = new RectangleF(margin, margin, size - 2 * margin, size - 2 * margin);
                using (GraphicsPath path = Theme.RoundedRect(tile, tile.Width * 0.22f))
                using (LinearGradientBrush brush = new LinearGradientBrush(tile, TileTop, TileBottom, LinearGradientMode.Vertical))
                    g.FillPath(brush, path);

                // tazzina disegnata nello spazio 32x32 dell'icona del tray, ricentrata:
                // ingombro orizzontale 6.5..28.7 (manico incluso), verticale -1..27 col vapore, 9.5..27 senza
                bool  steam   = size >= 32;
                float centerX = 17.6f;
                float centerY = steam ? 13f : 18.3f;
                float scale   = tile.Width / 32f * (steam ? 0.74f : 0.92f);

                GraphicsState state = g.Save();
                g.TranslateTransform(size / 2f, size / 2f);
                g.ScaleTransform(scale, scale);
                g.TranslateTransform(-centerX, -centerY);
                if (steam) DrawSteam(g, CupCream);
                DrawCup(g, CupCream, CupBrew, CupCream);
                g.Restore(state);
            }
            return bmp;
        }

        private enum Overlay { None, Clock, Zzz }

        private static Icon Build(Color body, Color brew, Color lip, Color steamColor,
                                  Overlay overlay, bool steam, bool festive)
        {
            using (Bitmap bmp = new Bitmap(Size, Size))
            using (Graphics g = Prepare(bmp))
            {
                if (steam) DrawSteam(g, steamColor);
                DrawCup(g, body, brew, lip);
                if (festive) DrawHolly(g);
                if (overlay == Overlay.Clock) DrawClock(g);
                else if (overlay == Overlay.Zzz) DrawZzz(g);
                return bmp.ToIcon();
            }
        }

        // ── helpers ────────────────────────────────────────────────────────
        private static Graphics Prepare(Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode   = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            return g;
        }

        /// <summary>
        /// Tazzina paffuta: manico a D, corpo arrotondato, bordo ellittico (<paramref name="lip"/>,
        /// di solito uguale al corpo; bianco "neve" a Natale) col caffè a vista.
        /// </summary>
        private static void DrawCup(Graphics g, Color body, Color brew, Color lip)
        {
            using (SolidBrush bodyBrush = new SolidBrush(body))
            using (SolidBrush lipBrush  = new SolidBrush(lip))
            using (SolidBrush brewBrush = new SolidBrush(brew))
            {
                // Manico a D (dietro al corpo, così si innesta pulito sul fianco destro)
                using (Pen handle = new Pen(body, 3.2f))
                {
                    handle.StartCap = LineCap.Round;
                    handle.EndCap   = LineCap.Round;
                    // arco ampio: le estremità finiscono sotto il corpo, così il manico non resta "staccato"
                    g.DrawArc(handle, 19f, 14.5f, 9.5f, 9f, -105f, 210f);
                }

                // Corpo: rettangolo arrotondato, leggermente più tondo in basso (aspetto "cicciotto")
                using (GraphicsPath bodyPath = RoundedBody())
                    g.FillPath(bodyBrush, bodyPath);

                // Bordo/lip ellittico in cima
                g.FillEllipse(lipBrush, 6.5f, 9.5f, 16f, 6.5f);
                // Caffè a vista: ellisse più scura inset dentro il lip
                g.FillEllipse(brewBrush, 9f, 11f, 11f, 4f);
            }
        }

        /// <summary>Corpo della tazzina: rettangolo arrotondato (raggio maggiore in basso).</summary>
        private static GraphicsPath RoundedBody()
        {
            const float L = 7f, R = 22f, T = 13f, B = 27f, rt = 2.5f, rb = 5f;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(L, T, 2 * rt, 2 * rt, 180, 90);                 // alto sinistra
            p.AddArc(R - 2 * rt, T, 2 * rt, 2 * rt, 270, 90);        // alto destra
            p.AddArc(R - 2 * rb, B - 2 * rb, 2 * rb, 2 * rb, 0, 90); // basso destra
            p.AddArc(L, B - 2 * rb, 2 * rb, 2 * rb, 90, 90);         // basso sinistra
            p.CloseFigure();
            return p;
        }

        /// <summary>
        /// Rametto di agrifoglio (due foglie + tre bacche) sul fianco sinistro della tazzina,
        /// lontano dagli overlay (orologio in basso a destra, "zzz" in alto a destra).
        /// </summary>
        private static void DrawHolly(Graphics g)
        {
            using (SolidBrush leaf  = new SolidBrush(HollyLeaf))
            using (Pen        vein  = new Pen(HollyVein, 0.7f))
            using (SolidBrush berry = new SolidBrush(HollyBerry))
            using (Pen        edge  = new Pen(BerryEdge, 0.7f))
            using (SolidBrush shine = new SolidBrush(Color.FromArgb(170, Color.White)))
            {
                DrawLeaf(g, leaf, vein,  9.4f, 21.2f, -28f);
                DrawLeaf(g, leaf, vein, 14.4f, 21.2f,  28f);

                float[][] berries = { new[] { 10.9f, 18.6f }, new[] { 13.6f, 18.4f }, new[] { 12.2f, 20.1f } };
                foreach (float[] b in berries)
                {
                    g.FillEllipse(berry, b[0] - 1.45f, b[1] - 1.45f, 2.9f, 2.9f);
                    g.DrawEllipse(edge,  b[0] - 1.45f, b[1] - 1.45f, 2.9f, 2.9f);   // contorno chiaro
                    g.FillEllipse(shine, b[0] - 0.8f,  b[1] - 0.9f,  0.9f, 0.9f);   // riflesso
                }
            }
        }

        private static void DrawLeaf(Graphics g, Brush fill, Pen vein, float cx, float cy, float angle)
        {
            GraphicsState state = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            g.FillEllipse(fill, -3.4f, -1.5f, 6.8f, 3.0f);
            g.DrawLine(vein, -2.6f, 0f, 2.6f, 0f);
            g.Restore(state);
        }

        /// <summary>Due volute di vapore morbide sopra la tazzina (stato attivo).</summary>
        private static void DrawSteam(Graphics g, Color color)
        {
            using (Pen p = new Pen(color, 2.2f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap   = LineCap.Round;
                DrawWave(g, p, 11.5f);
                DrawWave(g, p, 17.5f);
            }
        }

        private static void DrawWave(Graphics g, Pen p, float x)
        {
            const float baseY = 9f;
            g.DrawBezier(p, x, baseY, x + 3f, baseY - 3f, x - 3f, baseY - 6f, x, baseY - 9f);
        }

        /// <summary>Orologino in basso a destra (modalità timer): quadrante crema + lancette.</summary>
        private static void DrawClock(Graphics g)
        {
            const float cx = 24f, cy = 24f, r = 7.5f;

            using (SolidBrush face = new SolidBrush(ClockFace))
                g.FillEllipse(face, cx - r, cy - r, 2 * r, 2 * r);

            using (Pen edge = new Pen(ClockInk, 1.6f))
                g.DrawEllipse(edge, cx - r, cy - r, 2 * r, 2 * r);

            using (Pen hands = new Pen(ClockInk, 1.5f))
            {
                hands.StartCap = LineCap.Round;
                hands.EndCap   = LineCap.Round;
                g.DrawLine(hands, cx, cy, cx, cy - r * 0.55f);   // lancetta ore (su)
                g.DrawLine(hands, cx, cy, cx + r * 0.72f, cy);   // lancetta minuti (destra)
            }

            using (SolidBrush pivot = new SolidBrush(ClockInk))
                g.FillEllipse(pivot, cx - 1.1f, cy - 1.1f, 2.2f, 2.2f);
        }

        /// <summary>Tre "z" crescenti verso l'alto a destra (sospeso per inattività).</summary>
        private static void DrawZzz(Graphics g)
        {
            // x, y, lato — dal più piccolo (in basso) al più grande (in alto)
            float[][] zs = {
                new float[] { 17f, 12f,  4.0f },
                new float[] { 20f,  7.5f, 4.8f },
                new float[] { 23f,  2.5f, 5.6f }
            };

            // Contorno scuro (alone) sotto, poi crema sopra: leggibile su sfondo chiaro o scuro.
            using (Pen edge = new Pen(ZzzEdge, 3.0f))
            using (Pen ink  = new Pen(ZzzInk, 1.5f))
            {
                edge.StartCap = edge.EndCap = LineCap.Round; edge.LineJoin = LineJoin.Round;
                ink.StartCap  = ink.EndCap  = LineCap.Round; ink.LineJoin  = LineJoin.Round;
                foreach (float[] z in zs) DrawZ(g, z[0], z[1], z[2], edge);
                foreach (float[] z in zs) DrawZ(g, z[0], z[1], z[2], ink);
            }
        }

        /// <summary>Una "z" stilizzata (zig-zag) di lato <paramref name="s"/> con angolo in alto a sinistra.</summary>
        private static void DrawZ(Graphics g, float x, float y, float s, Pen p)
        {
            PointF[] pts = {
                new PointF(x,     y),
                new PointF(x + s, y),
                new PointF(x,     y + s),
                new PointF(x + s, y + s)
            };
            g.DrawLines(p, pts);
        }
    }
}
