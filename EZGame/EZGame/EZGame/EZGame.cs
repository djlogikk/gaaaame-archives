using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EZGame
{
    public class EZGame
    {
        GameInternal game;
        private GameType gameType;
        Dictionary<string,Texture2D> images = new Dictionary<string,Texture2D>();


        public EZGame(GameType aType)
        {
            gameType = aType;

            // initialise user's game elements, behaviours and interactions
            GameInternal game = new GameInternal(images);
        }

        public void Run()
        {
            // create the game internals and start the game loop
            game = new GameInternal(images);
            game.Run();
        }

        private void AddImage(string imageName)
        {
            images[imageName] = null;
        }

        internal Sprite AddSprite(string imageName, SpriteBehaviour spriteBehaviour)
        {
            AddImage(imageName);
            Sprite sprite = new Sprite(imageName, spriteBehaviour, images);
            return sprite;
        }
    }

    public class Sprite
    {
        public int xpos;
        public int ypos;
        public int width;
        public int height;
        public int halfwidth;
        public int halfheight;
        public Vector2 hotspot = new Vector2();
        public int xvel;
        public int yvel;
        public string MyImage { get; set; }
        private Texture2D texture = null;
        private SpriteBehaviour MyBehaviour;
        private Dictionary<string, Texture2D> GameImages;

        public Sprite(string imageName, SpriteBehaviour behaviour, Dictionary<string, Texture2D> images)
        {
            MyImage = imageName;
            MyBehaviour = behaviour;
            GameImages = images;
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
            {
                texture = GameImages([[MyImage);
            }
            Texture2D retval = texture;
        }
    }

    public enum GameType
    {
        OneScreen,
        VerticalScroller,
        HorizontalScroller,
        Platformer,
        TopDownAdventure
    }

    public enum SpriteBehaviour
    {
        Bounce,
    }
}
