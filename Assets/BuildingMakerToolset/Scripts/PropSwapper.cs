using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BuildingMakerToolset.PropPlacer { 
    public class PropSwapper : BuildingUnit
    {
#if UNITY_EDITOR
        [System.NonSerialized]
        public int curIndex;

        public PropGroup variants;
        public GameObject curUsedPrefab;
        SwappedProp curUsed;
        public SwappedProp CurUsed
        {
            get
            {
                if (curUsed != null)
                    return curUsed;
                SwappedProp cu;
                for (int i = 0; i < pseudoChildren.Count; i++)
                {
                    if (pseudoChildren[i] == null)
                        continue;
                    if (pseudoChildren[i].TryGetComponent( out cu ) && cu.owner == this)
                    {
                        return cu;
                    }
                }
                return null;
            }
            set
            {
                curUsed = value;
            }
        }

        private void OnDestroy()
        {
            if (CurUsed != null && CurUsed.gameObject.activeInHierarchy && Application.isEditor && !BuildPipeline.isBuildingPlayer)
            {
                if (CurUsed.owner != this)
                    return;
                Undo.DestroyObjectImmediate( CurUsed );
            }
        }

        public void SyncCurIndex()
        {
            curIndex = 0;
            if (GetArrayRange() == 0)
                return;
            if (curUsedPrefab == null)
                return;
            for(int i = 0; i < variants.prefabs.Length; i++)
            {
                if ( curUsedPrefab == variants.prefabs[i])
                {
                    curIndex = i;
                    break;
                }
            }
            
        }

        public int GetArrayRange()
        {
      
            if (variants == null || variants.prefabs == null || variants.prefabs.Length == 0)
                return 0;
            return variants.prefabs.Length;
        }

        public void SetVariantFromGroupIndex(PropGroup variants , int index)
        {
            if (GetArrayRange() == 0)
                return;

            int tgtIndex = Mathf.Abs( index ) % variants.prefabs.Length;
            GameObject newVariant = variants.prefabs[tgtIndex];
            if(newVariant== null)
            {
                Debug.LogError( "null reference in " + variants.name + "(PrefabGroup) @ index: " + tgtIndex );
                return;
            }
            SetVariant( newVariant );
        }




        public void SetVariant(GameObject newVariant)
        {
       
            if (curUsedPrefab == newVariant && CurUsed != null)
                return;
            Undo.IncrementCurrentGroup();
            Undo.RecordObject( this, "swap" );
            EditorUtility.SetDirty( this );
            if (CurUsed != null)
            {
                if(CurUsed.transform.parent != null && BHUtilities.IsPrefab( CurUsed.transform.parent.gameObject ))
                {
                    Debug.LogWarning( "Can´t edit a prefab. If you want to edit please unpack the prefab: "+ CurUsed.transform.parent.name );
                    return;
                }
                if (Selection.activeObject == CurUsed)
                {
                    Selection.activeObject = this.gameObject;
                }
                Undo.DestroyObjectImmediate( CurUsed.gameObject );
            }

            if (newVariant == null || !BHUtilities.IsPrefab( newVariant))
            {
                curUsedPrefab = null;
                CurUsed = null;
                return;
            }
            curUsedPrefab = newVariant;
            
            Transform tgtParent = BHUtilities.FindMeshRelativeToTransform(transform).transform;
            if (tgtParent == null)
            {
                tgtParent = transform;
                if (PrefabUtility.GetPrefabInstanceStatus( tgtParent ) == PrefabInstanceStatus.Connected)
                    tgtParent = PrefabUtility.GetNearestPrefabInstanceRoot( tgtParent ).transform;
            }
            if (tgtParent.IsChildOf( transform ))
                tgtParent = transform;
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab( curUsedPrefab, tgtParent.parent );
            Undo.RegisterCreatedObjectUndo( go, "swap" );
            CurUsed = go.AddComponent<SwappedProp>();
            CurUsed.owner = this;
            CurUsed.transform.localScale = tgtParent== null ? Vector3.one : tgtParent.lossyScale;
            CurUsed.transform.SetPositionAndRotation(transform.position, transform.rotation);

            BuildingUnit bu = null;
            if (!CurUsed.gameObject.TryGetComponent<BuildingUnit>(out bu))
                bu = CurUsed.gameObject.AddComponent<BuildingUnit>();
            

           
            AddChild( bu );
            CheckIfValid();
        }
#endif
    }
}
