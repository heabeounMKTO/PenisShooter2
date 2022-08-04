using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace BuildingMakerToolset.PropPlacer
{
    [CustomEditor( typeof( PropSwapper ) )]
    [CanEditMultipleObjects]
    public class PropSwapperEditor : Editor
    {
        protected List<EditList> editLists;


        protected class EditList
        {
            public List<PropSwapper> variantSwaperList;

            public EditList(List<PropSwapper> variants)
            {
                this.variantSwaperList = variants;
            }
        }
        string SplitString(string str)
        {
            return str.Split( char.Parse( " " ))[0].Split( char.Parse( "_" ) )[0].Split( char.Parse( "-" ) )[0].Split( char.Parse( "." ) )[0].Split( char.Parse( "/" ) )[0];
        }


        protected void CreateEditLists(List<PropSwapper> tgts)
        {
            if (tgts == null || tgts.Count == 0)
                return;
            if(editLists == null)
                editLists = new List<EditList>();
            int testRange = tgts[0].GetArrayRange();
            tgts[0].SyncCurIndex();
            int testIndex = tgts[0].curIndex;
            string testName = "";
            if(tgts[0].variants != null)
                testName = SplitString( tgts[0].variants.name );

            List<PropSwapper> missfits = new List<PropSwapper>();
            for (int i = 1; i < tgts.Count; i++)
            {
                bool hasDiffrendRanges = false;
                bool hasDiffrendIndexies = false;
                bool hasDiffrentNames = false;

                if (testRange != tgts[i].GetArrayRange())
                    hasDiffrendRanges = true;

                tgts[i].SyncCurIndex();
                if (testIndex != tgts[i].curIndex || (tgts[0].CurUsed == null && tgts[i].CurUsed !=null))
                    hasDiffrendIndexies = true;

                if (tgts[i].variants == null || testName != SplitString( tgts[i].variants.name ))
                    hasDiffrentNames = true;

                if (!hasDiffrendRanges && !hasDiffrendIndexies && !hasDiffrentNames)
                {
                    continue;
                }
                missfits.Add( tgts[i] );
                tgts.RemoveAt( i );
                i--;
            }
            editLists.Add( new EditList( tgts ) );
            if (missfits.Count > 0)
                CreateEditLists( missfits );
        }



        private void OnEnable()
        {
            
            foreach(PropSwapper tgt in targets)
            {
                tgt.gameObject.tag = "EditorOnly";
            }
            
            CreateEditLists( targets.Select( x => x as PropSwapper ).ToList() );
        }
        [CanEditMultipleObjects]
        public override void OnInspectorGUI()
        {

            foreach(EditList list in editLists)
            {
                InternalOnInspectorGUIMultiple( list.variantSwaperList );
            }

            if (targets.Length == 1)
                BuildingUnitEditor.DrawBuildingUnitEditor( target as BuildingUnit );
        }

        bool UserChangeSettings(PropSwapper tgt, bool isMultiSelection)
        {
            int arrRange = tgt.GetArrayRange();
            int curIndex = 0;
            GUILayout.BeginHorizontal();


            if (arrRange > 0)
            {
                if (tgt.CurUsed != null && GUILayout.Button( "Clear", GUILayout.Width( 50 ) ))
                {
                    tgt.SetVariant( null );
                    return true;
                }
                else if (tgt.CurUsed == null && GUILayout.Button( "Add", GUILayout.Width( 50 ) ))
                {
                    tgt.SetVariantFromGroupIndex( tgt.variants, tgt.curIndex );
                    return true;
                }
                curIndex = (int)(GUILayout.HorizontalSlider( tgt.curIndex, 0, arrRange - 1 ));
                GUILayout.Label( "" + (curIndex+1) + "/" + arrRange, GUILayout.ExpandWidth( false ) );
            }
            SerializedObject serializeTgt = new SerializedObject( tgt );
            SerializedProperty prefabArrayProp = serializeTgt.FindProperty( "variants" );
            serializeTgt.UpdateIfRequiredOrScript();
            EditorGUIUtility.labelWidth = 50;

            if (isMultiSelection)
                GUI.enabled = false;
            EditorGUILayout.PropertyField( prefabArrayProp, new GUIContent( tgt.variants != null ? SplitString( tgt.variants.name ): "new") );
            GUI.enabled = true;


            serializeTgt.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
            if (tgt.curIndex == curIndex)
                return false;
     

            tgt.curIndex = curIndex;
            tgt.SetVariantFromGroupIndex( tgt.variants, tgt.curIndex );
            return true;
        }


        protected void InternalOnInspectorGUIMultiple(List<PropSwapper> tgts)
        {
            if(UserChangeSettings( tgts[0], tgts.Count > 1 ))
            {
                foreach(PropSwapper tgt in tgts)
                {
                    tgt.curIndex = tgts[0].curIndex;
                    if (tgts[0].CurUsed == null)
                        tgt.SetVariant( null );
                    else
                        tgt.SetVariantFromGroupIndex( tgt.variants, tgt.curIndex );
                }
            }
        }


        protected virtual void OnSceneGUI()
        {
            BuildingUnitEditor.ScenePickingMode( target as BuildingUnit );
        }
    }
}
