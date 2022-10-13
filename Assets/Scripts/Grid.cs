using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class Grid : ScriptableObject
    {
        [SerializeField]private int cols;
        [SerializeField] private int rows;
        [SerializeField] private float cellSize;
        [SerializeField] private Vector3 origin;
        [SerializeField] private GridCell[,] gridCells;

        [SerializeField]private Vector3[,] gridVertices;
        public GridCell[,] GridCells => gridCells;
        public Vector3 Origin => origin;

        public void Initialize(Vector3 origin, int _cols, int _rows, float _cellSize)
        {
            cols = _cols;
            rows = _rows;
            cellSize = _cellSize;
            origin = origin;
            gridCells = new GridCell[cols, rows];

            gridVertices = new Vector3[cols, rows];

            //Set the default value of all grid vertices. If no ground is found under that vertex, it counts as a hole in space
            //This system is intended to be able to handle "floating island" type map with multiple interconnected walkable areas of various shapes and levels
            //EDGE-CASE: A hole in the map that can fit within the size of 1 cell, but that will likely be readily visible to the level designer
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    gridVertices[i,j] = Vector3.negativeInfinity;
                }
            }
        }

        public void AddGridVertex(int colIndex, int rowIndex, Vector3 vertex)
        {
            gridVertices[colIndex, rowIndex] = vertex;
        }

        public void CreateCellsFromVertices()
        {
            //Step through all of the vertex points that form the walkable surface of the map and parse them out into quad cells for pathfinding
            for (int i = 0; i < cols-1; i++)
            {
                for (int j = 0; j < rows-1; j++)
                {
                    Vector3 LL = gridVertices[i, j];
                    Vector3 UL = gridVertices[i, j + 1];
                    Vector3 UR = gridVertices[i + 1, j + 1];
                    Vector3 LR = gridVertices[i + 1, j];
                    
                    //If all 4 vertices match up to a real point on the ground (not just a hole in space), then make a cell 
                    float defaultVal = Vector3.negativeInfinity.y;
                    if (LL.y > defaultVal && UL.y > defaultVal && UR.y > defaultVal && LR.y > defaultVal)
                    {
                        gridCells[i, j] = new GridCell(LL, UL, UR, LR);
                    }
                }
            }
        }


        /// <summary>
        /// Display the grid at runtime in the editor. Gizmos must be turned on for the grid to be visible.
        /// </summary>
        public void DebugDrawGrid()
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (gridCells[i, j] == null) continue;
                    gridCells[i, j].DebugDrawCell();
                }
            }
        }
    }

    
}
