using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing.Text;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace WeSupport
{
    public partial class Form1 : Form
    {
        private Label questionLabel;
        private Button yesButton;
        private Button noButton;
        private PictureBox logoPicture;
        private Label loadingSvg;
        private Label titleLabel;
        private Panel questionPanel;


        // Zaokrąglenie rogów
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        // Przeciąganie okna
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

            this.Text = "WeSupport"; // Ustawienie tytułu okna

            // Ukrycie paska tytułu
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Zaokrąglone rogi na starcie
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));

            // Obsługa przeciągania
            this.MouseDown += Form1_MouseDown;

            this.Opacity = 0; // start z przezroczystości
        }

        // Obsługa przeciągania formularza myszką
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // Ponowne zaokrąglenie rogów przy zmianie rozmiaru
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetupUI(); // przygotowanie layoutu

            await Task.Delay(100); // krótki czas na pełne zrenderowanie

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


        // Dodanie cienia pod oknem
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
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

        // Klasa przycisku z zaokrąglonymi rogami
        public class RoundedButton : Button
        {
            public int CornerRadius { get; set; } = 18;
            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                Rectangle rect = this.ClientRectangle;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, CornerRadius, CornerRadius, 180, 90);
                    path.AddArc(rect.Right - CornerRadius, rect.Y, CornerRadius, CornerRadius, 270, 90);
                    path.AddArc(rect.Right - CornerRadius, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                    path.CloseAllFigures();
                    this.Region = new Region(path);
                }
            }
        }

        // Klasa panelu z zaokrąglonymi rogami
        public class RoundedPanel : Panel
        {
            public int CornerRadius { get; set; } = 18;
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (GraphicsPath path = new GraphicsPath())
                {
                    Rectangle rect = this.ClientRectangle;
                    path.AddArc(rect.X, rect.Y, CornerRadius, CornerRadius, 180, 90);
                    path.AddArc(rect.Right - CornerRadius, rect.Y, CornerRadius, CornerRadius, 270, 90);
                    path.AddArc(rect.Right - CornerRadius, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                    path.CloseAllFigures();
                    this.Region = new Region(path);
                }
            }
        }

        private void SetupUI()
        {
            // 1. Panel z pytaniem
            questionPanel = new RoundedPanel
            {
                BackColor = Color.Black,
                Size = new Size(520, 56), // większy panel, aby tekst się mieścił
                Height = 56,
                Width = 520,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                CornerRadius = 18
            };
            Controls.Add(questionPanel);

            questionLabel = new Label
            {
                Text = "CZY MOŻEMY PODŁĄCZYĆ SIĘ ZDALNIE?",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f), // mniejsza czcionka
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            questionPanel.Controls.Add(questionLabel);

            // 2. Przyciski TAK/NIE
            yesButton = new RoundedButton
            {
                Text = "TAK",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f, FontStyle.Bold), // mniejsza czcionka
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 48), // mniejszy rozmiar
                Cursor = Cursors.Hand,
                CornerRadius = 18
            };
            yesButton.FlatAppearance.BorderSize = 0;
            yesButton.FlatAppearance.MouseOverBackColor = Color.LightGray;

            noButton = new RoundedButton
            {
                Text = "NIE",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f, FontStyle.Bold), // mniejsza czcionka
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 48), // mniejszy rozmiar
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

            // Panel z pytaniem - prawa dolna część okna
            questionPanel.Left = this.ClientSize.Width - questionPanel.Width - marginRight;
            questionPanel.Top = this.ClientSize.Height - questionPanel.Height - marginBottom - yesButton.Height - spacing;

            // Przyciski - pod panelem, wyrównane do prawej
            int totalWidth = yesButton.Width + spacing + noButton.Width;
            int startX = questionPanel.Left + questionPanel.Width / 2 - totalWidth / 2;
            int buttonsTop = questionPanel.Bottom + spacing;
            yesButton.Location = new Point(startX, buttonsTop);
            noButton.Location = new Point(startX + yesButton.Width + spacing, buttonsTop);
        }

        private Font LoadFont(string resourcePath, float size, FontStyle style = FontStyle.Regular)
        {
            var fontCollection = new PrivateFontCollection();
            using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                byte[] fontData = new byte[fontStream.Length];
                fontStream.Read(fontData, 0, (int)fontStream.Length);
                unsafe
                {
                    fixed (byte* pFontData = fontData)
                    {
                        fontCollection.AddMemoryFont((IntPtr)pFontData, fontData.Length);
                    }
                }
            }

            return new Font(fontCollection.Families[0], size, style);
        }

        private async void YesButton_Click(object sender, EventArgs e)
        {
            questionLabel.Visible = false;
            yesButton.Visible = false;
            noButton.Visible = false;

            // Wyświetl napis 'Zaraz zaczynamy..' w panelu z pytaniem
            Label startingLabel = new Label
            {
                Text = "Zaraz zaczynamy..",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 18f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            questionPanel.Controls.Add(startingLabel);
            startingLabel.BringToFront();

            // pobieranie i uruchamianie pliku WeSupport_Lite.exe
            string url = "https://www.we-support.pl/app/WeSupport_Lite.exe";
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wesupportDir = Path.Combine(userDir, "WeSupport");
            Directory.CreateDirectory(wesupportDir);

            string exePath = Path.Combine(wesupportDir, "WeSupport_Lite.exe");

            using (var client = new HttpClient())
            {
                try
                {
                    var data = await client.GetByteArrayAsync(url);
                    // plik już istniał, nadpisz
                    if (File.Exists(exePath))
                    {
                        File.Delete(exePath);
                    }
                    await File.WriteAllBytesAsync(exePath, data);

                    // Tworzenie skrótu na pulpicie
                    CreateShortcut(exePath);

                    // Uruchomienie aplikacji
                    Process.Start(exePath);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd pobierania lub uruchamiania pliku: " + ex.Message);
                }
            }
        }

        private void CreateShortcut(string targetExePath)
        {
            string shortcutName = "WeSupport.lnk";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, shortcutName);

            // Nadpisanie istniejącego skrótu
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }

            string iconPath = ExtractIconFromResources();

            string powershellCommand = $@"
$WshShell = New-Object -ComObject WScript.Shell;
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}');
$Shortcut.TargetPath = '{targetExePath}';
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetExePath)}';
$Shortcut.WindowStyle = 1;
$Shortcut.IconLocation = '{iconPath}';
$Shortcut.Description = 'WeSupport aplikacja zdalna';
$Shortcut.Save();";


            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{powershellCommand}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd tworzenia skrótu: " + ex.Message);
            }
        }
        private string ExtractIconFromResources()
        {
            string wesupportDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "WeSupport");
            Directory.CreateDirectory(wesupportDir);

            string iconPath = Path.Combine(wesupportDir, "shortcut_icon.ico");

            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WeSupport.Assets.ikona.ico"))
            {
                if (iconStream != null)
                {
                    using (FileStream fileStream = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                    {
                        iconStream.CopyTo(fileStream);
                    }
                }
            }

            return iconPath;
        }


    }
}
