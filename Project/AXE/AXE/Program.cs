using System;

namespace AXE
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AxeGame game = new AxeGame())
            {
                game.Run();
            }
        }
    }
#endif
}

