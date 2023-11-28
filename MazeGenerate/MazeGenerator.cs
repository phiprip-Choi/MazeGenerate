using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MazeGenerate
{
    class MazeGenerator
    {
        Stage[,] map;
        private readonly int xRange, yRange;
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
                    if (i > 0 && i % 2 == 0) for (int j = 2; j < xRange; j++) map[j, i] = Stage.Wall;
                }
            });
            Horizontal.Start();
            Vertical.Start();
            Task.WaitAll(Horizontal, Vertical);

            map[2, 0] = Stage.Start;
            map[3, 0] = Stage.Start;
            map[xRange - 2, yRange] = Stage.Goal;
            map[xRange - 3, yRange] = Stage.Goal;
        }

        private struct Point
        {
            public int x, y;
            public void Censor(int x, int y)
            {
                this.x = x; this.y = y;
            }
            public static Point operator + (Point p1, Point p2)
            {
                Point point;
                point.x = p1.x + p2.x;
                point.y = p1.y + p2.y;
                return point;
            }
            public static Point operator -  (Point p1, Point p2)
            {
                Point point;
                point.x = p1.x - p2.x;
                point.y = p1.y - p2.y;
                return point;
            }
        }
        private bool Range(Point p)
        {
            return p.y < yRange && p.x < xRange && p.y >= 0 && p.x >= 0;
        }

        // 첫번째 방법, 가지 치기(BinaryTree).
        public void BinaryTree()
        {
            Random rand = new Random();
            for (int i = xRange - 3; i > 4; i -= 4)
            {
                for (int j = yRange - 1; j > 1; j -= 2)
                {
                    if (rand.Next(2) == 1)
                    {
                        map[i - 1, j] = Stage.Room;
                        map[i - 2, j] = Stage.Room;

                    }
                    else
                    {
                        map[i, j - 1] = Stage.Room;
                        map[i + 1, j - 1] = Stage.Room;
                    }
                }
            }
            for (int i = 4; i < xRange - 3; i += 4)
            {
                map[i, 1] = Stage.Room;
                map[i + 1, 1] = Stage.Room;
            }
            for (int i = 2; i < yRange - 1; i += 2)
            {
                map[2, i] = Stage.Room;
                map[3, i] = Stage.Room;
            }
        }

        // 두번째 방법, 원점회귀 추적(Back-Tracking).
        public void BackTracking()
        {
            int x = 2, y = 1;
            Stack<Point> track = new Stack<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            Point p = new Point();
            Random rand = new Random();
            Point[] position = new Point[4];
            position[0].y = 2; position[1].x = 4;

            p.Censor(x, y);
            track.Push(p);
            do
            {
                List<int> unBlock = new List<int>();
                for(int i = 0; i<4; i++) // 상하좌우 검사
                {
                    Point num = i < 2 ? p + position[i] : p - position[i - 2];
                    if (!track.Contains(num) && !visited.Contains(num) && Range(num)) unBlock.Add(i);
                }

                if (unBlock.Count != 0)
                {
                    p.Censor(x, y);
                    int r = unBlock[rand.Next(unBlock.Count)];
                    if (r == 0)
                    {
                        map[x, y + 1] = Stage.Room;
                        map[x + 1, y + 1] = Stage.Room;
                        p.y += 2;
                    }
                    else if (r == 1)
                    {
                        map[x + 2, y] = Stage.Room;
                        map[x + 3, y] = Stage.Room;
                        p.x += 4;
                    }
                    else if (r == 2)
                    {
                        map[x, y - 1] = Stage.Room;
                        map[x + 1, y - 1] = Stage.Room;
                        p.y -= 2;
                    }
                    else if (r == 3)
                    {
                        map[x - 1, y] = Stage.Room;
                        map[x - 2, y] = Stage.Room;
                        p.x -= 4;
                    }
                    track.Push(p);
                    x = p.x; y = p.y;
                }
                else
                {
                    visited.Add(track.Peek());
                    track.Pop();
                    x = track.Peek().x;
                    y = track.Peek().y;
                }
                p.Censor(x, y);
            } while (track.Count > 1);
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
                        if (!list.Exists(p => p.Contains(point))) list.Add(new HashSet<Point>() { point });
                        // 획정된 구역이 아닐 경우 새로 할당하기
                    }

                    // 획정된 구역을 무작위 합병
                    for (int x = 2; x < xRange - 3; x += 4)
                    {
                        point.Censor(x, y);
                        int presentIndex = list.FindIndex(a => a.Contains(point));

                        point.Censor(x + 4, y);
                        if (rand.Next(4) > 1 && !list[presentIndex].Contains(point))
                        {
                            map[x + 2, y] = Stage.Room;
                            map[x + 3, y] = Stage.Room;
                            int nextIndex = list.FindIndex(s => s.Contains(point));
                            if (rand.Next(2) == 1)
                            {
                                list[presentIndex].UnionWith(list[nextIndex]);
                                list.RemoveAt(nextIndex);
                            }
                            else
                            {
                                list[nextIndex].UnionWith(list[presentIndex]);
                                list.RemoveAt(presentIndex);
                            }
                        }
                    }
                }
                else // 짝수 행의 벽들을 무작위로 허물기
                {
                    List<Point> unBlockList = new List<Point>();
                    for (int x = 2; x < xRange - 1; x += 4)
                    {
                        point.Censor(x, y+1);
                        unBlockList.Add(point);
                        if (map[x + 2, y - 1] != Stage.Room) // 해당 구역의 끝에 다다르는 조건
                        {
                            point.Censor(x, y - 1);
                            int presentIndex = list.FindIndex(i => i.Contains(point));
                            int randCount = rand.Next(unBlockList.Count);
                            do
                            {
                                int randIndex = rand.Next(unBlockList.Count);
                                map[unBlockList[randIndex].x, unBlockList[randIndex].y - 1] = Stage.Room;
                                map[unBlockList[randIndex].x + 1, unBlockList[randIndex].y - 1] = Stage.Room;
                                map[unBlockList[randIndex].x, unBlockList[randIndex].y] = Stage.Room;
                                map[unBlockList[randIndex].x + 1, unBlockList[randIndex].y] = Stage.Room;
                                list[presentIndex].Add(unBlockList[randIndex]);
                                unBlockList.RemoveAt(randIndex);
                                randCount--;
                            } while (randCount > 2);
                            unBlockList.Clear();
                        }
                    }
                }
            }

            //막 행 정리, 한 구역으로 통일
            point.Censor(2, yRange - 1);
            int r = list.FindIndex(i => i.Contains(point));
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
            Point[] position = new Point[2];
            position[0].y = 2; position[1].x = 4;

            int x = rand.Next(1, (xRange - 1) / 4) * 4 - 2; // 일반항 x = 4*n - 2, 그러므로 가로축 방의 최대항은 N = (X + 2)/4
            int y = rand.Next(1, yRange / 2) * 2 - 1; // 일반항 y = 2*n - 1, 그러므로 세로축 방의 최대항은 N = (Y + 1)/2
            p.Censor(x, y);
            list.Add(p);
            do
            {
                for (int i = 0; i < 4; i++) // 상하좌우 검사
                {
                    Point num = (i < 2) ? p + position[i] : p - position[i - 2];
                    if (!list.Contains(num) && !frontier.ContainsKey(num) && Range(num)) frontier.Add(num, i);
                }

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
                p.Censor(x, y);
                list.Add(frontier.ElementAt(randIndex).Key);
                frontier.Remove(frontier.ElementAt(randIndex).Key);
            } while (frontier.Count > 0);
            list.Clear();
        }

        // 다섯 번째 방법, 프림과 같이 분류되는 알고리즘 크루스칼(Kruskal)
        public void Kruskal()
        {
            Random rand = new Random();
            Point p = new Point();
            List<List<Point>> list = new List<List<Point>>();
            HashSet<Point> closed = new HashSet<Point>();
            Point[] position = new Point[2];
            position[0].y = 2; position[1].x = 4;

            for (int y = 1; y < yRange; y += 2) // 미로의 놓인 모든 방을 리스트에 할당
            {
                for (int x = 2; x < xRange; x += 4)
                {
                    p.Censor(x, y);
                    list.Add(new List<Point>());
                    list[list.Count - 1].Add(p);
                }
            }

            while (list.Count > 1) // 리스트가 하나만 남는 즉시 미로 제작 완료
            {
                //상위 리스트와 하위 리스트의 모든 항 중 하나를 무작위로 설정
                int randIndexFirst = rand.Next(list.Count), randIndexSecond = rand.Next(list[randIndexFirst].Count);
                int x = list[randIndexFirst][randIndexSecond].x, y = list[randIndexFirst][randIndexSecond].y;
                while (true)
                {
                    p.Censor(x, y);
                    List<int> unBlock = new List<int>();
                    for (int i = 0; i < 4; i++) // 상하좌우 검사
                    {
                        Point num = (i < 2) ? p + position[i] : p - position[i - 2];
                        if (!list[randIndexFirst].Contains(num) && !closed.Contains(num) && Range(num)) unBlock.Add(i);
                    }

                    if (unBlock.Count != 0)
                    {
                        int r = unBlock[rand.Next(unBlock.Count)];
                        if (r == 0)
                        {
                            map[x, y + 1] = Stage.Room;
                            map[x + 1, y + 1] = Stage.Room;
                            p.y += 2;
                        }
                        else if (r == 1)
                        {
                            map[x + 2, y] = Stage.Room;
                            map[x + 3, y] = Stage.Room;
                            p.x += 4;
                        }
                        else if (r == 2)
                        {
                            map[x, y - 1] = Stage.Room;
                            map[x + 1, y - 1] = Stage.Room;
                            p.y -= 2;
                        }
                        else if (r == 3)
                        {
                            map[x - 1, y] = Stage.Room;
                            map[x - 2, y] = Stage.Room;
                            p.x -= 4;
                        }
                        break;
                    }
                    else
                    {
                        closed.Add(p);
                        list[randIndexFirst].Remove(p);
                        randIndexSecond = rand.Next(list[randIndexFirst].Count);
                        x = list[randIndexFirst][randIndexSecond].x;
                        y = list[randIndexFirst][randIndexSecond].y;
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
            List<Point> list = new List<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            Random rand = new Random();

            Point p = new Point();
            Point[] position = new Point[2];
            position[0].y = 2; position[1].x = 4;

            bool isEnd = false;
            int x = rand.Next(1, (xRange - 1) / 4) * 4 - 2;
            int y = rand.Next(1, yRange / 2) * 2 - 1;
            p.Censor(x, y);
            list.Add(p);

            while (!isEnd)
            {
                bool isRemainRoom = false;
                while (true)
                {
                    p.Censor(x, y);
                    List<int> unBlock = new List<int>();
                    for (int i = 0; i < 4; i++) // 상하좌우 검사
                    {
                        Point num = i < 2 ? p + position[i] : p - position[i - 2];
                        if (!list.Contains(num) && !visited.Contains(num) && Range(num)) unBlock.Add(i);
                    }

                    if (unBlock.Count != 0)
                    {
                        int r = unBlock[rand.Next(unBlock.Count)];
                        if (r == 0)
                        {
                            map[x, y + 1] = Stage.Room;
                            map[x + 1, y + 1] = Stage.Room;
                            p.y += 2;
                        }
                        else if (r == 1)
                        {
                            map[x + 2, y] = Stage.Room;
                            map[x + 3, y] = Stage.Room;
                            p.x += 4;
                        }
                        else if (r == 2)
                        {
                            map[x, y - 1] = Stage.Room;
                            map[x + 1, y - 1] = Stage.Room;
                            p.y -= 2;
                        }
                        else if (r == 3)
                        {
                            map[x - 1, y] = Stage.Room;
                            map[x - 2, y] = Stage.Room;
                            p.x -= 4;
                        }
                        list.Add(p);
                        x = p.x; y = p.y;
                    }
                    else break;
                }

                visited.UnionWith(list);
                list.Clear();
                for (int yPos = 1; yPos < yRange; yPos += 2) // 포함되지 않은 방을 색출하여 주변 벽을 무작위 축출하기
                {
                    for (int xPos = 2; xPos <= xRange - 3; xPos += 4)
                    {
                        p.Censor(xPos, yPos);
                        if (!visited.Contains(p))
                        {
                            List<byte> openWay = new List<byte>();
                            for (byte i = 0; i < 4; i++) // 상하좌우 검사
                            {
                                Point num = i < 2 ? p + position[i] : p - position[i - 2];
                                if (visited.Contains(num)) openWay.Add(i);
                            }

                            if (openWay.Count > 0)
                            {
                                byte dir = openWay[rand.Next(openWay.Count)];
                                switch(dir) // 시계 방향 순서로 상 우 하 좌
                                {
                                    case 0:
                                        map[xPos, yPos + 1] = Stage.Room;
                                        map[xPos + 1, yPos + 1] = Stage.Room;
                                        break;
                                    case 1:
                                        map[xPos + 2, yPos] = Stage.Room;
                                        map[xPos + 3, yPos] = Stage.Room;
                                        break;
                                    case 2:
                                        map[xPos, yPos - 1] = Stage.Room;
                                        map[xPos + 1, yPos - 1] = Stage.Room;
                                        break;
                                    case 3:
                                        map[xPos - 1, yPos] = Stage.Room;
                                        map[xPos - 2, yPos] = Stage.Room;
                                        break;
                                }
                                isRemainRoom = true;
                                list.Add(p);
                                x = xPos; y = yPos;
                                yPos = yRange + 1;
                                break;
                            }
                        }
                    }
                }
                if (!isRemainRoom) isEnd = true;
            }
            visited.Clear();
        }
    }
}