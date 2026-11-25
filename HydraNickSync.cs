using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class HydraStyleForm : Form {
    private string titleText = "HYDRA NICK SYNC";
    private string statusText = "Scanning Hydra Launcher library...";
    private string subText = "Checking installed games for default 'Player' tag";
    private float progress = 0.25f;
    private bool isDone = false;
    private Image appIconImg = null;
    private Timer animTimer;
    private Timer closeTimer;

    public HydraStyleForm() {
        this.Text = "Hydra Nick Sync";
        this.Size = new Size(440, 148);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.DoubleBuffered = true;
        this.BackColor = Color.FromArgb(21, 21, 24);

        try {
            Icon extracted = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (extracted != null) appIconImg = extracted.ToBitmap();
        } catch {}

        GraphicsPath path = CreateRoundedRect(new Rectangle(0, 0, this.Width, this.Height), 14);
        this.Region = new Region(path);

        animTimer = new Timer();
        animTimer.Interval = 25;
        animTimer.Tick += delegate {
            if (!isDone && progress < 0.85f) {
                progress += 0.04f;
                this.Invalidate();
            }
        };
        animTimer.Start();

        this.Shown += delegate {
            Timer startDelay = new Timer();
            startDelay.Interval = 350;
            startDelay.Tick += delegate {
                startDelay.Stop();
                RunSync();
            };
            startDelay.Start();
        };
    }

    private GraphicsPath CreateRoundedRect(Rectangle bounds, int radius) {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(21, 21, 24)))
        using (Pen borderPen = new Pen(Color.FromArgb(46, 46, 54), 1.5f)) {
            GraphicsPath outerPath = CreateRoundedRect(new Rectangle(0, 0, this.Width - 1, this.Height - 1), 14);
            g.FillPath(bgBrush, outerPath);
            g.DrawPath(borderPen, outerPath);
        }

        Rectangle cardRect = new Rectangle(12, 12, this.Width - 24, this.Height - 36);
        using (SolidBrush cardBrush = new SolidBrush(Color.FromArgb(28, 28, 33)))
        using (Pen cardPen = new Pen(Color.FromArgb(39, 39, 46), 1f)) {
            GraphicsPath cardPath = CreateRoundedRect(cardRect, 10);
            g.FillPath(cardBrush, cardPath);
            g.DrawPath(cardPen, cardPath);
        }

        int textLeft = 28;
        if (appIconImg != null) {
            Rectangle iconRect = new Rectangle(26, 28, 48, 48);
            GraphicsPath iconClip = CreateRoundedRect(iconRect, 10);
            g.SetClip(iconClip);
            g.DrawImage(appIconImg, iconRect);
            g.ResetClip();
            textLeft = 88;
        }

        using (Font fontTitle = new Font("Segoe UI", 8.5f, FontStyle.Bold))
        using (SolidBrush mutedBrush = new SolidBrush(Color.FromArgb(142, 142, 153))) {
            g.DrawString(titleText, fontTitle, mutedBrush, textLeft, 24);
        }

        using (Font fontStatus = new Font("Segoe UI", 11f, FontStyle.Bold))
        using (SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(244, 244, 245))) {
            g.DrawString(statusText, fontStatus, whiteBrush, textLeft, 43);
        }

        Color subColor = isDone ? Color.FromArgb(74, 222, 128) : Color.FromArgb(161, 161, 170);
        using (Font fontSub = new Font("Segoe UI", 9f, FontStyle.Regular))
        using (SolidBrush subBrush = new SolidBrush(subColor)) {
            g.DrawString(subText, fontSub, subBrush, textLeft, 67);
        }

        int barX = 14;
        int barY = this.Height - 14;
        int barW = this.Width - 28;
        int barH = 4;

        using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(39, 39, 46))) {
            g.FillRectangle(trackBrush, barX, barY, barW, barH);
        }

        int fillW = (int)(barW * Math.Min(1.0f, progress));
        Color barColor = isDone ? Color.FromArgb(34, 197, 94) : Color.FromArgb(139, 92, 246);
        using (SolidBrush fillBrush = new SolidBrush(barColor)) {
            g.FillRectangle(fillBrush, barX, barY, fillW, barH);
        }
    }

    private string ReadLockedFile(string path) {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (MemoryStream ms = new MemoryStream()) {
            fs.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    private string GetGameRootFolder(string exePath) {
        string dir = File.Exists(exePath) ? Path.GetDirectoryName(exePath) : exePath;
        if (string.IsNullOrEmpty(dir)) return null;

        DirectoryInfo di = new DirectoryInfo(dir);
        string[] subNames = new string[] { "win64", "win32", "win_x64", "win_x86", "x64", "x86", "binaries", "bin", "retail", "shipping", "sp26" };
        while (di.Parent != null && di.Parent.Parent != null && Array.IndexOf(subNames, di.Name.ToLower()) >= 0) {
            di = di.Parent;
        }
        return di.FullName;
    }

    private void RunSync() {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"hydralauncher\hydra-db");
        string nick = "";
        HashSet<string> folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(dbPath)) {
            foreach (string file in Directory.GetFiles(dbPath, "*.*", SearchOption.AllDirectories)) {
                string ext = Path.GetExtension(file).ToLower();
                if (ext != ".ldb" && ext != ".log") continue;
                try {
                    string content = ReadLockedFile(file);

                    foreach (Match mUser in Regex.Matches(content, "\"displayName\"\\s*:\\s*\"([^\"]{2,32})\"")) {
                        string candidate = mUser.Result("$1").Trim();
                        if (!string.IsNullOrEmpty(candidate) && candidate != "Player" && !candidate.Contains("\\")) {
                            nick = candidate;
                        }
                    }

                    foreach (Match mPath in Regex.Matches(content, "\"executablePath\"\\s*:\\s*\"([^\"]+)\"")) {
                        string p = mPath.Result("$1").Replace("\\\\", "\\");
                        if (p.IndexOf("steamapps", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        if (File.Exists(p)) {
                            string root = GetGameRootFolder(p);
                            if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) folders.Add(root);
                        }
                    }
                } catch {}
            }
        }

        if (string.IsNullOrEmpty(nick)) nick = Environment.UserName;

        int changed = 0;
        List<string> iniNames = new List<string>();
        iniNames.Add("steam_emu.ini"); iniNames.Add("codex.ini"); iniNames.Add("rune.ini");
        iniNames.Add("onlinefix.ini"); iniNames.Add("flt.ini"); iniNames.Add("valve.ini");
        iniNames.Add("3dmgame.ini"); iniNames.Add("ali213.ini"); iniNames.Add("upc_r2.ini");
        iniNames.Add("upc_r1.ini"); iniNames.Add("uplay_r2.ini"); iniNames.Add("uplay_r1.ini");
        iniNames.Add("configs.user.ini"); iniNames.Add("settings.ini");

        foreach (string dir in folders) {
            try {
                foreach (string f in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)) {
                    string name = Path.GetFileName(f).ToLower();
                    string parent = Path.GetFileName(Path.GetDirectoryName(f)).ToLower();

                    if (name == "account_name.txt" && parent == "steam_settings") {
if (File.ReadAllText(f).Trim() == "Player") {
File.WriteAllText(f, nick, Encoding.UTF8);
changed++;
}
} else if (iniNames.Contains(name)) {
string txt = File.ReadAllText(f);
string updated = Regex.Replace(
txt,
"(?im)^(\\s*(?:UserName|PlayerName|PersonaName|account_name)\\s*=\\s*)Player\\s*$",
"${1}" + nick
);
if (txt != updated) {
File.WriteAllText(f, updated, Encoding.UTF8);
changed++;
}
}
}
} catch {}
}
isDone = true;
progress = 1.0f;
statusText = "Synced as " + nick;
subText = "Hydra games: " + folders.Count + "   •   Updated files: " + changed;
this.Invalidate();
closeTimer = new Timer();
closeTimer.Interval = 2400;
closeTimer.Tick += delegate { Application.Exit(); };
closeTimer.Start();
}
[STAThread]
public static void Main() {
Application.EnableVisualStyles();
Application.Run(new HydraStyleForm());
}
}
