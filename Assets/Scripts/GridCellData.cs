using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    [Serializable]
    public class GridCellData
    {
        public int xIndex;
        public int yIndex;
        public bool walkable = true;

        [NonSerialized] private Vector3 center;
        public float centerX;
        public float centerY;
        public float centerZ;

        [NonSerialized] private Vector3[] vertices;
        public float vertexLL_X;
        public float vertexLL_Y;
        public float vertexLL_Z;
        
        public float vertexUL_X;
        public float vertexUL_Y;
        public float vertexUL_Z;
        
        public float vertexUR_X;
        public float vertexUR_Y;
        public float vertexUR_Z;
        
        public float vertexLR_X;
        public float vertexLR_Y;
        public float vertexLR_Z;

        #region GETTERS/SETTERS
        public Vector3 Origin => Vertices[0];

        public float MinY => Mathf.Min(vertexLL_Y, vertexUL_Y, vertexUR_Y, vertexLR_Y);
        public float MaxY => Mathf.Max(vertexLL_Y, vertexUL_Y, vertexUR_Y, vertexLR_Y);
        
        
        public Vector3 Center
        {
            get
            {
                if (center == default(Vector3))
                    center = new Vector3(centerX, centerY, centerZ);
                return center;
            }
            set
            {
                center = value;
                centerX = center.x;
                centerY = center.y;
                centerZ = center.z;
            }
        }
        
        public Vector3[] Vertices
        {
            get
            {
                return vertices ??= new[]
                {
                    new Vector3(vertexLL_X, vertexLL_Y, vertexLL_Z),
                    new Vector3(vertexUL_X, vertexUL_Y, vertexUL_Z),
                    new Vector3(vertexUR_X, vertexUR_Y, vertexUR_Z),
                    new Vector3(vertexLR_X, vertexLR_Y, vertexLR_Z)
                };
            }
            set
            {
                vertices = value;
                vertexLL_X = vertices[0].x;
                vertexLL_Y = vertices[0].y;
                vertexLL_Z = vertices[0].z;
            
                vertexUL_X = vertices[1].x;
                vertexUL_Y = vertices[1].y;
                vertexUL_Z = vertices[1].z;
            
                vertexUR_X = vertices[2].x;
                vertexUR_Y = vertices[2].y;
                vertexUR_Z = vertices[2].z;
            
                vertexLR_X = vertices[3].x;
                vertexLR_Y = vertices[3].y;
                vertexLR_Z = vertices[3].z;
            }
        }
        #endregion
    }
}
