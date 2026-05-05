using Insonnia.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Insonnia
{
    internal class InsonniaLevel
    {
        // I nomi dei livelli sono proprietà lazy per supportare il cambio lingua a runtime
        private static readonly InsonniaLevel Off      = new InsonniaLevel(TimeSpan.Zero,            "Level_Off",       Resources.coffee.ToIcon(),    null);
        private static readonly InsonniaLevel Quiet    = new InsonniaLevel(TimeSpan.FromMinutes(30),  "Level_Quiet",     Resources.quiet.ToIcon(),     BuildTimerIcon(Resources.quiet,    Color.DodgerBlue));
        private static readonly InsonniaLevel Happy    = new InsonniaLevel(TimeSpan.FromHours(2),    "Level_Happy",     Resources.happy.ToIcon(),     BuildTimerIcon(Resources.happy,    Color.DodgerBlue));
        private static readonly InsonniaLevel Weary    = new InsonniaLevel(TimeSpan.FromHours(4),    "Level_Weary",     Resources.weary.ToIcon(),     BuildTimerIcon(Resources.weary,    Color.Orange));
        private static readonly InsonniaLevel Exhausted= new InsonniaLevel(TimeSpan.MaxValue,        "Level_Exhausted", Resources.exhausted.ToIcon(), BuildTimerIcon(Resources.exhausted, Color.Red));

        public static readonly InsonniaLevel[] Levels = new InsonniaLevel[] { Off, Quiet, Happy, Weary, Exhausted };

        private readonly TimeSpan _timeKeptAwake;
        private readonly string   _nameKey;   // chiave risorsa
        private readonly Icon     _icon;
        private readonly Icon     _timerIcon;

        public TimeSpan TimeKeptAwake { get { return _timeKeptAwake; } }
        /// <summary>Nome localizzato del livello — risolto a runtime dalla cultura corrente.</summary>
        public string   Name          { get { return L.Get(_nameKey); } }
        public Icon     Icon          { get { return _icon; } }
        public Icon     TimerIcon     { get { return _timerIcon; } }

        private InsonniaLevel(TimeSpan timeKeptAwake, string nameKey, Icon icon, Icon timerIcon)
        {
            _timeKeptAwake = timeKeptAwake;
            _nameKey       = nameKey;
            _icon          = icon;
            _timerIcon     = (timerIcon != null) ? timerIcon : icon;
        }

        public static InsonniaLevel GetCurrent(TimeSpan timeAwake)
        {
            foreach (InsonniaLevel level in Levels)
                if (timeAwake <= level._timeKeptAwake)
                    return level;
            return Exhausted;
        }

        private static Icon BuildTimerIcon(Bitmap source, Color clockColor)
        {
            using (Bitmap bmp = new Bitmap(source, 16, 16))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle clockRect = new Rectangle(9, 9, 6, 6);
                using (SolidBrush bg = new SolidBrush(Color.White))
                    g.FillEllipse(bg, clockRect);
                using (Pen border = new Pen(clockColor, 1.2f))
                    g.DrawEllipse(border, clockRect);

                float cx = clockRect.X + clockRect.Width / 2f;
                float cy = clockRect.Y + clockRect.Height / 2f;
                using (Pen pen = new Pen(clockColor, 1f))
                {
                    g.DrawLine(pen, cx, cy, cx, cy - 2f);
                    g.DrawLine(pen, cx, cy, cx + 1.5f, cy);
                }

                return bmp.ToIcon();
            }
        }
    }
}
