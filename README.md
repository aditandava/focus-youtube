# Focus Mode Application

A strict, distraction-free YouTube viewer designed for deep work. Locks your system, kills distractions, and forces you to focus on a single video or playlist until the timer expires.

## Features

- **Hard Lock**: Kills `explorer.exe` (Taskbar, Desktop) and `taskmgr.exe` to prevent exiting.
- **Strict Controls**: ONLY `L` (speed up), `K` (speed down), `A` (volume down), `S` (volume up), and `R` (reset speed) keys work. All other keys (Space, Esc, Arrows) are disabled.
- **Timer Modes**: Select from presets (30m, 1h, etc.), custom duration, or "Full Video" mode.
- **Process Blocking**: Continuously blocks Task Manager to prevent forced termination.
- **Clean UI**: Minimalist Material Design interface.

## Installation

1.  Download the latest release.
2.  Ensure you have `mpv.exe` in the same directory (or `bin` folder).
3.  Run `FocusMode.exe`.

## Requirements

- Windows 10/11
- `.NET Framework 4.7.2` or later
- `mpv` media player

## Building from Source

1.  **Dependencies**: You need the C# compiler (`csc.exe`) which typically comes with .NET Framework at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`.
2.  **Compile**:
    ```powershell
    csc /target:winexe /out:FocusMode.exe /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll FocusMode.cs
    ```
3.  **Run**: Place `FocusMode.exe` in the same folder as `mpv.exe` and `config/`.

## Configuration

The application uses strict MPV configurations located in `config/`:
- `mpv.conf`: Enforces fullscreen, always-on-top, and disables UI.
- `input.conf`: Restricted key bindings.

## License

MIT License. See [LICENSE](LICENSE) file.
