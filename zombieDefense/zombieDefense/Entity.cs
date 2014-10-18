using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Base entity class. Manages mouse interaction, hit points, life, and gutter control.
    /// </summary>
    public class Entity
    {
        private static Entity mouseCaptured;
        private bool alive;
        private double nextHpPull;
        private Vector2 origionalPos;
        private bool mouseLocked;

        protected ZombieDefense game;
        protected Vector2 position;
        protected Vector2 destination;
        protected double expires;
        protected int hp;
        protected int maxHp;

        public Entity(ZombieDefense game, int maxHp)
        {
            this.game = game;
            this.destination = Vector2.Zero;
            this.alive = true;
            this.maxHp = maxHp;
            this.hp = this.maxHp;
        }

        public static void Reset()
        {
            mouseCaptured = null;
        }

        public static bool IsAlive(Entity entity)
        {
            return (entity != null) && entity.alive;
        }

        public Vector2 Position { get { return position; } }
        public Vector2 Destination { get { return destination; } }
        public bool Alive 
        { 
            get 
            {
                return alive; 
            }
            set
            {
                if (alive == value)
                    return;
                alive = value;
                if (!alive)
                    game.EntityKilled(this);
            }
        }
        public virtual Vector2 Center { get { return position; } }
        public virtual GutterDetail GutterDetail { get { return null; } }
        public virtual bool LineOfSightInterference { get { return true; } }
        public virtual float Depth { get { return 0.01f; } }
        public virtual float GutterDepth { get { return 0.8f; } }
        public virtual bool CanDropAnywhere { get { return false; } }
        public double Expires { get { return expires; } }
        public int MaxHp { get { return maxHp; } }

        public virtual Rectangle Rectangle
        {
            get { return new Rectangle((int)position.X, (int)position.Y, 1, 1); }
        }

        public virtual Rectangle GutterRectangle { get { return Rectangle; } }

        public virtual Entity Clone()
        {
            return null;
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        public void SetPositionCenter(Vector2 position)
        {
            Rectangle r = Rectangle;
            this.position.X = position.X + r.Width / 2;
            this.position.Y = position.Y + r.Height / 2;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!Alive)
                return;

            Rectangle r = Rectangle;
            if (Rectangle.Contains(game.MousePosition()))
            {
                if (game.IsMouseButtonDown(false))
                    Damage(gameTime, int.MaxValue, null);
                else if (game.IsMouseButtonDown(true) && (gameTime.TotalGameTime.TotalMilliseconds > nextHpPull) && (mouseCaptured == null))
                {
                    if (hp < maxHp)
                    {
                        int amount = maxHp - hp;
                        if (amount > 5)
                            amount = 5;
                        int newhp = game.WithdrawCredits (amount);
                        Damage(gameTime, 0 - newhp, null);
                        nextHpPull = gameTime.TotalGameTime.TotalMilliseconds + 100;
                    }
                    else
                        Damage(gameTime, 0, null);
                }
            }
        }

        private void DrawCircle(SpriteBatch spriteBatch, int radius, Color color, float depth)
        {
            float max = 2 * (float)Math.PI;
            float step = max / (float)12;

            Vector2 pt1 = Vector2.Zero;
            for (float theta = 0; theta < max; theta += step)
            {
                Vector2 pt2 = new Vector2(radius * (float)Math.Cos((double)theta), radius * (float)Math.Sin((double)theta));
                pt2 += Center;
                if (pt1 != Vector2.Zero)
                    game.DrawLine(spriteBatch, pt1, pt2, color, 1, depth);
                pt1 = pt2;
            }
        }

        public virtual void UpdateGutter(GameTime gameTime)
        {
            if (!game.Paused)
                GutterDetail.Update(gameTime);

            Rectangle r = GutterRectangle;
            if (!mouseLocked)
            {
                if (game.IsMouseButtonDown(true) && (game.CanUseGutterEntity(gameTime, this) == ZombieDefense.GutterEntityStatus.OK))
                {
                    if ((mouseCaptured != null) && (!mouseCaptured.Alive))
                        mouseCaptured = null;
                    if ((mouseCaptured == null) && r.Contains(game.MousePosition()))
                    {
                        origionalPos = position;
                        mouseLocked = true;
                        mouseCaptured = this;
                    }
                }
            }
            else if (!game.IsMouseButtonDown(true))
                mouseLocked = false;

            if (mouseLocked)
            {
                Point mouse = game.MousePosition();
                position.X = mouse.X - r.Width / 2;
                position.Y = mouse.Y - r.Height / 2;
            }
            else if (mouseCaptured == this)
            {
                mouseCaptured = null;
                game.DropNewEntityHere(gameTime, this);
                position = origionalPos;
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (game.Debug)
                DrawCircle(spriteBatch, 5, Color.Red, 0.01f);
        }

        public virtual void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (mouseCaptured == this)
                return;

            Vector2 pos = Center;
            pos.Y = game.TotalHeight - 15;
            SpriteFont font = game.LoadFont("gutterfont");
            ZombieDefense.GutterEntityStatus status = game.CanUseGutterEntity(gameTime, this);

            switch (status)
            {
                case ZombieDefense.GutterEntityStatus.InsufficientCredits:
                    game.DrawStringCenter(spriteBatch, font, String.Format("$-{0:0,0}", GutterDetail.CostToUse - game.Credits), pos, Color.White);
                    break;
                case ZombieDefense.GutterEntityStatus.MaxLimitReached:
                    game.DrawStringCenter(spriteBatch, font, "Limited", pos, Color.White);
                    break;
                case ZombieDefense.GutterEntityStatus.NotAvailable:
                    TimeSpan wait = new TimeSpan(0, 0, 0, 0, GutterDetail.Available);
                    game.DrawStringCenter(spriteBatch, font, String.Format("{0:D2}s:{1:D3}ms", wait.Seconds, wait.Milliseconds), pos, Color.White);
                    break;
                default:
                    string text = String.Empty;
                    if (GutterDetail.CostToUse > 0)
                        text = String.Format("${0:0,0}", GutterDetail.CostToUse);
                    if (GutterDetail.MaxConcurrent > 0)
                    {
                        if (text.Length > 0)
                            text += " ";
                        text += String.Format("{0}/{1}", game.NumberOfEntitiesOfType(GetType()), GutterDetail.MaxConcurrent);
                    }
                    game.DrawStringCenter(spriteBatch, font, text, pos, Color.White);
                    break;
            }


            if (game.Debug)
                DrawCircle(spriteBatch, 5, Color.Red, 0.01f);
        }

        public virtual void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
        }

        public virtual void GamePaused(bool paused)
        {
        }

        protected Color GetGutterColor(GameTime gameTime)
        {
            Color color = Color.White;

            if (game.EntityInPlayField(this))
            {
                if (!game.CanDropEntitityInPlayField(this))
                    color = Color.Red;
            }
            else if (mouseLocked)
                color = Color.Red;
            else if (game.CanUseGutterEntity(gameTime, this) != ZombieDefense.GutterEntityStatus.OK)
                color = new Color(40, 40, 40);

            return color;
        }
    }
}
