using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace zombieDefense
{
    /// <summary>
    /// Main game loop and input management.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ZombieDefense zombieDefense;
        MouseState lastMouseState;
        KeyboardState lastKbdState;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
#if !DEBUG
            graphics.IsFullScreen = true;
#endif
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = displayMode.Format;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
        }

        protected override void Initialize()
        {
            zombieDefense = new ZombieDefense(this.GraphicsDevice, graphics, this.Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            zombieDefense.LoadContent();
        }

        protected override void UnloadContent()
        {
            zombieDefense.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKbdState = Keyboard.GetState();

            zombieDefense.Update(gameTime, currentMouseState, lastMouseState, currentKbdState, lastKbdState);

            lastMouseState = currentMouseState;
            lastKbdState = currentKbdState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            zombieDefense.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
