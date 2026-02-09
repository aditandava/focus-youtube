# 🎯 Focus Mode - Ultimate Study Lock

**Focus Mode** is a strict, distraction-free YouTube player designed for serious studying. It locks your system environment and restricts player controls to ensure you focus ONLY on the content.

![Focus Mode Icon](https://raw.githubusercontent.com/Wait-What/FocusMode/main/focus_icon.ico)

## ✨ Features

-   **🔒 Hard System Lock**: Minimizes distractions by hiding the database bar and blocking Task Manager (optional in code) during sessions.
-   **📺 Resolution Control**: Select your preferred quality from **360p** up to **4K** (Default: 720p).
-   **⚡ Strict Input Enforcement**: Only 5 specific keys work. Everything else (Space, Arrows, Enter, Mouse Wheel) is **BANNED** to prevent skipping or diversion.
-   **🛡️ Robust Playback**:
    -   **Session-Unique Configs**: Generates fresh settings for every video to prevent glitches.
    -   **Smart Buffering**: 150MB cache pre-buffer for smooth 4K/1080p streaming.
    -   **IPv4 Enforcement**: Prevents common YouTube throttling/buffering issues.
-   **⏱️ Study Timer**: Preset durations (30m, 1h, etc.) or Custom Timer.

## 🎹 Strict Key Bindings

Only these keys are allowed. All others are ignored.

| Key | Action |
| :--- | :--- |
| **L** | Speed +0.1x |
| **K** | Speed -0.1x |
| **R** | Reset Speed to 1.0x |
| **S** | Volume +5% |
| **A** | Volume -5% |

## 🛠️ Installation & Requirements

The application requires the following **4 files** in the same folder as `FocusMode.exe`:

1.  **`mpv.exe`** (Media Player)
2.  **`ffmpeg.exe`** (CRITICAL: Required for 1080p/4K streaming)
3.  **`yt-dlp.exe`** (Downloader)
4.  **`focus_icon.ico`** (App Icon)

### 📂 Directory Structure
```
FocusMode/
├── FocusMode.exe       # Main Application
├── mpv.exe            # Player Core
├── ffmpeg.exe         # Processing Core (MUST BE PRESENT)
├── yt-dlp.exe         # Stream Extractor
├── focus_icon.ico     # Icon
└── config\            # (Auto-Generated per session)
```

## 🚀 How to Build

You can compile this project using the built-in C# compiler on any Windows machine (no Visual Studio required).

**Run this command in PowerShell:**
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"FocusMode.exe" /reference:System.dll,System.Windows.Forms.dll,System.Drawing.dll "FocusMode.cs" /win32icon:"focus_icon.ico"
```

## 📝 Configuration Logic

Focus Mode uses a **Hybrid Input Strategy**:
1.  **On Launch**: It generates a unique `input_{GUID}.conf` file.
2.  **Explicit Whitelist**: It allows only specific keys (L, K, A, S, R).
3.  **Explicit Grant**: It explicitly ignores common keys (Space, Enter, Esc, Arrows) to prevent default MPV behavior overrides.
4.  **Auto-Cleanup**: The config file is deleted immediately after the session ends.

## ⚠️ Disclaimer
This tool includes a `Task Manager Blocker` to prevent force-quitting during a focus session. Use responsibly. To exit in an emergency, you may need to restart your computer if the timer hasn't finished.

---
*Built for the ultimate focused mind.*
