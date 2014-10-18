using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Animated entity class 
    /// </summary>
    public class AnimatedSpriteEntity : Entity
    {
        private Animation animation;
        private int frameIndex;
        private float time;
        private Color color;
        private int dir;
        private bool ended;
        protected float drawAngle;

        public AnimatedSpriteEntity(ZombieDefense game, int maxHp)
            : base(game, maxHp)
        {
            this.color = Color.White;
            this.drawAngle = 0;
        }

        public override Rectangle Rectangle
        {
            get { return new Rectangle((int)position.X, (int)position.Y, animation.FrameWidth, animation.FrameHeight); }
        }

        public Animation CurrentAnimation { get { return animation; } }

        public override Vector2 Center 
        { 
            get 
            { 
                Rectangle r = Rectangle;
                return new Vector2(r.Left + r.Width / 2, r.Top + r.Height / 2); 
            } 
        }

        public virtual void SetAnimation(Animation animation)
        {
            if (this.animation == animation)
                return;

            this.animation = animation;
            this.frameIndex = 0;
            if (animation.AniType == Animation.AnimationType.LoopingRandomStart)
                this.frameIndex = game.Random(animation.FrameCount);
            this.time = 0.0f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (null == animation)
                return;
            if ((animation.AniType == Animation.AnimationType.RunOnceAutoDelete) && ended)
                return;
            DrawObject(gameTime, spriteBatch, Depth);
            base.Draw(gameTime, spriteBatch);
        }

        private void DrawObject(GameTime gameTime, SpriteBatch spriteBatch, float depth)
        {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > animation.FrameTime)
            {
                time -= animation.FrameTime;
                switch (animation.AniType)
                {
                    case Animation.AnimationType.Looping:
                    case Animation.AnimationType.LoopingRandomStart:
                        frameIndex = (frameIndex + 1) % animation.FrameCount;
                        break;
                    case Animation.AnimationType.LoopingCylon:
                        if (frameIndex == animation.FrameCount - 1)
                            dir = -1;
                        else if (frameIndex == 0)
                            dir = 1;
                        frameIndex += dir;
                        break;
                    case Animation.AnimationType.RunOnce:
                    case Animation.AnimationType.RunOnceAutoDelete:
                        if (!ended && (frameIndex == animation.FrameCount - 1))
                        {
                            animation.AnimationEnded(this, gameTime);
                            ended = true;
                        }
                        frameIndex = Math.Min(frameIndex + 1, animation.FrameCount - 1);
                        break;
                }
            }
            Rectangle source = new Rectangle(frameIndex * animation.FrameWidth, 0, animation.FrameWidth, animation.Texture.Height);
            SpriteEffects effects = SpriteEffects.None;
            if (animation.FlipSprite)
                effects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(animation.Texture, position, source, color, drawAngle, animation.Origin, 1.0f, effects, depth);
        }

        public override void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            color = GetGutterColor(gameTime);
            base.DrawGutter(gameTime, spriteBatch);
            DrawObject(gameTime, spriteBatch, GutterDepth);
        }
    }
}
