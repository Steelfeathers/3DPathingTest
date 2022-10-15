using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities;

namespace UnknownWorldsTest
{
    //---------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------
    public class GridBuilder : SingletonComponent<GridBuilder>
    {
        [SerializeField][Range(0f,89f)] private float maxSlopeAngle = 45f;
        private string gridFileName => $"{SceneManager.GetActiveScene().name}_pathGrid";
        private Grid grid;
        public Grid Grid => grid;
        

#if UNITY_EDITOR
        private void SaveGrid()
        {
            if (grid == null) return;
            Utils.SaveToJson(grid.Data, Application.dataPath + $"/Resources/LevelGrids/{gridFileName}.json");
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        public void LoadGrid()
        {
            var data = Utils.LoadTextAsset($"LevelGrids/{gridFileName}").FromJson<GridData>();
            if (data != null)
            {
                grid = new Grid(data);
            }
            else
            {
                Debug.LogError($"No grid exists for {SceneManager.GetActiveScene().name}! A grid must be generated in the editor before pathfinding can be used in this scene. " +
                               $"TIP: Make sure only 1 scene is loaded in the scene view when trying to generate the grid");
            }
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            if (GameRoot.Instance.ShowDebug)
            {
                if (grid != null)
                    grid.DebugDrawGrid();
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Pre-build the pathing grid for the current scene and save it to a json file for quick loading at runtime.
        /// Requirements:
        ///     - The scene MUST have a collider marked as "Player" to serve as a reference for how big each cell on the pathing grid needs to be
        ///     - The scene MUST contain at least 1 collider marked as "Ground" to serve as a walkable surface
        ///     - The scene MUST be isolated in the editor (no other scenes open) in order for the BuildGrid function to work!
        /// Assumptions:
        ///     - The environment is static at runtime - no colliders will be added or removed from the scene while running
        ///     - The environment does not have overlapping vertical levels, ex. a tunnel that the player can walk both over and through, or a house with 2 stories
        /// Supports:
        ///     - Arches that the player can pass under
        ///     - Ramps of varying steepness - the maximum walkable steepness can be configured in the GridBuilder
        ///     - Multiple disconnected and/or non-rectangular walkable areas of varying heights (so long as they don't overlap vertically)
        /// </summary>
        public void BuildGrid()
        {
            //First check to see if we have a reference player prefab in the scene
            List<Collider> playerColliders = GetAllCollidersInScene(LayerMask.NameToLayer("Player"));
            if (playerColliders.Count == 0)
            {
                Debug.LogError($"No Player objects were found in the loaded scene, aborting grid creation");
                return;
            }
            
            //Check to see if we have any walkable ground at all
            List<Collider> gndColliders = GetAllCollidersInScene(LayerMask.NameToLayer("Ground"));
            if (gndColliders.Count == 0)
            {
                Debug.LogError($"No colliders tagged as Ground were found in the loaded scene, aborting grid creation");
                return;
            }

            //Get the maximum bounds of the scene, as defined by the furthest extent of all ground colliders in the scene
            //This will be used to determine the bounds for raycasting against the ground to determine walkability
            float minX = Single.PositiveInfinity, minY = Single.PositiveInfinity, minZ = Single.PositiveInfinity;
            float maxX = Single.NegativeInfinity, maxY = Single.NegativeInfinity, maxZ = Single.NegativeInfinity;
            foreach (var col in gndColliders)
            {
                var colMin = col.bounds.min;
                var colMax = col.bounds.max;
                if (colMin.x < minX)
                    minX = colMin.x;
                if (colMin.y < minY)
                    minY = colMin.y;
                if (colMin.z < minZ)
                    minZ = colMin.z;
                
                if (colMax.x > maxX)
                    maxX = colMax.x;
                if (colMax.y > maxY)
                    maxY = colMax.y;
                if (colMax.z > maxZ)
                    maxZ = colMax.z;
            }
            Vector3 gridOrigin = new Vector3(minX, minY, minZ);

            //Get the size of the player object. This will determine the grid size.
            float cellSize = playerColliders[0].bounds.extents.x * 2f;
            float cellHeight = playerColliders[0].bounds.extents.y * 2f;

            //Determine the number of rows and columns needed for a rectangluar grid to cover the whole area
            int cols = Mathf.CeilToInt((maxX - minX) / cellSize);
            int rows = Mathf.CeilToInt((maxZ - minZ) / cellSize);

            //Set the default value of all grid vertices. If no ground is found under that vertex, it counts as a hole in space
            //This system is intended to be able to handle "floating island" type map with multiple interconnected walkable areas of various shapes and levels
            //EDGE-CASE: A hole in the map that can fit within the size of 1 cell, but that will likely be readily visible to the level designer
            Vector3[,] gridVertices =  new Vector3[cols+1, rows+1];
            for (int i = 0; i <= cols; i++)
            {
                for (int j = 0; j <= rows; j++)
                {
                    gridVertices[i,j] = Vector3.negativeInfinity;
                }
            }

            //Cycle through every point on the grid and raycast downward to find the "heightmap" vertex Y position for that point
            //This allows for disconnected and strangely shaped maps, like floating islands or ramps, since we're not assuming that the ground is a flat continuous plane
            //NOTE: This does NOT allow for multi-layered map architecture
            int maskGround = 1 << LayerMask.NameToLayer("Ground");
            float castDist = maxY - minY + 1;
            
            for (int i = 0; i <= cols; i++)
            {
                for (int j = 0; j <= rows; j++)
                {
                    Vector3 rayOrigin = new Vector3(gridOrigin.x + (i * cellSize), gridOrigin.y + castDist, gridOrigin.z + (j * cellSize));
                    var ray = new Ray(rayOrigin, Vector3.down);
                    if (Physics.Raycast(ray, out var hitData, castDist+1, maskGround))
                    {
                        gridVertices[i, j] = hitData.point;
                    }
                }
            }

            //Create the grid from the heightmap of vertices 
            grid = new Grid(cols, rows, cellSize, gridOrigin, gridVertices);
            
            //Cycle through each cell on the grid and boxcast downward to see if there are any obstacles blocking that spot
            //The boxcast starts at player height above the highest point in the cell, to allow for pathing beneath arches
            int maskObstacles = ~(maskGround | (1 << LayerMask.NameToLayer("Player")));
            Vector3 boxHalfExtents = new Vector3(cellSize / 2f, 0.01f, cellSize / 2f);
            castDist = cellHeight + boxHalfExtents.y;
            
            foreach (var cell in grid.GridCells)
            {
                if (cell == null || cell.Data == null) continue;

                //If the angle of the slope of this cell is too steep to walk, mark as unwalkable
                float maxVerticalDist = Mathf.Tan(maxSlopeAngle * Mathf.Deg2Rad) * cellSize;
                if (cell.CheckTooSteep(maxVerticalDist))
                    continue;
                
                Vector3 boxOrigin = new Vector3(cell.Data.Center.x, cell.Data.MaxY + castDist, cell.Data.Center.z);
                
                //Boxcast all to make sure we also hit colliders that the box starts in 
                var hit = Physics.BoxCastAll(boxOrigin, boxHalfExtents, Vector3.down, Quaternion.identity, castDist + 1, maskObstacles, QueryTriggerInteraction.Ignore);
                
                if (hit.Length > 0 && hit[0].collider != null)
                {
                    cell.SetWalkable(false);
                }
            }
            
            SaveGrid();
        }
        
        private List<Collider> GetAllCollidersInScene(int targetLayer=-1)
        {
            List<Collider> colliders = new List<Collider>();
            foreach (var o in FindObjectsOfType(typeof(Collider)))
            {
                var col = (Collider)o;
                if (col != null && (targetLayer == -1 || col.gameObject.layer == targetLayer))
                    colliders.Add(col);
            }

            return colliders;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (grid == null)
            {
                LoadGrid();
                if (grid == null) return;
            }
            grid.DebugDrawGrid();
        } 
#endif
    }
}
