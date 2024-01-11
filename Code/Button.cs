using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Raycast_Game
{
    class Button : Drawable
    {
        public RectangleShape rect;
        public Texture texture;

        private float shade = 1;
        private Color color;

        public Button(RectangleShape rect, Texture texture)
        {
            this.rect = rect;
            this.texture = texture;

            rect.Texture = texture;
            color = rect.FillColor;
        }

        public void Update()
        {
            if (isPressed())
                shade = 0.1f;
            else if (isHovered())
                shade = 0.2f;
            else
                shade = 1;
            rect.FillColor = color * new Color((byte)Math.Clamp(shade * 1000, 1, 255),
                                               (byte)Math.Clamp(shade * 1000, 1, 255),
                                               (byte)Math.Clamp(shade * 1000, 1, 255));
        }
        public bool isHovered()
        {
            Vector2i mousePos = Mouse.GetPosition();
            if (rect.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                return true;
            return false;
        }
        public bool isPressed()
        {
            Vector2i mousePos = Mouse.GetPosition();
            if (rect.GetGlobalBounds().Contains(mousePos.X, mousePos.Y)
                && Mouse.IsButtonPressed(Mouse.Button.Left))
                return true;
            return false;
        }
        public void Draw(RenderTarget renderTarget, RenderStates renderStates)
        {
            renderTarget.Draw(rect);
        }
    }
}
