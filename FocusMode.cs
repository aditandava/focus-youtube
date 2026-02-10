using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;

namespace FocusMode
{
    // =========================================================================
    // AUDIO ISOLATION HELPERS (COM Interfaces for WASAPI)
    // =========================================================================
    public static class AudioIsolation
    {
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int NotImpl1();
            [PreserveSig]
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
            // Other methods omitted
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            // Other methods omitted
        }

        [ComImport]
        [Guid("77AA99A0-1BD6-484F-8BC2-33C936E09E95")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            int NotImpl1();
            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
            // Other methods omitted
        }

        [ComImport]
        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);
            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [ComImport]
        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl
        {
            int NotImpl1();
            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
            int NotImpl2();
            int NotImpl3();
            int NotImpl4();
            int NotImpl5();
            int NotImpl6();
            int NotImpl7();
            [PreserveSig]
            int GetProcessId(out uint pRetVal);
            // Other methods omitted
        }

        [ComImport]
        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);
            [PreserveSig]
            int GetMasterVolume(out float pfLevel);
            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);
            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        public static void MuteAllExcept(int allowedPid)
        {
            try
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                IMMDeviceEnumerator devEnum = (IMMDeviceEnumerator)enumerator;
                IMMDevice defaultDevice;
                
                // eRender = 0, eMultimedia = 1
                devEnum.GetDefaultAudioEndpoint(0, 1, out defaultDevice);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object obj;
                defaultDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out obj);
                IAudioSessionManager2 sessionManager = (IAudioSessionManager2)obj;

                IAudioSessionEnumerator sessionEnum;
                sessionManager.GetSessionEnumerator(out sessionEnum);

                int count;
                sessionEnum.GetCount(out count);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl session;
                    sessionEnum.GetSession(i, out session);
                    
                    uint pid;
                    session.GetProcessId(out pid);

                    if (pid != allowedPid && pid != 0) // pid 0 is system sounds usually
                    {
                         ISimpleAudioVolume volume = session as ISimpleAudioVolume;
                         if (volume != null)
                         {
                             Guid guid = Guid.Empty;
                             volume.SetMute(true, ref guid);
                         }
                    }

                    Marshal.ReleaseComObject(session);
                }

                Marshal.ReleaseComObject(sessionEnum);
                Marshal.ReleaseComObject(sessionManager);
                Marshal.ReleaseComObject(defaultDevice);
                Marshal.ReleaseComObject(enumerator);
            }
            catch { }
        }

        public static void UnmuteAll()
        {
             try
            {
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                IMMDeviceEnumerator devEnum = (IMMDeviceEnumerator)enumerator;
                IMMDevice defaultDevice;
                
                devEnum.GetDefaultAudioEndpoint(0, 1, out defaultDevice);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object obj;
                defaultDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out obj);
                IAudioSessionManager2 sessionManager = (IAudioSessionManager2)obj;

                IAudioSessionEnumerator sessionEnum;
                sessionManager.GetSessionEnumerator(out sessionEnum);

                int count;
                sessionEnum.GetCount(out count);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl session;
                    sessionEnum.GetSession(i, out session);
                    
                    ISimpleAudioVolume volume = session as ISimpleAudioVolume;
                    if (volume != null)
                    {
                        Guid guid = Guid.Empty;
                        volume.SetMute(false, ref guid);
                    }

                    Marshal.ReleaseComObject(session);
                }

                Marshal.ReleaseComObject(sessionEnum);
                Marshal.ReleaseComObject(sessionManager);
                Marshal.ReleaseComObject(defaultDevice);
                Marshal.ReleaseComObject(enumerator);
            }
            catch { }
        }
    }

    // =========================================================================
    // BLACKOUT FORM - Covers secondary monitors with a black screen
    // =========================================================================
    public class BlackoutForm : Form
    {
        public BlackoutForm(Screen screen)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = screen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }
    }

    // =========================================================================
    // SESSION STATS FORM - Shows study statistics after session ends
    // =========================================================================
    public class StatsForm : Form
    {
        private TextBox notesBox;
        private string notesFilePath;
        private string sessionDate;

        public StatsForm(DateTime startTime, DateTime endTime, string url, int streak, string appDir)
        {
            sessionDate = startTime.ToString("yyyy-MM-dd HH:mm");
            notesFilePath = Path.Combine(appDir, "focus_notes.txt");

            this.Text = "Focus Mode - Session Complete";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 15, 25);
            this.ForeColor = Color.White;

            string iconPath = Path.Combine(appDir, "focus_icon.ico");
            if (File.Exists(iconPath))
                this.Icon = new Icon(iconPath);

            TimeSpan duration = endTime - startTime;
            int y = 20;

            Label titleLbl = new Label();
            titleLbl.Text = "🎉 Session Complete!";
            titleLbl.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(16, 185, 129);
            titleLbl.AutoSize = false;
            titleLbl.Size = new Size(500, 45);
            titleLbl.TextAlign = ContentAlignment.MiddleCenter;
            titleLbl.Location = new Point(0, y);
            Controls.Add(titleLbl);
            y += 55;

            string stats = 
                "📅  Date:  " + startTime.ToString("MMM dd, yyyy") + "\n\n" +
                "🕐  Start:  " + startTime.ToString("hh:mm tt") + "\n\n" +
                "🕐  End:  " + endTime.ToString("hh:mm tt") + "\n\n" +
                "⏱  Duration:  " + (int)duration.TotalHours + "h " + duration.Minutes + "m " + duration.Seconds + "s\n\n" +
                "🔥  Study Streak:  " + streak + " day(s)\n\n" +
                "🔗  URL:  " + (url.Length > 50 ? url.Substring(0, 50) + "..." : url);

            Label statsLbl = new Label();
            statsLbl.Text = stats;
            statsLbl.Font = new Font("Segoe UI", 11);
            statsLbl.ForeColor = Color.FromArgb(229, 231, 235);
            statsLbl.AutoSize = false;
            statsLbl.Size = new Size(460, 190);
            statsLbl.Location = new Point(30, y);
            Controls.Add(statsLbl);
            y += 195;

            Label notesLbl = new Label();
            notesLbl.Text = "📝 Session Notes (optional):";
            notesLbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            notesLbl.ForeColor = Color.FromArgb(167, 139, 250);
            notesLbl.Location = new Point(30, y);
            notesLbl.AutoSize = true;
            Controls.Add(notesLbl);
            y += 28;

            notesBox = new TextBox();
            notesBox.Multiline = true;
            notesBox.Size = new Size(440, 80);
            notesBox.Location = new Point(30, y);
            notesBox.Font = new Font("Segoe UI", 10);
            notesBox.BackColor = Color.FromArgb(31, 31, 46);
            notesBox.ForeColor = Color.White;
            notesBox.BorderStyle = BorderStyle.FixedSingle;
            notesBox.ScrollBars = ScrollBars.Vertical;
            Controls.Add(notesBox);
            y += 90;

            Button saveBtn = new Button();
            saveBtn.Text = "Save & Close";
            saveBtn.Size = new Size(180, 42);
            saveBtn.Location = new Point((520 - 180) / 2, y);
            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.BackColor = Color.FromArgb(124, 58, 237);
            saveBtn.ForeColor = Color.White;
            saveBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Click += delegate 
            {
                SaveNotes();
                this.Close();
            };
            Controls.Add(saveBtn);
        }

        private void SaveNotes()
        {
            try
            {
                if (!string.IsNullOrEmpty(notesBox.Text) && notesBox.Text.Trim().Length > 0)
                {
                    string entry = "\n--- " + sessionDate + " ---\n" + notesBox.Text.Trim() + "\n";
                    File.AppendAllText(notesFilePath, entry);
                }
            }
            catch { }
        }
    }

    // =========================================================================
    // MAIN FORM
    // =========================================================================
    public class MainForm : Form
    {
        private TextBox urlTextBox;
        private ComboBox resolutionBox;
        private Label statusLabel;
        private Button[] timerButtons;
        private TextBox hoursBox, minsBox;
        private Label loadingLabel;
        private CheckBox pomodoroCheck;
        private CheckBox subtitleCheck;
        private CheckBox playlistCheck;
        private int selectedMinutes = 0;
        private bool focusActive = false;
        private Process mpvProcess;
        private CancellationTokenSource blockerCts;
        private string appDir;
        private Point dragOffset;
        private bool isDragging = false;
        private DateTime sessionStartTime;
        private string sessionUrl;
        private List<BlackoutForm> blackoutForms = new List<BlackoutForm>();
        private IntPtr keyboardHookId = IntPtr.Zero;
        private KeyboardHookDelegate keyboardHookProc;

        // ---- P/Invoke for rounded corners ----
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        // ---- P/Invoke for low-level keyboard hook (Anti-Alt+Tab & Screenshot Blocker) ----
        private delegate IntPtr KeyboardHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // ---- Motivational Quotes ----
        private static readonly string[] QUOTES = new string[] {
            "\"The secret of getting ahead is getting started.\" - Mark Twain",
            "\"It does not matter how slowly you go as long as you do not stop.\" - Confucius",
            "\"Success is the sum of small efforts repeated day in and day out.\" - Robert Collier",
            "\"The expert in anything was once a beginner.\" - Helen Hayes",
            "\"Don't watch the clock; do what it does. Keep going.\" - Sam Levenson",
            "\"Education is the most powerful weapon you can use to change the world.\" - Nelson Mandela",
            "\"The mind is not a vessel to be filled, but a fire to be kindled.\" - Plutarch",
            "\"Learning is not attained by chance, it must be sought for with ardor.\" - Abigail Adams",
            "\"Study hard what interests you the most in the most undisciplined way.\" - Richard Feynman",
            "\"Push yourself, because no one else is going to do it for you.\"",
            "\"There are no shortcuts to any place worth going.\" - Beverly Sills",
            "\"The only way to do great work is to love what you do.\" - Steve Jobs",
            "\"Your future is created by what you do today, not tomorrow.\" - Robert Kiyosaki",
            "\"Discipline is the bridge between goals and accomplishment.\" - Jim Rohn",
            "\"Focus on being productive instead of busy.\" - Tim Ferriss"
        };

        public MainForm()
        {
            appDir = @"C:\mpv";
            if (!Directory.Exists(appDir) || !File.Exists(Path.Combine(appDir, "mpv.exe")))
            {
                appDir = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(Path.Combine(appDir, "mpv.exe")))
                    appDir = Directory.GetParent(appDir) != null ? Directory.GetParent(appDir).FullName : appDir;
            }

            InitializeUI();
            CheckDependencies();
            CleanupOldConfigs();
            LoadLastUrl();
        }

        private void CleanupOldConfigs()
        {
            try
            {
                string oldConfigDir = Path.Combine(appDir, "config");
                if (Directory.Exists(oldConfigDir))
                {
                    try { Directory.Delete(oldConfigDir, true); } catch { }
                }
            }
            catch { }
        }

        private void CheckDependencies()
        {
            string ffPath = Path.Combine(appDir, "ffmpeg.exe");
            if (!File.Exists(ffPath))
            {
                MessageBox.Show("Warning: ffmpeg.exe is missing!\nYouTube playback may fail.", "Missing Dependency", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        // LAST URL MEMORY
        // =====================================================================
        private void LoadLastUrl()
        {
            try
            {
                string lastUrlFile = Path.Combine(appDir, "last_url.txt");
                if (File.Exists(lastUrlFile))
                {
                    string lastUrl = File.ReadAllText(lastUrlFile).Trim();
                    if (!string.IsNullOrEmpty(lastUrl))
                    {
                        urlTextBox.Text = lastUrl;
                        UpdateStatus("Last URL loaded", false);
                    }
                }
            }
            catch { }
        }

        private void SaveLastUrl(string url)
        {
            try
            {
                File.WriteAllText(Path.Combine(appDir, "last_url.txt"), url);
            }
            catch { }
        }

        // =====================================================================
        // SESSION LOG
        // =====================================================================
        private void LogSession(DateTime start, DateTime end, string url)
        {
            try
            {
                TimeSpan dur = end - start;
                string logPath = Path.Combine(appDir, "focus_log.txt");
                string entry = start.ToString("yyyy-MM-dd HH:mm") + " | " +
                               (int)dur.TotalHours + "h " + dur.Minutes + "m " + dur.Seconds + "s | " +
                               url + "\n";
                File.AppendAllText(logPath, entry);
            }
            catch { }
        }

        // =====================================================================
        // FOCUS STREAK TRACKER
        // =====================================================================
        private int UpdateStreak()
        {
            try
            {
                string streakFile = Path.Combine(appDir, "focus_streak.txt");
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

                int streak = 0;
                string lastDate = "";

                if (File.Exists(streakFile))
                {
                    string[] lines = File.ReadAllLines(streakFile);
                    if (lines.Length >= 2)
                    {
                        int.TryParse(lines[0], out streak);
                        lastDate = lines[1].Trim();
                    }
                }

                if (lastDate == today)
                {
                    // Already studied today
                    return streak;
                }
                else if (lastDate == yesterday)
                {
                    // Consecutive day!
                    streak++;
                }
                else
                {
                    // Streak broken, reset to 1
                    streak = 1;
                }

                File.WriteAllText(streakFile, streak + "\n" + today);
                return streak;
            }
            catch { return 1; }
        }

        // =====================================================================
        // MULTI-MONITOR BLACKOUT
        // =====================================================================
        private void BlackoutOtherScreens()
        {
            try
            {
                Screen primary = Screen.PrimaryScreen;
                foreach (Screen scr in Screen.AllScreens)
                {
                    if (!scr.Equals(primary))
                    {
                        BlackoutForm bf = new BlackoutForm(scr);
                        bf.Show();
                        blackoutForms.Add(bf);
                    }
                }
            }
            catch { }
        }

        private void RemoveBlackout()
        {
            try
            {
                foreach (BlackoutForm bf in blackoutForms)
                {
                    try { bf.Close(); bf.Dispose(); } catch { }
                }
                blackoutForms.Clear();
            }
            catch { }
        }

        // =====================================================================
        // ANTI ALT+TAB & SCREENSHOT BLOCKER (Low-Level Keyboard Hook)
        // =====================================================================
        private void InstallKeyboardHook()
        {
            keyboardHookProc = KeyboardHookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookProc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void RemoveKeyboardHook()
        {
            if (keyboardHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(keyboardHookId);
                keyboardHookId = IntPtr.Zero;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && focusActive)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                int msg = (int)wParam;

                // Block Alt+Tab (VK_TAB=9 with Alt modifier via WM_SYSKEYDOWN)
                if (msg == WM_SYSKEYDOWN && vkCode == 0x09) return (IntPtr)1; // Tab
                // Block Alt+F4
                if (msg == WM_SYSKEYDOWN && vkCode == 0x73) return (IntPtr)1; // F4
                // Block Alt+Esc
                if (msg == WM_SYSKEYDOWN && vkCode == 0x1B) return (IntPtr)1; // Esc
                // Block PrintScreen (VK_SNAPSHOT = 0x2C)
                if (vkCode == 0x2C) return (IntPtr)1;
                // Block Win key (VK_LWIN=0x5B, VK_RWIN=0x5C)
                if (vkCode == 0x5B || vkCode == 0x5C) return (IntPtr)1;
                // Block Ctrl+Esc (Start menu)
                if (vkCode == 0x1B && (msg == WM_KEYDOWN)) return (IntPtr)1;
            }
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        // =====================================================================
        // UI INITIALIZATION
        // =====================================================================
        private void InitializeUI()
        {
            this.Text = "Focus Mode";
            this.Size = new Size(700, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(15, 15, 25);
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));

            string iconPath = Path.Combine(appDir, "focus_icon.ico");
            if (File.Exists(iconPath))
                this.Icon = new Icon(iconPath);

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

            // Close Button
            Button closeBtn = new Button();
            closeBtn.Text = "X";
            closeBtn.Size = new Size(30, 30);
            closeBtn.Location = new Point(Width - 50, 15);
            closeBtn.FlatStyle = FlatStyle.Flat;
            closeBtn.BackColor = Color.Transparent;
            closeBtn.ForeColor = Color.FromArgb(107, 114, 128);
            closeBtn.Font = new Font("Segoe UI", 12);
            closeBtn.Cursor = Cursors.Hand;
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += delegate { if (!focusActive) Application.Exit(); };
            closeBtn.MouseEnter += delegate { closeBtn.BackColor = Color.FromArgb(239, 68, 68); };
            closeBtn.MouseLeave += delegate { closeBtn.BackColor = Color.Transparent; };
            Controls.Add(closeBtn);

            // Update Button
            Button updateBtn = new Button();
            updateBtn.Text = "↻";
            updateBtn.Size = new Size(30, 30);
            updateBtn.Location = new Point(Width - 90, 15);
            updateBtn.FlatStyle = FlatStyle.Flat;
            updateBtn.BackColor = Color.Transparent;
            updateBtn.ForeColor = Color.FromArgb(107, 114, 128);
            updateBtn.Font = new Font("Segoe UI", 12);
            updateBtn.Cursor = Cursors.Hand;
            updateBtn.FlatAppearance.BorderSize = 0;
            updateBtn.Click += delegate {
                ShowLoading("Updating yt-dlp...\nPlease wait...");
                ThreadPool.QueueUserWorkItem(delegate {
                    bool success = UpdateYtDlp();
                    HideLoading();
                    if (success) MessageBox.Show("Update Complete!", "Focus Mode");
                });
            };
            Controls.Add(updateBtn);

            // Streak badge
            int streak = 0;
            try
            {
                string streakFile = Path.Combine(appDir, "focus_streak.txt");
                if (File.Exists(streakFile))
                {
                    string[] sl = File.ReadAllLines(streakFile);
                    if (sl.Length >= 1) int.TryParse(sl[0], out streak);
                }
            }
            catch { }

            Label streakBadge = new Label();
            streakBadge.Text = "🔥 " + streak + " day streak";
            streakBadge.Font = new Font("Segoe UI", 9);
            streakBadge.ForeColor = streak > 0 ? Color.FromArgb(251, 191, 36) : Color.FromArgb(107, 114, 128);
            streakBadge.Location = new Point(15, 20);
            streakBadge.AutoSize = true;
            Controls.Add(streakBadge);

            int y = 25;

            Label iconLabel = new Label();
            iconLabel.Text = "TARGET";
            iconLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            iconLabel.ForeColor = Color.FromArgb(124, 58, 237);
            iconLabel.AutoSize = false;
            iconLabel.Size = new Size(Width, 42);
            iconLabel.TextAlign = ContentAlignment.MiddleCenter;
            iconLabel.Location = new Point(0, y);
            Controls.Add(iconLabel);
            y += 42;

            Label titleLabel = new Label();
            titleLabel.Text = "FOCUS MODE";
            titleLabel.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(167, 139, 250);
            titleLabel.AutoSize = false;
            titleLabel.Size = new Size(Width, 40);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Location = new Point(0, y);
            Controls.Add(titleLabel);
            y += 40;

            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Premium Study Lock - Zero Distractions";
            subtitleLabel.Font = new Font("Segoe UI", 10);
            subtitleLabel.ForeColor = Color.FromArgb(156, 163, 175);
            subtitleLabel.AutoSize = false;
            subtitleLabel.Size = new Size(Width, 22);
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            subtitleLabel.Location = new Point(0, y);
            Controls.Add(subtitleLabel);
            y += 35;

            // URL
            Label urlLabel = new Label();
            urlLabel.Text = "YouTube URL";
            urlLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            urlLabel.ForeColor = Color.FromArgb(229, 231, 235);
            urlLabel.Location = new Point(40, y);
            urlLabel.AutoSize = true;
            Controls.Add(urlLabel);
            y += 28;

            urlTextBox = new TextBox();
            urlTextBox.Size = new Size(Width - 80, 32);
            urlTextBox.Location = new Point(40, y);
            urlTextBox.Font = new Font("Segoe UI", 11);
            urlTextBox.BackColor = Color.FromArgb(31, 31, 46);
            urlTextBox.ForeColor = Color.FromArgb(243, 244, 246);
            urlTextBox.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(urlTextBox);
            y += 42;

            // Options row: Quality | Playlist | Subtitles | Pomodoro
            Label resLabel = new Label();
            resLabel.Text = "Quality";
            resLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            resLabel.ForeColor = Color.FromArgb(229, 231, 235);
            resLabel.Location = new Point(40, y + 4);
            resLabel.AutoSize = true;
            Controls.Add(resLabel);

            resolutionBox = new ComboBox();
            resolutionBox.Items.AddRange(new object[] { "360p", "480p", "720p", "1080p", "1440p", "4K" });
            resolutionBox.SelectedIndex = 2;
            resolutionBox.Location = new Point(105, y);
            resolutionBox.Size = new Size(80, 28);
            resolutionBox.Font = new Font("Segoe UI", 10);
            resolutionBox.BackColor = Color.FromArgb(31, 31, 46);
            resolutionBox.ForeColor = Color.White;
            resolutionBox.FlatStyle = FlatStyle.Flat;
            resolutionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(resolutionBox);

            playlistCheck = new CheckBox();
            playlistCheck.Text = "📋 Playlist";
            playlistCheck.Font = new Font("Segoe UI", 9);
            playlistCheck.ForeColor = Color.FromArgb(229, 231, 235);
            playlistCheck.Location = new Point(200, y + 2);
            playlistCheck.AutoSize = true;
            Controls.Add(playlistCheck);

            subtitleCheck = new CheckBox();
            subtitleCheck.Text = "🔤 Subtitles";
            subtitleCheck.Font = new Font("Segoe UI", 9);
            subtitleCheck.ForeColor = Color.FromArgb(229, 231, 235);
            subtitleCheck.Location = new Point(310, y + 2);
            subtitleCheck.AutoSize = true;
            Controls.Add(subtitleCheck);

            pomodoroCheck = new CheckBox();
            pomodoroCheck.Text = "🍅 Pomodoro";
            pomodoroCheck.Font = new Font("Segoe UI", 9);
            pomodoroCheck.ForeColor = Color.FromArgb(229, 231, 235);
            pomodoroCheck.Location = new Point(430, y + 2);
            pomodoroCheck.AutoSize = true;
            Controls.Add(pomodoroCheck);

            y += 40;

            // Duration Buttons
            Label timerLabel = new Label();
            timerLabel.Text = "Duration Mode";
            timerLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            timerLabel.ForeColor = Color.FromArgb(229, 231, 235);
            timerLabel.Location = new Point(40, y);
            timerLabel.AutoSize = true;
            Controls.Add(timerLabel);
            y += 30;

            string[] labels = { "Full Video", "30 min", "40 min", "1 hour", "1h 30m", "1h 40m", "2 hours" };
            int[] minutes = { 0, 30, 40, 60, 90, 100, 120 };
            timerButtons = new Button[labels.Length];

            int btnX = 40;
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                int mins = minutes[i];
                Button btn = CreateTimerButton(labels[i], btnX, y, i == 0);
                btn.Click += delegate { SelectTimer(idx, mins); };
                timerButtons[i] = btn;
                Controls.Add(btn);
                btnX += 92;
                if (btnX > Width - 120) { btnX = 40; y += 44; }
            }
            y += 52;

            // Custom time row
            Label customLabel = new Label();
            customLabel.Text = "Custom:";
            customLabel.Font = new Font("Segoe UI", 10);
            customLabel.ForeColor = Color.FromArgb(156, 163, 175);
            customLabel.Location = new Point(40, y + 4);
            customLabel.AutoSize = true;
            Controls.Add(customLabel);

            hoursBox = new TextBox();
            hoursBox.Text = "0";
            hoursBox.Size = new Size(45, 28);
            hoursBox.Location = new Point(115, y);
            hoursBox.BackColor = Color.FromArgb(31, 31, 46);
            hoursBox.ForeColor = Color.White;
            hoursBox.Font = new Font("Segoe UI", 10);
            hoursBox.TextAlign = HorizontalAlignment.Center;
            Controls.Add(hoursBox);

            Label hLabel = new Label();
            hLabel.Text = "h";
            hLabel.ForeColor = Color.FromArgb(156, 163, 175);
            hLabel.Location = new Point(163, y + 4);
            hLabel.AutoSize = true;
            Controls.Add(hLabel);

            minsBox = new TextBox();
            minsBox.Text = "0";
            minsBox.Size = new Size(45, 28);
            minsBox.Location = new Point(180, y);
            minsBox.BackColor = Color.FromArgb(31, 31, 46);
            minsBox.ForeColor = Color.White;
            minsBox.Font = new Font("Segoe UI", 10);
            minsBox.TextAlign = HorizontalAlignment.Center;
            Controls.Add(minsBox);

            Label mLabel = new Label();
            mLabel.Text = "m";
            mLabel.ForeColor = Color.FromArgb(156, 163, 175);
            mLabel.Location = new Point(228, y + 4);
            mLabel.AutoSize = true;
            Controls.Add(mLabel);

            Button setBtn = new Button();
            setBtn.Text = "Set";
            setBtn.Size = new Size(60, 28);
            setBtn.Location = new Point(255, y);
            setBtn.FlatStyle = FlatStyle.Flat;
            setBtn.BackColor = Color.FromArgb(59, 130, 246);
            setBtn.ForeColor = Color.White;
            setBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            setBtn.Cursor = Cursors.Hand;
            setBtn.FlatAppearance.BorderSize = 0;
            setBtn.Click += SetCustomTime;
            Controls.Add(setBtn);
            y += 45;

            // START button
            Button startBtn = new Button();
            startBtn.Text = "▶  START FOCUS MODE";
            startBtn.Size = new Size(300, 52);
            startBtn.Location = new Point((Width - 300) / 2, y);
            startBtn.FlatStyle = FlatStyle.Flat;
            startBtn.BackColor = Color.FromArgb(124, 58, 237);
            startBtn.ForeColor = Color.White;
            startBtn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            startBtn.Cursor = Cursors.Hand;
            startBtn.FlatAppearance.BorderColor = Color.FromArgb(167, 139, 250);
            startBtn.FlatAppearance.BorderSize = 2;
            startBtn.Click += StartFocus;
            Controls.Add(startBtn);
            y += 65;

            // Warning panel
            Panel warningPanel = new Panel();
            warningPanel.Size = new Size(Width - 80, 65);
            warningPanel.Location = new Point(40, y);
            warningPanel.BackColor = Color.FromArgb(31, 31, 46);

            Label warningLabel = new Label();
            warningLabel.Text = "⚠ Keyboard DISABLED | Alt+Tab BLOCKED | Other Audio MUTED\n🖱 Scroll = Speed | Right-Click = Vol+ | Left-Click = Vol- | Mid = Reset";
            warningLabel.Font = new Font("Segoe UI", 9);
            warningLabel.ForeColor = Color.FromArgb(251, 191, 36);
            warningLabel.AutoSize = false;
            warningLabel.Size = new Size(Width - 100, 55);
            warningLabel.Location = new Point(10, 5);
            warningPanel.Controls.Add(warningLabel);
            Controls.Add(warningPanel);
            y += 72;

            statusLabel = new Label();
            statusLabel.Text = "Ready - Enter YouTube URL and select duration";
            statusLabel.Font = new Font("Segoe UI", 9);
            statusLabel.ForeColor = Color.FromArgb(107, 114, 128);
            statusLabel.AutoSize = false;
            statusLabel.Size = new Size(Width, 22);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Location = new Point(0, y);
            Controls.Add(statusLabel);

            // Loading overlay
            loadingLabel = new Label();
            loadingLabel.Text = "";
            loadingLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            loadingLabel.ForeColor = Color.FromArgb(124, 58, 237);
            loadingLabel.BackColor = Color.FromArgb(15, 15, 25);
            loadingLabel.AutoSize = false;
            loadingLabel.Size = new Size(Width, Height);
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            loadingLabel.Location = new Point(0, 0);
            loadingLabel.Visible = false;
            Controls.Add(loadingLabel);
            loadingLabel.BringToFront();
        }

        private Button CreateTimerButton(string text, int x, int y, bool selected)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(88, 38);
            btn.Location = new Point(x, y);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = selected ? Color.FromArgb(124, 58, 237) : Color.FromArgb(31, 31, 46);
            btn.ForeColor = Color.FromArgb(229, 231, 235);
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Tag = selected;
            btn.FlatAppearance.BorderColor = selected ? Color.FromArgb(167, 139, 250) : Color.FromArgb(59, 59, 79);
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        private void SelectTimer(int index, int mins)
        {
            selectedMinutes = mins;
            for (int i = 0; i < timerButtons.Length; i++)
            {
                bool sel = (i == index);
                timerButtons[i].BackColor = sel ? Color.FromArgb(124, 58, 237) : Color.FromArgb(31, 31, 46);
                timerButtons[i].FlatAppearance.BorderColor = sel ? Color.FromArgb(167, 139, 250) : Color.FromArgb(59, 59, 79);
                timerButtons[i].Tag = sel;
            }
            UpdateStatus("Timer: " + timerButtons[index].Text, false);
        }

        private void SetCustomTime(object sender, EventArgs e)
        {
            int hours = 0, mins = 0;
            int.TryParse(hoursBox.Text, out hours);
            int.TryParse(minsBox.Text, out mins);
            int total = hours * 60 + mins;
            if (total <= 0) { UpdateStatus("Enter a valid time", true); return; }

            selectedMinutes = total;
            foreach (Button btn in timerButtons)
            {
                btn.BackColor = Color.FromArgb(31, 31, 46);
                btn.FlatAppearance.BorderColor = Color.FromArgb(59, 59, 79);
                btn.Tag = false;
            }
            UpdateStatus("Custom: " + hours + "h " + mins + "m", false);
        }

        private void UpdateStatus(string msg, bool error)
        {
            if (statusLabel.InvokeRequired)
                statusLabel.Invoke((MethodInvoker)delegate { statusLabel.Text = msg; statusLabel.ForeColor = error ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129); });
            else
            {
                statusLabel.Text = msg;
                statusLabel.ForeColor = error ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129);
            }
        }

        private bool ValidateUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return Regex.IsMatch(url, @"(youtube\.com|youtu\.be)", RegexOptions.IgnoreCase);
        }

        private string GetRandomQuote()
        {
            Random rng = new Random();
            return QUOTES[rng.Next(QUOTES.Length)];
        }

        // =====================================================================
        // START FOCUS
        // =====================================================================
        private void StartFocus(object sender, EventArgs e)
        {
            string url = urlTextBox.Text;
            if (url != null) url = url.Trim();
            if (string.IsNullOrEmpty(url)) { UpdateStatus("Enter a YouTube URL", true); return; }
            if (!ValidateUrl(url)) { UpdateStatus("Invalid YouTube URL", true); return; }

            sessionUrl = url;
            SaveLastUrl(url);

            ShowLoading("Starting Focus Mode...\n\n" + GetRandomQuote());
            focusActive = true;

            KillProcess("mpv");
            
            // Ensure audio is unmuted before starting (in case of crash previously)
            AudioIsolation.UnmuteAll();
            
            ThreadPool.QueueUserWorkItem(delegate { RunFocusMode(url, false); });
        }

        private void ShowLoading(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                loadingLabel.Text = message;
                loadingLabel.Visible = true;
            });
        }

        private void HideLoading()
        {
            this.Invoke((MethodInvoker)delegate { loadingLabel.Visible = false; });
        }

        // =====================================================================
        // CORE FOCUS MODE
        // =====================================================================
        private void RunFocusMode(string url, bool isRetry)
        {
            try
            {
                sessionStartTime = DateTime.Now;

                KillProcess("explorer");
                KillProcess("taskmgr");

                blockerCts = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem(delegate { BlockTaskMgrAndEnforceAudio(); });

                // Install keyboard hook on UI thread
                this.Invoke((MethodInvoker)delegate {
                    InstallKeyboardHook();
                    BlackoutOtherScreens();
                });

                string mpvPath = Path.Combine(appDir, "mpv.exe");

                // portable_config
                string portableConfigDir = Path.Combine(appDir, "portable_config");
                if (!Directory.Exists(portableConfigDir)) Directory.CreateDirectory(portableConfigDir);

                string inputConfPath = Path.Combine(portableConfigDir, "input.conf");

                // MOUSE-ONLY CONTROLS
                string inputContent =
                    "# Focus Mode - MOUSE ONLY CONTROLS\n" +
                    "WHEEL_UP add speed 0.1\n" +
                    "WHEEL_DOWN add speed -0.1\n" +
                    "MBTN_RIGHT add volume 5\n" +
                    "MBTN_LEFT add volume -5\n" +
                    "MBTN_MID set speed 1.0\n" +
                    "MBTN_LEFT_DBL ignore\n" +
                    "MBTN_RIGHT_DBL ignore\n" +
                    "\n" +
                    "# DISABLE ALL KEYBOARD KEYS\n" +
                    "a ignore\nb ignore\nc ignore\nd ignore\ne ignore\nf ignore\ng ignore\nh ignore\n" +
                    "i ignore\nj ignore\nk ignore\nl ignore\nm ignore\nn ignore\no ignore\np ignore\n" +
                    "q ignore\nr ignore\ns ignore\nt ignore\nu ignore\nv ignore\nw ignore\nx ignore\n" +
                    "y ignore\nz ignore\n" +
                    "A ignore\nB ignore\nC ignore\nD ignore\nE ignore\nF ignore\nG ignore\nH ignore\n" +
                    "I ignore\nJ ignore\nK ignore\nL ignore\nM ignore\nN ignore\nO ignore\nP ignore\n" +
                    "Q ignore\nR ignore\nS ignore\nT ignore\nU ignore\nV ignore\nW ignore\nX ignore\n" +
                    "Y ignore\nZ ignore\n" +
                    "0 ignore\n1 ignore\n2 ignore\n3 ignore\n4 ignore\n" +
                    "5 ignore\n6 ignore\n7 ignore\n8 ignore\n9 ignore\n" +
                    "SPACE ignore\nENTER ignore\nESC ignore\nTAB ignore\nBS ignore\nDEL ignore\n" +
                    "INS ignore\nHOME ignore\nEND ignore\nPGUP ignore\nPGDWN ignore\n" +
                    "LEFT ignore\nRIGHT ignore\nUP ignore\nDOWN ignore\n" +
                    "F1 ignore\nF2 ignore\nF3 ignore\nF4 ignore\nF5 ignore\nF6 ignore\n" +
                    "F7 ignore\nF8 ignore\nF9 ignore\nF10 ignore\nF11 ignore\nF12 ignore\n" +
                    "SHARP ignore\nPOWER ignore\nMENU ignore\nPLAY ignore\nPAUSE ignore\n" +
                    "PLAYPAUSE ignore\nSTOP ignore\nFORWARD ignore\nREWIND ignore\n" +
                    "NEXT ignore\nPREV ignore\nVOLUME_UP ignore\nVOLUME_DOWN ignore\n" +
                    "MUTE ignore\nCLOSE_WIN ignore\n" +
                    "CTRL+a ignore\nCTRL+b ignore\nCTRL+c ignore\nCTRL+d ignore\nCTRL+e ignore\n" +
                    "CTRL+f ignore\nCTRL+g ignore\nCTRL+h ignore\nCTRL+i ignore\nCTRL+j ignore\n" +
                    "CTRL+k ignore\nCTRL+l ignore\nCTRL+m ignore\nCTRL+n ignore\nCTRL+o ignore\n" +
                    "CTRL+p ignore\nCTRL+q ignore\nCTRL+r ignore\nCTRL+s ignore\nCTRL+t ignore\n" +
                    "CTRL+u ignore\nCTRL+v ignore\nCTRL+w ignore\nCTRL+x ignore\nCTRL+y ignore\n" +
                    "CTRL+z ignore\n" +
                    "ALT+ENTER ignore\nWHEEL_LEFT ignore\nWHEEL_RIGHT ignore\n";

                File.WriteAllText(inputConfPath, inputContent);

                // Get UI options
                string resText = "720";
                bool usePlaylist = false;
                bool useSubs = false;
                bool usePomodoro = false;
                this.Invoke((MethodInvoker)delegate {
                    resText = resolutionBox.Text;
                    usePlaylist = playlistCheck.Checked;
                    useSubs = subtitleCheck.Checked;
                    usePomodoro = pomodoroCheck.Checked;
                });

                // mpv.conf
                string mpvConfPath = Path.Combine(portableConfigDir, "mpv.conf");
                string mpvConfContent =
                    "osc=no\nosd-level=1\nosd-bar=yes\nosd-duration=2000\n" +
                    "cache=yes\ndemuxer-max-bytes=150MiB\n" +
                    "fullscreen=yes\ncursor-autohide=500\ncursor-autohide-fs-only=no\n";
                if (useSubs)
                    mpvConfContent += "sub-auto=fuzzy\nsub-visibility=yes\nslang=en,eng\n";
                else
                    mpvConfContent += "sub-visibility=no\n";
                File.WriteAllText(mpvConfPath, mpvConfContent);

                // Resolution
                string maxHeight = "720";
                if (resText.Contains("360")) maxHeight = "360";
                else if (resText.Contains("480")) maxHeight = "480";
                else if (resText.Contains("1080")) maxHeight = "1080";
                else if (resText.Contains("1440")) maxHeight = "1440";
                else if (resText.Contains("4K") || resText.Contains("2160")) maxHeight = "2160";

                string ytdlFormat = "bestvideo[height<=" + maxHeight + "]+bestaudio/best[height<=" + maxHeight + "]";

                // Build args
                string args = "--force-window=yes " +
                              "--no-input-default-bindings " +
                              "--ytdl-raw-options=force-ipv4= " +
                              "--ytdl-format=\"" + ytdlFormat + "\" ";

                if (usePlaylist)
                    args += "--ytdl-raw-options-append=yes-playlist= ";

                args += "\"" + url + "\"";

                // ---- POMODORO MODE ----
                if (usePomodoro)
                {
                    RunPomodoroMode(url, args, mpvPath);
                    return;
                }

                // ---- NORMAL MODE ----
                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = mpvPath;
                si.Arguments = args;
                si.UseShellExecute = true;
                si.WindowStyle = ProcessWindowStyle.Normal;

                DateTime startTime = DateTime.Now;
                mpvProcess = Process.Start(si);

                if (mpvProcess == null)
                    throw new Exception("MPV failed to start");

                if (selectedMinutes > 0)
                {
                    int waitMs = selectedMinutes * 60 * 1000;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(waitMs);
                        if (focusActive && mpvProcess != null && !mpvProcess.HasExited)
                        {
                            PlayEndSound();
                            try { mpvProcess.Kill(); } catch { }
                        }
                    });
                }

                if (mpvProcess != null)
                    mpvProcess.WaitForExit();

                TimeSpan duration = DateTime.Now - startTime;

                if (duration.TotalSeconds < 30 && !isRetry)
                {
                    ShowLoading("Video failed to load.\nUpdating yt-dlp...\n\n" + GetRandomQuote());
                    if (UpdateYtDlp())
                    {
                        ShowLoading("Retrying...\n\n" + GetRandomQuote());
                        Thread.Sleep(1000);
                        RunFocusMode(url, true);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally { EndFocus(); }
        }

        // =====================================================================
        // POMODORO MODE
        // =====================================================================
        private void RunPomodoroMode(string url, string baseArgs, string mpvPath)
        {
            try
            {
                int focusMinutes = 25;
                int breakMinutes = 5;
                int totalCycles = selectedMinutes > 0 ? (selectedMinutes / (focusMinutes + breakMinutes)) : 4;
                if (totalCycles < 1) totalCycles = 1;

                for (int cycle = 0; cycle < totalCycles && focusActive; cycle++)
                {
                    // FOCUS phase
                    ShowLoading("🍅 Pomodoro " + (cycle + 1) + "/" + totalCycles + "\nFOCUS - " + focusMinutes + " min\n\n" + GetRandomQuote());
                    Thread.Sleep(2000);

                    ProcessStartInfo si = new ProcessStartInfo();
                    si.FileName = mpvPath;
                    si.Arguments = baseArgs;
                    si.UseShellExecute = true;
                    si.WindowStyle = ProcessWindowStyle.Normal;

                    mpvProcess = Process.Start(si);
                    if (mpvProcess == null) break;

                    // Timer to kill after focus period
                    int focusMs = focusMinutes * 60 * 1000;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(focusMs);
                        if (mpvProcess != null && !mpvProcess.HasExited)
                        {
                            PlayEndSound();
                            try { mpvProcess.Kill(); } catch { }
                        }
                    });

                    mpvProcess.WaitForExit();

                    if (!focusActive) break;

                    // BREAK phase (not on last cycle)
                    if (cycle < totalCycles - 1)
                    {
                        AudioIsolation.UnmuteAll(); // Unmute during break
                        PlayEndSound();
                        ShowLoading("🍅 Pomodoro " + (cycle + 1) + "/" + totalCycles + " DONE!\n\n☕ BREAK - " + breakMinutes + " min\nRelax your eyes...\n\n" + GetRandomQuote());
                        Thread.Sleep(breakMinutes * 60 * 1000);
                    }
                }

                PlayEndSound();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pomodoro Error: " + ex.Message);
            }
            finally { EndFocus(); }
        }

        // =====================================================================
        // SESSION END SOUND
        // =====================================================================
        private void PlayEndSound()
        {
            try
            {
                // Play system notification sounds 3 times for attention
                for (int i = 0; i < 3; i++)
                {
                    SystemSounds.Exclamation.Play();
                    Thread.Sleep(600);
                }
            }
            catch { }
        }

        // =====================================================================
        // END FOCUS - Show Stats
        // =====================================================================
        private void EndFocus()
        {
            focusActive = false;
            if (blockerCts != null) blockerCts.Cancel();

            // Unmute EVERYTHING
            try { AudioIsolation.UnmuteAll(); } catch { }

            // Remove keyboard hook
            try { this.Invoke((MethodInvoker)delegate { RemoveKeyboardHook(); RemoveBlackout(); }); } catch { }

            KillProcess("mpv");
            try { Process.Start("explorer.exe"); } catch { }

            DateTime endTime = DateTime.Now;
            TimeSpan sessionDuration = endTime - sessionStartTime;

            // Only show stats if session lasted more than 30 seconds
            if (sessionDuration.TotalSeconds > 30)
            {
                LogSession(sessionStartTime, endTime, sessionUrl ?? "unknown");
                int streak = UpdateStreak();

                PlayEndSound();

                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        StatsForm statsForm = new StatsForm(sessionStartTime, endTime, sessionUrl ?? "unknown", streak, appDir);
                        statsForm.ShowDialog();
                    });
                }
                catch { }
            }

            Application.Exit();
        }

        // =====================================================================
        // UTILITIES
        // =====================================================================
        private bool UpdateYtDlp()
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                string ytdlpPath = Path.Combine(appDir, "yt-dlp.exe");
                string backupPath = Path.Combine(appDir, "yt-dlp.old");
                string dlUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

                if (File.Exists(ytdlpPath))
                {
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                    File.Move(ytdlpPath, backupPath);
                }
                using (WebClient client = new WebClient()) { client.DownloadFile(dlUrl, ytdlpPath); }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Failed: " + ex.Message);
                try
                {
                    string ytdlpPath = Path.Combine(appDir, "yt-dlp.exe");
                    string backupPath = Path.Combine(appDir, "yt-dlp.old");
                    if (!File.Exists(ytdlpPath) && File.Exists(backupPath))
                        File.Move(backupPath, ytdlpPath);
                }
                catch { }
                return false;
            }
        }

        private void KillProcess(string name)
        {
            try
            {
                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = "taskkill";
                si.Arguments = "/F /IM " + name + ".exe";
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                Process p = Process.Start(si);
                if (p != null) p.WaitForExit();
            }
            catch { }
        }

        private void BlockTaskMgrAndEnforceAudio()
        {
            while (focusActive)
            {
                KillProcess("taskmgr");
                
                // Audio Enforcer
                if (mpvProcess != null && !mpvProcess.HasExited)
                {
                    try { AudioIsolation.MuteAllExcept(mpvProcess.Id); } catch { }
                }
                
                Thread.Sleep(2000);
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragOffset = e.Location; }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging) this.Location = new Point(Cursor.Position.X - dragOffset.X, Cursor.Position.Y - dragOffset.Y);
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
