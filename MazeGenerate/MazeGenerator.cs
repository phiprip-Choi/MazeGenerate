using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MazeGenerate
{
    class MazeGenerator
    {
        Stage[,] map;
        private int xRange, yRange;
        public MazeGenerator(Stage[,] map)
        {
            this.map = map;
            xRange = map.GetLength(0) - 3;
            yRange = map.GetLength(1) - 2;

            Task Horizontal = new Task(() =>
            {
                for (int i = 0; i < xRange; i += 2)
                {
                    if (i > 2 && (i / 2) % 2 == 0)
                    {
                        for (int j = 0; j < yRange; j++)
                        {
                            map[i, j] = Stage.Wall;
                            map[i + 1, j] = Stage.Wall;
                        }
                    }
                    else
                    {
                        map[i, 0] = Stage.Wall;
                        map[i + 1, 0] = Stage.Wall;
                    }
                }
            });
            Task Vertical = new Task(() =>
            {
                for (int i = 0; i < yRange + 1; i++)
                {
                    map[0, i] = Stage.Wall;
                    map[1, i] = Stage.Wall;
                    if (i > 0 && i % 2 == 0) for (int j = 2; j < xRange; j++)  map[j, i] = Stage.Wall;
                }
            });
            Horizontal.Start();
            Vertical.Start();
            Horizontal.Wait();
            Vertical.Wait();

            map[2, 0] = Stage.Start;
            map[3, 0] = Stage.Start;
            map[xRange - 2, yRange] = Stage.Goal;
            map[xRange - 3, yRange] = Stage.Goal;
        }

        private struct Point
        {
            public int x, y, r;
            public void Censor(int x, int y)
            {
                this.x = x; this.y = y;
            }
            public void Censor(int x, int y, int r)
            {
                this.x = x; this.y = y; this.r = r;
            }
        }

        // 첫번째 방법, 가지 치기(BinaryTree).
        public void BinaryTree()
        {
            Random rand = new Random();
            for (int i = 2; i < xRange - 5; i += 4)
            {
                for (int j = 1; j < yRange - 1; j += 2)
                {
                    if (rand.Next(2) == 1)
                    {
                        map[i + 2, j] = Stage.Room;
                        map[i + 3, j] = Stage.Room;

                    }
                    else
                    {
                        map[i, j + 1] = Stage.Room;
                        map[i + 1, j + 1] = Stage.Room;
                    }
                }
            }
            for (int i = 4; i < xRange - 3; i += 4)
            {
                map[i, yRange - 1] = Stage.Room;
                map[i + 1, yRange - 1] = Stage.Room;
            }
            for (int i = 2; i < yRange - 1; i += 2)
            {
                map[xRange - 2, i] = Stage.Room;
                map[xRange - 3, i] = Stage.Room;
            }

        }

        // 두번째 방법, 원점회귀 추적(Back-Tracking).
        public void BackTracking()
        {
            Point point = new Point();
            List<Point> prevRoom = new List<Point>();
            Random rand = new Random();

            bool[] isBlock = new bool[4];
            int xTrack = xRange - 3, yTrack = yRange - 1, r, i = 0; // 난도 설정, 쉬움(2,1) 어려움(xRange - 3, yRange - 1)
            point.Censor(xTrack, yTrack);
            prevRoom.Add(point);

            while (true)
            {
                if (isBlock[0] && isBlock[1] && isBlock[2] && isBlock[3])
                {
                    i--;
                    if (i == 0) break;
                    xTrack = prevRoom[i].x;
                    yTrack = prevRoom[i].y;
                    Clear(isBlock);
                }

                r = rand.Next(4);
                if (isBlock[r]) continue; //막힌 방향인지 미리 확인하는 조건
                if (r == 0) // 우
                {
                    point.Censor(xTrack + 4, yTrack);
                    if (point.x >= xRange || prevRoom.Contains(point)) isBlock[r] = true;
                    else
                    {
                        Clear(isBlock);
                        map[xTrack + 2, yTrack] = Stage.Room;
                        map[xTrack + 3, yTrack] = Stage.Room;
                        xTrack += 4;
                        prevRoom.Add(point);
                        i = prevRoom.Count - 1;
                    }
                }
                else if (r == 1) // 상
                {
                    point.Censor(xTrack, yTrack + 2);
                    if (point.y >= yRange || prevRoom.Contains(point)) isBlock[r] = true;
                    else
                    {
                        Clear(isBlock);
                        map[xTrack, yTrack + 1] = Stage.Room;
                        map[xTrack + 1, yTrack + 1] = Stage.Room;
                        yTrack += 2;
                        prevRoom.Add(point);
                        i = prevRoom.Count - 1;
                    }
                }
                else if (r == 2) // 좌
                {
                    point.Censor(xTrack - 4, yTrack);
                    if (point.x <= 0 || prevRoom.Contains(point)) isBlock[r] = true;
                    else
                    {
                        Clear(isBlock);
                        map[xTrack - 1, yTrack] = Stage.Room;
                        map[xTrack - 2, yTrack] = Stage.Room;
                        xTrack -= 4;
                        prevRoom.Add(point);
                        i = prevRoom.Count - 1;
                    }
                }
                else if (r == 3) // 하
                {
                    point.Censor(xTrack, yTrack - 2);
                    if (point.y <= 0 || prevRoom.Contains(point)) isBlock[r] = true;
                    else
                    {
                        Clear(isBlock);
                        map[xTrack, yTrack - 1] = Stage.Room;
                        map[xTrack + 1, yTrack - 1] = Stage.Room;
                        yTrack -= 2;
                        prevRoom.Add(point);
                        i = prevRoom.Count - 1;
                    }
                }
            }
            prevRoom.Clear();
        }

        // 세번째 방법, 무작위 획정(Eller's).
        public void Eller()
        {
            List<HashSet<Point>> list = new List<HashSet<Point>>();
            Random rand = new Random();
            Point point = new Point();

            for (int y = 1; y <= yRange - 1; y++)
            {
                if (y % 2 != 0)
                {
                    // 홀수 행의 열들을 각각 획정
                    for (int x = 2; x < xRange; x += 4)
                    {
                        point.Censor(x, y);
                        if (!list.Any(p => p.Contains(point))) // 획정된 구역이 아닐 경우 새로 할당하기
                        {
                            list.Add(new HashSet<Point>());
                            list[list.Count - 1].Add(point);
                        }
                    }

                    // 획정된 구역을 무작위 합병
                    for (int x = 2; x < xRange - 3; x += 4)
                    {
                        point.Censor(x, y);
                        int presentIndex = list.FindIndex(a => a.Contains(point));

                        point.Censor(x + 4, y);
                        if (rand.Next(5) > 1 && !list[presentIndex].Contains(point))
                        {
                            int nextIndex = list.FindIndex(s => s.Contains(point));
                            if (rand.Next(2) == 1)
                            {
                                map[x + 2, y] = Stage.Room;
                                map[x + 3, y] = Stage.Room;
                                list[presentIndex].UnionWith(list[nextIndex]);
                                list.RemoveAt(nextIndex);
                            }
                            else
                            {
                                map[x + 2, y] = Stage.Room;
                                map[x + 3, y] = Stage.Room;
                                list[nextIndex].UnionWith(list[presentIndex]);
                                list.RemoveAt(presentIndex);
                            }
                        }
                    }
                }
                else // 짝수 행의 벽들을 무작위로 허물기
                {
                    int unblockCount = 0;
                    int count = 0;
                    for (int x = 2; x < xRange - 1; x += 4)
                    {
                        count++;
                        point.Censor(x, y - 1);
                        int presentIndex = list.FindIndex(i => i.Contains(point));

                        if (rand.Next(15) == 1) // 난도 조절, 숫자(자연수)가 낮을 수록 난도가 높아짐.
                        {
                            unblockCount++;
                            map[x, y] = Stage.Room;
                            map[x + 1, y] = Stage.Room;
                            map[x, y + 1] = Stage.Room;
                            map[x + 1, y + 1] = Stage.Room;

                            point.Censor(x, y + 1);
                            list[presentIndex].Add(point);
                        }

                        if (map[x + 2, y - 1] != Stage.Room) // 해당 구역의 끝에 다다르는 조건
                        {
                            if (unblockCount < 1) x -= count * 4; // 획정된 구역의 아래 벽을 적어도 하나 허물라는 조건
                            else unblockCount = 0;
                            count = 0;
                        }
                    }
                }
            }

            //막 행 정리, 한 구역으로 통일
            point.Censor(2, yRange - 1);
            int r = list.FindIndex(u => u.Contains(point));
            for (int x = 2; x < xRange - 3; x += 4)
            {
                point.Censor(x + 4, yRange - 1);
                if (!list[r].Contains(point))
                {
                    int nextIndex = list.FindIndex(index => index.Contains(point));
                    list[r].UnionWith(list[nextIndex]);
                    map[x + 2, yRange - 1] = Stage.Room;
                    map[x + 3, yRange - 1] = Stage.Room;
                }
            }
            list.Clear();
        }

        // 네 번째 방법, 저비용 최소 갈래 알고리즘(Minimum-Cost Spanning Tree Algorithm) 프림(Prim's)
        public void Prim()
        {
            Point p = new Point();
            List<Point> list = new List<Point>();
            Dictionary<Point, int> frontier = new Dictionary<Point, int>();
            Random rand = new Random();

            int x = rand.Next(1, (xRange - 1) / 4) * 4 - 2; // 일반항 x = 4*n - 2, 그러므로 가로축 방의 최대항은 N = (X + 2)/4
            int y = rand.Next(1, yRange / 2) * 2 - 1; // 일반항 y = 2*n - 1, 그러므로 세로축 방의 최대항은 N = (Y + 1)/2
            p.Censor(x, y);
            list.Add(p);
            do
            {
                p.Censor(x + 4, y); // 우측
                if (p.x <= xRange && !list.Contains(p) && !frontier.ContainsKey(p)) frontier.Add(p, 1);

                p.Censor(x - 4, y); // 좌측
                if (p.x >= 0 && !list.Contains(p) && !frontier.ContainsKey(p)) frontier.Add(p, 3); 

                p.Censor(x, y + 2); // 상향
                if (p.y <= yRange && !list.Contains(p) && !frontier.ContainsKey(p)) frontier.Add(p, 0); 

                p.Censor(x, y - 2); // 하향
                if (p.y >= 0 && !list.Contains(p) && !frontier.ContainsKey(p)) frontier.Add(p, 2); 


                int randIndex = rand.Next(frontier.Count);
                x = frontier.ElementAt(randIndex).Key.x;
                y = frontier.ElementAt(randIndex).Key.y;
                if (frontier.ElementAt(randIndex).Value == 0)
                {
                    map[x, y - 1] = Stage.Room;
                    map[x + 1, y - 1] = Stage.Room;
                }
                else if (frontier.ElementAt(randIndex).Value == 1)
                {
                    map[x - 1, y] = Stage.Room;
                    map[x - 2, y] = Stage.Room;
                }
                else if (frontier.ElementAt(randIndex).Value == 2)
                {
                    map[x, y + 1] = Stage.Room;
                    map[x + 1, y + 1] = Stage.Room;
                }
                else if (frontier.ElementAt(randIndex).Value == 3)
                {
                    map[x + 2, y] = Stage.Room;
                    map[x + 3, y] = Stage.Room;
                }
                list.Add(frontier.ElementAt(randIndex).Key);
                frontier.Remove(frontier.ElementAt(randIndex).Key);
            } while (frontier.Count > 0);
            list.Clear();
        }

        //다섯 번째 방법, 프림과 같이 분류되는 알고리즘 크루스칼(Kruskal)
        public void Kruskal()
        {
            Random rand = new Random();
            Point p = new Point();
            List<List<Point>> list = new List<List<Point>>();

            for (int y = 1; y < yRange; y += 2) // 미로의 놓인 모든 방을 리스트에 할당
            {
                for (int x = 2; x <= xRange - 3; x += 4)
                {
                    p.Censor(x, y);
                    list.Add(new List<Point>());
                    list[list.Count - 1].Add(p);
                }
            }

            while (list.Count > 1) // 리스트가 하나만 남는 즉시 미로 생성 완료
            {
                //상위 리스트와 하위 리스트의 모든 항 중 하나를 무작위로 설정
                int randIndexFirst = rand.Next(list.Count), randIndexSecond = rand.Next(list[randIndexFirst].Count);
                int x = list[randIndexFirst][randIndexSecond].x, y = list[randIndexFirst][randIndexSecond].y;
                bool[] isBlock = new bool[4];

                while (true)
                {
                    if (isBlock[0] && isBlock[1] && isBlock[2] && isBlock[3])
                    {
                        randIndexSecond = rand.Next(list[randIndexFirst].Count);
                        x = list[randIndexFirst][randIndexSecond].x;
                        y = list[randIndexFirst][randIndexSecond].y;
                        Clear(isBlock);
                    }

                    int r = rand.Next(4);
                    if (isBlock[r]) continue;
                    if (r == 0) // 상향
                    {
                        p.Censor(x, y + 2);
                        if (p.y >= yRange || list[randIndexFirst].Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x, y + 1] = Stage.Room;
                            map[x + 1, y + 1] = Stage.Room;
                            break;
                        }
                    }
                    else if (r == 1) // 우측
                    {
                        p.Censor(x + 4, y);
                        if (p.x >= xRange || list[randIndexFirst].Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x + 2, y] = Stage.Room;
                            map[x + 3, y] = Stage.Room;
                            break;
                        }
                    }
                    else if (r == 2) // 하향
                    {
                        p.Censor(x, y - 2);
                        if (p.y <= 0 || list[randIndexFirst].Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x, y - 1] = Stage.Room;
                            map[x + 1, y - 1] = Stage.Room;
                            break;
                        }
                    }
                    else if (r == 3) // 좌측
                    {
                        p.Censor(x - 4, y);
                        if (p.x <= 0 || list[randIndexFirst].Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x - 1, y] = Stage.Room;
                            map[x - 2, y] = Stage.Room;
                            break;
                        }
                    }
                }
                list[randIndexFirst].AddRange(list[list.FindIndex(i => i.Contains(p))]);
                list.RemoveAt(list.FindIndex(i => i.Contains(p) && list[randIndexFirst].Count > i.Count));
                // 하위 리스트 비교로 오인 방지
            }
            list.Clear();
        }

        // 번외, 원점휘귀 추적을 개량한 알고리즘 '색출과 축출(Hunt-and-Kill)'
        public void HuntAndKill()
        {
            Point p = new Point();
            List<Point> list = new List<Point>();
            Random rand = new Random();

            bool isEnd = false;
            int x = rand.Next(1, (xRange - 1) / 4) * 4 - 2;
            int y = rand.Next(1, yRange / 2) * 2 - 1;
            p.Censor(x, y);
            list.Add(p);

            while (!isEnd)
            {
                bool isRemainRoom = false;
                bool[] isBlock = new bool[4];
                while (true)
                {
                    int r = rand.Next(4);
                    if (isBlock[r]) continue; // 막힌 방향인지 미리 확인하는 조건
                    if (r == 0) // 우
                    {
                        p.Censor(x + 4, y);
                        if (p.x >= xRange || list.Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x + 2, y] = Stage.Room;
                            map[x + 3, y] = Stage.Room;
                            x += 4;
                            list.Add(p);
                        }
                    }
                    else if (r == 1) // 상
                    {
                        p.Censor(x, y + 2);
                        if (p.y >= yRange || list.Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x, y + 1] = Stage.Room;
                            map[x + 1, y + 1] = Stage.Room;
                            y += 2;
                            list.Add(p);
                        }
                    }
                    else if (r == 2) // 좌
                    {
                        p.Censor(x - 4, y);
                        if (p.x <= 0 || list.Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x - 1, y] = Stage.Room;
                            map[x - 2, y] = Stage.Room;
                            x -= 4;
                            list.Add(p);
                        }
                    }
                    else if (r == 3) // 하
                    {
                        p.Censor(x, y - 2);
                        if (p.y <= 0 || list.Contains(p)) isBlock[r] = true;
                        else
                        {
                            Clear(isBlock);
                            map[x, y - 1] = Stage.Room;
                            map[x + 1, y - 1] = Stage.Room;
                            y -= 2;
                            list.Add(p);
                        }
                    }

                    if (isBlock[0] && isBlock[1] && isBlock[2] && isBlock[3]) break;
                }

                for (int yPos = 1; yPos < yRange; yPos += 2) // 포함되지 않은 방을 색출하여 주변 벽을 축출하기
                {
                    for (int xPos = 2; xPos <= xRange - 3; xPos += 4)
                    {
                        p.Censor(xPos, yPos);
                        if (!list.Contains(p))
                        {
                            p.Censor(xPos + 4, yPos);
                            if (list.Contains(p))
                            {
                                isRemainRoom = true;
                                p.Censor(xPos, yPos);
                                list.Add(p);
                                map[xPos + 2, yPos] = Stage.Room;
                                map[xPos + 3, yPos] = Stage.Room;
                                x = xPos; y = yPos;
                                yPos = yRange + 1;
                                break;
                            }
                            p.Censor(xPos - 4, yPos);
                            if (list.Contains(p))
                            {
                                isRemainRoom = true;
                                p.Censor(xPos, yPos);
                                list.Add(p);
                                map[xPos - 1, yPos] = Stage.Room;
                                map[xPos - 2, yPos] = Stage.Room;
                                x = xPos; y = yPos;
                                yPos = yRange + 1;
                                break;
                            }
                            p.Censor(xPos, yPos - 2);
                            if (list.Contains(p))
                            {
                                isRemainRoom = true;
                                p.Censor(xPos, yPos);
                                list.Add(p);
                                map[xPos, yPos - 1] = Stage.Room;
                                map[xPos + 1, yPos - 1] = Stage.Room;
                                x = xPos; y = yPos;
                                yPos = yRange + 1;
                                break;
                            }
                        }
                    }
                }
                if (!isRemainRoom) isEnd = true;
            }
            list.Clear();
        }

        void Clear(bool[] isBlock){ for (int j = 0; j < isBlock.Length; ++j) isBlock[j] = false; }
    }
}