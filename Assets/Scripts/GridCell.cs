using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public class GridCell
    {
        private Vector3 origin; //Bottom left corner on an XZ top-down grid
        private Vector3 center;
        private float highestPoint;
        private Vector3[] vertices; //Clockwise from bottom left corner
        private GridCell[] neighborCells; //Clockwise from bottom left corner

        public bool Walkable = true;
        public Vector3 Center => center;
        public Vector3 Origin => origin;
        public float HighestPoint => highestPoint;


        public GridCell(Vector3 vertexLL, Vector3 vertexUL, Vector3 vertexUR, Vector3 vertexLR)
        {
            vertices = new Vector3[4] { vertexLL, vertexUL, vertexUR, vertexLR };
            origin = vertexLL;
            highestPoint = Mathf.Max(vertexLL.y, vertexUL.y, vertexUR.y, vertexLR.y);
            
            //Calculate the center of the cell, which will be used in pathfinding
            var dir = vertices[2] - vertices[0];
            var mag = Vector3.Distance(vertices[2], vertices[0]) / 2f;
            center = (dir * mag) + vertices[0];
        }

        //Calculate if the vertical slope of the cell is too steep to be walkable
        public bool CheckTooSteep(float maxVerticalDistance)
        {
            float minY = Single.PositiveInfinity, maxY = Single.NegativeInfinity;
            foreach (var vertex in vertices)
            {
                if (vertex.y < minY) minY = vertex.y;
                if (vertex.y > maxY) maxY = vertex.y;
            }

            if (maxY - minY > maxVerticalDistance)
            {
                Walkable = false;
                return true;
            }

            return false;
        }

        public void DebugDrawCell()
        {
            Gizmos.color = Walkable ? Color.white : Color.red;
            
            Gizmos.DrawLine(vertices[0], vertices[1]);
            Gizmos.DrawLine(vertices[1], vertices[2]);
            Gizmos.DrawLine(vertices[2], vertices[3]);
            Gizmos.DrawLine(vertices[3], vertices[0]);
        }
    }
}
