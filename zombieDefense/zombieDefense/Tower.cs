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
    /// Tower entity class. Manages target tracking and explosion when within target range.
    /// </summary>
    public class Tower : BaseTurret
    {
        public class TowerGutterDetail : GutterDetail
        {
            private ZombieDefense game;
            private int numPlayed;

            public TowerGutterDetail(ZombieDefense game)
            {
                this.game = game;
            }

            public override int Available { get { return 0; } }

            public override void Played(GameTime gameTime)
            {
                numPlayed++;
            }

            public override int MaxConcurrent { get { return 0; } }

            public override int CostToUse 
            { 
                get 
                {
                    if (numPlayed < 4)
                        return 0;
                    int multi = (numPlayed / 5) + 1;
                    return (numPlayed - 4) * (100 * multi);
                } 
            }
        }

        private static TowerGutterDetail gutterDetail;

        public Tower(ZombieDefense game, bool inGutter)
            : base(game, 110, 200, "entity/turret/base1", inGutter ? "entity/turret/base1" : "entity/turret/base2", "entity/turret/barrel")
        {
            if (inGutter)
                gutterDetail = new TowerGutterDetail(game);
        }

        public override GutterDetail GutterDetail { get { return gutterDetail; } }

        public override Entity Clone()
        {
            Tower t = new Tower(game, false);
            t.position = position;
            t.barrelAngle = barrelAngle;
            return t;
        }

        public override Ballistic CreateBallistic(ZombieDefense game, Entity from, Entity to)
        {
            int modifier = 0;
            if (hp > 100)
                modifier = ((hp - 100) / 25) * 5;
            return new Lazer(game, modifier, from, to);
        }

        public override int MinSightDistance { get { return 0; } }
        public override int MaxSightDistance { get { return 100 + hp; } }
        public override int NextShotInterval { get { return 400; } }
        public override int NumShotDamage { get { return 10; } }
    }
}
