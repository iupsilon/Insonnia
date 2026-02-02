using Insonnia.Properties;
using System;
using System.Drawing;

namespace Insonnia
{
    internal class InsonniaLevel
    {
        private static readonly InsonniaLevel Off = new InsonniaLevel(TimeSpan.Zero, Resources.coffee.ToIcon());
        private static readonly InsonniaLevel Quiet = new InsonniaLevel(TimeSpan.FromMinutes(30), Resources.quiet.ToIcon());
        private static readonly InsonniaLevel Happy = new InsonniaLevel(TimeSpan.FromHours(2), Resources.happy.ToIcon());
        private static readonly InsonniaLevel Weary = new InsonniaLevel(TimeSpan.FromHours(4), Resources.weary.ToIcon());
        private static readonly InsonniaLevel Exhausted = new InsonniaLevel(TimeSpan.MaxValue, Resources.exhausted.ToIcon());

        public static readonly InsonniaLevel[] Levels = new InsonniaLevel[] { Off, Quiet, Happy, Weary, Exhausted };

        public TimeSpan TimeKeptAwake { get; private set; }
        public Icon Icon { get; private set; }

        private InsonniaLevel(TimeSpan timeKeptAwake, Icon icon)
        {
            TimeKeptAwake = timeKeptAwake;
            Icon = icon;
        }
    }
}
