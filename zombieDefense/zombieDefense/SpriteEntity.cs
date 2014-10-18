using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
    public class SpriteEntity : Entity
    {
        protected Texture2D sprite;
        protected Vector2 centerOffset;
        protected float gutterScale;

        public SpriteEntity(ZombieDefense game, int maxHp, string spriteName)
            : base(game, maxHp)
        {
            this.sprite = game.LoadTexture(spriteName);
            this.gutterScale = 1.0f;
        }

        public Texture2D Sprite { get { return sprite; } }
        public override Vector2 Center
        {
            get
            {
                Vector2 pos;
                pos.X = position.X + centerOffset.X + sprite.Width / 2;
                pos.Y = position.Y + centerOffset.Y + sprite.Height / 2;
                return pos;
            }
        }

        public override Rectangle Rectangle
        {
            get { return new Rectangle((int)position.X, (int)position.Y, sprite.Width, sprite.Height); }
        }

        public override Rectangle GutterRectangle
        {
            get 
            { 
                Rectangle r = Rectangle;
                r.Width = (int)(r.Width * gutterScale);
                r.Height = (int)(r.Height * gutterScale);
                return r;
            } 
        }

        public float GutterScale
        {
            get
            {
                if (game.EntityInPlayField(this))
                    return 1.0f;
                return gutterScale;
            }
        }

        public override void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, position, null, GetGutterColor(gameTime), 0.0f, Vector2.Zero, GutterScale, SpriteEffects.None, GutterDepth);
            base.DrawGutter(gameTime, spriteBatch);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            spriteBatch.Draw(sprite, position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, Depth);
        }
    }
}
