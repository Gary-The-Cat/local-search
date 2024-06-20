using Game.SFML_Text;
using SFML.Graphics;
using SFML.System;

namespace Game.ExtensionMethods
{
    public static class RenderWindowExtensions
    {
        // Draws a sprite with a specified scale and position on the window
        public static void Draw(this RenderWindow window, Sprite sprite, float scale, Vector2f position)
        {
            sprite.Scale = new Vector2f(scale, scale);
            sprite.Position = position;
            window.Draw(sprite);
        }

        // Draws a text string with optional centering on the window
        public static void DrawString(this RenderWindow window, FontText fontText, bool centre = true)
        {
            var text = new Text(fontText.StringText, fontText.Font);
            var textBounds = text.GetLocalBounds();
            var scale = fontText.Scale;
            var textWidth = textBounds.Width * scale;
            var textHeight = textBounds.Height * scale;
            text.Scale = new Vector2f(scale, scale);
            text.FillColor = fontText.TextColour;
            text.OutlineColor = fontText.TextColour;

            if (centre)
            {
                text.Origin = new Vector2f(textWidth / 2, textHeight / 2);
                text.Position = new Vector2f(window.Size.X / 2f, window.Size.Y / 2f);
            }

            window.Draw(text);
        }
    }
}