using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    public class Generator : AnimatedSpriteEntity
    {
        private Animation animation;
        private Animation damaged;
        private SpriteFont font;

        public Generator(ZombieDefense game, Vector2 position)
            : base(game, GameParams.GENERATOR_HP)
        {
            this.position = position;
            this.animation = new Animation(game.LoadTexture("entity/generator/base"), 0.1f, Animation.AnimationType.LoopingRandomStart, false, 80);
            this.damaged = new Animation(game.LoadTexture("entity/generator/damaged"), 0.1f, Animation.AnimationType.RunOnce, false, 80);
            this.font = game.LoadFont("towerfont");
            SetAnimation(animation);
        }

        public override Vector2 Center
        {
            get
            {
                Vector2 pos;
                pos.X = position.X + animation.FrameWidth / 2;
                pos.Y = position.Y + animation.FrameHeight / 2;
                return pos;
            }
        }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            hp -= dmg;
            Alive = hp > 0;
            if (!Alive)
            {
                game.EntityKilled(this);
                SetAnimation(damaged);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            Vector2 pos = Center;
            pos.Y += 45;
            game.DrawStringCenter(spriteBatch, font, hp.ToString(), pos, Color.White);
        }
    }
}
