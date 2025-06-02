using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace ClassmatesRPG
{
    // Form1 is the main interface for the Classmates RPG Battle Simulator.
    // It allows users to enter player names, select character types, and start a battle.
    // The battle log displays each attack and its damage, and the winner is announced at the end.
    public class Form1 : Form
    {
        private TextBox txtPlayer1 = new TextBox();
        private TextBox txtPlayer2 = new TextBox();
        private ComboBox comboBox1 = new ComboBox();
        private ComboBox comboBox2 = new ComboBox();
        private Button btnBattle = new Button();
        private TextBox txtLog = new TextBox();
        private Label lblHealth1 = new Label();
        private Label lblHealth2 = new Label();
        private PictureBox picPlayer1 = new PictureBox();
        private PictureBox picPlayer2 = new PictureBox();
        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
        private bool isAttacking = false;
        private int animationFrame = 0;
        private readonly Random random = new Random();

        private ClassFighter? player1;
        private ClassFighter? player2;

        private Image? oaSorcererImage;
        private Image? anteMaloiImage;
        private Image? taoLangImage;
        private Image? backgroundImage;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Classmates RPG Battle Simulator";
            this.Size = new Size(1000, 800);  // Increased form size for better visibility
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            LoadImages();
            
            // Set up animation timer
            animationTimer.Interval = 50; // Faster animation for smoother movement
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void LoadImages()
        {
            try
            {
                // Dispose of any existing images
                DisposeImages();

                // Try different possible paths for the characters folder
                string[] possiblePaths = new[]
                {
                    Path.Combine(Application.StartupPath, "characters"),
                    Path.Combine(Directory.GetCurrentDirectory(), "characters"),
                    "characters",
                    Path.Combine("..", "characters"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "characters")
                };

                string? charactersPath = null;
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path) && 
                        File.Exists(Path.Combine(path, "oa_sorcerer.png")) &&
                        File.Exists(Path.Combine(path, "antemaloi_crusader.png")) &&
                        File.Exists(Path.Combine(path, "taolangadventurer.png")) &&
                        File.Exists(Path.Combine(path, "bgbg.png")))
                    {
                        charactersPath = path;
                        break;
                    }
                }

                if (charactersPath == null)
                {
                    throw new FileNotFoundException("Could not find the characters directory with all required images.");
                }

                string oaSorcererPath = Path.Combine(charactersPath, "oa_sorcerer.png");
                string anteMaloiPath = Path.Combine(charactersPath, "antemaloi_crusader.png");
                string taoLangPath = Path.Combine(charactersPath, "taolangadventurer.png");
                string backgroundPath = Path.Combine(charactersPath, "bgbg.png");

                // Load images with proper error handling
                try
                {
                    using var oaSorcererStream = new FileStream(oaSorcererPath, FileMode.Open, FileAccess.Read);
                    using var anteMaloiStream = new FileStream(anteMaloiPath, FileMode.Open, FileAccess.Read);
                    using var taoLangStream = new FileStream(taoLangPath, FileMode.Open, FileAccess.Read);
                    using var backgroundStream = new FileStream(backgroundPath, FileMode.Open, FileAccess.Read);

                    using var tempOASorcerer = Image.FromStream(oaSorcererStream);
                    using var tempAnteMaloi = Image.FromStream(anteMaloiStream);
                    using var tempTaoLang = Image.FromStream(taoLangStream);
                    using var tempBackground = Image.FromStream(backgroundStream);

                    oaSorcererImage = new Bitmap(tempOASorcerer);
                    anteMaloiImage = new Bitmap(tempAnteMaloi);
                    taoLangImage = new Bitmap(tempTaoLang);
                    backgroundImage = new Bitmap(tempBackground);
                }
                catch (Exception ex)
                {
                    DisposeImages();
                    throw new Exception($"Error loading character images: {ex.Message}", ex);
                }

                // Force a redraw
                this.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}\n\nPlease ensure all character images are in the 'characters' folder.", 
                    "Image Loading Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void DisposeImages()
        {
            if (oaSorcererImage != null) { oaSorcererImage.Dispose(); oaSorcererImage = null; }
            if (anteMaloiImage != null) { anteMaloiImage.Dispose(); anteMaloiImage = null; }
            if (taoLangImage != null) { taoLangImage.Dispose(); taoLangImage = null; }
            if (backgroundImage != null) { backgroundImage.Dispose(); backgroundImage = null; }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DisposeImages();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (backgroundImage != null)
            {
                // Draw the background image stretched to fill the form
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(backgroundImage, ClientRectangle);

                // Add a semi-transparent overlay for better readability
                using (var overlay = new SolidBrush(Color.FromArgb(40, 0, 0, 20)))
                {
                    e.Graphics.FillRectangle(overlay, ClientRectangle);
                }
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            animationFrame = (animationFrame + 1) % 2;
            picPlayer1.Invalidate();
            picPlayer2.Invalidate();
        }

        private void InitializeComponent()
        {
            // Add a top banner panel for the title
            Panel titleBanner = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, 110),  // Reduced from 130 to 110
                BackColor = Color.FromArgb(20, 25, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(10, 10, 10, 5)  // Reduced bottom padding
            };
            Controls.Add(titleBanner);

            // Title label with strong contrast and centered
            Label lblTitle = new Label()
            {
                Location = new Point(0, 2),  // Slight adjustment
                Width = this.Width,
                Height = 60,
                Text = "RPG Battle Simulator",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(220, 220, 255),
                BackColor = Color.Transparent,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(5)
            };
            titleBanner.Controls.Add(lblTitle);

            // Add subheading with more room
            Label lblSubtitle = new Label()
            {
                Location = new Point(0, 62),  // Adjusted to be closer to title
                Width = this.Width,
                Height = 35,  // Slightly reduced
                Text = "turning our classroom into a fantasy adventure!",
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.FromArgb(180, 180, 220),
                BackColor = Color.Transparent,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(5, 2, 5, 2)  // Reduced vertical padding
            };
            titleBanner.Controls.Add(lblSubtitle);

            // Adjust the rest of the controls to start below the banner
            int topOffset = 120;  // Reduced from 140 to 120

            // Player 1 Group Box with modern styling
            GroupBox groupBox1 = CreateStyledGroupBox("Player 1", 50, topOffset + 10);

            // Player 1 Name Label
            Label lblPlayer1 = CreateStyledLabel("Name:", 20, 30);
            groupBox1.Controls.Add(lblPlayer1);

            // Player 1 Name Input with modern styling
            txtPlayer1 = CreateStyledTextBox(20, 55, "Enter Champion Name");
            groupBox1.Controls.Add(txtPlayer1);

            // Player 1 Character Label
            Label lblChar1 = CreateStyledLabel("Champion:", 20, 90);
            groupBox1.Controls.Add(lblChar1);

            // Player 1 Character Selection with modern styling
            comboBox1 = CreateStyledComboBox(20, 115);
            comboBox1.Items.AddRange(new string[] { "The OA Sorcerer", "Ante Maloi Crusader", "The Tao Lang Adventurer" });
            groupBox1.Controls.Add(comboBox1);

            Controls.Add(groupBox1);

            // Player 2 Group Box with modern styling
            GroupBox groupBox2 = CreateStyledGroupBox("Player 2", 550, topOffset + 10);

            // Player 2 Name Label
            Label lblPlayer2 = CreateStyledLabel("Name:", 20, 30);
            groupBox2.Controls.Add(lblPlayer2);

            // Player 2 Name Input with modern styling
            txtPlayer2 = CreateStyledTextBox(20, 55, "Enter Champion Name");
            groupBox2.Controls.Add(txtPlayer2);

            // Player 2 Character Label
            Label lblChar2 = CreateStyledLabel("Champion:", 20, 90);
            groupBox2.Controls.Add(lblChar2);

            // Player 2 Character Selection with modern styling
            comboBox2 = CreateStyledComboBox(20, 115);
            comboBox2.Items.AddRange(new string[] { "The OA Sorcerer", "Ante Maloi Crusader", "The Tao Lang Adventurer" });
            groupBox2.Controls.Add(comboBox2);

            Controls.Add(groupBox2);

            // Battle button with modern styling
            btnBattle = CreateStyledButton("Start Battle", 375, topOffset + 80);
            btnBattle.Click += btnBattle_Click;
            Controls.Add(btnBattle);

            // Health labels with modern styling
            lblHealth1 = CreateStyledHealthLabel(50, topOffset + 220);
            lblHealth2 = CreateStyledHealthLabel(550, topOffset + 220);

            // Character images with custom drawing
            picPlayer1 = new PictureBox()
            {
                Location = new Point(50, topOffset + 260),
                Size = new Size(150, 150),  // Match BattleForm size
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Normal
            };
            picPlayer1.Paint += PicPlayer1_Paint;

            picPlayer2 = new PictureBox()
            {
                Location = new Point(550, topOffset + 260),
                Size = new Size(150, 150),  // Match BattleForm size
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Normal
            };
            picPlayer2.Paint += PicPlayer2_Paint;

            // Battle log with modern styling
            txtLog = CreateStyledBattleLog(50, topOffset + 570);

            // Add controls to form
            Controls.Add(txtLog);
            Controls.Add(lblHealth1);
            Controls.Add(lblHealth2);
            Controls.Add(picPlayer1);
            Controls.Add(picPlayer2);
        }

        private GroupBox CreateStyledGroupBox(string text, int x, int y)
        {
            return new GroupBox()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(300, 200),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 255),
                BackColor = Color.FromArgb(40, 40, 60)
            };
        }

        private Label CreateStyledLabel(string text, int x, int y)
        {
            return new Label()
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 255),
                AutoSize = true
            };
        }

        private TextBox CreateStyledTextBox(int x, int y, string placeholder)
        {
            return new TextBox()
            {
                Location = new Point(x, y),
                Width = 260,
                Font = new Font("Segoe UI", 10),
                PlaceholderText = placeholder,
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.FromArgb(220, 220, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private ComboBox CreateStyledComboBox(int x, int y)
        {
            return new ComboBox()
            {
                Location = new Point(x, y),
                Width = 260,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.FromArgb(220, 220, 255),
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private Button CreateStyledButton(string text, int x, int y)
        {
            return new Button()
            {
                Location = new Point(x, y),
                Text = text,
                Size = new Size(150, 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.FromArgb(220, 220, 255),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private Label CreateStyledHealthLabel(int x, int y)
        {
            return new Label()
            {
                Location = new Point(x, y),
                Width = 300,
                Text = "HP: ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 255),
                BackColor = Color.Transparent
            };
        }

        private PictureBox CreateStyledPictureBox(int x, int y)
        {
            return new PictureBox()
            {
                Location = new Point(x, y + 20),  // Reduced vertical offset
                Size = new Size(150, 150),  // Smaller container size
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Normal
            };
        }

        private TextBox CreateStyledBattleLog(int x, int y)
        {
            var txtLog = new TextBox()
            {
                Location = new Point(40, ClientSize.Height - 120),
                Width = ClientSize.Width - 80,
                Height = 100,
                Multiline = true,
                ScrollBars = ScrollBars.None,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.FromArgb(255, 255, 200),
                TextAlign = HorizontalAlignment.Center,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(10)
            };
            
            // Subscribe to form resize to update position
            this.Resize += (s, e) => {
                txtLog.Width = ClientSize.Width - 80;
                txtLog.Location = new Point(40, ClientSize.Height - 120);
            };
            
            return txtLog;
        }

        private void PicPlayer1_Paint(object? sender, PaintEventArgs e)
        {
            if (player1 != null && comboBox1.SelectedItem != null)
            {
                DrawCharacter(e.Graphics, picPlayer1.ClientRectangle, comboBox1.SelectedItem.ToString() ?? string.Empty, Color.Blue, animationFrame == 0 && isAttacking, true, player1);
            }
        }

        private void PicPlayer2_Paint(object? sender, PaintEventArgs e)
        {
            if (player2 != null && comboBox2.SelectedItem != null)
            {
                DrawCharacter(e.Graphics, picPlayer2.ClientRectangle, comboBox2.SelectedItem.ToString() ?? string.Empty, Color.Green, animationFrame == 1 && isAttacking, false, player2);
            }
        }

        private void DrawCharacter(Graphics g, Rectangle bounds, string name, Color color, bool isAttacking, bool isRightSide, ClassFighter? fighter)
        {
            if (g == null || bounds.Width <= 0 || bounds.Height <= 0 || fighter == null) return;

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                // If attacking, add a subtle lunging effect
                int attackOffsetX = 0;
                if (isAttacking)
                {
                    attackOffsetX = isRightSide ? -10 : 10;  // Reduced movement
                }

                int baseX = (isRightSide ? bounds.Right - bounds.Width/2 : bounds.X + bounds.Width/2) + attackOffsetX;
                int baseY = bounds.Y + 30;

                // Draw character glow effect when attacking
                if (isAttacking)
                {
                    // Single subtle glow layer
                    using (var glowBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    {
                        g.FillEllipse(glowBrush, 
                            baseX - bounds.Width/2 - 5, 
                            baseY - 5,
                            bounds.Width + 10,
                            bounds.Height + 10);
                    }

                    // Add floating red pixels around character
                    DrawFloatingPixels(g, baseX, baseY, bounds);
                }

                // Get the appropriate character image
                Image? characterImage = GetCharacterImage(fighter);
                if (characterImage == null) return;

                // Calculate dimensions and draw character
                var (x, y, imageWidth, imageHeight) = CalculateCharacterDimensions(bounds, characterImage, attackOffsetX);
                DrawCharacterImage(g, characterImage, x, y, imageWidth, imageHeight, isRightSide, isAttacking);

                // Draw attack effects if attacking
                if (isAttacking)
                {
                    DrawAttackEffects(g, x, y, imageWidth, imageHeight, isRightSide);
                }

                // Draw health bar and name
                DrawHealthBar(g, bounds, fighter);
                DrawCharacterName(g, baseX, baseY, name);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the game
                System.Diagnostics.Debug.WriteLine($"Error in DrawCharacter: {ex.Message}");
            }
        }

        private void DrawFloatingPixels(Graphics g, int baseX, int baseY, Rectangle bounds)
        {
            try
            {
                for (int p = 0; p < 12; p++)
                {
                    // Ensure we stay within valid bounds
                    int maxWidth = Math.Min(bounds.Width/2, 100);  // Limit maximum spread
                    int maxHeight = Math.Min(bounds.Height, 150);  // Limit maximum height

                    int pixelX = baseX + random.Next(-maxWidth, maxWidth);
                    int pixelY = baseY + random.Next(-maxHeight/3, maxHeight);
                    int pixelSize = random.Next(3, 6);  // Increased pixel size from (1,3) to (3,6)

                    // Ensure pixels stay within screen bounds
                    pixelX = Math.Max(0, Math.Min(pixelX, bounds.Right));
                    pixelY = Math.Max(0, Math.Min(pixelY, bounds.Bottom));

                    float distanceFromCenter = (float)Math.Sqrt(
                        Math.Pow(pixelX - baseX, 2) + 
                        Math.Pow(pixelY - baseY, 2)
                    );
                    int alpha = Math.Max(50, 255 - (int)(distanceFromCenter * 0.5f));  // Increased minimum alpha from 30 to 50

                    using (var pixelBrush = new SolidBrush(Color.FromArgb(alpha, 255, 50, 50)))
                    using (var glowBrush = new SolidBrush(Color.FromArgb(alpha/2, 255, 100, 100)))  // Increased glow alpha from alpha/3 to alpha/2
                    {
                        // Draw larger pixel
                        g.FillRectangle(pixelBrush, pixelX, pixelY, pixelSize, pixelSize);

                        // Draw larger glow
                        int glowSize = pixelSize * 4;  // Increased glow multiplier from 3 to 4
                        g.FillEllipse(glowBrush, 
                            pixelX - pixelSize, 
                            pixelY - pixelSize, 
                            glowSize, 
                            glowSize);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawFloatingPixels: {ex.Message}");
            }
        }

        private void DrawAttackEffects(Graphics g, int x, int y, int imageWidth, int imageHeight, bool isRightSide)
        {
            try
            {
                // Draw blue squares first (behind other effects)
                DrawBlueSquares(g, x, y, imageWidth, imageHeight, isRightSide);

                int effectX = isRightSide ? x - 10 : x + imageWidth - 10;
                int effectY = y + (imageHeight / 2) - 10;
                float effectScale = Math.Max(0.2f, Math.Min(0.4f, imageWidth / 400.0f));

                for (int i = 0; i < 2; i++)
                {
                    int offsetY = i * 5;
                    float slashScale = effectScale * (1.0f - i * 0.2f);
                    
                    using (var path = CreateSlashPath(effectX, effectY, offsetY, isRightSide))
                    {
                        // Draw glow layers
                        for (int j = 4; j >= 0; j--)
                        {
                            using (var glowPen = new Pen(Color.FromArgb(
                                150 - j * 25,
                                255, 255, 255), 
                                (4 - j) * 1 * slashScale))
                            {
                                g.DrawPath(glowPen, path);
                            }
                        }

                        // Draw main slash
                        using (var slashPen = new Pen(Color.FromArgb(255, 255, 255, 255), 1 * slashScale))
                        {
                            g.DrawPath(slashPen, path);
                        }
                    }

                    // Draw sparkles
                    DrawSparkles(g, effectX, effectY, offsetY, isRightSide);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawAttackEffects: {ex.Message}");
            }
        }

        private void DrawBlueSquares(Graphics g, int x, int y, int imageWidth, int imageHeight, bool isRightSide)
        {
            try
            {
                // Create 8 squares around the character
                for (int i = 0; i < 8; i++)
                {
                    // Calculate angle for this square (evenly distributed around character)
                    double angle = (i * Math.PI * 2 / 8) + random.NextDouble() * 0.5;
                    
                    // Calculate distance from character (with some randomness)
                    int distance = random.Next(imageWidth/2, imageWidth);
                    
                    // Calculate position
                    int squareX = x + (int)(Math.Cos(angle) * distance);
                    int squareY = y + imageHeight/2 + (int)(Math.Sin(angle) * distance);
                    
                    // Random size between 10 and 20 pixels
                    int size = random.Next(10, 21);
                    
                    // Calculate rotation for the square
                    float rotation = (float)(angle * 180 / Math.PI);
                    
                    // Save current transform
                    var originalTransform = g.Transform;
                    
                    // Set up rotation around square center
                    g.TranslateTransform(squareX + size/2, squareY + size/2);
                    g.RotateTransform(rotation);
                    g.TranslateTransform(-(squareX + size/2), -(squareY + size/2));

                    // Draw outer glow
                    for (int glow = 3; glow >= 0; glow--)
                    {
                        int glowSize = size + glow * 2;
                        using (var glowBrush = new SolidBrush(Color.FromArgb(
                            40 - glow * 10,  // Fade out glow
                            100, 150, 255)))  // Light blue color
                        {
                            g.FillRectangle(glowBrush,
                                squareX - glow,
                                squareY - glow,
                                glowSize,
                                glowSize);
                        }
                    }

                    // Draw main square
                    using (var squareBrush = new SolidBrush(Color.FromArgb(180, 50, 100, 255)))  // Darker blue for main square
                    {
                        g.FillRectangle(squareBrush, squareX, squareY, size, size);
                    }

                    // Draw inner highlight
                    using (var highlightPen = new Pen(Color.FromArgb(150, 150, 200, 255), 1))
                    {
                        g.DrawRectangle(highlightPen, 
                            squareX + size/4, 
                            squareY + size/4, 
                            size/2, 
                            size/2);
                    }

                    // Restore original transform
                    g.Transform = originalTransform;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawBlueSquares: {ex.Message}");
            }
        }

        private GraphicsPath CreateSlashPath(int effectX, int effectY, int offsetY, bool isRightSide)
        {
            var path = new GraphicsPath();
            
            Point start = new Point(
                effectX + (isRightSide ? -5 : 5), 
                effectY + 15 + offsetY
            );
            Point end = new Point(
                effectX + (isRightSide ? -20 : 20), 
                effectY - 10 + offsetY
            );
            Point control1 = new Point(
                effectX + (isRightSide ? -8 : 8), 
                effectY + 10 + offsetY
            );
            Point control2 = new Point(
                effectX + (isRightSide ? -17 : 17), 
                effectY - 5 + offsetY
            );

            path.AddBezier(start, control1, control2, end);
            return path;
        }

        private void DrawSparkles(Graphics g, int effectX, int effectY, int offsetY, bool isRightSide)
        {
            try
            {
                for (int s = 0; s < 4; s++)
                {
                    int sparkX = effectX + (isRightSide ? -20 : 20) + random.Next(-5, 5);
                    int sparkY = effectY - 10 + offsetY + random.Next(-5, 5);
                    int sparkSize = random.Next(1, 3);

                    for (int glowLayer = 2; glowLayer >= 0; glowLayer--)
                    {
                        using (var sparkBrush = new SolidBrush(Color.FromArgb(
                            200 - glowLayer * 60,
                            255, 255, 255)))
                        {
                            g.FillEllipse(sparkBrush,
                                sparkX - (sparkSize + glowLayer) / 2,
                                sparkY - (sparkSize + glowLayer) / 2,
                                sparkSize + glowLayer,
                                sparkSize + glowLayer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawSparkles: {ex.Message}");
            }
        }

        private Image? GetCharacterImage(ClassFighter fighter)
        {
            return fighter switch
            {
                AnteMaloiCrusader => anteMaloiImage,
                TaoLangAdventurer => taoLangImage,
                TheOASorcerer => oaSorcererImage,
                _ => null
            };
        }

        private (int x, int y, int width, int height) CalculateCharacterDimensions(Rectangle bounds, Image characterImage, int attackOffsetX)
        {
            float baseScale = Math.Max(0.1f, Math.Min(
                (float)(bounds.Width - 20) / characterImage.Width,
                (float)(bounds.Height - 20) / characterImage.Height
            )) * 1.0f;

            float scale = (float)Math.Round(baseScale * 10) / 10;
            scale = Math.Max(0.1f, scale);

            int imageWidth = Math.Max(10, Math.Min((int)(characterImage.Width * scale), bounds.Width - 4));
            int imageHeight = Math.Max(10, Math.Min((int)(characterImage.Height * scale), bounds.Height - 4));

            int x = bounds.X + (bounds.Width - imageWidth) / 2 + attackOffsetX;
            int y = bounds.Y + bounds.Height - imageHeight - 5;

            return (x, y, imageWidth, imageHeight);
        }

        private void DrawCharacterImage(Graphics g, Image characterImage, int x, int y, int imageWidth, int imageHeight, bool isRightSide, bool isAttacking)
        {
            if (g == null || characterImage == null) return;

            try
            {
                using (var tempBitmap = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                using (var tempGraphics = Graphics.FromImage(tempBitmap))
                {
                    tempGraphics.Clear(Color.Transparent);
                    tempGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    tempGraphics.PixelOffsetMode = PixelOffsetMode.Half;

                    if (!isRightSide)
                    {
                        tempGraphics.TranslateTransform(imageWidth, 0);
                        tempGraphics.ScaleTransform(-1, 1);
                    }

                    var destRect = new Rectangle(0, 0, imageWidth, imageHeight);
                    var sourceRect = new Rectangle(0, 0, characterImage.Width, characterImage.Height);
                    tempGraphics.DrawImage(characterImage, destRect, sourceRect, GraphicsUnit.Pixel);

                    if (isAttacking)
                    {
                        // Make yellow glow effect 5x smaller
                        using (var glowBrush = new SolidBrush(Color.FromArgb(20, 255, 220, 100)))  // Reduced alpha from 100 to 20
                        {
                            var smallerRect = new Rectangle(
                                destRect.X + destRect.Width * 2/5,  // Center the smaller effect
                                destRect.Y + destRect.Height * 2/5,
                                destRect.Width / 5,  // Make it 5x smaller
                                destRect.Height / 5
                            );
                            tempGraphics.FillRectangle(glowBrush, smallerRect);
                        }
                    }

                    g.DrawImage(tempBitmap, x, y, imageWidth, imageHeight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawCharacterImage: {ex.Message}");
            }
        }

        private void DrawHealthBar(Graphics g, Rectangle bounds, ClassFighter? fighter)
        {
            if (fighter == null) return;

            int barWidth = Math.Max(20, bounds.Width / 2);  // Ensure minimum width
            int barHeight = Math.Max(8, bounds.Height / 20);   // Ensure minimum height
            int barX = bounds.X + (bounds.Width - barWidth) / 2;
            int barY = bounds.Y + 5;  // Position at top with small gap

            // Draw health bar background
            using (var gradientBrush = new LinearGradientBrush(
                new Rectangle(barX, barY, barWidth, barHeight),
                Color.FromArgb(180, 20, 20, 20),
                Color.FromArgb(220, 30, 30, 30),
                LinearGradientMode.Vertical))
            {
                using (var path = CreateRoundedRectangle(barX, barY, barWidth, barHeight, Math.Min(4, barHeight / 2)))
                {
                    g.FillPath(gradientBrush, path);
                    
                    float percent = Math.Max(0, (float)fighter.Health / Math.Max(1, fighter.MaxHealth));
                    int healthWidth = Math.Max(1, (int)(barWidth * percent));
                    using (var healthGradient = new LinearGradientBrush(
                        new Rectangle(barX, barY, healthWidth, barHeight),
                        Color.FromArgb(255, 80, 220, 80),
                        Color.FromArgb(255, 60, 200, 60),
                        LinearGradientMode.Vertical))
                    {
                        using (var clipPath = CreateRoundedRectangle(barX, barY, healthWidth, barHeight, Math.Min(4, barHeight / 2)))
                        {
                            g.FillPath(healthGradient, clipPath);
                        }
                    }

                    using (var glowPen = new Pen(Color.FromArgb(100, 120, 255, 120), 1))
                    {
                        g.DrawPath(glowPen, path);
                    }
                }
            }

            // Draw health text
            using (var healthFont = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                string healthText = $"{fighter.Health}/{fighter.MaxHealth}";
                var size = g.MeasureString(healthText, healthFont);
                float textX = barX + (barWidth - size.Width) / 2;
                float textY = barY + (barHeight - size.Height) / 2;

                // Text outline
                using (var outlineBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            g.DrawString(healthText, healthFont, outlineBrush,
                                textX + offsetX,
                                textY + offsetY);
                        }
                    }
                }

                using (var healthBrush = new SolidBrush(Color.FromArgb(255, 220, 220, 255)))
                {
                    g.DrawString(healthText, healthFont, healthBrush, textX, textY);
                }
            }
        }

        private void DrawCharacterName(Graphics g, int baseX, int baseY, string name)
        {
            if (g == null || string.IsNullOrEmpty(name)) return;

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(220, 220, 255)))
                using (var bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 35)))
                {
                    var size = g.MeasureString(name, font);
                    var textRect = new RectangleF(baseX - size.Width / 2, baseY + 120, size.Width + 10, size.Height + 5);
                    
                    using (var path = CreateRoundedRectangle(
                        (int)textRect.X, (int)textRect.Y, 
                        (int)textRect.Width, (int)textRect.Height, 5))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(new Pen(Color.FromArgb(100, 255, 255, 255), 1), path);
                    }
                    
                    g.DrawString(name, font, brush, baseX - size.Width / 2 + 5, baseY + 120);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawCharacterName: {ex.Message}");
            }
        }

        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            var rect = new Rectangle(x, y, width, height);
            var diameter = radius * 2;

            // Top left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top right corner
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom right corner
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom left corner
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }

        private void btnBattle_Click(object? sender, EventArgs e)
        {
            try
            {
                string name1 = txtPlayer1.Text.Trim();
                string name2 = txtPlayer2.Text.Trim();
                string? char1 = comboBox1.SelectedItem?.ToString();
                string? char2 = comboBox2.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
                    throw new Exception("Please enter names for both champions.");

                if (string.IsNullOrEmpty(char1) || string.IsNullOrEmpty(char2))
                    throw new Exception("Please select champion types for both players.");

                if (name1 == name2 && char1 == char2)
                    throw new Exception("Champions must have unique names if they are the same type.");

                btnBattle.Enabled = false;
                player1 = CreateCharacter(char1, name1);
                player2 = CreateCharacter(char2, name2);

                if (player1 == null || player2 == null)
                {
                    throw new Exception("Failed to create one or both champions.");
                }

                StartBattle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Battle Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBattle.Enabled = true;
            }
        }

        private ClassFighter CreateCharacter(string characterType, string name)
        {
            return characterType switch
            {
                "The OA Sorcerer" => new TheOASorcerer(name),
                "Ante Maloi Crusader" => new AnteMaloiCrusader(name),
                "The Tao Lang Adventurer" => new TaoLangAdventurer(name),
                _ => throw new Exception($"Unknown champion type: {characterType}")
            };
        }

        private void StartBattle()
        {
            try
            {
                if (player1 == null || player2 == null)
                {
                    throw new InvalidOperationException("Players not properly initialized.");
                }

                bool player1Turn = random.Next(0, 2) == 0;
                animationTimer.Start();

                // Clear previous battle log
                txtLog.Clear();
                Log("⚔️ Battle Begins! ⚔️");
                Log($"🎲 {(player1Turn ? player1.Name : player2.Name)} goes first!");
                Log("");

                while (player1.Health > 0 && player2.Health > 0)
                {
                    try
                    {
                        int damage;
                        isAttacking = true;

                        if (player1Turn)
                        {
                            damage = player1.Attack();
                            player2.TakeDamage(damage);
                            Log($"🗡️ {player1.Name} strikes {player2.Name} for {damage} damage!");
                        }
                        else
                        {
                            damage = player2.Attack();
                            player1.TakeDamage(damage);
                            Log($"🗡️ {player2.Name} strikes {player1.Name} for {damage} damage!");
                        }

                        UpdateHealthLabels();
                        player1Turn = !player1Turn;
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(800);
                        isAttacking = false;
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(400);
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Error during battle turn: {ex.Message}");
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }
                }

                animationTimer.Stop();
                string winner = player1.Health > 0 ? player1.Name : player2.Name;
                Log("");
                Log($"🏆 {winner} is victorious! 🏆");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical battle error: {ex.Message}\n\nThe battle cannot continue.", 
                    "Battle Error",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnBattle.Enabled = true;
                isAttacking = false;
                animationTimer.Stop();
            }
        }

        private void UpdateHealthLabels()
        {
            if (player1 != null && player2 != null)
            {
                lblHealth1.Text = $"{player1.Name}\nHP: {player1.Health}/{player1.MaxHealth}";
                lblHealth2.Text = $"{player2.Name}\nHP: {player2.Health}/{player2.MaxHealth}";

                float healthPercent1 = (float)player1.Health / Math.Max(1, player1.MaxHealth);
                float healthPercent2 = (float)player2.Health / Math.Max(1, player2.MaxHealth);

                lblHealth1.ForeColor = GetHealthColor(healthPercent1);
                lblHealth2.ForeColor = GetHealthColor(healthPercent2);
            }
        }

        private Color GetHealthColor(float healthPercent)
        {
            if (healthPercent > 0.5f)
                return Color.FromArgb(100, 255, 100); // Healthy green
            else if (healthPercent > 0.2f)
                return Color.FromArgb(255, 200, 0); // Warning yellow
            else
                return Color.FromArgb(255, 80, 80); // Critical red
        }

        private void Log(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.ScrollToCaret();
        }
    }
}
