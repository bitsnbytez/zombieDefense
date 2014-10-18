using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    public class Brain : AnimatedSpriteEntity
    {
        private Animation ani;
        private SpriteFont font;

        public Brain(ZombieDefense game)
            : base(game, GameParams.GENERATOR_HP)
        {
            this.ani = new Animation(game.LoadTexture("entity/brain/entity"), 0.1f, Animation.AnimationType.LoopingCylon, false, 127);
            this.font = game.LoadFont("towerfont");
            SetAnimation(ani);
        }

        public override Vector2 Center
        {
            get
            {
                Vector2 pos;
                pos.X = position.X + ani.FrameWidth / 2;
                pos.Y = position.Y + ani.FrameHeight / 2;
                return pos;
            }
        }
        public override float Depth { get { return 0.1f; } }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            hp -= dmg;
            Alive = hp > 0;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;

            base.Draw(gameTime, spriteBatch);

            Vector2 pos = Center;
            pos.Y += 45;
            game.DrawStringCenter(spriteBatch, font, hp.ToString(), pos, Color.White);
        }
    }
}
