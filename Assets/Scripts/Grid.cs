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
        
        public Grid(int _cols, int _rows, Vector3[,] _gridVertices)
        {
            data = new GridData();
            data.cols = _cols;
            data.rows = _rows;
            data.gridCellDatas = new GridCellData[data.cols * data.rows];
            
            //Step through all of the vertex points that form the walkable surface of the map and parse them out into quad cells for pathfinding
            for (int i = 0; i < data.cols-1; i++)
            {
                for (int j = 0; j < data.rows-1; j++)
                {
                    Vector3 LL = _gridVertices[i, j];
                    Vector3 UL = _gridVertices[i, j + 1];
                    Vector3 UR = _gridVertices[i + 1, j + 1];
                    Vector3 LR = _gridVertices[i + 1, j];
                    
                    //If all 4 vertices match up to a real point on the ground (not just a hole in space), then make a cell 
                    float defaultVal = Vector3.negativeInfinity.y;
                    if (LL.y > defaultVal && UL.y > defaultVal && UR.y > defaultVal && LR.y > defaultVal)
                    {
                        var gridCell = new GridCell(i, j, LL, UL, UR, LR);
                        data.gridCellDatas[j * data.cols + i] = gridCell.Data;
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
    }
}
