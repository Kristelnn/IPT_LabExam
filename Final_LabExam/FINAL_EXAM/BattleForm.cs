using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClassmatesRPG
{
    public partial class BattleForm : Form
    {
        private readonly string player1Name;
        private readonly string player2Name;
        private readonly string char1Type;
        private readonly string char2Type;
        private ClassFighter? player1;
        private ClassFighter? player2;
        private Label lblHealth1 = new Label();
        private Label lblHealth2 = new Label();
        private PictureBox picPlayer1 = new PictureBox();
        private PictureBox picPlayer2 = new PictureBox();
        private TextBox txtLog = new TextBox();
        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
        private bool isAttacking = false;
        private int animationFrame = 0;

        // Update image fields
        private Image? oaSorcererImage;
        private Image? anteMaloiImage;
        private Image? taoLangImage;
        private Image? backgroundImage;

        // Animation related fields
        private List<VisualEffect> activeEffects = new List<VisualEffect>();
        private const int ATTACK_ANIMATION_FRAMES = 5;
        private int currentAttackFrame = 0;

        private Color textBoxBackColor = Color.FromArgb(180, 20, 20, 35);
        private Color textBoxForeColor = Color.FromArgb(220, 220, 255);
        private Color healthBarBackColor = Color.FromArgb(60, 10, 10, 15);
        private Color nameTagBackColor = Color.FromArgb(180, 20, 20, 35);

        private bool player1Attacking = false;
        private bool player2Attacking = false;

        private float backgroundOffset = 0;
        private const float BACKGROUND_SCROLL_SPEED = 0.5f;

        private string? winnerName = null;
        private const int MIN_FORM_WIDTH = 800;
        private const int MIN_FORM_HEIGHT = 600;

        private readonly Random random = new Random();

        public BattleForm(string player1Name, string char1Type, string player2Name, string char2Type)
        {
            this.player1Name = player1Name;
            this.player2Name = player2Name;
            this.char1Type = char1Type;
            this.char2Type = char2Type;
            LoadImages();
            InitializeComponent();
            StartBattle();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Dispose of images
            if (oaSorcererImage != null) oaSorcererImage.Dispose();
            if (anteMaloiImage != null) anteMaloiImage.Dispose();
            if (taoLangImage != null) taoLangImage.Dispose();
            if (backgroundImage != null) backgroundImage.Dispose();
        }

        private void LoadImages()
        {
            try
            {
                // Dispose of any existing images
                if (oaSorcererImage != null) oaSorcererImage.Dispose();
                if (anteMaloiImage != null) anteMaloiImage.Dispose();
                if (taoLangImage != null) taoLangImage.Dispose();
                if (backgroundImage != null) backgroundImage.Dispose();

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

                // Load images from files with proper pixel format
                try
                {
                    using (var stream = new FileStream(oaSorcererPath, FileMode.Open, FileAccess.Read))
                    {
                        var tempImage = Image.FromStream(stream);
                        oaSorcererImage = new Bitmap(tempImage);
                        tempImage.Dispose();
                    }
                    using (var stream = new FileStream(anteMaloiPath, FileMode.Open, FileAccess.Read))
                    {
                        var tempImage = Image.FromStream(stream);
                        anteMaloiImage = new Bitmap(tempImage);
                        tempImage.Dispose();
                    }
                    using (var stream = new FileStream(taoLangPath, FileMode.Open, FileAccess.Read))
                    {
                        var tempImage = Image.FromStream(stream);
                        taoLangImage = new Bitmap(tempImage);
                        tempImage.Dispose();
                    }
                    using (var stream = new FileStream(backgroundPath, FileMode.Open, FileAccess.Read))
                    {
                        var tempImage = Image.FromStream(stream);
                        backgroundImage = new Bitmap(tempImage);
                        tempImage.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    // Clean up any images that were loaded before the error
                    if (oaSorcererImage != null) { oaSorcererImage.Dispose(); oaSorcererImage = null; }
                    if (anteMaloiImage != null) { anteMaloiImage.Dispose(); anteMaloiImage = null; }
                    if (taoLangImage != null) { taoLangImage.Dispose(); taoLangImage = null; }
                    if (backgroundImage != null) { backgroundImage.Dispose(); backgroundImage = null; }
                    
                    throw new Exception($"Error loading character images: {ex.Message}");
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

        private void InitializeComponent()
        {
            this.Text = "Battle!";
            this.MinimumSize = new Size(MIN_FORM_WIDTH, MIN_FORM_HEIGHT);
            this.Size = new Size(1200, 900);  // Larger initial size
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Resize += BattleForm_Resize;

            // Set up health labels with custom styling and anchoring
            lblHealth1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblHealth1.AutoSize = true;
            lblHealth1.Font = new Font("Segoe UI", 16, FontStyle.Bold);  // Increased from 12
            lblHealth1.ForeColor = Color.FromArgb(220, 255, 220);
            lblHealth1.BackColor = Color.FromArgb(40, 40, 40);  // Changed from transparent
            lblHealth1.Padding = new Padding(10, 5, 10, 5);  // Add padding

            lblHealth2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblHealth2.AutoSize = true;
            lblHealth2.Font = new Font("Segoe UI", 16, FontStyle.Bold);  // Increased from 12
            lblHealth2.ForeColor = Color.FromArgb(220, 255, 220);
            lblHealth2.BackColor = Color.FromArgb(40, 40, 40);  // Changed from transparent
            lblHealth2.Padding = new Padding(10, 5, 10, 5);  // Add padding

            // Set up character picture boxes with anchoring
            picPlayer1.Anchor = AnchorStyles.None;
            picPlayer1.Width = 150;    // Smaller size
            picPlayer1.Height = 150;   // Smaller size
            picPlayer1.BorderStyle = BorderStyle.None;
            picPlayer1.BackColor = Color.Transparent;
            picPlayer1.SizeMode = PictureBoxSizeMode.Normal;
            picPlayer1.Paint += PicPlayer1_Paint;

            picPlayer2.Anchor = AnchorStyles.None;
            picPlayer2.Width = 150;    // Smaller size
            picPlayer2.Height = 150;   // Smaller size
            picPlayer2.BorderStyle = BorderStyle.None;
            picPlayer2.BackColor = Color.Transparent;
            picPlayer2.SizeMode = PictureBoxSizeMode.Normal;
            picPlayer2.Paint += PicPlayer2_Paint;

            // Set up battle log with anchoring at bottom
            txtLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Height = 100;
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.None;
            txtLog.ReadOnly = true;
            txtLog.BorderStyle = BorderStyle.None;
            txtLog.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            txtLog.BackColor = Color.FromArgb(40, 40, 40);  // Changed from transparent to dark color
            txtLog.ForeColor = Color.FromArgb(255, 255, 200);
            txtLog.TextAlign = HorizontalAlignment.Center;

            UpdateControlPositions();

            Controls.Add(lblHealth1);
            Controls.Add(lblHealth2);
            Controls.Add(picPlayer1);
            Controls.Add(picPlayer2);
            Controls.Add(txtLog);

            this.Paint += BattleForm_Paint;
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Update attack animation
            if (isAttacking)
            {
                currentAttackFrame++;
                if (currentAttackFrame >= ATTACK_ANIMATION_FRAMES)
                {
                    currentAttackFrame = 0;
                    isAttacking = false;
                    player1Attacking = false;
                    player2Attacking = false;
                }
            }

            // Update background parallax
            backgroundOffset -= BACKGROUND_SCROLL_SPEED;
            if (backgroundImage != null)
            {
                float scaleRatio = Math.Max(
                    (float)ClientSize.Width / backgroundImage.Width,
                    (float)ClientSize.Height / backgroundImage.Height
                );
                int scaledWidth = (int)(backgroundImage.Width * scaleRatio);
                
                // Reset offset when a full width has scrolled
                if (Math.Abs(backgroundOffset) >= scaledWidth)
                {
                    backgroundOffset = 0;
                }
            }

            // Update all active effects
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].Update();
                if (activeEffects[i].IsComplete)
                {
                    activeEffects.RemoveAt(i);
                }
            }

            picPlayer1.Invalidate();
            picPlayer2.Invalidate();
            this.Invalidate(false);
        }

        private void PicPlayer1_Paint(object? sender, PaintEventArgs e)
        {
            if (player1 != null)
            {
                DrawCharacter(e.Graphics, picPlayer1.ClientRectangle, player1.Name, player1Attacking, true, player1);
            }
        }

        private void PicPlayer2_Paint(object? sender, PaintEventArgs e)
        {
            if (player2 != null)
            {
                DrawCharacter(e.Graphics, picPlayer2.ClientRectangle, player2.Name, player2Attacking, false, player2);
            }
        }

        private void BattleForm_Paint(object? sender, PaintEventArgs e)
        {
            if (backgroundImage != null)
            {
                // Enable high quality rendering for background
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Calculate background scaling to cover the form while maintaining aspect ratio
                float scaleRatio = Math.Max(
                    (float)ClientSize.Width / backgroundImage.Width,
                    (float)ClientSize.Height / backgroundImage.Height
                );

                int scaledWidth = (int)(backgroundImage.Width * scaleRatio);
                int scaledHeight = (int)(backgroundImage.Height * scaleRatio);

                // Center the background
                int x = (ClientSize.Width - scaledWidth) / 2;
                int y = (ClientSize.Height - scaledHeight) / 2;

                // Draw the background with slight parallax effect
                e.Graphics.DrawImage(backgroundImage,
                    new Rectangle(x + (int)backgroundOffset, y, scaledWidth, scaledHeight),
                    new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height),
                    GraphicsUnit.Pixel);

                // Draw a second copy for seamless scrolling
                e.Graphics.DrawImage(backgroundImage,
                    new Rectangle(x + scaledWidth + (int)backgroundOffset, y, scaledWidth, scaledHeight),
                    new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height),
                    GraphicsUnit.Pixel);

                // Reset graphics settings for pixel art
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            }

            // Draw semi-transparent dark overlay for better contrast
            using (var overlay = new SolidBrush(Color.FromArgb(120, 0, 0, 20)))
            {
                e.Graphics.FillRectangle(overlay, ClientRectangle);
            }

            // Draw all active effects
            foreach (var effect in activeEffects)
            {
                effect.Draw(e.Graphics);
            }

            // Draw winner announcement if game is over
            if (winnerName != null)
            {
                // Draw dark overlay
                using (var darkOverlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(darkOverlay, ClientRectangle);
                }

                // Draw winner text with glow effect
                string winText = $"{winnerName} is VICTORIOUS!";
                
                // Use a size that scales with the form width
                float fontSize = ClientSize.Width * 0.05f; // 5% of form width
                using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
                {
                    // Measure text for centering
                    SizeF textSize = e.Graphics.MeasureString(winText, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;

                    // Draw glow effect
                    using (var glowBrush = new SolidBrush(Color.FromArgb(150, 255, 215, 0)))
                    using (var path = new GraphicsPath())
                    {
                        path.AddString(winText, font.FontFamily, (int)font.Style, font.Size,
                            new PointF(x, y), StringFormat.GenericDefault);
                        
                        // Draw outer glow
                        for (int i = 5; i >= 0; i--)
                        {
                            using (var pen = new Pen(Color.FromArgb(50, 255, 215, 0), i * 2))
                            {
                                e.Graphics.DrawPath(pen, path);
                            }
                        }

                        // Draw text
                        e.Graphics.FillPath(glowBrush, path);
                    }

                    // Draw solid text
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        e.Graphics.DrawString(winText, font, textBrush, x, y);
                    }
                }
            }
        }

        private void DrawCharacter(Graphics g, Rectangle bounds, string name, bool isAttacking, bool isRightSide, ClassFighter? fighter)
        {
            if (g == null || bounds.Width <= 0 || bounds.Height <= 0 || fighter == null) return;

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                // If attacking, add a subtle lunging effect
                int attackOffsetX = isAttacking ? (isRightSide ? -10 : 10) : 0;
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

        private Image? GetCharacterImage(ClassFighter? fighter)
        {
            if (fighter is AnteMaloiCrusader)
            {
                return anteMaloiImage;
            }
            else if (fighter is TaoLangAdventurer)
            {
                return taoLangImage;
            }
            else if (fighter is TheOASorcerer)
            {
                return oaSorcererImage;
            }
            return null;
        }

        private (int x, int y, int imageWidth, int imageHeight) CalculateCharacterDimensions(Rectangle bounds, Image? characterImage, int attackOffsetX)
        {
            if (characterImage == null) return (0, 0, 0, 0);

            // Calculate optimal scale to show full character
            float baseScale = Math.Max(0.1f, Math.Min(
                (float)(bounds.Width - 20) / characterImage.Width,
                (float)(bounds.Height - 20) / characterImage.Height
            )) * 1.0f;

            // Round scale to nearest 0.1 to maintain pixel perfection while preventing zero size
            float scale = (float)Math.Round(baseScale * 10) / 10;
            scale = Math.Max(0.1f, scale);

            // Calculate dimensions with minimum size enforcement
            int imageWidth = Math.Max(1, (int)(characterImage.Width * scale));
            int imageHeight = Math.Max(1, (int)(characterImage.Height * scale));

            // Ensure minimum dimensions
            imageWidth = Math.Max(10, Math.Min(imageWidth, bounds.Width - 4));
            imageHeight = Math.Max(10, Math.Min(imageHeight, bounds.Height - 4));

            // Center horizontally and align to bottom with padding
            int x = bounds.X + (bounds.Width - imageWidth) / 2 + attackOffsetX;
            int y = bounds.Y + bounds.Height - imageHeight - 5;

            return (x, y, imageWidth, imageHeight);
        }

        private void DrawCharacterImage(Graphics g, Image? characterImage, int x, int y, int imageWidth, int imageHeight, bool isRightSide, bool isAttacking)
        {
            if (characterImage == null) return;

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

        private void DrawHealthBar(Graphics g, Rectangle bounds, ClassFighter? fighter)
        {
            if (fighter == null) return;

            float healthPercent = (float)fighter.Health / Math.Max(1, fighter.MaxHealth);
            int barWidth = Math.Max(20, bounds.Width / 2);
            int barHeight = Math.Max(8, bounds.Height / 20);
            int barX = bounds.X + (bounds.Width - barWidth) / 2;
            int barY = bounds.Y + 5;

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

                    int healthWidth = Math.Max(1, (int)(barWidth * healthPercent));
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
        }

        private void DrawCharacterName(Graphics g, int baseX, int baseY, string name)
        {
            // Draw name with custom styling
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

        private async void StartBattle()
        {
            try
            {
                player1 = CreateCharacter(char1Type, player1Name);
                player2 = CreateCharacter(char2Type, player2Name);
                UpdateHealthLabels();
                txtLog.Clear();
                animationTimer.Interval = 50; // Make animation smoother
                animationTimer.Start();
                await Task.Delay(500);
                Random rand = new Random();
                bool player1Turn = rand.Next(0, 2) == 0;

                // Battle start message
                Log("⚔️ Battle Begins! ⚔️");
                Log($"🎲 {(player1Turn ? player1.Name : player2.Name)} goes first!");
                Log("");

                while (player1!.Health > 0 && player2!.Health > 0)
                {
                    try
                    {
                        int damage;
                        isAttacking = true;
                        currentAttackFrame = 0;
                        player1Attacking = player1Turn;
                        player2Attacking = !player1Turn;

                        ClassFighter attacker = player1Turn ? player1 : player2;
                        ClassFighter defender = player1Turn ? player2 : player1;

                        damage = attacker.Attack();
                        defender.TakeDamage(damage);

                        // Set damage number position
                        Rectangle bounds = player1Turn ? picPlayer2.Bounds : picPlayer1.Bounds;
                        Point damageNumberPosition = new Point(
                            bounds.X + bounds.Width / 2,
                            bounds.Y + bounds.Height / 3  // Higher position for better visibility
                        );

                        // Add hit effect and damage number
                        activeEffects.Add(new ClassmatesRPG.HitEffect(damageNumberPosition));
                        activeEffects.Add(new ClassmatesRPG.DamageNumberEffect(damageNumberPosition, damage));

                        Log($"🗡️ {attacker.Name} strikes {defender.Name} for {damage} damage!");
                        UpdateHealthLabels();
                        player1Turn = !player1Turn;
                        
                        await Task.Delay(800); // Attack animation duration
                        isAttacking = false;
                        player1Attacking = false;
                        player2Attacking = false;
                        await Task.Delay(400); // Pause between attacks
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Error during battle turn: {ex.Message}");
                        await Task.Delay(1000); // Brief pause after error
                        continue; // Try to continue battle
                    }
                }

                animationTimer.Stop();
                winnerName = player1.Health > 0 ? player1.Name : player2.Name;
                Log("");
                Log($"🏆 {winnerName} is victorious! 🏆");
                this.Invalidate(); // Trigger repaint for winner announcement
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical battle error: {ex.Message}", "Battle Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close(); // Close form on critical error
            }
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(message)));
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void UpdateHealthLabels()
        {
            if (lblHealth1.InvokeRequired || lblHealth2.InvokeRequired)
            {
                this.Invoke(new Action(UpdateHealthLabels));
                return;
            }

            if (player1 != null && player2 != null)
            {
                lblHealth1.Text = $"{player1.Name}\nHP: {player1.Health}/{player1.MaxHealth}";
                lblHealth2.Text = $"{player2.Name}\nHP: {player2.Health}/{player2.MaxHealth}";
            }
        }

        private ClassFighter CreateCharacter(string characterType, string name)
        {
            return characterType switch
            {
                "The Tao Lang Adventurer" => new TaoLangAdventurer(name),
                "Ante Maloi Crusader" => new AnteMaloiCrusader(name),
                "The OA Sorcerer" => new TheOASorcerer(name),
                _ => throw new Exception($"Unknown champion type: {characterType}")
            };
        }

        private void UpdateControlPositions()
        {
            // Calculate character box size based on form height (90%)
            int characterSize = (int)(ClientSize.Height * 0.90);
            
            // Update PictureBox sizes
            picPlayer1.Width = characterSize;
            picPlayer1.Height = characterSize;
            picPlayer2.Width = characterSize;
            picPlayer2.Height = characterSize;

            // Calculate vertical position to center characters
            int centerY = (ClientSize.Height - characterSize) / 2;
            int spacing = 20;  // Minimal spacing for larger characters

            // Position health labels with better visibility
            lblHealth1.Location = new Point(spacing * 2, spacing * 2);
            lblHealth2.Location = new Point(ClientSize.Width - lblHealth2.Width - spacing * 2, spacing * 2);

            // Position character boxes with more space
            picPlayer1.Location = new Point(
                spacing * 3,  // More space from left
                centerY - 50  // Less upward shift
            );

            picPlayer2.Location = new Point(
                ClientSize.Width - characterSize - spacing * 3,  // More space from right
                centerY - 50  // Less upward shift
            );

            // Position battle log at bottom
            txtLog.Width = ClientSize.Width - spacing * 4;  // Full width with margins
            txtLog.Location = new Point(
                spacing * 2,  // Left margin
                ClientSize.Height - txtLog.Height - spacing * 2  // Bottom position with margin
            );
        }

        private void BattleForm_Resize(object? sender, EventArgs e)
        {
            UpdateControlPositions();
            this.Invalidate();  // Redraw the form
        }
    }
}