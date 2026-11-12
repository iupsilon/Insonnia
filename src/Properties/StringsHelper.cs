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
        public static string UI_Subtitle       { get { return Get(Strings.UI_Subtitle,       StringsEn.UI_Subtitle); } }
        public static string UI_RingElapsed    { get { return Get(Strings.UI_RingElapsed,    StringsEn.UI_RingElapsed); } }
        public static string UI_RingRemaining  { get { return Get(Strings.UI_RingRemaining,  StringsEn.UI_RingRemaining); } }
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
        // Cambio ingresso monitor — menu tray
        public static string Menu_MonitorSwitch          { get { return Get(Strings.Menu_MonitorSwitch,          StringsEn.Menu_MonitorSwitch); } }
        public static string Menu_MonitorSwitchEnabled   { get { return Get(Strings.Menu_MonitorSwitchEnabled,   StringsEn.Menu_MonitorSwitchEnabled); } }
        public static string Menu_MonitorSwitchConfigure { get { return Get(Strings.Menu_MonitorSwitchConfigure, StringsEn.Menu_MonitorSwitchConfigure); } }
        public static string Menu_MonitorSwitchNow(string input)
        { return string.Format(Get(Strings.Menu_MonitorSwitchNow, StringsEn.Menu_MonitorSwitchNow), input); }
        public static string Balloon_MonitorSwitchFailed { get { return Get(Strings.Balloon_MonitorSwitchFailed, StringsEn.Balloon_MonitorSwitchFailed); } }
        // Cambio ingresso monitor — finestra di configurazione
        public static string MS_Enable            { get { return Get(Strings.MS_Enable,            StringsEn.MS_Enable); } }
        public static string MS_UsbDevice         { get { return Get(Strings.MS_UsbDevice,         StringsEn.MS_UsbDevice); } }
        public static string MS_Monitors          { get { return Get(Strings.MS_Monitors,          StringsEn.MS_Monitors); } }
        public static string MS_MonitorId         { get { return Get(Strings.MS_MonitorId,         StringsEn.MS_MonitorId); } }
        public static string MS_InputOnConnect    { get { return Get(Strings.MS_InputOnConnect,    StringsEn.MS_InputOnConnect); } }
        public static string MS_InputOnDisconnect { get { return Get(Strings.MS_InputOnDisconnect, StringsEn.MS_InputOnDisconnect); } }
        public static string MS_NoAction          { get { return Get(Strings.MS_NoAction,          StringsEn.MS_NoAction); } }
        public static string MS_Refresh           { get { return Get(Strings.MS_Refresh,           StringsEn.MS_Refresh); } }
        public static string MS_RefreshMonitors   { get { return Get(Strings.MS_RefreshMonitors,   StringsEn.MS_RefreshMonitors); } }
        public static string MS_CopyList          { get { return Get(Strings.MS_CopyList,          StringsEn.MS_CopyList); } }
        public static string MS_Test              { get { return Get(Strings.MS_Test,              StringsEn.MS_Test); } }
        public static string MS_Save              { get { return Get(Strings.MS_Save,              StringsEn.MS_Save); } }
        public static string MS_Cancel            { get { return Get(Strings.MS_Cancel,            StringsEn.MS_Cancel); } }
        public static string MS_ColName           { get { return Get(Strings.MS_ColName,           StringsEn.MS_ColName); } }
        public static string MS_ColSerial         { get { return Get(Strings.MS_ColSerial,         StringsEn.MS_ColSerial); } }
        public static string MS_ColInput          { get { return Get(Strings.MS_ColInput,          StringsEn.MS_ColInput); } }
        public static string MS_ColInstance       { get { return Get(Strings.MS_ColInstance,       StringsEn.MS_ColInstance); } }
        public static string MS_ColDisplay        { get { return Get(Strings.MS_ColDisplay,        StringsEn.MS_ColDisplay); } }
        public static string MS_Detecting         { get { return Get(Strings.MS_Detecting,         StringsEn.MS_Detecting); } }
        public static string MS_Detected(int count)
        { return string.Format(Get(Strings.MS_Detected, StringsEn.MS_Detected), count); }
        public static string MS_DdcUnavailable    { get { return Get(Strings.MS_DdcUnavailable,    StringsEn.MS_DdcUnavailable); } }
        public static string MS_Copied            { get { return Get(Strings.MS_Copied,            StringsEn.MS_Copied); } }
        public static string MS_Sending           { get { return Get(Strings.MS_Sending,           StringsEn.MS_Sending); } }
        public static string MS_InvalidUsb        { get { return Get(Strings.MS_InvalidUsb,        StringsEn.MS_InvalidUsb); } }
        public static string MS_InvalidInput      { get { return Get(Strings.MS_InvalidInput,      StringsEn.MS_InvalidInput); } }
        public static string MS_MissingMonitor    { get { return Get(Strings.MS_MissingMonitor,    StringsEn.MS_MissingMonitor); } }
        public static string MS_Outcome_NotConfigured { get { return Get(Strings.MS_Outcome_NotConfigured, StringsEn.MS_Outcome_NotConfigured); } }
        public static string MS_Outcome_DdcError(string detail)
        { return string.Format(Get(Strings.MS_Outcome_DdcError, StringsEn.MS_Outcome_DdcError), detail); }
        public static string MS_Outcome_Switched(string input)
        { return string.Format(Get(Strings.MS_Outcome_Switched, StringsEn.MS_Outcome_Switched), input); }
        public static string MS_Outcome_AlreadyActive(string input)
        { return string.Format(Get(Strings.MS_Outcome_AlreadyActive, StringsEn.MS_Outcome_AlreadyActive), input); }
        public static string MS_Outcome_NotFound(string monitorId)
        { return string.Format(Get(Strings.MS_Outcome_NotFound, StringsEn.MS_Outcome_NotFound), monitorId); }
        public static string MS_Outcome_Ambiguous(string monitorId)
        { return string.Format(Get(Strings.MS_Outcome_Ambiguous, StringsEn.MS_Outcome_Ambiguous), monitorId); }
    }
}
