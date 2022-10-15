using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class GridCell
    {
        public int gCost;  //Cost to reach this node from the start node
        public int hCost;  //Cost to reach the end node from this node, assuming no obstacles
        public int fCost;  //G + H, used for filtering 

        private GridCellData data;
        public GridCellData Data => data;

        public GridCell(int _xIndex, int _yIndex, Vector3 _vertexLL, Vector3 _vertexUL, Vector3 _vertexUR, Vector3 _vertexLR)
        {
            data = new GridCellData();
            data.xIndex = _xIndex;
            data.yIndex = _yIndex;
            data.walkable = true;
            data.Vertices = new[] { _vertexLL, _vertexUL, _vertexUR, _vertexLR };
            
            //Calculate the center of the cell, which will be used in pathfinding
            var dir = _vertexUR - _vertexLL;
            var mag = Vector3.Distance(_vertexUR, _vertexLL) / 2f;
            data.Center = (dir * mag) + _vertexLL;
        }
        
        public GridCell(GridCellData _data)
        {
            data = _data;
        }

        public void SetWalkable(bool canWalk)
        {
            data.walkable = canWalk;
        }
        
        //Calculate if the vertical slope of the cell is too steep to be walkable
        public bool CheckTooSteep(float maxVerticalDistance)
        {
            if (data.MaxY - data.MinY > maxVerticalDistance)
            {
                SetWalkable(false);
                return true;
            }

            return false;
        }

        #region PATHFINDING

        public GridCell PreviousCell;
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
        #endregion

        #region DEBUG
        public void DebugDrawCell()
        {
            if (data == null) return;
            Gizmos.color = data.walkable ? Color.white : Color.red;

            var verts = data.Vertices;
            Gizmos.DrawLine(verts[0], verts[1]);
            Gizmos.DrawLine(verts[1], verts[2]);
            Gizmos.DrawLine(verts[2], verts[3]);
            Gizmos.DrawLine(verts[3], verts[0]);
        }
        #endregion
    }
}
