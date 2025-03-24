using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexPathFinder
{
    private class PathNode
    {
        public HexCell Cell { get; set; }
        public PathNode Parent { get; set; }
        public float G { get; set; } // Chi phí từ điểm bắt đầu đến node hiện tại
        public float H { get; set; } // Chi phí ước tính từ node hiện tại đến đích
        public float F => G + H; // Tổng chi phí

        public PathNode(HexCell cell, PathNode parent, float g, float h)
        {
            Cell = cell;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    private readonly HexGrid grid;
    private readonly HashSet<HexCell> occupiedCells;
    private readonly System.Random random;

    public HexPathFinder(HexGrid grid)
    {
        this.grid = grid;
        this.occupiedCells = new HashSet<HexCell>();
        this.random = new System.Random();
    }

    public List<HexCell> FindPath(HexCell start, HexCell target, int range)
    {
        var fullPath = FindPath(start, target);
        if (fullPath == null || fullPath.Count <= 1)
        {
            return null;
        }

        return fullPath.Skip(1).Take(fullPath.Count - 1 - range).ToList();
    }

    public List<HexCell> FindPathWithoutStart(HexCell start, HexCell target)
    {
        var fullPath = FindPath(start, target);
        if (fullPath == null || fullPath.Count <= 1)
        {
            return null;
        }

        return fullPath.Skip(1).ToList();
    }

    public List<HexCell> FindPath(HexCell start, HexCell target)
    {
        if (start == null || target == null) return null;

        // Cập nhật danh sách ô bị chiếm
        UpdateOccupiedCells();

        var openSet = new SortedDictionary<float, List<PathNode>>();
        var closedSet = new HashSet<HexCell>();
        var startNode = new PathNode(start, null, 0, CalculateHeuristic(start, target));

        AddToOpenSet(openSet, startNode);

        while (openSet.Count > 0)
        {
            // Lấy node có F thấp nhất
            var currentNode = GetRandomBestNode(openSet);

            if (currentNode.Cell == target)
            {
                return ReconstructPath(currentNode);
            }
            RemoveFromOpenSet(openSet, currentNode);
            closedSet.Add(currentNode.Cell);

            foreach (var neighbor in GetValidNeighbors(currentNode.Cell, start, target))
            {
                if (closedSet.Contains(neighbor)) continue;

                float newG = currentNode.G + 1; // Chi phí di chuyển giữa các ô là 1

                var neighborNode = FindNodeInOpenSet(openSet, neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new PathNode(
                        neighbor,
                        currentNode,
                        newG,
                        CalculateHeuristic(neighbor, target)
                    );
                    AddToOpenSet(openSet, neighborNode);
                }
                else if (newG < neighborNode.G)
                {
                    neighborNode.Parent = currentNode;
                    neighborNode.G = newG;
                    // Cập nhật vị trí trong openSet
                    UpdateNodeInOpenSet(openSet, neighborNode);
                }
            }
        }

        return null; // Không tìm thấy đường đi
    }

    private float CalculateHeuristic(HexCell from, HexCell to)
    {
        return from.Coordinates.DistanceTo(to.Coordinates);
    }

    private void UpdateOccupiedCells()
    {
        occupiedCells.Clear();
        foreach (var cell in grid.GetAllCells())
        {
            if (cell.IsOccupied)
            {
                occupiedCells.Add(cell);
            }
        }
    }

    private List<HexCell> GetValidNeighbors(HexCell cell, HexCell start, HexCell target)
    {
        var neighbors = grid.GetNeighbors(cell);
        return neighbors.Where(n => n != null && 
            (n == start || 
             n == target || 
             (!n.IsOccupied))
        ).ToList();
    }

    private void AddToOpenSet(SortedDictionary<float, List<PathNode>> openSet, PathNode node)
    {
        if (!openSet.ContainsKey(node.F))
        {
            openSet[node.F] = new List<PathNode>();
        }
        openSet[node.F].Add(node);
    }

    private void RemoveFromOpenSet(SortedDictionary<float, List<PathNode>> openSet, PathNode node)
    {
        if (openSet.ContainsKey(node.F))
        {
            openSet[node.F].Remove(node);
            if (openSet[node.F].Count == 0)
            {
                openSet.Remove(node.F);
            }
        }
    }

    private PathNode GetRandomBestNode(SortedDictionary<float, List<PathNode>> openSet)
    {
        var bestF = openSet.Keys.First();
        var bestNodes = openSet[bestF];
        int randomIndex = random.Next(bestNodes.Count);
        return bestNodes[randomIndex];
    }

    private PathNode FindNodeInOpenSet(SortedDictionary<float, List<PathNode>> openSet, HexCell cell)
    {
        foreach (var nodes in openSet.Values)
        {
            foreach (var node in nodes)
            {
                if (node.Cell == cell) return node;
            }
        }
        return null;
    }

    private void UpdateNodeInOpenSet(SortedDictionary<float, List<PathNode>> openSet, PathNode node)
    {
        foreach (var nodes in openSet.Values.ToList())
        {
            if (nodes.Remove(node))
            {
                AddToOpenSet(openSet, node);
                return;
            }
        }
    }

    private List<HexCell> ReconstructPath(PathNode endNode)
    {
        var path = new List<HexCell>();
        var current = endNode;

        while (current != null)
        {
            path.Add(current.Cell);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}