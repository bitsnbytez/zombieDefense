/*
# Zombie Defence - a zombie survival game
#
# Copyright (C) 2014 Robert Nijkamp
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful, 
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Brain entity. Manages the Metroid brain animation and hit points 
    /// </summary>
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
