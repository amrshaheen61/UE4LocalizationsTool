using System;

namespace AssetParser
{
    public static class ConsoleMode
    {

        public static void Print(string Str, ConsoleColor color = ConsoleColor.White)
        {
            bool Show = false;
            if (Show)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(Str);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
