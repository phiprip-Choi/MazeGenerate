using System;
using System.Data;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Text;

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

