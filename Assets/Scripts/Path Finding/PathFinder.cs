using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{   
    PathGrid[,] pathGrids;
    List<PathGrid> extractedGridList;

    [Header("Reference")]
    [SerializeField] private BoxCollider reference;

    int gridLenght;

    int[,] directions;

    public PathGrid centerGrid;

    private void Start()
    {
        directions = new int[8, 2]
        {
            {0,1},{1,0},{-1,0},{0,-1},{1,1},{-1,-1},{1,-1},{-1,1}
        };
      
    }

    public void InitPathGridTable(int gridRadius)
    {
        CreateGrids(gridRadius);
    }
    private void CreateGrids(int radius)
    {
        gridLenght = radius * 2 + 1;
        pathGrids = new PathGrid[gridLenght, gridLenght];
        Vector3 agentPosition = transform.position;
        Debug.Log("Agent pos:" + agentPosition);
        Vector2 bottomSize = new Vector2(reference.size.x * transform.lossyScale.x, reference.size.z * transform.lossyScale.z);
        Debug.Log($"Size x: {bottomSize.x} - Size y: {bottomSize.y}");
        Vector3 startPosition = new Vector3(agentPosition.x - 
            (bottomSize.x * radius), agentPosition.y, agentPosition.z + (bottomSize.y * radius));
        float capturedX = startPosition.x;
        Debug.Log($"Size x: {bottomSize.x} - Size y: {bottomSize.y}");

        for(int i = 0; i < pathGrids.GetLength(0); i++)
        {
            for(int j = 0; j < pathGrids.GetLength(1); j++)
            {
                GameObject pathGridObject = new GameObject($"PathGrid {i}-{j}");
                PathGrid pathGrid = pathGridObject.AddComponent<PathGrid>();
                pathGrid.InitPathGrid(transform,reference,startPosition,j,i);
                pathGrids[i, j] = pathGrid;
                startPosition.x += bottomSize.x;
            }
            startPosition.z -= bottomSize.y;
            startPosition.x = capturedX;
        }
        extractedGridList = ExtractGridsToList();
        centerGrid = pathGrids[radius, radius];

    }

    public List<PathGrid> DrawPath(Vector3 startPosition,Vector3 positionToMove)
    {
        List<PathGrid> foundPath = FindPath(startPosition,positionToMove);
        return foundPath;
    }

    private List<PathGrid> FindPath(Vector3 startPosition, Vector3 positionToMove)
    {
        List<PathGrid> gridList = extractedGridList;
        PathGrid startGrid = GetClosestGridToPosition(startPosition);
        PathGrid destinationGrid = GetClosestGridToPosition(positionToMove);

        if (destinationGrid == null || startGrid == null) return new List<PathGrid>();
        
        PriorityQueue<PathGrid> openGrids = new PriorityQueue<PathGrid>();
        HashSet<PathGrid> closedGrids = new HashSet<PathGrid>();
        Dictionary<PathGrid, PathGrid> cameFrom = new Dictionary<PathGrid, PathGrid>();

        foreach (var grid in gridList)
        {
            grid.GScore = float.MaxValue;
            grid.FScore = float.MaxValue;
        }

        startGrid.GScore = 0;
        startGrid.FScore = startGrid.CalculateDistance(destinationGrid.transform.position);
        openGrids.Enqueue(startGrid);

        while (openGrids.Count > 0)
        {
            PathGrid currentGrid = openGrids.Dequeue();

            if (currentGrid == destinationGrid)
                return ConstructPath(cameFrom, destinationGrid);

            closedGrids.Add(currentGrid);

            foreach (var neighbor in GetNeighbours(currentGrid))
            {
                if(neighbor != destinationGrid)
                    if (closedGrids.Contains(neighbor) || !neighbor.isMovable)
                        continue;

                float tentativeGScore = currentGrid.GScore + currentGrid.CalculateDistance(neighbor.transform.position);

                if (tentativeGScore < neighbor.GScore)
                {
                    cameFrom[neighbor] = currentGrid;
                    neighbor.GScore = tentativeGScore;
                    neighbor.FScore = neighbor.GScore + neighbor.CalculateDistance(destinationGrid.transform.position);

                    if (!openGrids.Contains(neighbor))
                        openGrids.Enqueue(neighbor);
                        
                }
            }
        }

        return new List<PathGrid>(); 
    }

    private List<PathGrid> ConstructPath(Dictionary<PathGrid, PathGrid> cameFrom, PathGrid destinationGrid)
    {
        PathGrid grid = destinationGrid;
        List<PathGrid> path = new List<PathGrid>();
        while(cameFrom.ContainsKey(grid))
        {
            path.Add(grid);
            grid = cameFrom[grid];

        }
        path.Reverse();
        return path;
    }
    private List<PathGrid> GetNeighbours(PathGrid grid)
    {
        List<PathGrid> neighbours = new List<PathGrid>();
        for(int i = 0; i < 8; i++)
        {
            PathGrid neighbourGrid;
            if (IsInBoundaries(grid, i))
                neighbourGrid = pathGrids[grid.Y + directions[i, 1], grid.X + directions[i, 0]];
            else
                neighbourGrid = null;

            if(neighbourGrid != null)
            {
                neighbours.Add(neighbourGrid);
            }
                
        }

        return neighbours;
    }

    private bool IsInBoundaries(PathGrid grid, int i)
    {
        return grid.X + directions[i, 0] < pathGrids.GetLength(0) && grid.X + directions[i, 0] >= 0 &&
            grid.Y + directions[i, 1] < pathGrids.GetLength(1) && grid.Y + directions[i, 1] >= 0;   
    }

    private PathGrid GetClosestGridToPosition(Vector3 position)
    {
        List<PathGrid> gridList = extractedGridList;

        return gridList.Aggregate((closest, next) => next.CalculateDistance(position) < closest.CalculateDistance(position) ? next : closest);
    }

    private List<PathGrid> ExtractGridsToList()
    {
        List<PathGrid> gridList= new List<PathGrid>();
        for(int i = 0; i < pathGrids.GetLength(0); i++)
        {
            for(int j = 0; j < pathGrids.GetLength(1); j++)
            {
                gridList.Add(pathGrids[i, j]);
            }
        }
        return gridList;
    }

}
