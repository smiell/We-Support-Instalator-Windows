using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WeSupport
{
    public partial class Form1 : Form
    {
        private Label questionLabel;
        private Button yesButton;
        private Button noButton;
        private RoundedPanel questionPanel;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public Form1()
        {
            InitializeComponent();
            SetBackgroundImageFromResources();
            SetWindowIconFromResources();
            this.Text = "WeSupport";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
            this.MouseDown += Form1_MouseDown;
            this.Opacity = 0;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetupUI();
            await Task.Delay(100);
            this.Visible = true;
            var fadeTimer = new System.Windows.Forms.Timer { Interval = 30 };
            fadeTimer.Tick += (s, args) =>
            {
                if (this.Opacity < 1.0)
                {
                    this.Opacity += 0.05;
                }
                else
                {
                    this.Opacity = 1.0;
                    fadeTimer.Stop();
                }
            };
            fadeTimer.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000;
                return cp;
            }
        }

        private void SetBackgroundImageFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("WeSupport.Assets.background.png"))
            {
                if (stream != null)
                {
                    this.BackgroundImage = new Bitmap(stream);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
        }
        private void SetWindowIconFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("WeSupport.Assets.ikona.ico"))
            {
                if (stream != null)
                {
                    this.Icon = new Icon(stream);
                }
            }
        }

        private void SetupUI()
        {
            questionPanel = new RoundedPanel
            {
                BackColor = Color.Black,
                Size = new Size(520, 56),
                Height = 56,
                Width = 520,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                CornerRadius = 18
            };
            Controls.Add(questionPanel);

            questionLabel = new Label
            {
                Text = "CZY MOŻEMY PODŁĄCZYĆ SIĘ ZDALNIE?",
                Font = FontHelper.LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            questionPanel.Controls.Add(questionLabel);

            yesButton = new RoundedButton
            {
                Text = "TAK",
                Font = FontHelper.LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 48),
                Cursor = Cursors.Hand,
                CornerRadius = 18
            };
            yesButton.FlatAppearance.BorderSize = 0;
            yesButton.FlatAppearance.MouseOverBackColor = Color.LightGray;

            noButton = new RoundedButton
            {
                Text = "NIE",
                Font = FontHelper.LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 48),
                Cursor = Cursors.Hand,
                CornerRadius = 18
            };
            noButton.FlatAppearance.BorderSize = 0;
            noButton.FlatAppearance.MouseOverBackColor = Color.LightGray;

            yesButton.Click += YesButton_Click;
            noButton.Click += (s, e) => Application.Exit();

            Controls.Add(yesButton);
            Controls.Add(noButton);

            LayoutControls();
        }

        private void LayoutControls()
        {
            int marginRight = 80;
            int marginBottom = 100;
            int spacing = 30;
            questionPanel.Left = this.ClientSize.Width - questionPanel.Width - marginRight;
            questionPanel.Top = this.ClientSize.Height - questionPanel.Height - marginBottom - yesButton.Height - spacing;
            int totalWidth = yesButton.Width + spacing + noButton.Width;
            int startX = questionPanel.Left + questionPanel.Width / 2 - totalWidth / 2;
            int buttonsTop = questionPanel.Bottom + spacing;
            yesButton.Location = new Point(startX, buttonsTop);
            noButton.Location = new Point(startX + yesButton.Width + spacing, buttonsTop);
        }

        private async void YesButton_Click(object sender, EventArgs e)
        {
            questionLabel.Visible = false;
            yesButton.Visible = false;
            noButton.Visible = false;

            Label startingLabel = new Label
            {
                Text = "Zaraz zaczynamy...",
                Font = FontHelper.LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            questionPanel.Controls.Add(startingLabel);
            startingLabel.BringToFront();

            string url = "https://www.we-support.pl/app/WeSupport_Lite.exe";
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wesupportDir = Path.Combine(userDir, "WeSupport");
            Directory.CreateDirectory(wesupportDir);
            string exePath = Path.Combine(wesupportDir, "WeSupport_Lite.exe");
            await DownloadHelper.DownloadAndRunAsync(url, exePath, err => MessageBox.Show(err));
        }
    }
}
