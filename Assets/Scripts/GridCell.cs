using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class GridCell : ScriptableObject
    {
        [SerializeField] private bool walkable = true;
        
        [SerializeField] private float centerX;
        [SerializeField] private float centerY;
        [SerializeField] private float centerZ;

        [SerializeField] private float vertexLL_X;
        [SerializeField] private float vertexLL_Y;
        [SerializeField] private float vertexLL_Z;
        
        [SerializeField] private float vertexUL_X;
        [SerializeField] private float vertexUL_Y;
        [SerializeField] private float vertexUL_Z;
        
        [SerializeField] private float vertexUR_X;
        [SerializeField] private float vertexUR_Y;
        [SerializeField] private float vertexUR_Z;
        
        [SerializeField] private float vertexLR_X;
        [SerializeField] private float vertexLR_Y;
        [SerializeField] private float vertexLR_Z;

        public bool Walkable => walkable;
        public Vector3 Center => new Vector3(centerX, centerY, centerZ);
        public Vector3[] Vertices => new Vector3[]
        {
            new Vector3(vertexLL_X, vertexLL_Y, vertexLL_Z),
            new Vector3(vertexUL_X, vertexUL_Y, vertexUL_Z),
            new Vector3(vertexUR_X, vertexUR_Y, vertexUR_Z),
            new Vector3(vertexLR_X, vertexLR_Y, vertexLR_Z)
        };
        public Vector3 Origin => Vertices[0];
        public float MinY => Mathf.Min(vertexLL_Y, vertexUL_Y, vertexUR_Y, vertexLR_Y);
        public float MaxY => Mathf.Max(vertexLL_Y, vertexUL_Y, vertexUR_Y, vertexLR_Y);

        public void Initialize(Vector3 _vertexLL, Vector3 _vertexUL, Vector3 _vertexUR, Vector3 _vertexLR)
        {
            vertexLL_X = _vertexLL.x;
            vertexLL_Y = _vertexLL.y;
            vertexLL_Z = _vertexLL.z;
            
            vertexUL_X = _vertexUL.x;
            vertexUL_Y = _vertexUL.y;
            vertexUL_Z = _vertexUL.z;
            
            vertexUR_X = _vertexUR.x;
            vertexUR_Y = _vertexUR.y;
            vertexUR_Z = _vertexUR.z;
            
            vertexLR_X = _vertexLR.x;
            vertexLR_Y = _vertexLR.y;
            vertexLR_Z = _vertexLR.z;
            
            //Calculate the center of the cell, which will be used in pathfinding
            var verts = Vertices;
            var dir = verts[2] - verts[0];
            var mag = Vector3.Distance(verts[2], verts[0]) / 2f;
            var centerPt = (dir * mag) + verts[0];
            centerX = centerPt.x;
            centerY = centerPt.y;
            centerZ = centerPt.z;
        }

        public void SetWalkable(bool canWalk)
        {
            walkable = canWalk;
        }
        
        //Calculate if the vertical slope of the cell is too steep to be walkable
        public bool CheckTooSteep(float maxVerticalDistance)
        {
            if (MaxY - MinY > maxVerticalDistance)
            {
                SetWalkable(false);
                return true;
            }

            return false;
        }

        public void DebugDrawCell()
        {
            Gizmos.color = Walkable ? Color.white : Color.red;

            var verts = Vertices;
            Gizmos.DrawLine(verts[0], verts[1]);
            Gizmos.DrawLine(verts[1], verts[2]);
            Gizmos.DrawLine(verts[2], verts[3]);
            Gizmos.DrawLine(verts[3], verts[0]);
        }
    }
}
