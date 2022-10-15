using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class Grid
    {
        private GridCell[,] gridCells;
        public GridCell[,] GridCells
        {
            get
            {
                if (gridCells == null)
                {
                    gridCells = new GridCell[data.cols, data.rows];
                    for (int index = 0; index < data.gridCellDatas.Length; index++)
                    {
                        if (data.gridCellDatas[index] == null || !data.gridCellDatas[index].valid) continue;
                        
                        int x = index % data.cols; // get the remainder of dividing index by columns 
                        int y = index / data.cols; // get the result of dividing index by columns
                        
                        gridCells[x, y] = new GridCell(data.gridCellDatas[index]);
                    }
                }

                return gridCells;
            }
        }

        private GridData data;
        public GridData Data => data;

        //--------------------------------------------------------------------------------
        public Grid(GridData _data)
        {
            data = _data;
        }

        public GridCell GetCellForWorldPosition(Vector3 worldPos)
        {
            int xIndex = Mathf.FloorToInt((worldPos - data.Origin).x / data.cellSize);
            int yIndex = Mathf.FloorToInt((worldPos - data.Origin).z / data.cellSize);
            if (xIndex < 0 || yIndex < 0 || xIndex >= GridCells.GetLength(0) || yIndex >= GridCells.GetLength(1))
                return null;
            
            return GridCells[xIndex, yIndex];
        }
        
#if UNITY_EDITOR
        #region EDITOR
        public Grid(int _cols, int _rows, float _cellSize, Vector3 _origin, Vector3[,] _gridVertices)
        {
            data = new GridData();
            data.cols = _cols;
            data.rows = _rows;
            data.cellSize = _cellSize;
            data.Origin = _origin;
            data.gridCellDatas = new GridCellData[data.cols * data.rows];
            
            //Step through all of the vertex points that form the walkable surface of the map and parse them out into quad cells for pathfinding
            //Vertex (0,0) is at the bottom left corner -> i and j increase along the positive x and z directions
            for (int i = 0; i < data.cols; i++)
            {
                for (int j = 0; j < data.rows; j++)
                {
                    Vector3 LL, UL, UR, LR;
                    LL = _gridVertices[i, j];
                    UL = _gridVertices[i, j + 1];
                    UR = _gridVertices[i + 1, j + 1];
                    LR = _gridVertices[i + 1, j];
                    
                    //If all 4 vertices match up to a real point on the ground (not just a hole in space), then make a cell 
                    float defaultVal = Vector3.negativeInfinity.y;
                    if (LL.y > defaultVal && UL.y > defaultVal && UR.y > defaultVal && LR.y > defaultVal)
                    {
                        var gridCell = new GridCell(i, j, LL, UL, UR, LR);
                        data.gridCellDatas[j * data.cols + i] = gridCell.Data;
                    }
                    else
                    {
                        Debug.Log($"Invalid cell at ({i},{j})");
                    }
                }
            }
        }

        /// <summary>
        /// Display the grid at runtime in the editor. Gizmos must be turned on for the grid to be visible.
        /// </summary>
        public void DebugDrawGrid()
        {
            foreach (var cell in GridCells)
            {
                if (cell == null || cell.Data == null) continue;
                cell.DebugDrawCell();
            } 
        }
        #endregion
#endif
    }
}
