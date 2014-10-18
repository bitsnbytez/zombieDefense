using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Simple lazer ballistic. Draws a line between source and target entities.
    /// </summary>
    public class Lazer : Ballistic
    {
        private int shotModifier;

        protected double time;
        protected bool fired;

        public Lazer(ZombieDefense game, int shotModifier, Entity from, Entity to)
            : base(game, from, to)
        {
            this.time = GameParams.LAZER_FIRE_DURATION - (shotModifier * 4);
            this.shotModifier = shotModifier;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Alive)
                return;

            time -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (!fired)
            {
                fired = true;
                int dmg = game.Random(4, 9) + shotModifier;
                Target.Damage(gameTime, dmg, Origin);
            }
            if (time < 0)
                Alive = false;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;
            Color[] pixels = new Color[1];
            Color c = Color.Blue;
            float alpha = MathHelper.Clamp((float)(time / GameParams.LAZER_FIRE_DURATION)-0.2f, 0.0f, 0.8f);
            pixels[0] = c * alpha;

            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
            texture.SetData(pixels);

            game.DrawLine(spriteBatch, texture, Origin.Center, Target.Center, Color.Blue, 2, 0.9f);
        }        
    }
}
