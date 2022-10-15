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
        public GridCellData[] gridCellDatas;
    }
}
