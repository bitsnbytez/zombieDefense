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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
    /// <summary>
    /// Sprite entity class. Manages the texture sprite and gutter detail.
    /// </summary>
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
