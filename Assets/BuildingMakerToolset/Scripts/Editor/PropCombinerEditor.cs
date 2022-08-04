using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BuildingMakerToolset.MeshCombiner {
    [CustomEditor( typeof( PropCombiner ) )]
    [CanEditMultipleObjects]
    public class PropCombinerEditor : Editor
    {

        
        [CanEditMultipleObjects]
        public override void OnInspectorGUI()
        {
            PropCombiner tgt = (PropCombiner)target;
            if (BHUtilities.IsPrefab( tgt.gameObject ))
            {
                EditorGUILayout.HelpBox( "Can´t edit a prefab. If you want to edit please unpack the prefab", MessageType.Warning );
                return;
            }
            if (GUILayout.Button( "Pack" ))
            {
                for (int i = 0; i < targets.Length; i++) {
                    PropCombiner pc = targets[i] as PropCombiner;
                    pc.Combine();
                }
            }
            if (GUILayout.Button( "Unpack" ))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    PropCombiner pc = targets[i] as PropCombiner;
                    pc.UnCombine();
                }
            }
        }
    }
}
