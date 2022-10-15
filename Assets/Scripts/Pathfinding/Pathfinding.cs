using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UnknownWorldsTest
{
    /// <summary>
    /// Implements the A* pathfinding algorithm for maximum runtime efficiency
    /// Video tutorials covering the algorithm were referenced in creating this code
    /// </summary>
    public class Pathfinding : SingletonObject<Pathfinding>
    {
        //The normalized distance to an adjacent cell is 1.0, while the normalized distance to a diagonal cell is 1.4. Multiplying by 10 allows working in ints.
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        
        private List<GridCell> openList;
        private List<GridCell> closedList;

        private Grid grid;

        private bool GetGrid()
        {
            grid = GridBuilder.Instance.Grid;
            return grid != null;
        }
        
        /// <summary>
        /// Given a start and end position, find a path made up of Vector3 nodes that trace out the optimal path, avoiding any obstacles
        /// </summary>
        /// <param name="startWorldPosition"></param>
        /// <param name="endWorldPosition"></param>
        /// <returns></returns>
        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            if (!GetGrid())
            {
                Debug.LogError($"Cannot pathfind, no grid found for scene");
                return null;
            }
            
            GridCell startCell = grid.GetCellForWorldPosition(startWorldPosition);
            GridCell endCell = grid.GetCellForWorldPosition(endWorldPosition);
            if (startCell == null || !startCell.Data.valid || endCell == null || !endCell.Data.valid)
                return null;

            List<GridCell> path = FindPath(startCell, endCell);
            if (path == null) 
            {
                return null;
            } 
            
            //Turn the list of path cells into a list of Vector3 points centered on those cells
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (GridCell pathCell in path) 
            {
                vectorPath.Add(pathCell.Data.Center);
            }
            return vectorPath;
        }
        
        public List<GridCell> FindPath(GridCell startCell,  GridCell endCell) 
        {
            if (!GetGrid())
            {
                Debug.LogError($"Cannot pathfind, no grid found for scene");
                return null;
            }
            
            // Invalid Path
            if (startCell == null || endCell == null) 
                return null;

            //OpenList contains cells queued to be searched; closedList contains cells already searched
            openList = new List<GridCell> { startCell };
            closedList = new List<GridCell>();

            for (int x = 0; x < grid.Data.cols; x++) 
            {
                for (int y = 0; y < grid.Data.rows; y++) {
                    GridCell cell = grid.GridCells[x, y];
                    if (cell == null) continue;
                    cell.gCost = Int32.MaxValue;
                    cell.CalculateFCost();
                    cell.PreviousCell = null;
                }
            }

            startCell.gCost = 0;
            startCell.hCost = CalculateDistanceCost(startCell, endCell);
            startCell.CalculateFCost();
        
            while (openList.Count > 0) 
            {
                GridCell curCell = GetLowestFCostCell(openList);
                if (curCell == endCell) {
                    // Reached final node
                    return CalculatePath(endCell);
                }

                openList.Remove(curCell);
                closedList.Add(curCell);

                foreach (GridCell neighborCell in GetNeighborList(curCell)) {
                    if (closedList.Contains(neighborCell) || neighborCell == null) continue;
                    if (!neighborCell.Data.walkable) {
                        closedList.Add(neighborCell);
                        continue;
                    }

                    //For each neighboring cell, see if we're getting closer to the target by calculating the no-obstacle distance to the end cell
                    int tentativeGCost = curCell.gCost + CalculateDistanceCost(curCell, neighborCell);
                    if (tentativeGCost < neighborCell.gCost) {
                        neighborCell.PreviousCell = curCell;
                        neighborCell.gCost = tentativeGCost;
                        neighborCell.hCost = CalculateDistanceCost(neighborCell, endCell);
                        neighborCell.CalculateFCost();

                        if (!openList.Contains(neighborCell)) {
                            openList.Add(neighborCell);
                        }
                    }
                }
            }

            // Out of nodes on the openList
            return null;
        }

        private List<GridCell> GetNeighborList(GridCell curCell) {
            List<GridCell> neighborList = new List<GridCell>();

            int x = curCell.Data.xIndex;
            int y = curCell.Data.yIndex;
            int rows = grid.Data.rows;
            int cols = grid.Data.cols;
            
            if (x - 1 >= 0) {
                // Left
                neighborList.Add(grid.GridCells[x - 1, y]);
                // Left Down
                if (y - 1 >= 0) neighborList.Add(grid.GridCells[x - 1, y - 1]);
                // Left Up
                if (y + 1 < rows) neighborList.Add(grid.GridCells[x - 1, y + 1]);
            }
            if (x + 1 < cols) {
                // Right
                neighborList.Add(grid.GridCells[x + 1, y]);
                // Right Down
                if (y - 1 >= 0) neighborList.Add(grid.GridCells[x + 1, y - 1]);
                // Right Up
                if (y + 1 < rows) neighborList.Add(grid.GridCells[x + 1, y + 1]);
            }
            // Down
            if (y - 1 >= 0) neighborList.Add(grid.GridCells[x, y - 1]);
            // Up
            if (y + 1 < rows) neighborList.Add(grid.GridCells[x, y + 1]);

            return neighborList;
        }
        
        private List<GridCell> CalculatePath(GridCell endCell)
        {
            List<GridCell> path = new List<GridCell>();
            path.Add(endCell);
            GridCell currentNode = endCell;
            while (currentNode.PreviousCell != null) {
                path.Add(currentNode.PreviousCell);
                currentNode = currentNode.PreviousCell;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(GridCell endCell, GridCell startCell)
        {
            int xDistance = (int)Mathf.Abs(endCell.Data.xIndex - startCell.Data.xIndex);
            int yDistance = (int)Mathf.Abs(endCell.Data.yIndex - startCell.Data.yIndex);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private GridCell GetLowestFCostCell(List<GridCell> gridCellList)
        {
            GridCell lowestFCostCell = gridCellList[0];
            for (int i = 1; i < gridCellList.Count; i++)
            {
                if (gridCellList[i].fCost < lowestFCostCell.fCost)
                {
                    lowestFCostCell = gridCellList[i];
                }
            }

            return lowestFCostCell;
        }
        
    }
}
