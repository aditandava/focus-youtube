# Focus Mode - Distraction-Free YouTube Player

**Focus Mode** is a strict, distraction-free media player designed for focused study sessions. By locking down the system environment and restricting player controls to mouse-only input, it eliminates external distractions and ensures concentration on the content.

<p align="center">
  <img src="https://raw.githubusercontent.com/aditandava/focus-youtube/main/focus_icon.ico" alt="Focus Mode Icon" width="128">
</p>

## Key Features

- **System Lockdown**: Minimizes distractions by hiding the Windows taskbar and blocking Task Manager access during active sessions.
- **Resolution Control**: Supports playback quality ranging from **360p** to **4K** (Default: 720p).
- **Immersive Playback**: Video plays in true fullscreen mode without window decorations.
- **Mouse-Only Interface**: Keyboard input is completely disabled to prevent accidental exits or distractions.
- **Auto-Hide Cursor**: Cursor disappears after 500ms of inactivity.
- **Robust Streaming**:
    - **Session-Unique Configurations**: Generates fresh settings for every session to ensure reliability.
    - **Smart Buffering**: Pre-buffers up to 150MB for smooth playback of high-resolution content.
    - **IPv4 Enforcement**: Mitigates common throttling and buffering issues associated with YouTube playback.
- **Study Timer**: Configurable session durations (e.g., 30m, 1h) or custom timer options.

## Controls

To ensure focus, keyboard input is explicitly ignored. All control is managed via the mouse.

| Action | Input |
| :--- | :--- |
| **Increase Speed** | Scroll Up (+0.1x) |
| **Decrease Speed** | Scroll Down (-0.1x) |
| **Increase Volume** | Right-Click (+5%) |
| **Decrease Volume** | Left-Click (-5%) |
| **Reset Speed** | Middle-Click (Reset to 1.0x) |

> **Note:** Common keyboard shortcuts (Space, Escape, Arrow Keys, etc.) are disabled during playback.

## Installation & Requirements

The application relies on `mpv` and `ffmpeg` for playback and processing. Ensure the following files are present in the application directory:

1.  **`FocusMode.exe`**: The main application executable.
2.  **`mpv.exe`**: [Download Build](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
3.  **`ffmpeg.exe`**: [Download Build](https://github.com/yt-dlp/FFmpeg-Builds/releases) (Required for 1080p+ streaming)
4.  **`yt-dlp.exe`**: [Download Release](https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe)
5.  **`focus_icon.ico`**: Application icon file.

### Directory Structure

```text
C:\mpv\
├── FocusMode.exe        # Application
├── mpv.exe              # Player Core
├── ffmpeg.exe           # Processing Core
├── yt-dlp.exe           # Extractor
├── focus_icon.ico       # Icon Resource
└── portable_config\     # Session Configuration (Auto-Generated)
    ├── input.conf
    └── mpv.conf
```

## Compilation

The project can be compiled using the standard .NET Framework compiler included with Windows. 

Run the following command in **PowerShell**:

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"FocusMode.exe" /win32icon:"focus_icon.ico" "FocusMode.cs"
```

## Configuration Logic

Focus Mode utilizes a rigorous configuration strategy to enforce study conditions:

1.  **Session Initialization**: Automatically generates a clean `portable_config` directory on launch.
2.  **Input Filtering**: Whitelists specific mouse inputs while blacklisting all keyboard events (A-Z, F-Keys, System Keys).
3.  **Interface Simplification**: Disables the On-Screen Controller (OSC) to prevent interactive distractions.
4.  **Environment Control**: Enforces fullscreen mode and manages cursor visibility.

## Disclaimer

**Use Responsibly**: This application includes a Task Manager blocker designed to prevent early termination of study sessions. In the event of an emergency or software freeze, a system restart may be required if the session timer has not elapsed.
