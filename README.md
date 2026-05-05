# ☕ Insonnia

> Keep your PC awake — simply, silently, from the system tray.

**Insonnia** is a lightweight Windows tray application that prevents your PC from going to sleep or turning off the display. It uses the native Windows `SetThreadExecutionState` API — no drivers, no background services, no admin rights required.

---

## ✨ Features

- 🖥️ **Prevents sleep and display shutoff** via Windows API
- ⏱️ **Configurable duration** — always-on or with an auto-stop timer
- 🔔 **Progressive balloon notifications** as the timer approaches expiry
- 📊 **Live progress bar** with color feedback (green → orange → red)
- 🕐 **Elapsed / remaining time** shown in real time in the form and tray tooltip
- 🌍 **Bilingual UI** — Italian and English, auto-detected from Windows language
- 💾 **Persistent settings** — last duration and custom minutes are remembered across restarts
- 🔁 **Start with Windows** option (system tray menu)
- 🪟 **Minimizes to tray** — close button hides the window, app stays alive
- 🔒 **Single instance** — prevents accidental double launch

---

## 🚀 Getting Started

### Requirements

- Windows 10 / 11
- .NET Framework 4.8

### Installation

No installer required. Download the latest release, extract and run `Insonnia.exe`.

---

## 🖱️ Usage

### Main Window

| Control | Description |
|---|---|
| **Duration** combo | Choose how long to keep the PC awake |
| **Custom minutes** field | Appears when "Custom..." is selected — enter any value from 1 to 999 minutes |
| **▶ Start** | Activates keep-awake with the selected duration |
| **■ Stop** | Deactivates keep-awake immediately |
| Status label | Shows current level, elapsed time (∞ mode) or remaining time (timer mode) |
| Progress bar | Visible in timer mode — fills left-to-right, changes color near expiry |

### Duration Options

| Option | Behavior |
|---|---|
| ∞ Always on | Keeps PC awake indefinitely until manually stopped |
| 30 minutes | Auto-stops after 30 minutes |
| 1 hour | Auto-stops after 1 hour |
| 2 hours | Auto-stops after 2 hours |
| 4 hours | Auto-stops after 4 hours |
| Custom... | Enter any duration in minutes (1–999) |

### System Tray Menu

Right-click the tray icon to access:

| Menu item | Description |
|---|---|
| **Show...** | Brings the main window to front |
| **▶ Start / ■ Stop** | Toggles keep-awake (mirrors the form buttons) |
| **Duration ▶** | Submenu to select duration without opening the form |
| **Start with Windows** | Toggles auto-launch at Windows login |
| **Quit** | Fully exits the application |

> Double-clicking the tray icon also opens the main window.

---

## ⚙️ Command-line Parameters

| Parameter | Description |
|---|---|
| `-s` | **Silent autostart** — launches minimized to tray and immediately activates keep-awake. Used automatically by the "Start with Windows" feature. |

**Example:**
```
Insonnia.exe -s
```

---

## 🔔 Timer Notifications

When a finite duration is set, Insonnia shows non-intrusive balloon notifications at key moments:

| Threshold | Notification |
|---|---|
| 75% elapsed | "⏳ Remaining: X min" |
| 90% elapsed | "⚠️ Less than X min left" |
| 95% elapsed | "🔔 Expires in X min" |
| Last 60 seconds | Every 15 s: "⏰ Stops in X seconds" + tray icon blinks |
| Expiry | "⏰ Time's up — good night!" → auto-stop |

---

## 🎨 Activity Levels

The tray icon changes based on how long the PC has been kept awake:

| Icon | Level | After |
|---|---|---|
| ☕ Coffee | Idle / Just started | 0 min |
| 😌 Quiet | Awake | 30 min |
| 😊 Happy | Active | 2 h |
| 😩 Weary | Tired | 4 h |
| 😵 Exhausted | Exhausted | — |

In timer mode, icons show a small **clock overlay** to distinguish them from the standard mode.

---

## 🌍 Localization

The UI language is automatically selected based on the Windows display language:

- 🇮🇹 **Italian** — default
- 🇬🇧 **English** — when Windows language is `en-*`

All other languages fall back to Italian.

---

## 🔧 How It Works

Insonnia calls the Windows API function:

```
SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED)
```

- Called **once at Start** to activate keep-awake
- **Renewed every 60 seconds** to maintain the state
- Called with `ES_CONTINUOUS` only (no other flags) at **Stop** to fully release the lock

This is the same mechanism used by media players and presentation software to prevent sleep during playback.

> **Note:** `ES_AWAYMODE_REQUIRED` is intentionally **not used** — it is reserved for media server scenarios and may interfere with corporate policies.

---

## 💾 Persistent Settings

The following preferences are automatically saved and restored on next launch:

| Setting | Description |
|---|---|
| **Selected duration** | The last duration option chosen (e.g. "1 hour") |
| **Custom minutes** | The last custom duration value entered |

Settings are stored in `%APPDATA%\Insonnia\` via the standard .NET `ApplicationSettingsBase` mechanism.

---

## 📁 Project Structure

```
src/
├── Form1.cs               # Main form logic
├── Form1.Designer.cs      # UI layout
├── InsonniaLevel.cs       # Activity level definitions + timer icons
├── Win32Interop.cs        # SetThreadExecutionState P/Invoke
├── Extensions.cs          # Bitmap → Icon helper
├── Program.cs             # Entry point + single-instance Mutex
└── Properties/
    ├── Strings.resx        # Italian strings (default)
    ├── StringsEn.resx      # English strings
    ├── StringsHelper.cs    # Localization wrapper (class L)
    └── Settings.settings   # User preferences (duration, custom minutes)
```

---

## 📄 License

MIT — see [LICENSE](LICENSE) for details.
