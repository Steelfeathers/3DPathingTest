using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class PathNode
    {
        private int gCost;  //Cost to reach this node from the start node
        private int hCost;  //Cost to reach the end node from this node, assuming no obstacles
        private int fCost;  //G + H, used for filtering 

        public PathNode previousNode;
    }
}
