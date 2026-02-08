# Focus Mode Application

A strict, distraction-free YouTube viewer designed for deep work. Locks your system, kills distractions, and forces you to focus on a single video or playlist until the timer expires.

## Features

- **Hard Lock**: Kills `explorer.exe` (Taskbar, Desktop) and `taskmgr.exe` to prevent exiting.
- **Strict Controls**: ONLY `L` (speed up), `K` (speed down), `A` (volume down), `S` (volume up), and `R` (reset speed) keys work. All other keys (Space, Esc, Arrows) are disabled.
- **Timer Modes**: Select from presets (30m, 1h, etc.), custom duration, or "Full Video" mode.
- **Process Blocking**: Continuously blocks Task Manager to prevent forced termination.
- **Clean UI**: Minimalist Material Design interface.

## Installation & Setup

To run Focus Mode, you need the application executable and its dependencies (MPV media player and yt-dlp) in the same folder.

### 1. Download Focus Mode
Download the latest `FocusMode.exe` from the [Releases](../../releases) page (or compile it yourself).

### 2. Install Dependencies (CRITICAL)
Focus Mode requires **MPV** (to play videos) and **yt-dlp** (to stream from YouTube).

#### Step A: Get MPV
1.  Go to [MPV.io Installation](https://mpv.io/installation/) or directly to [SourceForge Builds](https://sourceforge.net/projects/mpv-player-windows/files/).
2.  Download the latest **64-bit bootstrapper** or 7zip archive (e.g., `mpv-x86_64-...`).
3.  Extract the contents. You specifically need:
    - `mpv.exe`
    - `d3dcompiler_43.dll` (if included/required by your version)
    - `ffmpeg.exe` (optional but recommended)

#### Step B: Get yt-dlp
1.  Go to the [yt-dlp GitHub Releases](https://github.com/yt-dlp/yt-dlp/releases).
2.  Download `yt-dlp.exe`.

### 3. Organize Files
Create a folder (e.g., `FocusMode`) and place ALL files inside it. Your folder should look like this:

```text
FocusMode/
├── FocusMode.exe       <-- The application
├── mpv.exe            <-- Media player
├── yt-dlp.exe         <-- YouTube downloader
├── d3dcompiler_43.dll <-- Graphics helper (if needed)
└── config/            <-- Config folder (created automatically or manually)
    ├── mpv.conf
    └── input.conf
```

**Note:** If you don't have the `config` folder, `FocusMode.exe` will use its internal strict defaults, but you can manually create it to customize behavior (though strict keys are enforced via command line).

## Usage

1.  Run `FocusMode.exe` as Administrator (recommended for system locking).
2.  Paste a YouTube link.
3.  Select a duration.
4.  Click **START FOCUS MODE**.

**To Exit Early:**
The only way to exit before the timer ends is to **Reboot your PC** or wait for the video to finish. This is by design.

## Building from Source

1.  **Dependencies**: You need the C# compiler (`csc.exe`) which typically comes with .NET Framework at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`.
2.  **Compile**:
    ```powershell
    csc /target:winexe /out:FocusMode.exe /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll FocusMode.cs
    ```

## License

MIT License. See [LICENSE](LICENSE) file.
