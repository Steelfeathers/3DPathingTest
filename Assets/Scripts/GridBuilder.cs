using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
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
            DrawDefaultInspector();
            if (GUILayout.Button("Build Pathing Grid"))
            {
              
                (target as GridBuilder).BuildGrid();
            }
        }
    }
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private Collider playerCollider;
        [SerializeField] private int testInt;
        
        private Grid grid;
        private void Start()
        {
            grid = new Grid(transform.position, 4, 4, 1f);

           
        }

        public void Test()
        {
            Debug.Log("It's alive: " + name);
            testInt += 1;
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        
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
            List<Collider> sceneColliders = GetAllCollidersInScene();
            float minX = Single.PositiveInfinity, minY = Single.PositiveInfinity, minZ = Single.PositiveInfinity;
            float maxX = Single.NegativeInfinity, maxY = Single.NegativeInfinity, maxZ = Single.NegativeInfinity;
            foreach (var col in sceneColliders)
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
            Color boundingBoxColor = Color.magenta;
            Debug.DrawLine(gridOrigin, new Vector3(minX, minY, maxZ), boundingBoxColor, 60f);
            Debug.DrawLine(gridOrigin, new Vector3(minX, maxY, minZ), boundingBoxColor, 60f);
            Debug.DrawLine(gridOrigin, new Vector3(maxX, minY, minZ), boundingBoxColor, 60f);
            
            //Get the size of the player object. This will determine the grid size.
            float cellSize = playerColliders[0].bounds.extents.x * 2f;
            float cellHeight = playerColliders[0].bounds.extents.y * 2f;

            //Create the empty grid based on the bounds of the level
            int cols = Mathf.CeilToInt((maxX - minX) / cellSize);
            int rows = Mathf.CeilToInt((maxZ - minZ) / cellSize);
            int layers = Mathf.CeilToInt((maxY - minY) / cellHeight);
            Grid grid = new Grid(gridOrigin, cols, rows, cellSize);
            
            //Cycle throu each cell on the grid and boxcast downward to see if there is a player-sized walkable area in that cell
            int maskObstacles = ~((1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Player")));
            Vector3 boxHalfExtents = new Vector3(cellSize / 2f, 0.01f, cellSize / 2f);
            float castDist = maxY - minY + 1;
            
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    var cell = grid.GridCells[i, j];
                    Vector3 castCenter = new Vector3(cell.Center.x, maxY+1, cell.Center.z);
                    if (Physics.BoxCast(castCenter, boxHalfExtents, Vector3.down, out RaycastHit hit, Quaternion.identity, castDist, maskObstacles, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null)
                            cell.Walkable = false;
                    }
                }
            }

            /*
            var ray = new Ray(Vector3.up * 100f, Vector3.down);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 10);

            int mask = 1 << LayerMask.NameToLayer("Ground");
            mask = ~mask;
            
            RaycastHit hitData;
            if (Physics.Raycast(ray, out hitData, Single.MaxValue, mask))
            {
                Debug.Log("Hit non-ground!");
                
            }
            */
            
            grid.DebugDrawGrid(60f);
        }

        private void OnDrawGizmosSelected()
        {
            if (grid == null) return;
            
        } 

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                grid.DebugDrawGrid();
            }
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
