using System;

namespace EZGame
{
#if WINDOWS || XBOX
    static class Program
    {
        private static EZGame game;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            game = new EZGame(GameType.OneScreen);
            game.AddSprite("Ball", SpriteBehaviour.Bounce);
            game.Run();
        }
    }
#endif
}

