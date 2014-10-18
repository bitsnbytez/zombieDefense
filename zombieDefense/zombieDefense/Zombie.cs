using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    public class Zombie : AnimatedSpriteEntity
    {
        private double dmgTime;
        private float speed;
        private Entity target;
        private Animation walk;
        private Animation die;
        private Animation idle;
        private Animation headshot;
        private Animation attack;
        private Animation lastAni;
        private double drawDmg;
        private SpriteFont font;
        private bool fromLeft;

        public Zombie(ZombieDefense game)
            : base(game, 1)
        {
            float anispeed;
            double rnd = game.Random();

            this.hp = game.Random(2, 30);

            if (rnd < 0.3)
            {
                anispeed = 0.2f;
                speed = 30f;
                if ((rnd < 0.01) && (game.EnemyCount > 100))
                    this.hp = game.Random(300, game.EnemyCount * 3);
            }
            else if (rnd < 0.7)
            {
                anispeed = 0.15f;
                speed = 40f;
            }
            else if (rnd < 0.9)
            {
                anispeed = 0.1f;
                speed = 50f;
            }
            else
            {
                anispeed = 0.05f;
                speed = 60f;
                if ((rnd > 0.995) && (game.EnemyCount > 100))
                    this.hp = game.Random(100, game.EnemyCount);
            }

            fromLeft = game.Random() > 0.5f;

            walk = new Animation(game.LoadTexture("zombie/walk"), anispeed, Animation.AnimationType.LoopingRandomStart, false, 0, fromLeft);
            //walk = new Animation(game.LoadTexture("zombie/walk2"), anispeed, Animation.AnimationType.LoopingRandomStart, false, 42, fromLeft);
            die = new Animation(game.LoadTexture("zombie/die1"), 0.15f, Animation.AnimationType.RunOnce, false, 0, fromLeft);
            headshot = new Animation(game.LoadTexture("zombie/die2"), 0.1f, Animation.AnimationType.RunOnce, false, 0, fromLeft);
            attack = new Animation(game.LoadTexture("zombie/attack"), 0.1f, Animation.AnimationType.Looping, false, 0, fromLeft);
            idle = new Animation(game.LoadTexture("zombie/idle"), 0.25f, Animation.AnimationType.Looping, false, 0, fromLeft);
            font = game.LoadFont("towerfont");

            SetAnimation(walk);
            int left;
            if (fromLeft)
                left = -100;
            else 
                left = game.Width + 20;

            position = new Vector2(left, game.Random(game.Height - 150) + 50);
            destination = game.Target.Position;
        }

        public override Vector2 Center 
        { 
            get 
            { 
                if (fromLeft)
                    return new Vector2(position.X + 70, position.Y + 53); 
                else
                    return new Vector2(position.X + 58, position.Y + 53); 
            } 
        }

        public override Entity Clone()
        {
            return new Zombie(game);
        }

        public override Rectangle Rectangle
        {
            get 
            {
                Rectangle rct = base.Rectangle;
                rct.Y += 10;
                rct.Inflate(-45, -35);
                return rct;
            }
        }

        public override float Depth 
        { 
            get 
            {
                if (Alive)
                    return 0.5f;
                return 0.2f; 
            } 
        }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            base.Damage(gameTime, dmg, dmgBy);
            hp -= dmg;
            Alive = hp > 0;
            drawDmg = 1500;
            if (!Alive)
            {
                expires = gameTime.TotalGameTime.TotalMilliseconds + game.Random(10, 6000);
                if (dmgBy is Tower)
                    SetAnimation(headshot);
                else
                    SetAnimation(die);
                target = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if ((target != null) && !target.Alive)
                target = null;

           //drawAngle = (float)Math.Atan2((position.Y - destination.Y), (position.X - destination.X));

            if (Alive && !game.Paused)
            {
                if (target == null)
                    target = game.GetCollidedEntity(this);

                if (target != null)
                {
                    SetAnimation(attack);
                    dmgTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (dmgTime < 0)
                    {
                        dmgTime = 500;
                        target.Damage(gameTime, 1, this);
                        if ((target != null) && !target.Alive)
                            target = null;
                    }
                }

                if ((target == null) && Alive)
                {
                    Vector2 direction = destination - position;
                    direction.Normalize();
                    position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    SetAnimation(walk);
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            drawDmg -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if ((drawDmg > 0) && Alive)
            {
                Vector2 pos = Center;
                pos.Y -= 15;
                game.DrawStringCenter(spriteBatch, font, hp.ToString(), pos, Color.Red);
            }
        }

        public override void SetAnimation(Animation animation)
        {
            base.SetAnimation(animation);
            if (animation != idle)
                lastAni = animation;
        }

        public override void GamePaused(bool paused)
        {
            if (paused && Alive)
                SetAnimation(idle);
            if (!paused && (lastAni != null))
                SetAnimation(lastAni);
        }
    }
}
