# ☕ Insonnia

> Keep your PC awake — simply, silently, from the system tray.

**Insonnia** is a lightweight Windows tray application that prevents your PC from going to sleep or turning off the display. It uses the native Windows `SetThreadExecutionState` API — no drivers, no background services, no admin rights required.

---

## ✨ Features

- 🖥️ **Prevents sleep and display shutoff** via Windows API
- ⏱️ **Configurable duration** — always-on or with an auto-stop timer
- 💤 **Idle auto-suspend** *(optional)* — if you walk away, releases the lock after N idle minutes so the PC can sleep, then resumes automatically on the next keyboard/mouse activity
- 🔕 **Minimal notifications** — a single balloon when the timer expires; everything else stays silent (status lives in the window and tray tooltip)
- 📊 **Live progress bar** with color feedback (green → orange → red)
- ☕ **Mode-aware tray icon** — a coffee cup with a small overlay that shows the current mode at a glance: a clock for a running timer, a "zzz" when idle-suspended, no overlay when always-on, grey when stopped
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
| Status label | Shows elapsed time (∞ mode) or remaining time (timer mode) |
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

## 🔕 Notifications

Insonnia stays quiet on purpose. Starting, stopping, suspending and resuming are all silent — the current state is always visible in the main window and the tray tooltip. The **only** balloon shown is when a finite timer expires:

> **"Time's up — good night!"** → the session auto-stops.

In the final minute the window status and progress bar turn crimson, but no popup interrupts you.

---

## ☕ Tray Icon

The tray icon is a coffee cup drawn as a vector (crisp at 16 px). A small overlay communicates the current mode:

| State | Appearance |
|---|---|
| **Always-on** (running, no timer) | Warm amber cup with steam — **no overlay** |
| **Timer** (running, finite duration) | Amber cup with a small **clock** badge (bottom-right) |
| **Idle-suspended** | Muted cup with a **"zzz"** overlay (top-right) — keep-awake released, the PC may sleep |
| **Stopped** | Grey cup — **no overlay** |

The same coffee-cup artwork is used for the window title-bar icon and the in-app header, for a consistent look. Icons are precomputed once (no per-second redraw).

---

## 🌍 Localization

The UI language is automatically selected based on the Windows display language:

- 🇬🇧 **English** — default
- 🇮🇹 **Italian** — when the Windows language is `it-*`

All other languages fall back to English.

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

- After the configured minutes with **no keyboard or mouse activity**, it **releases** the keep-awake lock — the PC can then sleep according to your normal Windows power settings. The session stays *armed* (the tray icon switches to the "zzz" overlay; no popup is shown).
- On the **first input** after that, the lock is **re-acquired** automatically and the tray icon returns to its active state.

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
├── TrayIconRenderer.cs    # Vector coffee-cup tray icons + mode overlays
├── Win32Interop.cs        # SetThreadExecutionState + idle-time P/Invoke
├── Extensions.cs          # Bitmap → Icon helper
├── Program.cs             # Entry point + single-instance Mutex
└── Properties/
    ├── Strings.resx        # Italian strings (default)
    ├── StringsEn.resx      # English strings
    ├── StringsHelper.cs    # Localization wrapper (class L)
    └── Settings.settings   # User preferences (duration, custom minutes, idle suspend)
```

---

## 📄 License

MIT — see [LICENSE](LICENSE) for details.
