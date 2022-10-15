using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnknownWorldsTest
{
    [CustomEditor(typeof(GridBuilder))]
    class GridBuilderEditor : Editor
    {
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
}
