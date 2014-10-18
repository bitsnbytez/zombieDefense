using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    public class MortarBallistic : Ballistic
    {
        Animation explode;
        AnimatedSpriteEntity effect;
        Texture2D bullet;
        Vector2 direction;
        Vector2 destination;
        bool exploded;

        public MortarBallistic(ZombieDefense game, Entity from, Entity to)
            : base(game, from, to)
        {
            this.effect = new AnimatedSpriteEntity(game, 1);
            this.explode = new Animation(game.LoadTexture("entity/mine/explode"), 0.06f, Animation.AnimationType.RunOnce, true);
            this.explode.EndAnimation += new Animation.OnEndAnimation(explode_EndAnimation);
            this.bullet = game.LoadTexture("entity/mortar/bullet");
            this.effect.SetAnimation(explode);
            this.effect.SetPositionCenter(to.Position);
            this.exploded = true;
            this.position = from.Position;
            this.destination = effect.Center;
            this.direction = destination - position;
            this.direction.Normalize();
        }

        void explode_EndAnimation(Animation sender, AnimatedSpriteEntity entity, GameTime gameTime)
        {
            List<Entity> nearby = new List<Entity>();
            game.ListNearby(effect.Position, 150, nearby);
            for (int i = 0; i < nearby.Count; ++i)
                nearby[i].Damage(gameTime, 25, this);

            Alive = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            position += direction * 400f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!exploded)
            {
                float distance = (destination - position).Length();
                if (distance < 10)
                {
                    exploded = true;
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;
            if (!exploded)
                spriteBatch.Draw(bullet, position, null, Color.Red, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1.0f); 
            else
                effect.Draw(gameTime, spriteBatch);
        }
    }
}
