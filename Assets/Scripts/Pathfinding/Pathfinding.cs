using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        
        private Grid grid;
        private List<GridCell> openList;
        private List<GridCell> closedList;

        public Pathfinding(Grid _grid)
        {
            grid = _grid;
        }

        /*
        private List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            GridCell startCell = grid.GetGridCell(startX, startY);
            GridCell endCell = grid.GetGridCell(endX, endY);
            
            openList = new List<GridCell> { startCell };
            closedList = new List<GridCell>();

            for (int x = 0; x < grid.Cols; x++)
            {
                for (int y = 0; y < grid.Rows; y++)
                {
                    GridCell cell = grid.GetGridCell(x, y);
                    cell.gCost = int.MaxValue;
                    cell.CalculateFCost();
                    cell.PreviousCell = null;
                }
            }

            startCell.gCost = 0;
            startCell.hCost = CalculateDistance(startX, endX, startY, endY);
            startCell.CalculateFCost();

            while (openList.Count > 0)
            {
                GridCell currentCell = GetLowestFCostCell(openList);
               // if (currentCell == endCell)
               //     return CalculatePath(endCell); //reached final cell

                openList.Remove(currentCell);
                closedList.Add(currentCell);
            }

            return null;
        }
        */

        //private List<GridCell> GetNeighborCells(GridCell currentCell)
        //{
        //    List<GridCell> neighborList = new List<GridCell>();
        //    if (currentCell.)
        //}
        private List<GridCell> CalculatePath(GridCell endCell)
        {
            return null;
        }

        private int CalculateDistance(float ax, float ay, float bx, float by)
        {
            int xDistance = (int)Mathf.Abs(ax - bx);
            int yDistance = (int)Mathf.Abs(ay - by);
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
