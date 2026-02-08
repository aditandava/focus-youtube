using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace FocusMode
{
    public class MainForm : Form
    {
        private TextBox urlTextBox;
        private Label statusLabel;
        private Button[] timerButtons;
        private TextBox hoursBox, minsBox;
        private int selectedMinutes = 0;
        private bool focusActive = false;
        private Process mpvProcess;
        private CancellationTokenSource blockerCts;
        private string appDir;
        private Point dragOffset;
        private bool isDragging = false;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public MainForm()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!File.Exists(Path.Combine(appDir, "mpv.exe")))
                appDir = Directory.GetParent(appDir) != null ? Directory.GetParent(appDir).FullName : appDir;

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Focus Mode";
            this.Size = new Size(700, 680);
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

            int y = 30;

            Label iconLabel = new Label();
            iconLabel.Text = "TARGET";
            iconLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            iconLabel.ForeColor = Color.FromArgb(124, 58, 237);
            iconLabel.AutoSize = false;
            iconLabel.Size = new Size(Width, 50);
            iconLabel.TextAlign = ContentAlignment.MiddleCenter;
            iconLabel.Location = new Point(0, y);
            Controls.Add(iconLabel);
            y += 55;

            Label titleLabel = new Label();
            titleLabel.Text = "FOCUS MODE";
            titleLabel.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(167, 139, 250);
            titleLabel.AutoSize = false;
            titleLabel.Size = new Size(Width, 45);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Location = new Point(0, y);
            Controls.Add(titleLabel);
            y += 45;

            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Premium Study Lock - Zero Distractions";
            subtitleLabel.Font = new Font("Segoe UI", 11);
            subtitleLabel.ForeColor = Color.FromArgb(156, 163, 175);
            subtitleLabel.AutoSize = false;
            subtitleLabel.Size = new Size(Width, 25);
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            subtitleLabel.Location = new Point(0, y);
            Controls.Add(subtitleLabel);
            y += 45;

            Label urlLabel = new Label();
            urlLabel.Text = "YouTube URL";
            urlLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            urlLabel.ForeColor = Color.FromArgb(229, 231, 235);
            urlLabel.Location = new Point(40, y);
            urlLabel.AutoSize = true;
            Controls.Add(urlLabel);
            y += 30;

            urlTextBox = new TextBox();
            urlTextBox.Size = new Size(Width - 80, 40);
            urlTextBox.Location = new Point(40, y);
            urlTextBox.Font = new Font("Segoe UI", 12);
            urlTextBox.BackColor = Color.FromArgb(31, 31, 46);
            urlTextBox.ForeColor = Color.FromArgb(243, 244, 246);
            urlTextBox.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(urlTextBox);
            y += 55;

            Label timerLabel = new Label();
            timerLabel.Text = "Duration Mode";
            timerLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            timerLabel.ForeColor = Color.FromArgb(229, 231, 235);
            timerLabel.Location = new Point(40, y);
            timerLabel.AutoSize = true;
            Controls.Add(timerLabel);
            y += 35;

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
                btnX += 95;
                if (btnX > Width - 120) { btnX = 40; y += 50; }
            }
            y += 60;

            Label customLabel = new Label();
            customLabel.Text = "Custom:";
            customLabel.Font = new Font("Segoe UI", 11);
            customLabel.ForeColor = Color.FromArgb(156, 163, 175);
            customLabel.Location = new Point(40, y + 5);
            customLabel.AutoSize = true;
            Controls.Add(customLabel);

            hoursBox = new TextBox();
            hoursBox.Text = "0";
            hoursBox.Size = new Size(50, 30);
            hoursBox.Location = new Point(120, y);
            hoursBox.BackColor = Color.FromArgb(31, 31, 46);
            hoursBox.ForeColor = Color.White;
            hoursBox.Font = new Font("Segoe UI", 11);
            hoursBox.TextAlign = HorizontalAlignment.Center;
            Controls.Add(hoursBox);

            Label hLabel = new Label();
            hLabel.Text = "h";
            hLabel.ForeColor = Color.FromArgb(156, 163, 175);
            hLabel.Location = new Point(175, y + 5);
            hLabel.AutoSize = true;
            Controls.Add(hLabel);

            minsBox = new TextBox();
            minsBox.Text = "0";
            minsBox.Size = new Size(50, 30);
            minsBox.Location = new Point(195, y);
            minsBox.BackColor = Color.FromArgb(31, 31, 46);
            minsBox.ForeColor = Color.White;
            minsBox.Font = new Font("Segoe UI", 11);
            minsBox.TextAlign = HorizontalAlignment.Center;
            Controls.Add(minsBox);

            Label mLabel = new Label();
            mLabel.Text = "m";
            mLabel.ForeColor = Color.FromArgb(156, 163, 175);
            mLabel.Location = new Point(250, y + 5);
            mLabel.AutoSize = true;
            Controls.Add(mLabel);

            Button setBtn = new Button();
            setBtn.Text = "Set";
            setBtn.Size = new Size(70, 32);
            setBtn.Location = new Point(290, y);
            setBtn.FlatStyle = FlatStyle.Flat;
            setBtn.BackColor = Color.FromArgb(59, 130, 246);
            setBtn.ForeColor = Color.White;
            setBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            setBtn.Cursor = Cursors.Hand;
            setBtn.FlatAppearance.BorderSize = 0;
            setBtn.Click += SetCustomTime;
            Controls.Add(setBtn);
            y += 55;

            Button startBtn = new Button();
            startBtn.Text = "START FOCUS MODE";
            startBtn.Size = new Size(280, 55);
            startBtn.Location = new Point((Width - 280) / 2, y);
            startBtn.FlatStyle = FlatStyle.Flat;
            startBtn.BackColor = Color.FromArgb(124, 58, 237);
            startBtn.ForeColor = Color.White;
            startBtn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            startBtn.Cursor = Cursors.Hand;
            startBtn.FlatAppearance.BorderColor = Color.FromArgb(167, 139, 250);
            startBtn.FlatAppearance.BorderSize = 2;
            startBtn.Click += StartFocus;
            Controls.Add(startBtn);
            y += 75;

            Panel warningPanel = new Panel();
            warningPanel.Size = new Size(Width - 80, 50);
            warningPanel.Location = new Point(40, y);
            warningPanel.BackColor = Color.FromArgb(31, 31, 46);
            
            Label warningLabel = new Label();
            warningLabel.Text = "Warning: This locks your system. Only video end, timer, or reboot will restore.";
            warningLabel.Font = new Font("Segoe UI", 10);
            warningLabel.ForeColor = Color.FromArgb(251, 191, 36);
            warningLabel.AutoSize = false;
            warningLabel.Size = new Size(Width - 100, 40);
            warningLabel.Location = new Point(10, 5);
            warningPanel.Controls.Add(warningLabel);
            Controls.Add(warningPanel);
            y += 65;

            statusLabel = new Label();
            statusLabel.Text = "Ready - Enter YouTube URL and select duration";
            statusLabel.Font = new Font("Segoe UI", 10);
            statusLabel.ForeColor = Color.FromArgb(107, 114, 128);
            statusLabel.AutoSize = false;
            statusLabel.Size = new Size(Width, 25);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Location = new Point(0, y);
            Controls.Add(statusLabel);
        }

        private Button CreateTimerButton(string text, int x, int y, bool selected)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(90, 40);
            btn.Location = new Point(x, y);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = selected ? Color.FromArgb(124, 58, 237) : Color.FromArgb(31, 31, 46);
            btn.ForeColor = Color.FromArgb(229, 231, 235);
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
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
            statusLabel.Text = msg;
            statusLabel.ForeColor = error ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129);
        }

        private bool ValidateUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return Regex.IsMatch(url, @"(youtube\.com|youtu\.be)", RegexOptions.IgnoreCase);
        }

        private void StartFocus(object sender, EventArgs e)
        {
            string url = urlTextBox.Text;
            if (url != null) url = url.Trim();
            if (string.IsNullOrEmpty(url)) { UpdateStatus("Enter a YouTube URL", true); return; }
            if (!ValidateUrl(url)) { UpdateStatus("Invalid YouTube URL", true); return; }

            UpdateStatus("Activating Focus Mode...", false);
            focusActive = true;
                        
            // Force kill any existing MPV instances FIRST
            KillProcess("mpv");
            
            ThreadPool.QueueUserWorkItem(delegate { RunFocusMode(url); });
        }

        private void RunFocusMode(string url)
        {
            try
            {
                KillProcess("explorer");
                KillProcess("taskmgr");

                blockerCts = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem(delegate { BlockTaskMgr(); });

                this.Invoke((MethodInvoker)delegate { this.Hide(); });

                string mpvPath = Path.Combine(appDir, "mpv.exe");
                
                string inputConfPath = Path.Combine(appDir, "config", "input.conf");
                string configDir = Path.Combine(appDir, "config");

                ProcessStartInfo si = new ProcessStartInfo();
                si.FileName = mpvPath;
                
                // Embed the critical strict settings DIRECTLY in the command arguments
                string args =  "--force-window=yes " +
                               "--input-default-bindings=no " + // BLOCK EVERYTHING
                               "--input-conf=\"" + inputConfPath + "\" " + // FORCE OUR INPUT FILE
                               "--config-dir=\"" + configDir + "\" " + // Load other settings
                               "\"" + url + "\"";
                               
                si.Arguments = args;
                si.UseShellExecute = true; 
                si.WindowStyle = ProcessWindowStyle.Normal;
                
                mpvProcess = Process.Start(si);

                if (selectedMinutes > 0)
                {
                    int waitMs = selectedMinutes * 60 * 1000;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(waitMs);
                        if (focusActive && mpvProcess != null && !mpvProcess.HasExited)
                        {
                            try { mpvProcess.Kill(); } catch { }
                        }
                    });
                }

                if (mpvProcess != null)
                    mpvProcess.WaitForExit();
            }
            catch { }
            finally { EndFocus(); }
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

        private void BlockTaskMgr()
        {
            while (focusActive)
            {
                KillProcess("taskmgr");
                Thread.Sleep(1000);
            }
        }

        private void EndFocus()
        {
            focusActive = false;
            if (blockerCts != null) blockerCts.Cancel();
            KillProcess("mpv");
            try { Process.Start("explorer.exe"); } catch { }
            Application.Exit();
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
