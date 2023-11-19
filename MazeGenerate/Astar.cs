using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace MazeGenerate
{
    class Astar
    {
        private readonly Stage[,] map;
        private readonly NodePosition startNode, goalNode;
        private  List<NodePosition> path;

        public Astar(Stage[,] map)
        {
            this.map = map;
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x+=2)
                {
                    if (Stage.Start == map[x, y]) startNode = new NodePosition(x, y);
                    else if (Stage.Goal == map[x, y]) goalNode = new NodePosition(x, y);                    
                }
            }
            FindingPath();
        }

        private struct NodePosition
        {
            public int x, y;

            public NodePosition(int x, int y)
            {
                this.x = x; this.y = y;
            }
            public NodePosition(NodePosition node)
            {
                this.x = node.x; this.y = node.y;
            }
            public static bool operator ==(NodePosition p1, NodePosition p2)
            {
                if ((p1.x == p2.x) && (p1.y == p2.y)) return true;
                return false;
            }
            public static bool operator !=(NodePosition p1, NodePosition p2)
            {
                if ((p1.x != p2.x) || (p1.y != p2.y)) return true;
                return false;
            }
        }

        private class NodeCost : IComparable<NodeCost>
        {
            public int hCost, gCost, fCost;
            public NodeCost prevNode = null;
            public NodePosition? nodePos;
            public NodeCost(NodePosition nodePos)
            {
                this.nodePos = nodePos;
            }
            public NodeCost(NodeCost prevNode = null, NodePosition? nodePos = null, int gCost = 0, int hCost= 0)
            {
                this.prevNode = prevNode;
                this.nodePos = nodePos;
                this.gCost = gCost;
                this.hCost = hCost;
                fCost = gCost + hCost;
            }
            public int CompareTo(NodeCost other)
            {
                return this.fCost - other.fCost;
            }
        }

        private void FindingPath()
        {
            Dictionary<NodePosition, NodeCost> openDic = new Dictionary<NodePosition, NodeCost>();
            HashSet<NodePosition> closeSet = new HashSet<NodePosition>();
            NodeCost lastNode = new NodeCost();
            bool isGoal = false;

            openDic.Add(startNode, new NodeCost(startNode));
            while(openDic.Count > 0 && !isGoal) 
            {
                NodePosition nodePos = new NodePosition(openDic.OrderBy(k => k.Value).FirstOrDefault().Key);
                for (int ySel = nodePos.y - 1; ySel < nodePos.y + 2; ySel++)
                {
                    for (int xSel = nodePos.x - 2; xSel < nodePos.x + 3; xSel++)
                    {
                        NodePosition currentNode = new NodePosition(xSel, ySel);
                        NodeCost currentCost = caculateCost(openDic[nodePos], nodePos, currentNode, openDic[nodePos].gCost);

                        if ((nodePos.x == xSel && nodePos.y == ySel) || // 현재 위치한 구역 제외
                            ySel < 0 || ySel > map.GetLength(1) - 2 || // 범위 외 제외
                            xSel < 0 || xSel > map.GetLength(0) - 3 || // 상동
                            closeSet.Contains(currentNode) || // 이미 등록된 구역 제외
                            map[xSel, ySel] == Stage.Wall || // 벽이 위치한 구역 제외
                            (ySel == nodePos.y + 1 && xSel > nodePos.x) || // 검사 구역 제한
                            (ySel == nodePos.y + 1 && xSel < nodePos.x) || // 상동
                            (openDic.ContainsKey(currentNode) && // 기존 F-Cost보다 높은 경우 제외
                            openDic[currentNode].fCost < currentCost.fCost)) continue;

                        openDic[currentNode] = currentCost;
                        if (map[xSel, ySel] == Stage.Goal)
                        {
                            lastNode = currentCost;
                            isGoal = true;
                            ySel = nodePos.y + 2;
                            break;
                        }
                    }
                }
                closeSet.Add(nodePos);
                openDic.Remove(nodePos);
            }
            path = new List<NodePosition>();
            while (lastNode != null)
            {
                path.Add((NodePosition)lastNode.nodePos);
                lastNode = lastNode.prevNode;
            }
        }
        private NodeCost caculateCost(NodeCost prevNode, NodePosition prevPos,  NodePosition presPos, int gCost) 
        {
            return new NodeCost(prevNode, presPos, 
                gCost + (presPos.x == prevPos.x || presPos.y == prevPos.y ? 10 : 14), //gCost 값 직선 10 대각선 14
                Math.Abs(presPos.x - goalNode.x) + Math.Abs(presPos.y - goalNode.y)); //hCost 값(맨하탄)
        }
        public void Tracking(Player p)
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                p.x = path[i].x;
                p.y = path[i].y;
                Console.SetCursorPosition(p.x, p.y);
                Console.Write(p.Image);
                Thread.Sleep(100);
                Console.SetCursorPosition(p.x, p.y);
                Console.Write(" ");
            }
        }
    }
}
