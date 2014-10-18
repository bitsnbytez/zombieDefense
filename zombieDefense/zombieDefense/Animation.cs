using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
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
