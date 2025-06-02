using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ClassmatesRPG
{
    public abstract class VisualEffect
    {
        public Point Position { get; protected set; }
        public bool IsComplete { get; protected set; }
        protected float lifetime = 1.0f;
        protected float currentTime = 0.0f;
        
        public abstract void Update();
        public abstract void Draw(Graphics g);
        
        protected Color InterpolateColor(Color start, Color end, float progress)
        {
            return Color.FromArgb(
                (int)(start.A + (end.A - start.A) * progress),
                (int)(start.R + (end.R - start.R) * progress),
                (int)(start.G + (end.G - start.G) * progress),
                (int)(start.B + (end.B - start.B) * progress)
            );
        }
    }

    public class HitEffect : VisualEffect
    {
        private float size = 5;
        private const float MAX_SIZE = 40;
        private const float EXPAND_SPEED = 2.5f;
        private readonly Color startColor = Color.FromArgb(255, 255, 220, 100);
        private readonly Color endColor = Color.FromArgb(0, 255, 100, 0);

        public HitEffect(Point position)
        {
            this.Position = position;
            this.lifetime = 0.5f;
        }

        public override void Update()
        {
            size += EXPAND_SPEED;
            currentTime += 0.05f;
            IsComplete = currentTime >= lifetime;
        }

        public override void Draw(Graphics g)
        {
            float progress = Math.Min(1.0f, currentTime / lifetime);
            Color currentColor = InterpolateColor(startColor, endColor, progress);
            
            using (var path = new GraphicsPath())
            {
                // Create a starburst pattern
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * (float)(Math.PI / 4);
                    float x = Position.X + (float)Math.Cos(angle) * size;
                    float y = Position.Y + (float)Math.Sin(angle) * size;
                    
                    if (i == 0)
                        path.StartFigure();
                    path.AddLine(Position.X, Position.Y, x, y);
                }
                path.CloseFigure();

                // Draw outer glow
                for (int i = 3; i >= 0; i--)
                {
                    using (var pen = new Pen(Color.FromArgb(
                        (int)(currentColor.A * 0.3f), 
                        currentColor.R,
                        currentColor.G,
                        currentColor.B), 2 + i))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Draw fill
                using (var brush = new SolidBrush(Color.FromArgb(
                    (int)(currentColor.A * 0.5f),
                    currentColor.R,
                    currentColor.G,
                    currentColor.B)))
                {
                    g.FillPath(brush, path);
                }
            }
        }
    }

    public class DamageNumberEffect : VisualEffect
    {
        private readonly int damage;
        private float yOffset = 0;
        private float xOffset = 0;
        private const float RISE_SPEED = 3.0f;
        private const float WOBBLE_AMOUNT = 2.0f;
        private readonly Color criticalColor = Color.FromArgb(255, 255, 50, 50);
        private readonly Color normalColor = Color.FromArgb(255, 255, 255, 200);
        private readonly bool isCritical;

        public DamageNumberEffect(Point position, int damage, bool isCritical = false)
        {
            this.Position = position;
            this.damage = damage;
            this.isCritical = isCritical;
            this.lifetime = 1.0f;
        }

        public override void Update()
        {
            yOffset -= RISE_SPEED;
            xOffset = (float)Math.Sin(currentTime * 10) * WOBBLE_AMOUNT;
            currentTime += 0.05f;
            IsComplete = currentTime >= lifetime;
        }

        public override void Draw(Graphics g)
        {
            float progress = Math.Min(1.0f, currentTime / lifetime);
            Color baseColor = isCritical ? criticalColor : normalColor;
            Color currentColor = Color.FromArgb(
                (int)(baseColor.A * (1.0f - progress)),
                baseColor.R,
                baseColor.G,
                baseColor.B);

            float scale = isCritical ? 1.5f : 1.0f;
            if (progress < 0.2f) scale *= progress * 5; // Pop-in effect
            
            using (var font = new Font("Arial", 16 * scale, FontStyle.Bold))
            {
                string text = damage.ToString();
                if (isCritical) text += "!";
                
                var size = g.MeasureString(text, font);
                float x = Position.X - size.Width / 2 + xOffset;
                float y = Position.Y + yOffset;

                // Draw outline
                using (var outlineBrush = new SolidBrush(Color.FromArgb(
                    currentColor.A,
                    0, 0, 0)))
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            g.DrawString(text, font, outlineBrush,
                                x + dx,
                                y + dy);
                        }
                    }
                }

                // Draw text with glow
                using (var glowBrush = new SolidBrush(Color.FromArgb(
                    (int)(currentColor.A * 0.5f),
                    255, 255, 200)))
                {
                    g.DrawString(text, font, glowBrush,
                        x, y);
                }

                // Draw main text
                using (var brush = new SolidBrush(currentColor))
                {
                    g.DrawString(text, font, brush,
                        x, y);
                }
            }
        }
    }
} 