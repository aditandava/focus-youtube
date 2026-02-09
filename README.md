# Focus Mode Application

A strict, distraction-free YouTube viewer designed for deep work. Locks your system, kills distractions, and forces you to focus on a single video or playlist until the timer expires.

## Features

- **Hard Lock**: Kills `explorer.exe` (Taskbar, Desktop) and `taskmgr.exe` to prevent exiting.
- **Strict Controls**: ONLY `L` (speed up), `K` (speed down), `A` (volume down), `S` (volume up), and `R` (reset speed) keys work. All other keys (Space, Esc, Arrows) are disabled.
- **Timer Modes**: Select from presets (30m, 1h, etc.), custom duration, or "Full Video" mode.
- **Process Blocking**: Continuously blocks Task Manager to prevent forced termination.
- **Clean UI**: Minimalist Material Design interface.

---

## Quick Start

### Step 1: Download All Required Files

You need **4 files** in the same folder:

| File | Description | Download Link |
|------|-------------|---------------|
| `FocusMode.exe` | The main application | **Included in this repo** |
| `mpv.exe` | Media player (required) | [SourceForge MPV Builds](https://sourceforge.net/projects/mpv-player-windows/files/) |
| `yt-dlp.exe` | YouTube downloader (required) | [yt-dlp Releases](https://github.com/yt-dlp/yt-dlp/releases/latest) |
| `d3dcompiler_43.dll` | Graphics library (often bundled with MPV) | Included with MPV download |

### Step 2: Setup Folder Structure

Create a folder (e.g., `C:\FocusMode\`) and place files like this:

```
C:\FocusMode\
├── FocusMode.exe       ← Main application
├── mpv.exe             ← Download from SourceForge
├── yt-dlp.exe          ← Download from GitHub
├── d3dcompiler_43.dll  ← Comes with MPV (copy if needed)
└── config\             ← Configuration folder
    ├── mpv.conf        ← MPV settings (included in repo)
    └── input.conf      ← Key bindings (included in repo)
```

### Step 3: Run

1. Right-click `FocusMode.exe` → **Run as Administrator** (recommended)
2. Paste a YouTube URL
3. Select duration
4. Click **START FOCUS MODE**

---

## How to Exit

**The ONLY ways to exit:**
1. Wait for the video/timer to end
2. Reboot your PC

This is intentional - it forces you to stay focused!

---

## Keyboard Controls (During Video)

| Key | Action |
|-----|--------|
| `L` | Speed up (+0.1x) |
| `K` | Speed down (-0.1x) |
| `S` | Volume up (+5%) |
| `A` | Volume down (-5%) |
| `R` | Reset speed to 1.0x |

**All other keys are disabled** (Space, Escape, arrows, etc.)

---

## Configuration Files

The `config/` folder contains MPV settings:

- **`mpv.conf`**: Fullscreen, always-on-top, disable UI elements
- **`input.conf`**: Strict key bindings (only L, K, A, S, R work)

You can copy these to `%APPDATA%\mpv\` if you want them to apply globally.

---

## Building from Source

Requires .NET Framework (comes with Windows):

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:FocusMode.exe /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll FocusMode.cs
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| MPV doesn't open | Make sure `mpv.exe` and `yt-dlp.exe` are in the same folder as `FocusMode.exe` |
| Video won't play | Update `yt-dlp.exe` to the latest version |
| Keys not working | Copy `config/` folder to `%APPDATA%\mpv\` |
| Black screen | Install `d3dcompiler_43.dll` (copy from MPV download) |

---

## License

MIT License. See [LICENSE](LICENSE) file.
