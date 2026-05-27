# ☕ Insonnia

> Keep your PC awake — simply, silently, from the system tray.

**Insonnia** is a lightweight Windows tray application that prevents your PC from going to sleep or turning off the display. It uses the native Windows `SetThreadExecutionState` API — no drivers, no background services, no admin rights required.

---

## ✨ Features

- 🖥️ **Prevents sleep and display shutoff** via Windows API
- ⏱️ **Configurable duration** — always-on or with an auto-stop timer
- 💤 **Idle auto-suspend** *(optional)* — if you walk away, releases the lock after N idle minutes so the PC can sleep, then resumes automatically on the next keyboard/mouse activity
- 🔔 **Progressive balloon notifications** as the timer approaches expiry
- 📊 **Live progress bar** with color feedback (green → orange → red)
- 🟢 **Dynamic tray icon** — a progress ring around the icon depletes and changes color as the timer runs (pulses when about to expire)
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
| **Suspend if idle** check + minutes | When enabled, releases keep-awake after the given idle minutes (1–240) and resumes on activity |
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
| **Idle suspend ▶** | Submenu to set the idle auto-suspend threshold (Off / 10 / 20 / 30 / 60 min / Custom) |
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

The tray icon is **rendered dynamically** around the mood glyph to reflect the current state:

- **Timer mode** — a **progress ring** depletes clockwise as time runs out, shifting **green → amber → red**, and pulses red in the final minute.
- **Always-on mode** — a full, steady **blue ring** (active, no countdown).
- **Idle-suspended** — the glyph is **dimmed** with a small **moon badge** (keep-awake released, the PC may sleep).

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

### Idle auto-suspend

When the optional **Suspend if idle** threshold is set, Insonnia polls the system idle time (Windows `GetLastInputInfo`) once per second while active:

- After the configured minutes with **no keyboard or mouse activity**, it **releases** the keep-awake lock — the PC can then sleep according to your normal Windows power settings. The session stays *armed* (the tray icon goes back to its calm state and a "Suspended" balloon appears).
- On the **first input** after that, the lock is **re-acquired** automatically and a "Resumed" balloon appears.

The duration countdown keeps running on wall-clock time while suspended, so a finite timer can still expire and stop the session even if you never come back.

---

## 💾 Persistent Settings

The following preferences are automatically saved and restored on next launch:

| Setting | Description |
|---|---|
| **Selected duration** | The last duration option chosen (e.g. "1 hour") |
| **Custom minutes** | The last custom duration value entered |
| **Idle suspend** | Whether idle auto-suspend is enabled, and its threshold in minutes |

Settings are stored in `%APPDATA%\Insonnia\` via the standard .NET `ApplicationSettingsBase` mechanism.

---

## 📁 Project Structure

```
src/
├── MainForm.cs            # Main form logic
├── MainForm.Designer.cs   # UI layout
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
