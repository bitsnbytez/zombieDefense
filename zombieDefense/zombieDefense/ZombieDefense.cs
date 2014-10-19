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
    /// <summary>
    /// The zombie defense game logic processor. Manages all on screen entities, gutter objects, keyboard and mouse interaction, 
    /// currency, and maintains the magic.
    /// </summary>
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

        /// <summary>
        /// Load sprites, fonts, and initialize new game state
        /// </summary>
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

        /// <summary>
        /// Delete all entities, fonts, and game state
        /// </summary>
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

        /// <summary>
        /// Load a sprite by name from the content manager. 
        /// Loaded sprites are cached for faster retrieval later.
        /// </summary>
        /// <param name="spriteName">Name of sprite</param>
        /// <returns>Sprite texture</returns>
        public Texture2D LoadTexture(string spriteName)
        {
            Texture2D sprite;
            if (sprites.TryGetValue(spriteName, out sprite))
                return sprite;
            sprite = contentManager.Load<Texture2D>(spriteName);
            sprites.Add(spriteName, sprite);
            return sprite;
        }

        /// <summary>
        /// Load a font by name from the content manager. 
        /// Loaded sprites are cached for faster retrieval later.
        /// </summary>
        /// <param name="spriteFont">Name of font</param>
        /// <returns>Sprite font</returns>
        public SpriteFont LoadFont(string spriteFont)
        {
            SpriteFont font;
            if (fonts.TryGetValue(spriteFont, out font))
                return font;
            font = contentManager.Load<SpriteFont>(spriteFont);
            fonts.Add(spriteFont, font);
            return font;
        }

        /// <summary>
        /// Playable game board width
        /// </summary>
        public int Width { get { return graphicsDevice.Viewport.Width; } }

        /// <summary>
        /// Playable game board height
        /// </summary>
        public int Height { get { return TotalHeight - GameParams.GUTTER_HEIGHT; } }

        /// <summary>
        /// Screen height including gutter height
        /// </summary>
        public int TotalHeight { get { return graphicsDevice.Viewport.Height; } }

        /// <summary>
        /// Rectangle representing the playable game board
        /// </summary>
        public Rectangle PlayField { get { return new Rectangle(0, 0, Width, Height); } }

        /// <summary>
        /// The main target in the game. In this case, this would be the Mother Brain from Metroid. Zombies love that stuff.
        /// </summary>
        public Entity Target { get { return target; } }

        /// <summary>
        /// Debug flag. When true will draw the hit box around all entities.
        /// </summary>
        public bool Debug { get { return debug; } }

        /// <summary>
        /// Number of earned / remaining credits available to the player.
        /// </summary>
        public int Credits { get { return credits; } }

        /// <summary>
        /// Current graphics device
        /// </summary>
        public GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

        /// <summary>
        /// Number of enimies in play
        /// </summary>
        public int EnemyCount { get { return enemies.Count; } }

        /// <summary>
        /// Game over flag
        /// </summary>
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

        /// <summary>
        /// Paused flag
        /// </summary>
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

        /// <summary>
        /// Returns a nonnegative random number less than the specified maxValue.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, and less than maxValue</returns>
        public int Random(int maxValue)
        {
            return random.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns></returns>
        public int Random(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
        public double Random()
        {
            return random.NextDouble();
        }

        /// <summary>
        /// Identifies if a mouse button is currently pressed down.
        /// </summary>
        /// <param name="left">True for left, false for right.</param>
        /// <returns>True if the button is pressed</returns>
        public bool IsMouseButtonDown(bool left)
        {
            if (left)
                return currentMouseState.LeftButton == ButtonState.Pressed;
            else
                return currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Withdraws a specified number of credits from the available credits. The available credits will not be overdrawn. 
        /// </summary>
        /// <param name="amount">An amount of credits to withdraw from the account</param>
        /// <returns>The number of credits withdrawn</returns>
        public int WithdrawCredits(int amount)
        {
            if (credits < 1)
                return 0;
            if (amount > credits)
                amount = credits;
            credits -= amount;
            return amount;
        }

        /// <summary>
        /// Identifies the current mouse position
        /// </summary>
        /// <returns>The current mouse position</returns>
        public Point MousePosition()
        {
            return new Point(currentMouseState.X, currentMouseState.Y);
        }

        /// <summary>
        /// Returns true if "key" is currently pressed down. 
        /// If the key is continually pressed, meaning the last keyboard frame also has this key pressed, the key will not be considered pressed. 
        /// </summary>
        /// <param name="key">Keyboard key value</param>
        /// <returns>True if this key was identified as a new key press</returns>
        public bool IsKbdButtonPressed(Keys key)
        {
            return currentKbdState.IsKeyDown(key) && !lastKbdState.IsKeyDown(key);
        }

        /// <summary>
        /// Adds an entity to the gutter. Determins a correct position for the entity given the current contents of the gutter.
        /// </summary>
        /// <param name="entity">The entity to be added to the gutter</param>
        private void AddGutterEntity(Entity entity)
        {
            int left = 25 + gutterEntities.Count * 200;
            Vector2 pos = new Vector2(left, Height - (entity.GutterRectangle.Height / 2) + (GameParams.GUTTER_HEIGHT / 2));
            entity.SetPosition(pos);
            gutterEntities.Add(entity);
        }

        /// <summary>
        /// Update game loop. Allows entities to reclaculate positions, interact with other entities, and process animation frames.
        /// </summary>
        /// <param name="gameTime">Amount of elapsed time since last update. Used to correct for lag.</param>
        /// <param name="currentMouseState">Current mouse state</param>
        /// <param name="lastMouseState">Last mouse state</param>
        /// <param name="currentKbdState">Current keyboard state</param>
        /// <param name="lastKbdState">Last keyboard state</param>
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

        /// <summary>
        /// When a zombie is killed, reanimate the zombie to participate in the next wave. You know... because fun!
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
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

        /// <summary>
        /// Time to add add a new wave of enimies to the game. 
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
        private void AddEnemies(GameTime gameTime)
        {
            if (GameOver)
                return;

            for (int i = 0; i < GameParams.ZOMBIE_SPAWN_NUMBER; ++i)
                enemies.Add(new Zombie(this));
            numenemies = enemies.Count;
        }

        /// <summary>
        /// Adds an entity to the game. Rather than always adding to the list of running entities, make use of some of the resources already allocated.
        /// </summary>
        /// <param name="entity">Entity to add to the game state</param>
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

        /// <summary>
        /// Draw game loop. Draw all active sprites on the screen.
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
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

        /// <summary>
        /// Draw the contents of the gutter in the gutter area. 
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
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

        /// <summary>
        /// Returns an enemy that happens to be within the given sight radius from position.
        /// </summary>
        /// <param name="position">Position of source</param>
        /// <param name="distance">Distance of enemy from source</param>
        /// <param name="minSight">Minimum search distance</param>
        /// <param name="maxSight">Maximum search distance</param>
        /// <returns>The enemy if one was found in the view distance. Otherwise returns null.</returns>
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

        /// <summary>
        /// Populates a list of nearby living enemies within a given sight radius from position.
        /// </summary>
        /// <param name="position">Position of source</param>
        /// <param name="sight">Maximum search distance</param>
        /// <param name="list">List to populate containing entities within the sight radius</param>
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

        /// <summary>
        /// Populates a list of nearby living entities within a given sight radius from position.
        /// </summary>
        /// <param name="position">Position of source</param>
        /// <param name="sight">Maximum search distance</param>
        /// <param name="list">List to populate containing entities within the sight radius</param>
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

        /// <summary>
        /// Populates a list of nearby living entities and enemies within a given sight radius from position.
        /// </summary>
        /// <param name="position">Position of source</param>
        /// <param name="sight">Maximum search distance</param>
        /// <param name="list">List to populate containing entities and enemies within the sight radius</param>
        public void ListNearby(Vector2 position, float sight, List<Entity> list)
        {
            ListNearbyEnemies(position, sight, list);
            ListNearbyEntities(position, sight, list);
        }

        /// <summary>
        /// Return a live entity that intersects with a given entity
        /// </summary>
        /// <param name="entity">Source entity to use as a position reference</param>
        /// <returns>An entity that intersects with the given entity. Null if nothing was found.</returns>
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

        /// <summary>
        /// Return a live emeny that intersects with a given entity
        /// </summary>
        /// <param name="entity">Source entity to use as a position reference</param>
        /// <returns>An emeny that intersects with the given entity. Null if nothing was found.</returns>
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

        /// <summary>
        /// Places a new entity on the play field. Removes credits from the account.
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
        /// <param name="cloneFrom">Entity to clone from</param>
        public void DropNewEntityHere(GameTime gameTime, Entity cloneFrom)
        {
            if (CanDropEntitityInPlayField(cloneFrom))
            {
                credits -= cloneFrom.GutterDetail.CostToUse;
                cloneFrom.GutterDetail.Played(gameTime);
                AddEntity(cloneFrom.Clone());
            }
        }

        /// <summary>
        /// Determines if an entity can be dropped on the play field in the currnet position. Ensures the entity is within the playable area,
        /// is currently not on top of another entity, and can be dropped.
        /// </summary>
        /// <param name="entity">Entity to drop on the play field</param>
        /// <returns>True if entity can be dropped at its current position</returns>
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

        /// <summary>
        /// Determines if the gutter entity can be used. Ensures the quota allows for another entity to be put into play (if applicable)
        /// and ensures the credits are available
        /// </summary>
        /// <param name="gameTime">Elapsed gametime since last update</param>
        /// <param name="entity">The entity currently selected for play</param>
        /// <returns>A status enumeration value</returns>
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

        /// <summary>
        /// Calculates the number of living entities for a given type currently in play
        /// </summary>
        /// <param name="type">Class type to search for</param>
        /// <returns>The total number of living entities for a given type currently in play</returns>
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

        /// <summary>
        /// Scans the field between 2 points to determine if there is a clear line of sight between them or if there is an entity between them 
        /// capable of blocking the line of sight such as a wall for example. 
        /// </summary>
        /// <param name="source">Source entity</param>
        /// <param name="target">Target entity</param>
        /// <returns>True of the line of sight beween source and target is not obstructed</returns>
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

        /// <summary>
        /// Determines if the entity rectangle is within the play field rectangle
        /// </summary>
        /// <param name="entity">Entity to test</param>
        /// <returns>True if the entity rectangle is within the play field rectangle</returns>
        public bool EntityInPlayField(Entity entity)
        {
            return PlayField.Contains(entity.Rectangle);
        }

        /// <summary>
        /// Draw a line between 2 vectors on the screen.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
        /// <param name="point1">End of line to start drawing</param>
        /// <param name="point2">End of line to stop drawing</param>
        /// <param name="color">Line colour</param>
        /// <param name="thickness">Line thickness</param>
        /// <param name="depth">The depth of the layer</param>
        public void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness, float depth)
        {
            DrawLine(spriteBatch, emptyTexture, point1, point2, color, thickness, depth);
        }

        /// <summary>
        /// Draw a line between 2 vectors on the screen.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
        /// <param name="texture">Texture to apply to the line</param>
        /// <param name="point1">End of line to start drawing</param>
        /// <param name="point2">End of line to stop drawing</param>
        /// <param name="color">Line colour</param>
        /// <param name="thickness">Line thickness</param>
        /// <param name="depth">The depth of the layer</param>
        public void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, Color color, float thickness, float depth)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();

            spriteBatch.Draw(texture, point1, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, depth);
        }

        /// <summary>
        /// Draw text on the screen with a given font and center position
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
        /// <param name="font">Font to use</param>
        /// <param name="text">Text value to draw on the screen</param>
        /// <param name="pos">Position of the center of the text</param>
        /// <param name="color">Text colour</param>
        public void DrawStringCenter(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 pos, Color color)
        {
            Vector2 fontDim = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, pos, color, 0, fontDim, 1.0f, SpriteEffects.None, 1.0f);
        }

        /// <summary>
        /// Draw a circle on the screen having the center of the circle specified by the position
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
        /// <param name="position">Position the center of the circle should be positioned</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="depth">The depth of the layer</param>
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

        /// <summary>
        /// Draws the outline of a circle having the center of the circle specified by the position
        /// </summary>
        /// <param name="spriteBatch">Sprite batch used for drawing our sprites</param>
        /// <param name="position">Position the center of the circle should be positioned</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="depth">The depth of the layer</param>
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

        /// <summary>
        /// Notifies the game that an entity has been killed. Credits or debits the account accordingly, 
        /// incriments the appropriate tally, and updates the game over state if the target entity was killed.
        /// </summary>
        /// <param name="entity">The entity that was killed in the game</param>
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
