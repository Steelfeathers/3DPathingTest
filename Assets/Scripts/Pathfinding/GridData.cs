using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    [Serializable]
    public class GridData
    {
        public int cols;
        public int rows;
        public float cellSize;
        public float[] originCoords;
        public GridCellData[] gridCellDatas;

        [NonSerialized] private Vector3 origin;
        public Vector3 Origin
        {
            get
            {
                if (origin == default(Vector3))
                {
                    origin = new Vector3(originCoords[0], originCoords[1], originCoords[2]);
                }

                return origin;
            }
            set
            {
                origin = value;
                originCoords = new[] { origin.x, origin.y, origin.z };
            }
        }
    }
}
