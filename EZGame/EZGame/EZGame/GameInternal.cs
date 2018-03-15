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

namespace EZGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameInternal : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Dictionary<string, Texture2D> images;
        private List<Sprite> sprites;
        public static int m_screenmax_x; // store a copy of max screen coordinate
        public static int m_screenmax_y; // store a copy of max screen coordinate
        public static int m_halfscreenmax_x; // store a copy of center screen coordinate
        public static int m_halfscreenmax_y; // store a copy of center screen coordinate

        public GameInternal(Dictionary<string, Texture2D> images, List<Sprite> sprites)
        {
            this.images = images;
            this.sprites = sprites;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // Create a new SpriteBatch, which can be used to draw textures.
            for (int i = 0; i < images.Count(); i++)
            {
                var item = images.ElementAt(i);
                Texture2D tmpTexture = Content.Load<Texture2D>(item.Key);
                images[item.Key] = tmpTexture;
            }

            ReconfigureScreen();
        }

        private void ReconfigureScreen()
        {
            // Screen
            m_screenmax_x = graphics.GraphicsDevice.Viewport.Width;
            m_screenmax_y = graphics.GraphicsDevice.Viewport.Height;
            m_halfscreenmax_x = m_screenmax_x/2;
            m_halfscreenmax_y = m_screenmax_y/2;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            foreach (Sprite sprite in sprites)
            {
                sprite.Update();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            foreach (Sprite sprite in sprites)
            {
                spriteBatch.Draw(sprite.GetTexture(), new Vector2(sprite.xpos, sprite.ypos),
                    null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
