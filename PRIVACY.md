# Privacy Policy - Focus Mode Application

**Last Updated:** February 9, 2026

## Overview

Focus Mode is a **local, offline desktop application** designed to help students stay focused on YouTube lectures without distractions. This privacy policy explains how the application handles (or doesn't handle) your data.

---

## Data Collection

**We collect ZERO data.** Focus Mode:

- ✅ Runs entirely on your local computer
- ✅ Does NOT connect to any remote servers
- ✅ Does NOT track your activity
- ✅ Does NOT collect analytics
- ✅ Does NOT store browsing history
- ✅ Does NOT upload any information
- ✅ Does NOT use cookies or tracking technologies

---

## What The App Does

Focus Mode is a system-level lock application that:

1. Accepts a YouTube URL you manually paste
2. Launches MPV media player locally to play the video
3. Temporarily locks system processes (explorer.exe, taskmgr.exe) to prevent distractions
4. Restores your system when the video/timer ends

**All operations happen locally on your PC.**

---

## Third-Party Services

The app uses these tools **directly on your computer**:

### MPV Media Player
- **Purpose:** Plays YouTube videos locally
- **Privacy:** MPV is open-source and runs offline. It does not collect data.
- **Website:** [mpv.io](https://mpv.io)

### yt-dlp
- **Purpose:** Downloads video streams from YouTube
- **Privacy:** Open-source tool that connects to YouTube only to fetch the video stream
- **Data Sent:** Only the YouTube URL you provide
- **Website:** [github.com/yt-dlp/yt-dlp](https://github.com/yt-dlp/yt-dlp)

---

## Data Storage

Focus Mode does NOT store:
- ❌ YouTube URLs
- ❌ Watch history
- ❌ Usage statistics
- ❌ User preferences (beyond hardcoded timer presets)
- ❌ Personal information

**Exception:** The `config/` folder contains MPV settings (video quality, key bindings) which you can manually edit.

---

## YouTube Privacy

When you use Focus Mode with YouTube:
- **What YouTube Sees:** Your IP address and the video request (standard for any YouTube playback)
- **What We See:** Nothing. We don't intercept or log this data.
- **YouTube's Policy:** Governed by [YouTube's Privacy Policy](https://policies.google.com/privacy)

---

## System Permissions

Focus Mode requires:
- **Process Termination:** To kill `explorer.exe` and `taskmgr.exe` for focus lock
- **Process Launch:** To start MPV player

These are **temporary** and **reversible** (system restores on exit or reboot).

---

## Open Source

Focus Mode is **fully open-source**:
- **Code:** [github.com/aditandava/focus-youtube](https://github.com/aditandava/focus-youtube)
- **Audit:** Anyone can inspect the source code to verify privacy claims

---

## Changes to This Policy

Any updates to this privacy policy will be reflected in this document with a new "Last Updated" date.

---

## Contact

For questions or concerns about privacy:
- Open an issue on [GitHub](https://github.com/aditandava/focus-youtube/issues)

---

**Summary:** Focus Mode is a privacy-respecting, local-only tool. We don't collect, store, or transmit your data. Period.
