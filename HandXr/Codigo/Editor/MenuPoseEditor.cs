using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoseEditor))]
public class MenuPoseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PoseEditor creator = (PoseEditor)target;
        if (GUILayout.Button("Create Pose"))
        {
            creator.SaveSubPose();
        }
         if (GUILayout.Button("Aplicar Pose"))
        {
            creator.ApplySubPose();
        }
    }
}
