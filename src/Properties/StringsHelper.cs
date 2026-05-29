namespace Insonnia.Properties
{
    using System.Globalization;
    /// <summary>
    /// Wrapper localizzato sopra le classi autogenerate Strings (IT) e StringsEn (EN).
    /// Default: inglese. L'italiano è usato solo per le culture it-*.
    /// </summary>
    internal static class L
    {
        private static bool IsItalian
        {
            get
            {
                return string.Equals(
                    CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                    "it",
                    System.StringComparison.OrdinalIgnoreCase);
            }
        }
        private static string Get(string it, string en)
        {
            return IsItalian ? it : en;
        }

        // UI
        public static string UI_Title         { get { return Get(Strings.UI_Title,         StringsEn.UI_Title); } }
        public static string UI_LabelDuration { get { return Get(Strings.UI_LabelDuration, StringsEn.UI_LabelDuration); } }
        public static string UI_LabelMinutes  { get { return Get(Strings.UI_LabelMinutes,  StringsEn.UI_LabelMinutes); } }
        public static string UI_BtnStart      { get { return Get(Strings.UI_BtnStart,      StringsEn.UI_BtnStart); } }
        public static string UI_BtnStop       { get { return Get(Strings.UI_BtnStop,       StringsEn.UI_BtnStop); } }
        public static string UI_StatusPaused  { get { return Get(Strings.UI_StatusPaused,  StringsEn.UI_StatusPaused); } }
        public static string UI_StatusActive(string t)
        { return string.Format(Get(Strings.UI_StatusActive, StringsEn.UI_StatusActive), t); }
        public static string UI_StatusTimer(string t)
        { return string.Format(Get(Strings.UI_StatusTimer, StringsEn.UI_StatusTimer), t); }
        public static string UI_IdleSuspendLabel    { get { return Get(Strings.UI_IdleSuspendLabel,    StringsEn.UI_IdleSuspendLabel); } }
        public static string UI_StatusIdleSuspended  { get { return Get(Strings.UI_StatusIdleSuspended, StringsEn.UI_StatusIdleSuspended); } }
        // Durate combo
        public static string Duration_Infinite { get { return Get(Strings.Duration_Infinite, StringsEn.Duration_Infinite); } }
        public static string Duration_30min    { get { return Get(Strings.Duration_30min,    StringsEn.Duration_30min); } }
        public static string Duration_1h       { get { return Get(Strings.Duration_1h,       StringsEn.Duration_1h); } }
        public static string Duration_2h       { get { return Get(Strings.Duration_2h,       StringsEn.Duration_2h); } }
        public static string Duration_4h       { get { return Get(Strings.Duration_4h,       StringsEn.Duration_4h); } }
        public static string Duration_Custom   { get { return Get(Strings.Duration_Custom,   StringsEn.Duration_Custom); } }
        // Context menu
        public static string Menu_Show             { get { return Get(Strings.Menu_Show,             StringsEn.Menu_Show); } }
        public static string Menu_Start            { get { return Get(Strings.Menu_Start,            StringsEn.Menu_Start); } }
        public static string Menu_Stop             { get { return Get(Strings.Menu_Stop,             StringsEn.Menu_Stop); } }
        public static string Menu_Duration         { get { return Get(Strings.Menu_Duration,         StringsEn.Menu_Duration); } }
        public static string Menu_StartWithWindows { get { return Get(Strings.Menu_StartWithWindows, StringsEn.Menu_StartWithWindows); } }
        public static string Menu_Quit             { get { return Get(Strings.Menu_Quit,             StringsEn.Menu_Quit); } }
        public static string Menu_CustomMinutes(int min)
        { return string.Format(Get(Strings.Menu_CustomMinutes, StringsEn.Menu_CustomMinutes), min); }
        public static string Menu_IdleSuspend { get { return Get(Strings.Menu_IdleSuspend, StringsEn.Menu_IdleSuspend); } }
        public static string Menu_IdleOff      { get { return Get(Strings.Menu_IdleOff,      StringsEn.Menu_IdleOff); } }
        public static string Menu_IdleMinutes(int min)
        { return string.Format(Get(Strings.Menu_IdleMinutes, StringsEn.Menu_IdleMinutes), min); }
        public static string Menu_IdleCustomMinutes(int min)
        { return string.Format(Get(Strings.Menu_IdleCustomMinutes, StringsEn.Menu_IdleCustomMinutes), min); }
        // Balloon
        public static string Balloon_StartedTitle    { get { return Get(Strings.Balloon_StartedTitle,    StringsEn.Balloon_StartedTitle); } }
        public static string Balloon_StoppedTitle    { get { return Get(Strings.Balloon_StoppedTitle,    StringsEn.Balloon_StoppedTitle); } }
        public static string Balloon_StoppedText     { get { return Get(Strings.Balloon_StoppedText,     StringsEn.Balloon_StoppedText); } }
        public static string Balloon_Expired         { get { return Get(Strings.Balloon_Expired,         StringsEn.Balloon_Expired); } }
        public static string Balloon_75Title         { get { return Get(Strings.Balloon_75Title,         StringsEn.Balloon_75Title); } }
        public static string Balloon_90Title         { get { return Get(Strings.Balloon_90Title,         StringsEn.Balloon_90Title); } }
        public static string Balloon_95Title         { get { return Get(Strings.Balloon_95Title,         StringsEn.Balloon_95Title); } }
        public static string Balloon_UrgentTitle     { get { return Get(Strings.Balloon_UrgentTitle,     StringsEn.Balloon_UrgentTitle); } }
        public static string Balloon_StartIndefinite { get { return Get(Strings.Balloon_StartIndefinite, StringsEn.Balloon_StartIndefinite); } }
        public static string Balloon_IdleSuspendTitle { get { return Get(Strings.Balloon_IdleSuspendTitle, StringsEn.Balloon_IdleSuspendTitle); } }
        public static string Balloon_IdleSuspendText  { get { return Get(Strings.Balloon_IdleSuspendText,  StringsEn.Balloon_IdleSuspendText); } }
        public static string Balloon_IdleResumeTitle  { get { return Get(Strings.Balloon_IdleResumeTitle,  StringsEn.Balloon_IdleResumeTitle); } }
        public static string Balloon_IdleResumeText   { get { return Get(Strings.Balloon_IdleResumeText,   StringsEn.Balloon_IdleResumeText); } }
        public static string Balloon_StartedText(string d)
        { return string.Format(Get(Strings.Balloon_StartedText, StringsEn.Balloon_StartedText), d); }
        public static string Balloon_StartFor(string d)
        { return string.Format(Get(Strings.Balloon_StartFor, StringsEn.Balloon_StartFor), d); }
        public static string Balloon_75Text(string r)
        { return string.Format(Get(Strings.Balloon_75Text, StringsEn.Balloon_75Text), r); }
        public static string Balloon_90Text(string r)
        { return string.Format(Get(Strings.Balloon_90Text, StringsEn.Balloon_90Text), r); }
        public static string Balloon_95Text(string r)
        { return string.Format(Get(Strings.Balloon_95Text, StringsEn.Balloon_95Text), r); }
        public static string Balloon_UrgentText(int s)
        { return string.Format(Get(Strings.Balloon_UrgentText, StringsEn.Balloon_UrgentText), s); }
        // Tray
        public static string Tray_Paused { get { return Get(Strings.Tray_Paused, StringsEn.Tray_Paused); } }
        public static string Tray_ActiveElapsed(string t)
        { return string.Format(Get(Strings.Tray_ActiveElapsed, StringsEn.Tray_ActiveElapsed), t); }
        public static string Tray_ActiveRemaining(string t)
        { return string.Format(Get(Strings.Tray_ActiveRemaining, StringsEn.Tray_ActiveRemaining), t); }
        public static string Tray_IdleSuspended { get { return Get(Strings.Tray_IdleSuspended, StringsEn.Tray_IdleSuspended); } }
        // Messaggi
        public static string Msg_AlreadyRunning { get { return Get(Strings.Msg_AlreadyRunning, StringsEn.Msg_AlreadyRunning); } }
    }
}
