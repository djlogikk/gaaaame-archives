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
        List<Sprite> sprites = new List<Sprite>();

        public EZGame(GameType aType)
        {
            gameType = aType;
        }

        public void Run()
        {
            // create the game internals and start the game loop
            game = new GameInternal(images, sprites);
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
            sprites.Add(sprite);
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
        private static Random rnd = new Random();

        public Sprite(string imageName, SpriteBehaviour behaviour, Dictionary<string, Texture2D> images)
        {
            MyImage = imageName;
            MyBehaviour = behaviour;
            GameImages = images;
            switch (behaviour)
            {
                case SpriteBehaviour.Bounce:
                    xvel = rnd.Next(10) - 5;
                    if (xvel == 0) xvel = 1;
                    yvel = rnd.Next(10) - 5;
                    if (yvel == 0) yvel = 1;
                    break;
            }
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
            {
                texture = GameImages[MyImage];
            }
            Texture2D retval = texture;
            return retval;
        }

        internal void Update()
        {
            switch (MyBehaviour)
            {
                case SpriteBehaviour.Bounce:
                    xpos = xpos + xvel;
                    ypos = ypos + yvel;
                    if (xpos > GameInternal.m_screenmax_x)
                    {
                        xpos = GameInternal.m_screenmax_x;
                        xvel = -Math.Abs(xvel);
                    }
                    if (ypos > GameInternal.m_screenmax_y)
                    {
                        ypos = GameInternal.m_screenmax_y;
                        yvel = -Math.Abs(yvel);
                    }
                    if (xpos < 0)
                    {
                        xpos = 0;
                        xvel = Math.Abs(xvel);
                    }
                    if (ypos < 0)
                    {
                        ypos = 0;
                        yvel = Math.Abs(yvel);
                    }
                    break;
            }
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
