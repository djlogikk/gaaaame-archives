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
using System.Timers;
using System.Windows;

/*******
 * 
 * Breakout Game Demo
 *  - It is the retro "Breakout" block-breaking game.
 *   
 * PRE-REQUISITES for building this demo app:
 *  - Microsoft Visual Studio 2015 - obtain from http://www.microsoft.com/visualstudio/eng/products/visual-studio-2010-express
 *  - XNA Game Studio 4.0.5 - obtain from https://www.dropbox.com/s/gy6o6bu51i607cm/XNA%20Game%20Studio%204.0.5.zip?dl=0
 *  (Drawing a Sprite Tutorial: http://msdn.microsoft.com/en-us/library/bb194908.aspx)
 *
 * INSTRUCTIONS FOR USE
 * 
 *   - Input summary:
 *      F – toggle FULLSCREEN mode (note, will also restart the game when toggling screen mode)
 *      C – recalibrate headset (look at center of screen)
 *      ESC – quit 
 *      Mouse/Arrow keys - move bat
 *   
 **/

namespace Breakout
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D BatTexture, BallTexture, SuperBallTexture, BrickRedTexture, BrickGreenTexture, BrickBlueTexture;
        private Texture2D HTNotCalib, HTCenter, HTLeft, HTLeftFast, HTRight, HTRightFast, HTNotConnected;
        private Texture2D LogoTex;

        private int m_screenmax_x; // store a copy of max screen coordinate
        private int m_screenmax_y; // store a copy of max screen coordinate
        private int m_halfscreenmax_x; // store a copy of center screen coordinate
        private int m_halfscreenmax_y; // store a copy of center screen coordinate

        Bat m_player1bat;
        Ball m_ball;
        Timer m_increasespeed;
        bool m_gameover = true; // initial game over
        int m_gamelives = 0; // initial no lives, game over
        int m_gamescore = 0; // initial no score, game over
        int m_gamelevel = 1; // initially level 1

        private int headtrack_xoffset = 0; // head track heading offset for sprite
        private int headtrack_yoffset = 0; // head track pitch offset for sprite
        private SpriteFont font;
        KeyboardState oldKeyboardState,
                          currentKeyboardState;// Keyboard state variables
        private Wall m_wall;
        private List<Brick> m_brickkilllist;
        private int m_batspeed = 0;
        private Vector2 m_htorigin = new Vector2(44, 8);
        private Vector2 m_htstatuspos;

        private Vector2 m_logopos, m_logohotspot, m_logovel;

        private bool m_superball = false;
        Timer m_superballtimer;
        private bool m_triggersuperball = false;
        private bool m_triggerslowdown = false;

        private Timer m_balldelay;
        private bool m_hideball = true; // initially hide ball until in play
        private HighScores m_highScores;
        private string m_nameinput;
        private bool m_getname = false;
        private SoundEffect SoundLevelUp;
        private SoundEffect SoundBounce1;
        private SoundEffect SoundBounce2;
        private SoundEffect SoundDing;
        private Song Song1;
        private float m_musicvolume = 0.2f;
        private bool m_playingmusic = false;
        private GameConfig m_gameconfig;
        private int m_prefwidth;
        private int m_prefheight;
        private List<DisplayMode> m_displaymodes;
        private bool m_gameinitted = false;
        private bool m_fullscreen = false;

        private string m_keyspressed;
        private bool m_suppressgamekeys;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            int prefferedwidth = (int)SystemParameters.PrimaryScreenWidth;
            int prefferedheight = (int)SystemParameters.PrimaryScreenHeight;
            if (prefferedwidth > 1280)
            {
                prefferedwidth = 1280;
                prefferedheight = 800;
            }
            graphics.IsFullScreen = false; // new, start in window
            graphics.PreferredBackBufferWidth = prefferedwidth;
            graphics.PreferredBackBufferHeight = prefferedheight;
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

            m_balldelay = new Timer();
            m_balldelay.Interval = 2000;
            m_balldelay.AutoReset = false;
            m_balldelay.Elapsed += new ElapsedEventHandler(m_balldelay_Elapsed);
            
            // new, game config form
            m_gameconfig = new GameConfig(this);
            m_gameconfig.Show();
            m_gameconfig.BringToFront();

            DiscoverScreenModes();

            base.Initialize();
        }

        private int DiscoverScreenModes()
        {
            int num = 0;
            int prefnum = 0;
            m_prefwidth = 800;
            m_prefheight = 600;

            m_displaymodes = new List<DisplayMode>();

            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                //mode.whatever (and use any of avaliable information)
                if (num == 0)
                {
                    // take first res as preferred
                    m_prefwidth = mode.Width;
                    m_prefheight = mode.Height;
                    prefnum = 0;
                }
                //unless we have first res with 1280 width, then take that:
                if (mode.Width == 1280 && prefnum == 0)
                {
                    m_prefwidth = mode.Width;
                    m_prefheight = mode.Height;
                    prefnum = num;
                }
                num++;
                string res = num.ToString() + ": " + mode.ToString();
                m_gameconfig.resCombo.Items.Add(res);

                m_displaymodes.Add(mode);
            }
            m_gameconfig.resCombo.SelectedIndex = prefnum;

            graphics.PreferredBackBufferWidth = m_prefwidth;
            graphics.PreferredBackBufferHeight = m_prefheight;
            graphics.ApplyChanges();
            return num;
        }

        void m_balldelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            // launch ball...            
            m_hideball = false;
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
            m_increasespeed = new Timer();
            m_increasespeed.Interval = 10000; // 10000;
            m_increasespeed.Elapsed += new ElapsedEventHandler(m_increasespeed_Elapsed);

            m_superballtimer = new Timer();
            m_superballtimer.Interval = 20000;
            m_superballtimer.Elapsed += new ElapsedEventHandler(m_superballtimer_Elapsed);

            BatTexture = Content.Load<Texture2D>("bat");
            BallTexture = Content.Load<Texture2D>("ball");
            SuperBallTexture = Content.Load<Texture2D>("superball");
            BrickRedTexture = Content.Load<Texture2D>("brickred");
            BrickGreenTexture = Content.Load<Texture2D>("brickgreen");
            BrickBlueTexture = Content.Load<Texture2D>("brickblue");

            LogoTex = Content.Load<Texture2D>("Breakout");

            SoundLevelUp = Content.Load<SoundEffect>("applause");
            SoundBounce1 = Content.Load<SoundEffect>("boing");
            SoundBounce2 = Content.Load<SoundEffect>("boing2");
            SoundDing = Content.Load<SoundEffect>("ding");

            Song1 = Content.Load<Song>("DJ Logikk - Random Stuff That Happens - EP - 02 Zirco");
            MediaPlayer.Play(Song1);
            m_playingmusic = true;

            m_player1bat = new Bat(BatTexture);
            m_ball = new Ball(BallTexture);

            ReconfigureScreen();

            font = Content.Load<SpriteFont>("SpriteFont1");

            DoHighScoreTable();

            m_musicvolume = 0.20f;
            MediaPlayer.Volume = m_musicvolume; // 20% vol

            IsMouseVisible = false;
            m_gameinitted = true;
        }

        void m_superballtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_superball = false;
            m_superballtimer.Stop();
        }

        void m_increasespeed_Elapsed(object sender, ElapsedEventArgs e)
        {
            //m_ball.xvel += (m_ball.xvel > 0 ? 1 : -1); // only make y speed increase, x speed varies based on bat incidence angle!
            m_ball.yvel += (m_ball.yvel > 0 ? 1 : -1);
        }

        private void CreateWall()
        {
            m_wall = new Wall();
            for (int j = 0; j < 3 + m_gamelevel; j++)
            {
                for (int i = 0; i < (graphics.GraphicsDevice.Viewport.Width / 42); i++)  // i < 4; i++) //
                {
                    if (i > 2 && i < (graphics.GraphicsDevice.Viewport.Width / 42) - 3)
                    {
                        Brick brick = new Brick(ChooseColor(j));

                        brick.xpos = (i * 42) + (BrickRedTexture.Width / 2) + (graphics.GraphicsDevice.Viewport.Width % 42);
                        brick.ypos = 50 + (j * 16) + 20;
                        brick.width = BrickRedTexture.Width;
                        brick.height = BrickRedTexture.Height;
                        brick.halfwidth = BrickRedTexture.Width / 2;
                        brick.halfheight = BrickRedTexture.Height / 2;
                        brick.hotspot.X = BrickRedTexture.Width / 2;
                        brick.hotspot.Y = BrickRedTexture.Height / 2;

                        m_wall.m_bricks.Add(brick);
                    }
                }
            }
        }

        private Texture2D ChooseColor(int j)
        {
            Texture2D tex = BrickGreenTexture;
            switch (j)
            {
                case 0:
                    tex = BrickGreenTexture;
                    break;
                case 1:
                    tex = BrickBlueTexture;
                    break;
                case 2:
                    tex = BrickRedTexture;
                    break;
                case 3:
                    tex = BrickGreenTexture;
                    break;
                case 4:
                    tex = BrickGreenTexture;
                    break;
                case 5:
                    tex = BrickBlueTexture;
                    break;
                case 6:
                    tex = BrickRedTexture;
                    break;
                case 7:
                    tex = BrickGreenTexture;
                    break;
                case 8:
                    tex = BrickGreenTexture;
                    break;
                case 9:
                    tex = BrickBlueTexture;
                    break;
                case 10:
                    tex = BrickRedTexture;
                    break;
                default:
                    tex = BrickGreenTexture;
                    break;
            }
            return tex;
        }

        private void ReconfigureScreen()
        {
            // Screen
            m_screenmax_x = graphics.GraphicsDevice.Viewport.Width;
            m_screenmax_y = graphics.GraphicsDevice.Viewport.Height;
            m_halfscreenmax_x = m_screenmax_x / 2;
            m_halfscreenmax_y = m_screenmax_y / 2;

            // Bat
            m_player1bat.xpos = m_halfscreenmax_x;
            m_player1bat.ypos = m_screenmax_y - 50;
            m_player1bat.width = BatTexture.Width;
            m_player1bat.height = BatTexture.Height;
            m_player1bat.halfwidth = BatTexture.Width / 2;
            m_player1bat.halfheight = BatTexture.Height / 2;
            m_player1bat.hotspot.X = BatTexture.Width / 2;
            m_player1bat.hotspot.Y = BatTexture.Height / 2;

            // Ball
            ResetBall();
            m_balldelay.Start();

            // Wall
            CreateWall();
        
            m_htstatuspos = new Vector2(m_halfscreenmax_x, m_screenmax_y - 10);

            m_logopos = new Vector2(m_halfscreenmax_x, (LogoTex.Height / 2) + 38);
            m_logohotspot = new Vector2(LogoTex.Width / 2, LogoTex.Height / 2);
            m_logovel = new Vector2(0, 0);
            AnimateLogo(m_gameover);
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
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // Thanks: http://mort8088.com/2011/03/06/xna-4-0-tutorial-3-input-from-keyboard/
            // Allows the game to exit
            if ((currentKeyboardState.IsKeyUp(Keys.Escape)) && (oldKeyboardState.IsKeyDown(Keys.Escape)))
            {
                this.Exit();
            }

            if (!m_suppressgamekeys)
            {

                if (!m_getname)
                {
                    // allow toggle fullscreen with F key:
                    if ((currentKeyboardState.IsKeyUp(Keys.F)) && (oldKeyboardState.IsKeyDown(Keys.F)))
                    {
                        SetFullScreen(!m_fullscreen);
                        //RestartGame();
                    }

                    if ((currentKeyboardState.IsKeyUp(Keys.PageUp)) && (oldKeyboardState.IsKeyDown(Keys.PageUp)))
                    {
                        m_musicvolume -= 0.1f;
                        if (m_musicvolume < 0.0f) m_musicvolume = 0.0f;
                        MediaPlayer.Volume = m_musicvolume;
                    }
                    if ((currentKeyboardState.IsKeyUp(Keys.PageDown)) && (oldKeyboardState.IsKeyDown(Keys.PageDown)))
                    {
                        m_musicvolume += 0.1f;
                        if (m_musicvolume > 1.0f) m_musicvolume = 1.0f;
                        MediaPlayer.Volume = m_musicvolume;
                    }
                    if ((currentKeyboardState.IsKeyUp(Keys.M)) && (oldKeyboardState.IsKeyDown(Keys.M)))
                    {
                        m_playingmusic = !m_playingmusic;
                        if (m_playingmusic) MediaPlayer.Play(Song1);
                        else MediaPlayer.Stop();
                    }

                    // restart game:
                    if ((currentKeyboardState.IsKeyUp(Keys.Space)) && (oldKeyboardState.IsKeyDown(Keys.Space)))
                    {
                        if (m_gameover)
                        {
                            RestartGame();
                        }
                    }

                    //if (currentKeyboardState.IsKeyDown(Keys.Right))
                    //{
                    //    headtrack_xoffset += 5;
                    //    if (headtrack_xoffset > (m_halfscreenmax_x - m_player1bat.halfwidth))
                    //    {
                    //        headtrack_xoffset = (m_halfscreenmax_x - m_player1bat.halfwidth);
                    //    }
                    //}
                    //if (currentKeyboardState.IsKeyDown(Keys.Left))
                    //{
                    //    headtrack_xoffset -= 5;
                    //    if (headtrack_xoffset < -(m_halfscreenmax_x - m_player1bat.halfwidth))
                    //    {
                    //        headtrack_xoffset = -(m_halfscreenmax_x - m_player1bat.halfwidth);
                    //    }
                    //}
                    MouseState current_mouse = Mouse.GetState();
                    m_player1bat.xpos = current_mouse.X;

                    if (m_batspeed != 0)
                    {
                        headtrack_xoffset += m_batspeed;
                        if (headtrack_xoffset > (m_halfscreenmax_x - m_player1bat.halfwidth))
                        {
                            headtrack_xoffset = (m_halfscreenmax_x - m_player1bat.halfwidth);
                        }
                        if (headtrack_xoffset < -(m_halfscreenmax_x - m_player1bat.halfwidth))
                        {
                            headtrack_xoffset = -(m_halfscreenmax_x - m_player1bat.halfwidth);
                        }
                    }
                }
                else if (m_getname)
                {
                    // in highscore name entry mode:
                    if ((currentKeyboardState.IsKeyUp(Keys.Enter)) && (oldKeyboardState.IsKeyDown(Keys.Enter)))
                    {
                        if (m_nameinput.Length < 1) m_nameinput = "noname";
                        if (m_nameinput.Length > 30) m_nameinput = m_nameinput.Substring(0, 30);

                        m_getname = false;

                        m_highScores.AddHighScore(m_nameinput, "", m_gamescore);
                    }
                    if ((currentKeyboardState.IsKeyUp(Keys.Back)) && (oldKeyboardState.IsKeyDown(Keys.Back)))
                    {
                        if (m_nameinput.Length > 0)
                            m_nameinput = m_nameinput.Substring(0, m_nameinput.Length - 1);
                    }
                    if ((currentKeyboardState.IsKeyUp(Keys.Space)) && (oldKeyboardState.IsKeyDown(Keys.Space)))
                    {
                        m_nameinput += " ";
                    }
                    if ((currentKeyboardState.IsKeyUp(Keys.A)) && (oldKeyboardState.IsKeyDown(Keys.A))) m_nameinput += "A";
                    if ((currentKeyboardState.IsKeyUp(Keys.B)) && (oldKeyboardState.IsKeyDown(Keys.B))) m_nameinput += "B";
                    if ((currentKeyboardState.IsKeyUp(Keys.C)) && (oldKeyboardState.IsKeyDown(Keys.C))) m_nameinput += "C";
                    if ((currentKeyboardState.IsKeyUp(Keys.D)) && (oldKeyboardState.IsKeyDown(Keys.D))) m_nameinput += "D";
                    if ((currentKeyboardState.IsKeyUp(Keys.E)) && (oldKeyboardState.IsKeyDown(Keys.E))) m_nameinput += "E";
                    if ((currentKeyboardState.IsKeyUp(Keys.F)) && (oldKeyboardState.IsKeyDown(Keys.F))) m_nameinput += "F";
                    if ((currentKeyboardState.IsKeyUp(Keys.G)) && (oldKeyboardState.IsKeyDown(Keys.G))) m_nameinput += "G";
                    if ((currentKeyboardState.IsKeyUp(Keys.H)) && (oldKeyboardState.IsKeyDown(Keys.H))) m_nameinput += "H";
                    if ((currentKeyboardState.IsKeyUp(Keys.I)) && (oldKeyboardState.IsKeyDown(Keys.I))) m_nameinput += "I";
                    if ((currentKeyboardState.IsKeyUp(Keys.J)) && (oldKeyboardState.IsKeyDown(Keys.J))) m_nameinput += "J";
                    if ((currentKeyboardState.IsKeyUp(Keys.K)) && (oldKeyboardState.IsKeyDown(Keys.K))) m_nameinput += "K";
                    if ((currentKeyboardState.IsKeyUp(Keys.L)) && (oldKeyboardState.IsKeyDown(Keys.L))) m_nameinput += "L";
                    if ((currentKeyboardState.IsKeyUp(Keys.M)) && (oldKeyboardState.IsKeyDown(Keys.M))) m_nameinput += "M";
                    if ((currentKeyboardState.IsKeyUp(Keys.N)) && (oldKeyboardState.IsKeyDown(Keys.N))) m_nameinput += "N";
                    if ((currentKeyboardState.IsKeyUp(Keys.O)) && (oldKeyboardState.IsKeyDown(Keys.O))) m_nameinput += "O";
                    if ((currentKeyboardState.IsKeyUp(Keys.P)) && (oldKeyboardState.IsKeyDown(Keys.P))) m_nameinput += "P";
                    if ((currentKeyboardState.IsKeyUp(Keys.Q)) && (oldKeyboardState.IsKeyDown(Keys.Q))) m_nameinput += "Q";
                    if ((currentKeyboardState.IsKeyUp(Keys.R)) && (oldKeyboardState.IsKeyDown(Keys.R))) m_nameinput += "R";
                    if ((currentKeyboardState.IsKeyUp(Keys.S)) && (oldKeyboardState.IsKeyDown(Keys.S))) m_nameinput += "S";
                    if ((currentKeyboardState.IsKeyUp(Keys.T)) && (oldKeyboardState.IsKeyDown(Keys.T))) m_nameinput += "T";
                    if ((currentKeyboardState.IsKeyUp(Keys.U)) && (oldKeyboardState.IsKeyDown(Keys.U))) m_nameinput += "U";
                    if ((currentKeyboardState.IsKeyUp(Keys.V)) && (oldKeyboardState.IsKeyDown(Keys.V))) m_nameinput += "V";
                    if ((currentKeyboardState.IsKeyUp(Keys.W)) && (oldKeyboardState.IsKeyDown(Keys.W))) m_nameinput += "W";
                    if ((currentKeyboardState.IsKeyUp(Keys.X)) && (oldKeyboardState.IsKeyDown(Keys.X))) m_nameinput += "X";
                    if ((currentKeyboardState.IsKeyUp(Keys.Y)) && (oldKeyboardState.IsKeyDown(Keys.Y))) m_nameinput += "Y";
                    if ((currentKeyboardState.IsKeyUp(Keys.Z)) && (oldKeyboardState.IsKeyDown(Keys.Z))) m_nameinput += "Z";
                }

                // for key debug
                //Keys[] keys = currentKeyboardState.GetPressedKeys();
                //m_keyspressed = "";
                //foreach (Keys key in keys)
                //{
                //    m_keyspressed += (key.ToString()+",");
                //}
            }

            base.Update(gameTime);
        }

        private void RestartGame()
        {
            m_gameover = false;
            m_gamelives = 5;
            m_gamescore = 0;
            m_gamelevel = 1;
            ReconfigureScreen();
            m_increasespeed.Interval = 10000;
            m_increasespeed.Enabled = true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0,38,77)); //new Color(1, 82, 147));

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            if (m_gameover)// && graphics.IsFullScreen)
            {
                m_logopos.X += m_logovel.X;
                m_logopos.Y += m_logovel.Y;
                if (m_logopos.X > m_screenmax_x - LogoTex.Width / 2)
                {
                    m_logopos.X = (m_screenmax_x - LogoTex.Width / 2);
                    m_logovel.X = -m_logovel.X;
                }
                if (m_logopos.X < LogoTex.Width / 2)
                {
                    m_logopos.X = LogoTex.Width / 2;
                    m_logovel.X = -m_logovel.X;
                }
                if (m_logopos.Y > m_screenmax_y - LogoTex.Height / 2)
                {
                    m_logopos.Y = (m_screenmax_y - LogoTex.Height / 2);
                    m_logovel.Y = -m_logovel.Y;
                }
                if (m_logopos.Y < LogoTex.Height / 2)
                {
                    m_logopos.Y = LogoTex.Height / 2;
                    m_logovel.Y = -m_logovel.Y;
                }
            }

            // new, draw logo
            spriteBatch.Draw(LogoTex, m_logopos, null, Color.RoyalBlue, 0f, m_logohotspot, 1.0f, SpriteEffects.None, 0f);

            // Draw the head tracker target on the screen offset from the center of the screen
            // by an amount on x/y axis which is based on the user's head movements
            // (for calculation see HeadsetTrackingUpdate function below)
            //Vector2 pos = new Vector2(m_halfscreenmax_x + headtrack_xoffset, m_player1bat.ypos);  //m_halfscreenmax_y + headtrack_yoffset);
            Vector2 pos = new Vector2(m_player1bat.xpos, m_player1bat.ypos);
            if (pos.X < m_player1bat.halfwidth) pos.X = m_player1bat.halfwidth;
            if (pos.X > (m_screenmax_x - m_player1bat.halfwidth)) pos.X = (m_screenmax_x - m_player1bat.halfwidth);
            spriteBatch.Draw(m_player1bat.myTexture, pos, null, Color.White, 0f, m_player1bat.hotspot, 1.0f, SpriteEffects.None, 0f);

            // animate the ball and
            // bounce ball off bat
            // idea: use x/y positions and widths to see if they are intersecting

            
            // plan, check bat ball collisions here
            // if they are in collision then move back one unit on x-axis
            // and reverse xvel

            //int batxpostemp = m_halfscreenmax_x + headtrack_xoffset;
            int batxpostemp = m_player1bat.xpos;

            if (batxpostemp < m_player1bat.halfwidth) batxpostemp = m_player1bat.halfwidth;
            if (batxpostemp > (m_screenmax_x - m_player1bat.halfwidth)) batxpostemp = (m_screenmax_x - m_player1bat.halfwidth);

            if (!m_gameover && !m_hideball)
            {
                // animate ball on x-axis
                m_ball.xpos += m_ball.xvel;

                // check if touching bat
                if (CollideDetectBallWithItem(batxpostemp, m_player1bat, true))
                {
                    m_ball.xpos -= m_ball.xvel; // move back
                    m_ball.xvel = -m_ball.xvel; // invert xvel
                    SoundBounce1.Play();
                }

                // check if touching bricks
                bool doneinvert = false;
                m_brickkilllist = new List<Brick>();
                foreach (Brick brick in m_wall.m_bricks)
                {
                    if (CollideDetectBallWithItem(brick.xpos, brick, false))
                    {
                        if (!m_superball)
                        {
                            m_ball.xpos -= m_ball.xvel; // move back
                            if (!doneinvert)
                            {
                                m_ball.xvel = -m_ball.xvel; // invert xvel
                                doneinvert = true;
                            }
                        }
                    }
                }

                // destroy bricks:
                foreach (Sprite brick in m_brickkilllist)
                {
                    m_wall.m_bricks.Remove((Brick)brick);
                    m_gamescore += 100;
                    SoundDing.Play();
                }

                // ---------------------------------------------------------
                // animate ball on y-axis
                m_ball.ypos += m_ball.yvel;

                // check if touching bat
                if (CollideDetectBallWithItem(batxpostemp, m_player1bat, true))
                {
                    SoundBounce1.Play();
                    m_ball.ypos -= m_ball.yvel; // move back
                    m_ball.yvel = -Math.Abs(m_ball.yvel); // invert xvel (up only)
                    // accelerate ball depending on position on bat
                    int difference = (int)Math.Abs(m_ball.xpos - batxpostemp) / 10;
                    if (batxpostemp > m_ball.xpos)
                    {
                        // bat is to right of ball
                        m_ball.xvel -= difference;
                        if (m_ball.xvel < -5) m_ball.xvel = -5;
                    }
                    else
                    {
                        // bat is to right of ball
                        m_ball.xvel += difference;
                        if (m_ball.xvel > 5) m_ball.xvel = 5;
                    }
                }

                // check if touching bricks
                doneinvert = false;
                m_brickkilllist = new List<Brick>();
                foreach (Brick brick in m_wall.m_bricks)
                {
                    if (CollideDetectBallWithItem(brick.xpos, brick, false))
                    {
                        if (!m_superball)
                        {
                            m_ball.ypos -= m_ball.yvel; // move back
                            if (!doneinvert)
                            {
                                m_ball.yvel = -m_ball.yvel; // invert xvel
                                doneinvert = true;
                            }
                        }
                    }
                }

                // destroy bricks:
                foreach (Sprite brick in m_brickkilllist)
                {
                    m_wall.m_bricks.Remove((Brick)brick);
                    m_gamescore += 100;
                    SoundDing.Play();
                }

                if (m_ball.xvel < -5) m_ball.xvel = -5;
                if (m_ball.xvel > 5) m_ball.xvel = 5;

                if (m_triggersuperball)
                {
                    // super ball
                    DoSuperBall();
                    m_gamescore += 200;
                }

                if (m_triggerslowdown)
                {
                    // slow ball
                    m_triggerslowdown = false;
                    if (m_ball.xvel > 0 && m_ball.xvel > 2) m_ball.xvel = 2;
                    if (m_ball.xvel < 0 && m_ball.xvel < -2) m_ball.xvel = -2;
                    if (m_ball.yvel > 0 && m_ball.yvel > 2) m_ball.yvel = 2;
                    if (m_ball.yvel < 0 && m_ball.yvel < -2) m_ball.yvel = -2;

                    m_gamescore += 150;
                }
            }
            else
            {
                m_superball = false;
                m_triggersuperball = false;
                m_triggerslowdown = false;
            }

            pos.X = m_ball.xpos;
            pos.Y = m_ball.ypos;

            // bounce ball off walls
            if (pos.X < m_ball.halfwidth)
            {
                pos.X = m_ball.halfwidth;
                m_ball.xvel = Math.Abs(m_ball.xvel);
                SoundBounce2.Play();
            }
            if (pos.X > (m_screenmax_x - m_ball.halfwidth))
            {
                pos.X = (m_screenmax_x - m_ball.halfwidth);
                m_ball.xvel = -Math.Abs(m_ball.xvel);
                SoundBounce2.Play();
            }
            if (pos.Y < m_ball.halfheight)
            {
                pos.Y = m_ball.halfheight;
                m_ball.yvel = Math.Abs(m_ball.yvel);
                SoundBounce2.Play();
            }
            if (pos.Y > (m_screenmax_y + m_ball.halfheight))
            {
                ResetBall();
                if (!m_gameover)
                {
                    m_gamelives--;
                    m_superball = false;
                    m_superballtimer.Stop();
                    m_triggersuperball = false;
                    m_triggerslowdown = false;
                    m_balldelay.Start();
                }
                if (m_gamelives == 0)
                {
                    m_gameover = true;
                    AnimateLogo(true);
                    DoHighScoreTable();
                }
            }

            // draw wall
            DrawWall(spriteBatch);

            // has level finished?
            if (m_wall.m_bricks.Count() < 1)
            {
                // level up, get faster quicker and more bricks...
                m_gamelevel++;
                SoundLevelUp.Play();
                int tempinterval = 10000 - (m_gamelevel * 2000);
                if (tempinterval < 500) tempinterval = 500;
                m_increasespeed.Interval = tempinterval;
                //if (m_calibrated)
                    m_increasespeed.Enabled = true;
                ResetBall();
                m_hideball = true;
                ReconfigureScreen();
            }

            // draw ball
            if (!m_gameover && !m_hideball)
                spriteBatch.Draw(m_superball ? SuperBallTexture : m_ball.myTexture, pos, null, Color.White, 0f, m_ball.hotspot, 1.0f, SpriteEffects.None, 0f);

            //if (!m_calibrated)
            //{
            //    spriteBatch.DrawString(font, "Awaiting calibration (turn on device, place headset on table)", new Vector2(20, 45), Color.White);
            //}
            //if (m_autoputoncalibratetimer.Enabled)
            //{
            //    spriteBatch.DrawString(font, "Headset put on, about to calibrate (2 seconds) - Look at center of screen!", new Vector2(20, 25), Color.White);
            //}

            spriteBatch.DrawString(font, "Lives = "+m_gamelives, new Vector2(3, 3), Color.White);
            spriteBatch.DrawString(font, "Level = " + m_gamelevel, new Vector2(m_halfscreenmax_x - 50, 3), Color.White);
            spriteBatch.DrawString(font, "Score = " + m_gamescore, new Vector2(m_screenmax_x - 200, 3), Color.White);
            if (m_gameover)
            {
                spriteBatch.DrawString(font, 
                    !m_getname ?
                    "GAME OVER! Space to restart..." : "GAME OVER!", new Vector2(m_halfscreenmax_x-120, m_halfscreenmax_y - 120), Color.LightPink);

                spriteBatch.DrawString(font, "HIGH SCORES:", new Vector2(m_halfscreenmax_x - 100, m_halfscreenmax_y - 50), Color.White);

                int ypos = m_halfscreenmax_y - 25;
                foreach (ScoreRecord rec in m_highScores.scores)
                {
                    if (rec.name.Length < 1) rec.name = "noname";
                    spriteBatch.DrawString(font, rec.name, new Vector2(m_halfscreenmax_x - 100, ypos), Color.LightBlue);
                    ypos += 20;
                }
                ypos = m_halfscreenmax_y - 25;
                foreach (ScoreRecord rec in m_highScores.scores)
                {
                    spriteBatch.DrawString(font, rec.score.ToString(), new Vector2(m_halfscreenmax_x + 50, ypos), Color.LightBlue);
                    ypos += 20;
                }

                // key guide:
                spriteBatch.DrawString(font, "INPUT GUIDE:", new Vector2(m_screenmax_x - 200, m_halfscreenmax_y - 50), Color.White);
                spriteBatch.DrawString(font, 
                    "Space = start\r\n"+
                    "F = toggle fullscreen\r\n" +
                    "C = calibrate headset\r\n"+
                    "   (look at center!)\r\n" +
                    "M - Play music\r\n" +
                    "PgUp/PgDown - \r\n" +
                    "   Music Volume\r\n" +
                    "Esc = exit game\r\n" +
                    "Mouse/arrow keys = control bat"
                    , new Vector2(m_screenmax_x - 200, m_halfscreenmax_y - 25), Color.LightBlue);
            }

            // prompt for high score name?
            if (m_getname)
            {
                spriteBatch.DrawString(font, "You got a HIGH SCORE!!!", new Vector2(30, m_halfscreenmax_y - 50), Color.LightGreen);
                spriteBatch.DrawString(font, "Type your name : ", new Vector2(30, m_halfscreenmax_y - 5), Color.LightGreen);
                spriteBatch.DrawString(font, "> " + m_nameinput, new Vector2(30, m_halfscreenmax_y + 15), Color.LightGreen);
            }

            //// for key debug
            //if (m_keyspressed.Length > 0)
            //{
            //    spriteBatch.DrawString(font, "KEYS: " + m_keyspressed, new Vector2(30, m_halfscreenmax_y + 50), Color.LightGreen);
            //}

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DoHighScoreTable()
        {
            m_highScores = new HighScores();

            if (m_highScores.IsHighScore(m_gamescore))
            {
                SoundLevelUp.Play();
                PromptForName();
            }
        }

        private void PromptForName()
        {
            m_nameinput = "";
            m_getname = true;
        }

        private void AnimateLogo(bool doanim)
        {
            //if (graphics.IsFullScreen)
            //{
                m_logovel.X = doanim ? 1 : 0;
                m_logovel.Y = doanim ? 1 : 0;
            //}
        }

        private bool CollideDetectBallWithItem(int itemxpos, Sprite item, bool isbat = true)
        {
            bool hitsomething = false;
            Rectangle itemrect = new Rectangle(
                itemxpos - item.halfwidth,
                item.ypos - item.halfheight,
                item.width,
                !isbat ? item.height : 2 + m_ball.yvel); // only detect hit of top 2 pixels of bat + bat y velocity!
            Rectangle ballrect = new Rectangle(
                m_ball.xpos - m_ball.halfwidth,
                m_ball.ypos - m_ball.halfheight,
                m_ball.width,
                m_ball.height);
            if (!Rectangle.Intersect(itemrect, ballrect).IsEmpty)
            {
                // we've hit a brick or the bat
                hitsomething = true;
                if (!isbat)
                {
                    m_brickkilllist.Add((Brick)item);
                    if (item.myTexture == BrickRedTexture)
                    {
                        // red brick triggers superball
                        m_triggersuperball = true;
                    }
                    else if (item.myTexture == BrickBlueTexture && !m_superball)
                    {
                        // blue brick triggers slow down
                        m_triggerslowdown = true;
                    }
                }
            }

            return hitsomething;
        }

        private void DoSuperBall()
        {
            m_triggersuperball = false;
            m_superball = true;
            // start timer to turn off again
            m_superballtimer.Start();
        }

        private void DrawWall(SpriteBatch spriteBatch)
        {
            foreach (Brick brick in m_wall.m_bricks)
            {
                Vector2 pos = new Vector2(brick.xpos, brick.ypos);
                spriteBatch.Draw(brick.myTexture, pos, null, Color.White, 0f, brick.hotspot, 1.0f, SpriteEffects.None, 0f);
            }
        }

        private void ResetBall()
        {
            // Ball
            m_ball.xpos = m_halfscreenmax_x;
            m_ball.ypos = 150;
            m_ball.width = BallTexture.Width;
            m_ball.height = BallTexture.Height;
            m_ball.halfwidth = BallTexture.Width / 2;
            m_ball.halfheight = BallTexture.Height / 2;
            m_ball.hotspot.X = BallTexture.Width / 2;
            m_ball.hotspot.Y = BallTexture.Height / 2;
            m_ball.xvel = 2;
            m_ball.yvel = 2;
            m_hideball = true;
        }

        internal void ChangeResolution(int newres)
        {
            if (m_gameinitted)
            {
                try
                {
                    m_prefwidth = m_displaymodes[newres].Width;
                    m_prefheight = m_displaymodes[newres].Height;
                    graphics.PreferredBackBufferWidth = m_prefwidth;
                    graphics.PreferredBackBufferHeight = m_prefheight;
                    graphics.ApplyChanges();
                    ReconfigureScreen();
                }
                catch (Exception)
                {
                    DiscoverScreenModes(); // rediscover screen modes
                }
            }
        }

        internal void SetFullScreen(bool fullscreen)
        {
            m_fullscreen = fullscreen;
            graphics.IsFullScreen = m_fullscreen;
            IsMouseVisible = false; // !m_fullscreen;
            graphics.ApplyChanges();
            m_gameconfig.checkBox1.Checked = fullscreen;
            if (!m_fullscreen)
            {
                if (m_gameconfig.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    m_gameconfig.WindowState = System.Windows.Forms.FormWindowState.Normal;
                }
            }
        }

        internal void SupressGameKeys(bool suppress)
        {
            m_suppressgamekeys = suppress;
        }
    }

    // few game objects:
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
        public Texture2D myTexture;

        public Sprite(Texture2D tex)
        {
            myTexture = tex;
        }
    }

    public class Bat : Sprite
    {
        public Bat(Texture2D tex)
            : base(tex)
        {
        }
    }

    public class Ball : Sprite
    {
        public Ball(Texture2D tex)
            : base(tex)
        {
        }
    }

    public class Brick : Sprite
    {
        public Brick(Texture2D tex)
            : base(tex)
        {
        }
    }

    public class Wall
    {
        public List<Brick> m_bricks = new List<Brick>();
    }
}
