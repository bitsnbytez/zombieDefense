using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
    public abstract class BaseTurret : SpriteEntity
    {
        private Ballistic bullet;
        private int numShots;
        private SpriteFont font;
        private int spriteIdx;
        private Texture2D barrel;
        private Vector2 barrelOrigin;
        private double nextShot;
        private double drawArea;
        private Entity dmgBy;
        private string spriteName;
        private string dmgSpriteName;

        protected float barrelAngle;

        public BaseTurret(ZombieDefense game, int hp, int maxHp, string spriteName, string dmgSpriteName, string barrelSpriteName)
            : base(game, maxHp, spriteName)
        {
            this.hp = hp;
            this.font = game.LoadFont("towerfont");
            this.barrel = game.LoadTexture(barrelSpriteName);
            this.barrelOrigin = new Vector2(barrel.Width / 2, barrel.Height / 2);
            this.centerOffset = new Vector2(0, -10);
            this.spriteName = spriteName;
            this.dmgSpriteName = dmgSpriteName;
            this.spriteIdx = -1;
            this.drawArea = 500;
            UpdateSpriteImg();
        }

        public override float Depth { get { return 0.1f; } }

        public override void UpdateGutter(GameTime gameTime)
        {
            base.UpdateGutter(gameTime);

            nextShot -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (nextShot < 0)
            {
                barrelAngle += 0.2f;
                if (barrelAngle > 360)
                    barrelAngle = 0;
                nextShot = 100;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Alive)
                return;

            float distance;

            if ((bullet != null) && !bullet.Alive)
                bullet = null;

            if (!game.Paused)
            {
                nextShot -= gameTime.ElapsedGameTime.TotalMilliseconds;
                drawArea -= gameTime.ElapsedGameTime.TotalMilliseconds;

                if ((bullet == null) && (nextShot < 0))
                {
                    Entity target = null;
                    if (Entity.IsAlive(dmgBy))
                    {
                        target = dmgBy;
                        distance = 0;
                    }
                    else
                        target = game.GetNearbyEnemy(this.position, out distance, MinSightDistance);

                    if (null != target)
                    {
                        if ((distance < MaxSightDistance) && game.EntityHasLineOfSight(this, target))
                        {
                            bullet = CreateBallistic(game, this, target);
                            nextShot = NextShotInterval;
                            if (++numShots == NumShotDamage)
                            {
                                Damage(gameTime, 1, null);
                                numShots = 0;
                            }
                        }
                    }
                    else
                        bullet = null;
                }
            }

            if (bullet != null)
            {
                barrelAngle = (float)Math.Atan2((position.Y - bullet.Target.Center.Y), (position.X - bullet.Target.Center.X));
                bullet.Update(gameTime);
            }
        }

        public abstract Ballistic CreateBallistic(ZombieDefense game, Entity from, Entity to);

        public abstract int MinSightDistance { get; }
        public abstract int MaxSightDistance { get; }
        public abstract int NextShotInterval { get; }
        public abstract int NumShotDamage { get; }

        public override void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawGutter(gameTime, spriteBatch);
            DrawBarrel(spriteBatch, GetGutterColor(gameTime));
            if (game.EntityInPlayField(this))
                game.DrawCircle(spriteBatch, Center, MaxSightDistance, 0.05f);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Alive)
                return;

            base.Draw(gameTime, spriteBatch);

            if (bullet != null)
                bullet.Draw(gameTime, spriteBatch);

            DrawBarrel(spriteBatch, Color.White);

            Vector2 pos = Center;
            pos.Y += 25;
            game.DrawStringCenter(spriteBatch, font, hp.ToString(), pos, Color.White);

            if (drawArea > 0)
                game.DrawCircle(spriteBatch, Center, MaxSightDistance, 0.05f);
        }

        private void DrawBarrel(SpriteBatch spriteBatch, Color color)
        {
            if (bullet != null)
            {
                Vector2 target = bullet.Target.Center;
                target.X -= barrel.Width / 2;
                target.Y -= barrel.Height / 2;
                barrelAngle = (float)Math.Atan2((position.Y - target.Y), (position.X - target.X));
            }
            spriteBatch.Draw(barrel, Center, null, color, barrelAngle, barrelOrigin, 1.0f, SpriteEffects.None, 1f);
        }

        private void UpdateSpriteImg()
        {
            if ((hp < MaxHp / 2) && (spriteIdx != 1))
            {
                sprite = game.LoadTexture(dmgSpriteName);
                spriteIdx = 1;
            }
            else if ((hp > MaxHp / 2) && (spriteIdx != 0))
            {
                sprite = game.LoadTexture(spriteName);
                spriteIdx = 0;
            }
        }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            if (dmgBy != null)
                this.dmgBy = dmgBy;
            if (dmg < 1)
                drawArea = 500;
            hp -= dmg;
            Alive = hp > 0;
            UpdateSpriteImg();
        }
    }
}
