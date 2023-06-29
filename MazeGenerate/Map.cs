﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MazeGenerate
{
    enum Stage { Room, Start, Wall, Goal };

    class Map
    {
        private readonly char wall = '□', road = ' '; // 벽 문자 '□'의 경우 세로축은 한 점씩, 가로축은 두 점씩 차지한다.
        private int xPos, yPos;
        private readonly int height, width;
        private bool isClear = true;
        Player P = new Player('P', 0, 0); //첫번은 플레이어 문자, 나머지는 차례로 가로축과 세로축 위치.
        Stage[,] map;
        public Map(int inWidth, int inHeight)
        {
            //크기 조정
            if (inWidth % 2 != 0 )
            {
                inWidth++;
                if ((inWidth / 2) % 2 != 0) inWidth -= 2;
            }
            else if((inWidth / 2) % 2 != 0) inWidth += 2;
            if (inHeight % 2 != 0) inHeight++;

            //할당
            width = inWidth;
            height = inHeight;
            map = new Stage[width, height];
        }

        public void Print()
        {
            P.Input();

            if (P.y < 0 || P.y >= height) P.y = yPos;
            else if (map[P.x, P.y] == Stage.Wall)
            {
                P.x = xPos;
                P.y = yPos;
            }
            else
            {
                Console.SetCursorPosition(xPos, yPos);
                Console.Write(road);
                xPos = P.x; yPos = P.y;
                Console.SetCursorPosition(xPos, yPos);
                Console.Write(P.Image);
                if (map[P.x, P.y] == Stage.Goal) isClear = true;
            }

        }

        public void Draw()
        {
            isClear = false;
            Console.Clear();
            MazeGenerator mazeGenerator = new MazeGenerator(map);
            //mazeGenerator.BinaryTree();
            //mazeGenerator.BackTracking();
            //mazeGenerator.Eller();
            //mazeGenerator.Prim();
            //mazeGenerator.Kruskal();
            //mazeGenerator.HuntAndKill();

            for (int i = 0; i < width - 2; i+=2)
            {
                for (int j = 0; j < height - 1; j++)
                {
                    if (Stage.Wall == map[i, j])
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(wall);
                    }
                    else if(Stage.Start == map[i,j])
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(P.Image);
                        P.x = i; P.y = j;
                        xPos = P.x; yPos = P.y;
                    }
                }
            }
        }
        public void Run()
        {
            Console.CursorVisible = false;
            if (isClear) Draw();
            Print();
        }
    }
}