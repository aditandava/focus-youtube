# 🎯 Focus Mode - Ultimate Study Lock

**Focus Mode** is a strict, distraction-free YouTube player designed for serious studying. It locks your system environment and restricts all controls to mouse-only, ensuring you focus ONLY on the content.

<p align="center">
  <img src="https://raw.githubusercontent.com/aditandava/focus-youtube/main/focus_icon.ico" alt="Focus Mode Icon" width="128">
</p>

## ✨ Features

### 🔒 System Lock
- **Taskbar Hidden** — Explorer killed during session
- **Task Manager Blocked** — Continuously killed in background
- **Alt+Tab Blocked** — Low-level keyboard hook prevents window switching
- **Screenshots Blocked** — PrintScreen key intercepted
- **Win Key Blocked** — Start menu disabled during focus
- **Multi-Monitor Blackout** — Secondary screens covered with black overlay

### 🖱️ Mouse-Only Controls (Keyboard 100% Disabled)
| Mouse Action | Effect |
| :--- | :--- |
| 🔄 **Scroll UP** | Speed +0.1x |
| 🔄 **Scroll DOWN** | Speed -0.1x |
| 🖱️ **Right-Click** | Volume +5% |
| 🖱️ **Left-Click** | Volume -5% |
| 🖱️ **Middle-Click** | Reset Speed to 1.0x |

### 📺 Playback
- **Resolution Control** — 360p to 4K
- **Fullscreen** — True fullscreen, no window bars
- **Cursor Auto-Hide** — Disappears after 500ms
- **Playlist Support** — Checkbox to play full YouTube playlists
- **Subtitle Toggle** — Enable auto-detected English subtitles
- **Smart Buffering** — 150MB cache for smooth streaming
- **IPv4 Enforcement** — Prevents YouTube throttling

### 🍅 Pomodoro Mode
- 25 min focus → 5 min break cycles
- Auto-pauses video during breaks
- Shows motivational quotes between cycles
- Sound alert at each phase transition

### 📊 Session Tracking
- **Session Log** — Every session saved to `focus_log.txt` (date, duration, URL)
- **Focus Streak** — Tracks consecutive study days (shown on main screen)
- **Session Stats** — Summary popup after each session with duration & streak
- **Notes** — Optional notes prompt after session (saved to `focus_notes.txt`)
- **Last URL Memory** — Auto-fills your last used URL on next launch

### 💬 Motivational Quotes
Random study quotes shown on loading screens for inspiration.

### 🔔 Session End Sound
System notification sounds play 3x when timer expires or Pomodoro phase ends.

## 🛠️ Installation & Requirements

Place these **4 files** in the same folder as `FocusMode.exe`:

1. **`mpv.exe`** — [Download](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
2. **`ffmpeg.exe`** — [Download](https://github.com/yt-dlp/FFmpeg-Builds/releases) *(Required for 1080p+)*
3. **`yt-dlp.exe`** — [Download](https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe)
4. **`focus_icon.ico`** — Included in repo

### 📂 Directory Structure
```
C:\mpv\
├── FocusMode.exe          # Main Application
├── mpv.exe                # Player Core
├── ffmpeg.exe             # Processing Core
├── yt-dlp.exe             # Stream Extractor
├── focus_icon.ico         # Icon
├── focus_log.txt          # (Auto) Session history
├── focus_streak.txt       # (Auto) Streak data
├── focus_notes.txt        # (Auto) Session notes
├── last_url.txt           # (Auto) Last used URL
└── portable_config\       # (Auto) MPV config
    ├── input.conf
    └── mpv.conf
```

## 🚀 How to Build

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /win32icon:"focus_icon.ico" /out:"FocusMode.exe" "FocusMode.cs"
```

## ⚠️ Disclaimer
This tool blocks Alt+Tab, Task Manager, Win key, and screenshots during sessions. Only video completion, timer expiry, or system restart will restore normal operation. Use responsibly.

---
*Built for the ultimate focused mind.* 🧠
