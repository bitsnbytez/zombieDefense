using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Air strike class. Manages gutter animation and explosions.
    /// </summary>
    public class AirStrike : SpriteEntity
    {
        public class AirStrikeGutterDetail : GutterDetail
        {
            public override int Available { get { return 0; } }
            public override int MaxConcurrent { get { return 0; } }

            public override void Played(GameTime gameTime)
            {
            }

            public override int CostToUse { get { return 500; } }
        }

        private static AirStrikeGutterDetail gutterDetail;
        private List<AnimatedSpriteEntity> explosions;
        private int exploded;
        private double dropBomb;

        public AirStrike(ZombieDefense game, bool inGutter)
            : base(game, 1, "entity/airstrike/base")
        {
            if (inGutter)
                gutterDetail = new AirStrikeGutterDetail();
            explosions = new List<AnimatedSpriteEntity>();
            dropBomb = 500;
        }

        public override GutterDetail GutterDetail { get { return gutterDetail; } }
        public override bool CanDropAnywhere { get { return true; } }

        public override Entity Clone()
        {
            AirStrike strike = new AirStrike(game, false);
            strike.position = Position;
            return strike;
        }

        public override void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawGutter(gameTime, spriteBatch);
            if (game.EntityInPlayField(this))
                game.DrawCircle(spriteBatch, Center, 250, 0.05f);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Alive)
                return;

            base.Update(gameTime);

            dropBomb -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if ((dropBomb < 0) && (explosions.Count < GameParams.AIRSTRIKE_BOMBS))
            {
                int rad = 180;
                Vector2 c = Center;

                AnimatedSpriteEntity entity = new AnimatedSpriteEntity(game, 1);
                explosions.Add(entity);
                Animation explode = new Animation(game.LoadTexture("entity/mine/explode"), 0.06f, Animation.AnimationType.RunOnceAutoDelete, true);
                entity.SetAnimation(explode);
                entity.SetPosition(new Vector2(
                    game.Random((int)c.X - rad, (int)c.X + rad),
                    game.Random((int)c.Y - rad, (int)c.Y + rad)
                ));
                explode.EndAnimation += new Animation.OnEndAnimation(explode_EndAnimation);
                dropBomb = game.Random(50, 300);
            }

            for (int i = 0; i < explosions.Count; ++i)
                explosions[i].Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;

            base.Draw(gameTime, spriteBatch);
            if (null == explosions)
                return;
            for (int i = 0; i < explosions.Count; ++i)
                explosions[i].Draw(gameTime, spriteBatch);
        }

        void explode_EndAnimation(Animation sender, AnimatedSpriteEntity entity, GameTime gameTime)
        {
            List<Entity> nearby = new List<Entity>();
            game.ListNearby(entity.Position, 120, nearby);
            for (int i = 0; i < nearby.Count; ++i)
                nearby[i].Damage(gameTime, 25, this);
            if (++exploded == GameParams.AIRSTRIKE_BOMBS)
            {
                explosions.Clear();
                Alive = false;
            }
        }

    }
}
