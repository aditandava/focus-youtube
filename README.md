# 🎯 Focus Mode - Ultimate Study Lock

**Focus Mode** is a strict, distraction-free YouTube player designed for serious studying. It locks your system environment and restricts player controls to ensure you focus ONLY on the content.

<p align="center">
  <img src="https://raw.githubusercontent.com/aditandava/focus-youtube/main/focus_icon.ico" alt="Focus Mode Icon" width="128">
</p>

## ✨ Features

-   **🔒 Hard System Lock**: Minimizes distractions by hiding the taskbar and blocking Task Manager during sessions.
-   **📺 Resolution Control**: Select your preferred quality from **360p** up to **4K** (Default: 720p).
-   **🖥️ True Fullscreen**: Video plays in complete fullscreen with no window decorations.
-   **🖱️ Mouse-Only Controls**: Keyboard is **completely disabled**. Only mouse controls work!
-   **👻 Auto-Hide Cursor**: Cursor disappears after 500ms of inactivity (mouse still works).
-   **🛡️ Robust Playback**:
    -   **Session-Unique Configs**: Generates fresh settings for every session.
    -   **Smart Buffering**: 150MB cache pre-buffer for smooth 4K/1080p streaming.
    -   **IPv4 Enforcement**: Prevents common YouTube throttling/buffering issues.
-   **⏱️ Study Timer**: Preset durations (30m, 1h, etc.) or Custom Timer.

## 🖱️ Mouse Controls

**Keyboard is COMPLETELY DISABLED.** Only these mouse actions work:

| Mouse Action | Effect |
| :--- | :--- |
| 🔄 **Scroll UP** | Speed +0.1x |
| 🔄 **Scroll DOWN** | Speed -0.1x |
| 🖱️ **Right-Click** | Volume +5% |
| 🖱️ **Left-Click** | Volume -5% |
| 🖱️ **Middle-Click** | Reset Speed to 1.0x |

> **Note:** All keyboard keys (Q, W, Space, Escape, Arrows, etc.) are ignored to prevent any distractions or accidental exits.

## 🛠️ Installation & Requirements

The application requires the following **4 files** in the same folder as `FocusMode.exe`:

1.  **`mpv.exe`** - [Download Latest (zip)](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
    -   *Extract `mpv.exe` from the zip.*
2.  **`ffmpeg.exe`** - [Download Latest (zip)](https://github.com/yt-dlp/FFmpeg-Builds/releases)
    -   *Extract `ffmpeg.exe` from the zip. (CRITICAL for 1080p+)*
3.  **`yt-dlp.exe`** - [Download Direct (.exe)](https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe)
4.  **`focus_icon.ico`** (Included in this repository)

### 📂 Directory Structure
```
C:\mpv\
├── FocusMode.exe        # Main Application
├── mpv.exe              # Player Core
├── ffmpeg.exe           # Processing Core (MUST BE PRESENT)
├── yt-dlp.exe           # Stream Extractor
├── focus_icon.ico       # Icon
└── portable_config\     # (Auto-Generated per session)
    ├── input.conf       # Mouse bindings
    └── mpv.conf         # Player settings
```

## 🚀 How to Build

You can compile this project using the built-in C# compiler on any Windows machine (no Visual Studio required).

**Run this command in PowerShell:**
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"FocusMode.exe" /win32icon:"focus_icon.ico" "FocusMode.cs"
```

## 📝 Configuration Logic

Focus Mode uses a **portable_config** approach:
1.  **On Launch**: Creates `portable_config` folder with `input.conf` and `mpv.conf`.
2.  **Mouse Whitelist**: Enables scroll wheel and mouse buttons for control.
3.  **Keyboard Blacklist**: Explicitly ignores ALL keyboard keys (a-z, 0-9, F1-F12, Space, Enter, Esc, Arrows, etc.).
4.  **OSC Disabled**: On-screen controller is disabled to prevent mouse click interception.
5.  **Fullscreen + Hidden Cursor**: True fullscreen with auto-hiding cursor.

## ⚠️ Disclaimer
This tool includes a `Task Manager Blocker` to prevent force-quitting during a focus session. Use responsibly. To exit in an emergency, you may need to restart your computer if the timer hasn't finished. The video will end naturally when complete or when the timer expires.

---
*Built for the ultimate focused mind.* 🧠
