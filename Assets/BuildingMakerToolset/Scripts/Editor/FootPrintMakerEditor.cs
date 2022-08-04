using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;


namespace BuildingMakerToolset.PlatformMaker
{

    [CustomEditor( typeof( FootPrintMaker ) )]
    [CanEditMultipleObjects]
    public class FloorGeneratorEditor : Editor
    {
        SerializedObject so;
        bool showBadSelectionWarning = false;
        private void OnEnable()
        {
            so = new SerializedObject( (FootPrintMaker)target );
        }
        [CanEditMultipleObjects]
        public override void OnInspectorGUI()
        {
            var tgt = (target as FootPrintMaker);
            if (Selection.objects.Length == 1 && GUILayout.Button( "Select neighbors" ))
                tgt.SelectNeighbors();

            if (GUILayout.Button( "Create shape from selection" ))
                showBadSelectionWarning = !tgt.TryGenerateShapeFromCurrentSelection();
            

            if (showBadSelectionWarning)
                EditorGUILayout.HelpBox( "could not create shape from selection", MessageType.Warning, true );
            

        }
    }
}
