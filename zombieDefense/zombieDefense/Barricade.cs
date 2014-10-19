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
    /// Barricade class. Manages gutter animation and hit points 
    /// </summary>
    public class Barricade : SpriteEntity
    {
        public class BarricadeGutterDetail : GutterDetail
        {
            public override int Available { get { return 0; } }

            public override void Played(GameTime gameTime)
            {
            }

            public override int MaxConcurrent { get { return 6; } }
        }

        private static BarricadeGutterDetail gutterDetail;

        private SpriteFont font;
        private int spriteIdx;

        public Barricade(ZombieDefense game, bool inGutter)
            : base(game, 1000, "entity/barricade/base1")
        {
            if (inGutter)
                gutterDetail = new BarricadeGutterDetail();

            this.hp = 500;
            this.font = game.LoadFont("towerfont");
            this.gutterScale = 0.5f;
        }

        public override GutterDetail GutterDetail { get { return gutterDetail; } }
        public override bool LineOfSightInterference { get { return false; } }
        public override float Depth { get { return 0.1f; } }

        public override Entity Clone()
        {
            Barricade b = new Barricade(game, false);
            b.position = position;
            return b;
        }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            hp -= dmg;
            Alive = hp > 0;
            if ((hp < 250) && (spriteIdx == 0))
            {
                sprite = game.LoadTexture("entity/barricade/base2");
                spriteIdx = 1;
            }
            else if ((hp > 250) && (spriteIdx == 1))
            {
                sprite = game.LoadTexture("entity/barricade/base1");
                spriteIdx = 0;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;

            base.Draw(gameTime, spriteBatch);

            game.DrawStringCenter(spriteBatch, font, hp.ToString(), Center, Color.Black);
        }
    }
}
