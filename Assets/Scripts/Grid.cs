using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class Grid
    {
        private int cols;
        private int rows;
        private float cellSize;
        private Vector3 gridOrigin;
        private GridCell[,] gridCells;

        public GridCell[,] GridCells => gridCells;

        public Grid(Vector3 _gridOrigin, int _cols, int _rows, float _cellSize)
        {
            cols = _cols;
            rows = _rows;
            cellSize = _cellSize;
            gridOrigin = _gridOrigin;
            gridCells = new GridCell[cols, rows];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    float x = (i * cellSize) + gridOrigin.x;
                    float z = (j * cellSize) + gridOrigin.z;
                    gridCells[i, j] = new GridCell(new Vector3(x, gridOrigin.y, z), cellSize);
                }
            }
        }

        /// <summary>
        /// Display the grid at runtime in the editor. Gizmos must be turned on for the grid to be visible.
        /// </summary>
        public void DebugDrawGrid(float displayTime=5f)
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector3 startPos = gridCells[i, j].Origin;
                    Vector3 endPosR = startPos + (Vector3.right * cellSize);
                    Vector3 endPosUp = startPos + (Vector3.forward * cellSize);

                    if (gridCells[i, j].Walkable)
                    {
                        Debug.DrawLine(startPos, endPosR, Color.white, displayTime);
                        Debug.DrawLine(startPos, endPosUp, Color.white, displayTime);
                    }
                    else
                    {
                        Debug.DrawLine(startPos, endPosR, Color.red, displayTime);
                        Debug.DrawLine(startPos, endPosUp, Color.red, displayTime);
                    }
                    
                }
            }
            
            Debug.DrawLine(
                new Vector3(gridOrigin.x + (cellSize * cols), gridOrigin.y, gridOrigin.z), 
                new Vector3(gridOrigin.x + (cellSize * cols), gridOrigin.y, gridOrigin.z + (cellSize * rows)), 
                Color.white, displayTime);
            
            Debug.DrawLine(
                new Vector3(gridOrigin.x, gridOrigin.y, gridOrigin.z + (cellSize * rows)), 
                new Vector3(gridOrigin.x + (cellSize * cols), gridOrigin.y, gridOrigin.z + (cellSize * rows)), 
                Color.white, displayTime);
        }
    }

    public class GridCell
    {
        private Vector3 origin; //Bottom left corner on an XZ top-down grid
        private float size; //Width/height 
        private Vector3 center;
        private Vector3[] vertices;

        public Vector3 Origin => origin;
        public bool Walkable = true;
        public Vector3 Center => center;

        public GridCell(Vector3 _origin, float _size)
        {
            origin = _origin;
            size = _size;
            center = new Vector3(origin.x + (size / 2f), origin.y, origin.z + (size / 2f));
        }
    }
}
