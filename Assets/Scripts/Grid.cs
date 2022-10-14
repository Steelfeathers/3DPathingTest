using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class Grid : ScriptableObject
    {
        [SerializeField] private int cols;
        [SerializeField] private int rows;
        [SerializeField] private GridCell[] gridCells;

        public GridCell[] GridCells => gridCells;

        public void Initialize(int _cols, int _rows)
        {
            cols = _cols;
            rows = _rows;
        }
        
        public void CreateCellsFromVertices(Vector3[,] gridVertices)
        {
            if (gridCells != null)
            {
                //Destroy the old nested scriptable objects before creating new ones
                foreach (var cellData in gridCells)
                {
                    UnityEngine.Object.DestroyImmediate(cellData, true);
                }
            }
            gridCells = new GridCell[cols * rows];
            
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
                        var gridCell = ScriptableObject.CreateInstance(typeof(GridCell)) as GridCell;
                        if (gridCell != null)
                        {
                            gridCell.hideFlags = HideFlags.HideInHierarchy;
                            gridCell.Initialize(LL, UL, UR, LR);
                            gridCells[j * cols + i] = gridCell;
                            AssetDatabase.AddObjectToAsset(gridCell, this);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Display the grid at runtime in the editor. Gizmos must be turned on for the grid to be visible.
        /// </summary>
        public void DebugDrawGrid()
        {
            foreach (var cellData in gridCells)
            {
                if (cellData == null) continue;
                cellData.DebugDrawCell();
            } 
        }
    }
}
