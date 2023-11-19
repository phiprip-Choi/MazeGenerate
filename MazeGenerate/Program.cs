using System;

namespace MazeGenerate
{
    class Program
    {
        public static void Main(String[] argc)
        {
            Map stage = new Map(50, 30);
            while (true) stage.Run();
        }
    }
}

