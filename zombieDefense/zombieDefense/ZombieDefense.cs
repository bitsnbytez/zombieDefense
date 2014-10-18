using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace zombieDefense
{
    public class ZombieDefense
    {
        public enum GutterEntityStatus
        {
            OK,
            NotAvailable,
            InsufficientCredits,
            MaxLimitReached
        }

        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphicsDeviceManager;
        private ContentManager contentManager;
        private Random random;
        private SortedList<string, Texture2D> sprites;
        private SortedList<string, SpriteFont> fonts;
        private Entity target;
        private List<Entity> entities;
        private List<Entity> enemies;
        private Texture2D emptyTexture;
        private Texture2D circle;
        private MouseState currentMouseState;
        private MouseState lastMouseState;
        private KeyboardState currentKbdState;
        private KeyboardState lastKbdState;
        private double nextWave;
        private SpriteFont font;
        private int totalFrames;
        private double elapsedTime;
        private int fps;
        private int entitiesKilled;
        private int enemiesKilled;
        private int numenemies;
        private int credits;
        private List<Entity> gutterEntities;
        private bool debug;
        private bool gameOver;
        private bool paused;

        public ZombieDefense(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager, ContentManager contentManager)        
        {
            this.sprites = new SortedList<string, Texture2D>();
            this.fonts = new SortedList<string, SpriteFont>();
            this.entities = new List<Entity>();
            this.enemies = new List<Entity>();
            this.gutterEntities = new List<Entity>();
            this.graphicsDevice = graphicsDevice;
            this.graphicsDeviceManager = graphicsDeviceManager;
            this.contentManager = contentManager;
            this.random = new Random(Environment.TickCount);
            this.emptyTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this.emptyTexture.SetData(new[] { Color.White });
            this.font = LoadFont("gamefont");
            this.circle = LoadTexture("circle");
        }

        private void ResetGame()
        {
            LoadContent();
        }

        public void LoadContent()
        {
            UnloadContent();

            entitiesKilled = 0;
            enemiesKilled = 0;
            credits = 0;
            gameOver = false;
            paused = false;

            target = new Brain(this);
            target.SetPosition(new Vector2(Width / 2 - target.Rectangle.Width / 2, Height / 2 - target.Rectangle.Height / 2));
            entities.Add(target);

            AddGutterEntity(new Tower(this, true));
            AddGutterEntity(new MortarTower(this, true));
            AddGutterEntity(new Mine(this, true));
            AddGutterEntity(new Barricade(this, true));
            AddGutterEntity(new AirStrike(this, true));

            for (int i = 0; i < GameParams.INITIAL_ZOMBIE_COUNT; ++i)
                enemies.Add(new Zombie(this));
        }

        public void UnloadContent()
        {
            SpriteEntity.Reset();
            sprites.Clear();
            fonts.Clear();
            entities.Clear();
            enemies.Clear();
            numenemies = 0;
            gutterEntities.Clear();
        }

        public Texture2D LoadTexture(string spriteName)
        {
            Texture2D sprite;
            if (sprites.TryGetValue(spriteName, out sprite))
                return sprite;
            sprite = contentManager.Load<Texture2D>(spriteName);
            sprites.Add(spriteName, sprite);
            return sprite;
        }

        public SpriteFont LoadFont(string spriteFont)
        {
            SpriteFont font;
            if (fonts.TryGetValue(spriteFont, out font))
                return font;
            font = contentManager.Load<SpriteFont>(spriteFont);
            fonts.Add(spriteFont, font);
            return font;
        }

        public int Width { get { return graphicsDevice.Viewport.Width; } }
        public int Height { get { return TotalHeight - GameParams.GUTTER_HEIGHT; } }
        public int TotalHeight { get { return graphicsDevice.Viewport.Height; } }
        public Rectangle PlayField { get { return new Rectangle(0, 0, Width, Height); } }
        public Entity Target { get { return target; } }
        public bool Debug { get { return debug; } }
        public int Credits { get { return credits; } }
        public GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }
        public int EnemyCount { get { return enemies.Count; } }
        public bool GameOver 
        { 
            get 
            { 
                return gameOver; 
            }
            set
            {
                if (gameOver == value)
                    return;
                gameOver = value;
                if (gameOver)
                    enemies.Clear();
            }
        }

        public bool Paused
        {
            get { return paused; }
            set
            {
                if (paused != value)
                {
                    paused = value;
                    for (int i = 0; i < entities.Count; ++i)
                        entities[i].GamePaused(value);
                    for (int i = 0; i < enemies.Count; ++i)
                        enemies[i].GamePaused(value);
                }

            }            
        }

        public int Random(int max)
        {
            return random.Next(max);
        }

        public int Random(int min, int max)
        {
            return random.Next(min, max);
        }

        public double Random()
        {
            return random.NextDouble();
        }

        public bool IsMouseButtonDown(bool left)
        {
            if (left)
                return currentMouseState.LeftButton == ButtonState.Pressed;
            else
                return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public int WithdrawCredits(int amount)
        {
            if (credits < 1)
                return 0;
            if (amount > credits)
                amount = credits;
            credits -= amount;
            return amount;
        }

        public Point MousePosition()
        {
            return new Point(currentMouseState.X, currentMouseState.Y);
        }

        public bool IsKbdButtonPressed(Keys key)
        {
            return currentKbdState.IsKeyDown(key) && !lastKbdState.IsKeyDown(key);
        }

        private void AddGutterEntity(Entity entity)
        {
            int left = 25 + gutterEntities.Count * 200;
            Vector2 pos = new Vector2(left, Height - (entity.GutterRectangle.Height / 2) + (GameParams.GUTTER_HEIGHT / 2));
            entity.SetPosition(pos);
            gutterEntities.Add(entity);
        }

        public void Update(GameTime gameTime, MouseState currentMouseState, MouseState lastMouseState, KeyboardState currentKbdState, KeyboardState lastKbdState)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime >= 1000.0f)
            {
                fps = totalFrames;
                totalFrames = 0;
                elapsedTime = 0;
            }

            this.currentMouseState = currentMouseState;
            this.lastMouseState = lastMouseState;
            this.currentKbdState = currentKbdState;
            this.lastKbdState = lastKbdState;

            if (IsKbdButtonPressed(Keys.R))
                ResetGame();
            if (IsKbdButtonPressed(Keys.D))
                debug = !debug;
            if (IsKbdButtonPressed(Keys.F))
            {
                graphicsDeviceManager.IsFullScreen = !graphicsDeviceManager.IsFullScreen;
                graphicsDeviceManager.ApplyChanges();
            }
            if (IsKbdButtonPressed(Keys.P))
                Paused = !Paused;

            if (!Paused)
            {
                ReanimateTheDead(gameTime);
                nextWave -= gameTime.ElapsedGameTime.TotalSeconds;
                if ((nextWave < 0) && (enemies.Count < GameParams.MAX_ZOMBIE_COUNT))
                {
                    AddEnemies(gameTime);
                    nextWave = GameParams.ZOMBIE_SPAWN_INTERVAL;
                }
            }

            for (int i = 0; i < entities.Count; ++i)
                entities[i].Update(gameTime);
            for (int i = 0; i < enemies.Count; ++i)
                enemies[i].Update(gameTime);
            for (int i = 0; i < gutterEntities.Count; ++i)
                gutterEntities[i].UpdateGutter(gameTime);
        }

        private void ReanimateTheDead(GameTime gameTime)
        {
            if (GameOver)
                return;

            double ms = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < enemies.Count; ++i)
            {
                Entity e = enemies[i];
                if (e.Alive)
                    continue;
                if (e.Expires > ms)
                    continue;
                enemies[i] = e.Clone();
            }
        }

        private void AddEnemies(GameTime gameTime)
        {
            if (GameOver)
                return;

            for (int i = 0; i < GameParams.ZOMBIE_SPAWN_NUMBER; ++i)
                enemies.Add(new Zombie(this));
            numenemies = enemies.Count;
        }

        private void AddEntity(Entity entity)
        {
            for (int i = 1; i < entities.Count; ++i)
            {
                if (entities[i].Alive)
                    continue;
                entities[i] = entity;
                return;
            }
            entities.Add(entity);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            ++totalFrames;
            int activeEntities;

            activeEntities = 0;

            graphicsDevice.Clear(Color.Green);

            for (int i = 0; i < enemies.Count; ++i)
            {
                Entity e = enemies[i];
                if (debug)
                    spriteBatch.Draw(emptyTexture, e.Rectangle, Color.Wheat);
                e.Draw(gameTime, spriteBatch);
            }

            for (int i = 0; i < entities.Count; ++i)
            {
                Entity e = entities[i];
                if (debug && e.Alive)
                    spriteBatch.Draw(emptyTexture, e.Rectangle, Color.Wheat);
                e.Draw(gameTime, spriteBatch);
                if (e.Alive)
                    ++activeEntities;
            }

            DrawGutter(gameTime, spriteBatch);

            if (!GameOver)
            {
                for (int i = 0; i < gutterEntities.Count; ++i)
                {
                    Entity e = gutterEntities[i];
                    if (debug)
                        spriteBatch.Draw(emptyTexture, e.GutterRectangle, Color.Wheat);
                    e.DrawGutter(gameTime, spriteBatch);
                }
            }

            string text = String.Format("fps: {0}  entities: {1} - {2}  enemies: {3:0,0} - {4:0,0}  Credits: ${5:0,0}", 
                fps, 
                activeEntities, 
                entitiesKilled,
                numenemies, 
                enemiesKilled,
                credits);

            spriteBatch.DrawString(font, text, new Vector2(10, 10), Color.White);
        }

        private void DrawGutter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 left = new Vector2(0, graphicsDevice.Viewport.Height - GameParams.GUTTER_HEIGHT);
            Vector2 right = new Vector2(Width, graphicsDevice.Viewport.Height - GameParams.GUTTER_HEIGHT);
            Rectangle gutterRect = new Rectangle(0, graphicsDevice.Viewport.Height + 4, Width, GameParams.GUTTER_HEIGHT - 4);
            DrawLine(spriteBatch, left, right, Color.DarkGray, 4, 0.7f);
            left.Y += 4;
            right.Y += 4;
            DrawLine(spriteBatch, left, right, Color.Black, GameParams.GUTTER_HEIGHT - 4, 0.7f);
        }

        public Entity GetNearbyEnemy(Vector2 position, out float distance, float minSight = 0, float maxSight = float.MaxValue)
        {
            Entity result = null;
            distance = maxSight;

            for (int i = 0; i < enemies.Count; ++i)
            {
                Entity e = enemies[i];
                if (!e.Alive)
                    continue;
                Vector2 p2 = e.Position;
                float d = Vector2.Distance(position, e.Center);
                if ((d >= minSight) && (d < distance))
                {
                    distance = d;
                    result = e;
                }
            }
            return result;
        }

        public void ListNearbyEnemies(Vector2 position, float sight, List<Entity> list)
        {
            for (int i = 0; i < enemies.Count; ++i)
            {
                Entity e = enemies[i];
                if (!e.Alive)
                    continue;
                float d = Vector2.Distance(position, e.Center);
                if (d <= sight)
                    list.Add(e);
            }
        }

        public void ListNearbyEntities(Vector2 position, float sight, List<Entity> list)
        {
            for (int i = 0; i < entities.Count; ++i)
            {
                Entity e = entities[i];
                if (!e.Alive)
                    continue;
                float d = Vector2.Distance(position, e.Center);
                if (d <= sight)
                    list.Add(e);
            }
        }

        public void ListNearby(Vector2 position, float sight, List<Entity> list)
        {
            ListNearbyEnemies(position, sight, list);
            ListNearbyEntities(position, sight, list);
        }

        public Entity GetCollidedEntity(Entity entity)
        {
            Rectangle entityRect = entity.Rectangle;

            for (int i = 0; i < entities.Count; ++i)
            {
                Entity e = entities[i];
                if (!e.Alive)
                    continue;
                if (e.CanDropAnywhere)
                    continue;
                Rectangle eRct = e.Rectangle;
                if (entityRect.Intersects(eRct))
                    return e;
            }
            return null;
        }

        public Entity GetCollidedEnemy(Entity entity)
        {
            Rectangle entityRect = entity.Rectangle;

            for (int i = 0; i < enemies.Count; ++i)
            {
                Entity e = enemies[i];
                if (!e.Alive)
                    continue;
                Rectangle eRct = e.Rectangle;
                if (entityRect.Intersects(eRct))
                    return e;
            }
            return null;
        }

        public void DropNewEntityHere(GameTime gameTime, Entity cloneFrom)
        {
            if (CanDropEntitityInPlayField(cloneFrom))
            {
                credits -= cloneFrom.GutterDetail.CostToUse;
                cloneFrom.GutterDetail.Played(gameTime);
                AddEntity(cloneFrom.Clone());
            }
        }

        public bool CanDropEntitityInPlayField(Entity entity)
        {
            if (!EntityInPlayField(entity))
                return false;
            if (entity.CanDropAnywhere)
                return true;
            if (GetCollidedEntity(entity) != null)
                return false;
            if (GetCollidedEnemy(entity) != null)
                return false;
            return true;
        }

        public GutterEntityStatus CanUseGutterEntity(GameTime gameTime, Entity entity)
        {
            if (entity.GutterDetail.MaxConcurrent > 0)
            {
                if (NumberOfEntitiesOfType(entity.GetType()) >= entity.GutterDetail.MaxConcurrent)
                    return GutterEntityStatus.MaxLimitReached;
            }

            if (entity.GutterDetail.Available > 0)
                return GutterEntityStatus.NotAvailable;

            if (entity.GutterDetail.CostToUse > credits)
                return GutterEntityStatus.InsufficientCredits;

            return GutterEntityStatus.OK;
        }

        public int NumberOfEntitiesOfType(Type type)
        {
            int count = 0;
            for (int i = 0; i < entities.Count; ++i)
            {
                Entity e = entities[i];
                if (!e.Alive)
                    continue;
                if (e.GetType() == type)
                    ++count;
            }
            return count;
        }

        public bool EntityHasLineOfSight(Entity source, Entity target)
        {
            Vector2 pos = source.Center;
            Vector2 tpos = target.Center;
            Vector2 direction = tpos - pos;
            float distance = direction.Length();
            direction.Normalize();
            Rectangle playField = PlayField;

            while (true)
            {
                Point ppos = new Point((int)pos.X, (int)pos.Y);
                for (int i = 0; i < entities.Count; ++i)
                {
                    Entity e = entities[i];
                    if ((e == source) || (e == target) || !e.Alive || !e.LineOfSightInterference)
                        continue;
                    if (e.Rectangle.Contains(ppos))
                        return false;
                }
                pos += direction * 1f;
                if ((pos - tpos).Length() < 1)
                    break;
                if (!playField.Contains(ppos))
                    break;
            }
            return true;
        }

        public bool EntityInPlayField(Entity entity)
        {
            return PlayField.Contains(entity.Rectangle);
        }

        public void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness, float depth)
        {
            DrawLine(spriteBatch, emptyTexture, point1, point2, color, thickness, depth);
        }

        public void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, Color color, float thickness, float depth)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();

            spriteBatch.Draw(texture, point1, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, depth);
        }

        public void DrawStringCenter(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 pos, Color color)
        {
            Vector2 fontDim = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, pos, color, 0, fontDim, 1.0f, SpriteEffects.None, 1.0f);
        }

        public void DrawCircleCenter(SpriteBatch spriteBatch, Vector2 position, float radius, float depth)
        {
            Vector2 pos = position;
            pos.X -= radius / 2;
            pos.Y -= radius / 2;
            DrawCircle(spriteBatch, pos, radius, depth);
        }

        public void DrawCircle(SpriteBatch spriteBatch, Vector2 position, float radius, float depth)
        {
            if (radius < 1)
                return;

            float scale = radius / 100f;
            Vector2 origin = new Vector2(circle.Width / 2f, circle.Height / 2f);

            spriteBatch.Draw(
                circle,
                position,
                null,
                Color.White,
                0.0f,
                origin,
                scale,
                SpriteEffects.None,
                depth);
        }

        public void DrawCircleOutline(SpriteBatch spriteBatch, Vector2 position, float radius, float depth)
        {
            if (radius < 1)
                return;

            float sides = 300f;
            float max = 2 * (float)Math.PI;
            float step = max / sides;
            Vector2 vector, vector0 = Vector2.Zero, vector1 = Vector2.Zero, vector2 = Vector2.Zero;

            for (float theta = 0; theta < max; theta += step)
            {
                vector = new Vector2(radius * (float)Math.Cos((double)theta), radius * (float)Math.Sin((double)theta));
                vector1 = vector2;
                vector2 = vector;
                if ((vector1.X == 0) && (vector1.Y == 0))
                {
                    vector0 = vector2;
                    continue;
                }

                float distance = Vector2.Distance(vector1, vector2);
                float angle = (float)Math.Atan2((double)(vector2.Y - vector1.Y), (double)(vector2.X - vector1.X));

                spriteBatch.Draw(emptyTexture,
                    position + vector1,
                    null,
                    Color.White,
                    angle,
                    Vector2.Zero,
                    new Vector2(distance, 1),
                    SpriteEffects.None,
                    depth);
            }

            {
                vector1 = vector2;
                vector2 = vector0;
                float distance = Vector2.Distance(vector1, vector2);
                float angle = (float)Math.Atan2((double)(vector2.Y - vector1.Y), (double)(vector2.X - vector1.X));

                spriteBatch.Draw(emptyTexture,
                    position + vector1,
                    null,
                    Color.White,
                    angle,
                    Vector2.Zero,
                    new Vector2(distance, 1),
                    SpriteEffects.None,
                    depth);
            }
        }

        internal void EntityKilled(Entity entity)
        {
            if (entity.GetType() == typeof(Zombie))
            {
                ++enemiesKilled;
                credits += 5;
            }
            else if ((entity.GetType() == typeof(Tower)) || (entity.GetType() == typeof(Barricade)) || (entity.GetType() == typeof(MortarTower)))
            {
                ++entitiesKilled;
                credits -= 10;
            }
            GameOver = GameOver || (entity == target);
        }
    }
}
