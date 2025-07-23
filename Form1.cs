using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing.Text;
using System.Diagnostics;

namespace WeSupport
{
    public partial class Form1 : Form
    {
        private Label questionLabel;
        private Button yesButton;
        private Button noButton;
        private PictureBox logoPicture;
        private Label loadingSvg;


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

        private void SetupUI()
        {
            // 1. Logo aplikacji
            logoPicture = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(400, 100),
                BackColor = Color.Transparent
            };

            using (Stream logoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WeSupport.Assets.Logo_WeSupport_biale.png"))
            {
                if (logoStream != null)
                {
                    logoPicture.Image = new Bitmap(logoStream);
                }
                else
                {
                    MessageBox.Show("Brak zasobu: Logo_WeSupport_biale.png");
                }
            }

            Controls.Add(logoPicture);

            // 2. Tekst pytania
            questionLabel = new Label
            {
                Text = "CZY MOŻEMY POŁĄCZYĆ SIĘ ZDALNIE?",
                Font = LoadFont("WeSupport.Assets.Poppins_Regular.ttf", 16f),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            Controls.Add(questionLabel);

            // 3. "Przyciski" jako tekst
            yesButton = new Button
            {
                Text = "TAK",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 14f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            yesButton.FlatAppearance.BorderSize = 0;
            yesButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            yesButton.FlatAppearance.MouseDownBackColor = Color.Transparent;

            yesButton.MouseEnter += (s, e) =>
            {
                yesButton.FlatAppearance.BorderSize = 1;
                yesButton.FlatAppearance.BorderColor = Color.White;
            };

            yesButton.MouseLeave += (s, e) =>
            {
                yesButton.FlatAppearance.BorderSize = 0;
            };

            noButton = new Button
            {
                Text = "NIE",
                Font = LoadFont("WeSupport.Assets.Poppins_Bold.ttf", 14f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            noButton.FlatAppearance.BorderSize = 0;
            noButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            noButton.FlatAppearance.MouseDownBackColor = Color.Transparent;

            noButton.MouseEnter += (s, e) =>
            {
                noButton.FlatAppearance.BorderSize = 1;
                noButton.FlatAppearance.BorderColor = Color.White;
            };

            noButton.MouseLeave += (s, e) =>
            {
                noButton.FlatAppearance.BorderSize = 0;
            };


            yesButton.Click += YesButton_Click;
            noButton.Click += (s, e) => Application.Exit();

            Controls.Add(yesButton);
            Controls.Add(noButton);

            // 4. Wycentrowanie pionowo i poziomo
            LayoutControls();
        }

        private void LayoutControls()
        {
            int centerX = this.ClientSize.Width / 2;

            // Logo
            logoPicture.Location = new Point(centerX - logoPicture.Width / 2, 80);

            // Pytanie – od razu po logo
            questionLabel.Location = new Point(
                centerX - questionLabel.Width / 2,
                logoPicture.Bottom + 80
            );

            // Przycisk TAK / NIE
            int spacing = 30;
            yesButton.Size = yesButton.PreferredSize;
            noButton.Size = noButton.PreferredSize;

            int totalWidth = yesButton.Width + spacing + noButton.Width;
            int startX = centerX - totalWidth / 2;

            int buttonsTop = questionLabel.Bottom + 30;
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

            // pobieranie i uruchamianie pliku KartaWeSupport.exe
            string url = "https://kartawesupport.pl/app/PomocWeSupport.exe";
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wesupportDir = Path.Combine(userDir, "WeSupport");
            Directory.CreateDirectory(wesupportDir);

            string exePath = Path.Combine(wesupportDir, "KartaWeSupport.exe");

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
