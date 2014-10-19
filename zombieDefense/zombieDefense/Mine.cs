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
    /// Mine entity. Manages entity proximity, contact explosions, and gutter detail.
    /// </summary>
    public class Mine : AnimatedSpriteEntity
    {
        public class MineGutterDetail : GutterDetail
        {
            private double nextPlay;
            private int numPlayed;

            public override int Available { get { return (int)nextPlay; } }

            public override void Played(GameTime gameTime)
            {
                int wait = 10000 + (numPlayed + 1) * 1000;
                nextPlay = wait;
                numPlayed++;
            }

            public override void Update(GameTime gameTime)
            {
                nextPlay -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            public override int MaxConcurrent { get { return 2; } }
        }

        private static MineGutterDetail gutterDetail;

        private Animation armed;
        private Animation explode;
        private bool exploded;

        public Mine(ZombieDefense game, bool inGutter)
            : base(game, 1)        
        {
            if (inGutter)
                gutterDetail = new MineGutterDetail();

            armed = new Animation(game.LoadTexture("entity/mine/base"), 0.1f, Animation.AnimationType.Looping);
            explode = new Animation(game.LoadTexture("entity/mine/explode"), 0.06f, Animation.AnimationType.RunOnce, true);
            explode.EndAnimation += new Animation.OnEndAnimation(explode_EndAnimation);
            SetAnimation(armed);
        }

        void explode_EndAnimation(Animation sender, AnimatedSpriteEntity entity, GameTime gameTime)
        {
            Alive = false;
        }

        public override GutterDetail GutterDetail { get { return gutterDetail; } }

        public override Rectangle Rectangle
        {
            get
            {
                Rectangle rct = base.Rectangle;
                rct.Inflate(-10, 0);
                return rct;
            }
        }

        public override Entity Clone()
        {
            Mine m = new Mine(game, false);
            m.position = position;
            return m;
        }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            if (exploded)
                return;
            exploded = true;
            base.Damage(gameTime, dmg, dmgBy);
            SetAnimation(explode);
            List<Entity> nearby = new List<Entity>();
            game.ListNearby(this.Position, 120, nearby);
            for (int i = 0; i < nearby.Count; ++i)
                nearby[i].Damage(gameTime, 25, this);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;
            base.Draw(gameTime, spriteBatch);
        }
    }
}
