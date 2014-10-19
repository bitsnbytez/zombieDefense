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
