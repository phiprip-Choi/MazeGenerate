using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MazeGenerate
{
    class Astar
    {
        Stage[,] map;
        private readonly int startX, startY, goalX, goalY;
        NodePosition node = new NodePosition(), lastNode = new NodePosition();
        HashSet<NodePosition> closeList = new HashSet<NodePosition>(); // 닫힌 구역(할당된 구역)
        Dictionary<NodePosition, int> openList = new Dictionary<NodePosition, int>(); // 열린 구역(비할당된 구역)
        Dictionary<NodePosition, NodePosition> root = new Dictionary<NodePosition, NodePosition>(); // 궤적 기록

        public Astar(Stage[,] map)
        {
            this.map = map;

            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (Stage.Start == map[x, y])
                    {
                        startX = x;
                        startY = y;
                        x++;
                    }
                    else if (Stage.Goal == map[x, y])
                    {
                        goalX = x;
                        goalY = y;
                        x++;
                    }
                }
            }
        }

        private struct NodePosition
        {
            public int x, y;
            public void Censor(int x, int y)
            {
                this.x = x; this.y = y;
            }
        }

        public void FindingPath()
        {
            node.Censor(startX, startY);
            openList.Add(node, 0);
            bool isGoal = false;
            while (openList.Count > 0 && !isGoal)
            {
                node = openList.OrderBy(k => k.Value).FirstOrDefault().Key; //오름차순으로 정렬해 가장 적은 값(F-cost)을 추출

                for (int ySel = node.y - 1; ySel < node.y + 2; ySel++)
                {
                    for (int xSel = node.x - 2; xSel < node.x + 3; xSel++)
                    {
                        NodePosition currentNode = new NodePosition();
                        currentNode.Censor(xSel, ySel);

                        if ((node.x == xSel && node.y == ySel) || // 현재 위치한 구역 제외
                            ySel < 0 || ySel > map.GetLength(1) - 2 || // 범위 외 제외
                            xSel < 0 || xSel > map.GetLength(0) - 3 || // 상동
                            closeList.Contains(currentNode) || // 이미 등록된 구역 제외
                            map[xSel, ySel] == Stage.Wall || // 벽이 위치한 구역 제외
                            (ySel == node.y + 1 && xSel > node.x) || // 검사 구역 제한
                            (ySel == node.y + 1 && xSel < node.x) || // 상동
                            (openList.ContainsKey(currentNode) && // 기존 F-Cost보다 높은 경우 제외
                            openList[currentNode] < FCost(node, currentNode))) continue;

                        if ((map[xSel, ySel] != Stage.Goal))
                        {
                            openList[currentNode] = FCost(node, currentNode);
                            root[currentNode] = node;
                        }
                        else
                        {
                            openList[currentNode] = FCost(node, currentNode);
                            root[currentNode] = node;
                            lastNode = currentNode;
                            isGoal = true;
                            ySel = node.y + 4;
                            break;
                        }
                    }
                }
                closeList.Add(node);
                openList.Remove(node);
            }
            closeList.Clear();
            openList.Clear();
        }

        private int FCost(NodePosition prev, NodePosition current)
        {
            if (prev.x == startX && prev.y == startY) // 초기 구역만 예외로 설정
                return (int)(Math.Pow(startX - current.x, 2) + Math.Pow(startY - current.y, 2) +
                    Math.Pow(current.x - goalX, 2) + Math.Pow(current.y - goalY, 2));
            else // 나머지 경우. 각각 인접한 구역 간 거리와 이전 구역의 'F-Cost - H-Cost' 값을 서로 더해, 현 구역의 H-Cost 값과 합친다.
                return (int)(Math.Pow(current.x - prev.x, 2) + Math.Pow(current.y - prev.y, 2) +
                    Math.Pow(current.x - goalX, 2) + Math.Pow(current.y - goalY, 2) +
                    openList[prev] - Math.Pow(prev.x - goalX, 2) - Math.Pow(prev.y - goalY, 2));
        }

        public void Tracking(Player p)
        {
            node.Censor(startX, startY);
            if (lastNode.x != goalX || lastNode.y != goalY)
            {
                Console.Clear();
                Console.WriteLine("미로를 제작해놓지 않았거나 \n수치 오류입니다.");
                throw new Exception("미로를 제작해놓지 않았거나 수치 오류입니다.");
            }
            else
            {
                List<NodePosition> nodePath = new List<NodePosition> { lastNode }; // 궤적 호출용
                while (!nodePath.Contains(node))
                {
                    lastNode = root[lastNode];
                    nodePath.Add(lastNode);
                }
                for (int i = nodePath.Count - 1; i >= 0; i--)
                {
                    p.x = nodePath[i].x;
                    p.y = nodePath[i].y;
                    Console.SetCursorPosition(p.x, p.y);
                    Console.Write(p.Image);
                    Thread.Sleep(100);
                    Console.SetCursorPosition(p.x, p.y);
                    Console.Write(" ");
                }
            }
        }
    }
}
