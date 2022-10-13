using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnknownWorldsTest
{
    [CustomEditor(typeof(GridBuilder))]
    class GridBuilderEditor : Editor {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDefaultInspector();
            if (GUILayout.Button("Build Pathing Grid"))
            {
                (target as GridBuilder).BuildGrid();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private Grid grid;
       
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

            //Get the maximum bounds of the scene, as defined by the furthest extent of all colliders in the scene
            //This will be used to determine the bounds for raycasting against the ground to determine walkability
            //List<Collider> sceneColliders = GetAllCollidersInScene();
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

            //Create the empty grid based on the bounds of the level
            int cols = Mathf.CeilToInt((maxX - minX) / cellSize);
            int rows = Mathf.CeilToInt((maxZ - minZ) / cellSize);


            if (grid == null)
            {
                grid = ScriptableObject.CreateInstance(typeof(Grid)) as Grid;
                if (grid != null)
                {
                    AssetDatabase.CreateAsset(grid, $"Assets/Grids/{SceneManager.GetActiveScene().name}_pathingGrid.asset");
                }
                else
                {
                    Debug.LogError("Failed to create grid!");
                    return;
                }
            }

            grid.Initialize(gridOrigin, cols, rows, cellSize);

            //Cycle through every point on the grid and raycast downward to find the "heightmap" vertex Y position for that point
            //This allows for disconnected and strangely shaped maps, like floating islands or ramps, since we're not assuming that the ground is a flat continuous plane
            //NOTE: This does NOT allow for multi-layered map architecture, such as a tunnel that you can walk both over and through
            int maskGround = 1 << LayerMask.NameToLayer("Ground");
            float castDist = maxY - minY + 1;
            
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector3 rayOrigin = new Vector3(gridOrigin.x + (i * cellSize), gridOrigin.y + castDist, gridOrigin.z + (j * cellSize));
                    var ray = new Ray(rayOrigin, Vector3.down);
                    if (Physics.Raycast(ray, out var hitData, castDist+1, maskGround))
                    {
                        grid.AddGridVertex(i, j, hitData.point);
                    }
                }
            }
            
            grid.CreateCellsFromVertices();
            
            //Cycle through each cell on the grid and boxcast downward to see if there are any obstacles blocking that spot
            //The boxcast starts at player height above the highest point in the cell, to allow for pathing beneath arches
            int maskObstacles = ~(maskGround | (1 << LayerMask.NameToLayer("Player")));
            Vector3 boxHalfExtents = new Vector3(cellSize / 2f, 0.01f, cellSize / 2f);
            castDist = cellHeight + boxHalfExtents.y;
            
            foreach (var cell in grid.GridCells)
            {
                if (cell == null) continue;
                if (cell.CheckTooSteep(1f))
                    continue;
                
                Vector3 boxOrigin = new Vector3(cell.Center.x, cell.HighestPoint + castDist, cell.Center.z);
                
                //Boxcast all to make sure we also hit colliders that the box starts in 
                var hit = Physics.BoxCastAll(boxOrigin, boxHalfExtents, Vector3.down, Quaternion.identity, castDist + 1, maskObstacles, QueryTriggerInteraction.Ignore);
                
                if (hit.Length > 0 && hit[0].collider != null)
                {
                    cell.Walkable = false;
                }
            }
            
            //testInt += 1;
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(grid);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private void OnDrawGizmosSelected()
        {
            if (grid == null) return;
           // Gizmos.color = Color.magenta;
            //Gizmos.DrawLine(grid.Origin, new Vector3(gridOrigin.x, gridOrigin.y, grid.Origin.z + ));
            //Debug.DrawLine(gridOrigin, new Vector3(minX, minY, maxZ), boundingBoxColor, 60f);
            //Debug.DrawLine(gridOrigin, new Vector3(minX, maxY, minZ), boundingBoxColor, 60f);
            //Debug.DrawLine(gridOrigin, new Vector3(maxX, minY, minZ), boundingBoxColor, 60f);
            grid.DebugDrawGrid();
        } 

        private void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.G))
            {
                grid.DebugDrawGrid();
            }
            #endif
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
    }
}
