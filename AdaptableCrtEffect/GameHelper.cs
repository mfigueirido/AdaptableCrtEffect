using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AdaptableCrtEffect
{
    internal static class GameHelper
    {
        internal static ContentManager Content { get; set; }
        internal static GraphicsDevice GraphicsDevice { get; set; }
        internal static SpriteBatch SpriteBatch { get; set; }
    }
}
