using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BuildingMakerToolset.PropPlacer {
    [CustomEditor( typeof( SwappedProp ) )]
    public class PropSwapperSlaveEditor : Editor
    {
        private void OnEnable()
        {
            var tgt = (SwappedProp)target;
            if (tgt.owner == null)
            {
                DestroyImmediate( tgt );
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            var tgt = (target as SwappedProp);
            if (GUILayout.Button( "Select Master" ))
                Selection.activeObject = tgt.owner;
            
        }
    }
}
