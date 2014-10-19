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
    /// Animation class used to manage animation frames, loops, direction, and timing.
    /// </summary>
    public class Animation
    {
        public delegate void OnEndAnimation(Animation sender, AnimatedSpriteEntity entity, GameTime gameTime);

        public enum AnimationType
        {
            Looping,
            LoopingRandomStart,
            LoopingCylon,
            RunOnce,
            RunOnceAutoDelete
        }

        private int frameWidth;
        private Vector2 origin;
        private Texture2D texture;
        private bool flipSprite;
        private float frameTime;
        private AnimationType animationType;

        public event OnEndAnimation EndAnimation;

        public Animation(Texture2D texture, float frameTime, AnimationType animationType, bool centerOrigin = false, int frameWidth = 0, bool flipSprite = false)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.animationType = animationType;
            this.frameWidth = frameWidth;
            this.flipSprite = flipSprite;
            if (0 == this.frameWidth)
                this.frameWidth = Texture.Height;
            if (centerOrigin)
                this.origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);
            else
                this.origin = Vector2.Zero;
        }

        public void AnimationEnded(AnimatedSpriteEntity entity, GameTime gameTime)
        {
            if (EndAnimation != null)
                EndAnimation(this, entity, gameTime);
        }

        public Texture2D Texture { get { return texture; } }
        public float FrameTime { get { return frameTime; } }
        public AnimationType AniType { get { return animationType; } }
        public int FrameCount { get { return Texture.Width / frameWidth; } }
        public int FrameWidth { get { return frameWidth; } }
        public int FrameHeight { get { return Texture.Height; } }
        public Vector2 Origin { get { return origin; } }
        public bool FlipSprite { get { return flipSprite; } }
    }
}
