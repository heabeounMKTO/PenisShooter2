using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace BuildingMakerToolset.PropPlacer
{
    [CustomEditor( typeof( PropSwapperHelper ) )]
    [CanEditMultipleObjects]
    public class PropSwapperHelperEditor : PropSwapperEditor
    {
        List<PropSwapper> variantSwapers;
        private void OnEnable()
        {
            if (variantSwapers == null)
                variantSwapers = new List<PropSwapper>();
            foreach (PropSwapperHelper tgt in targets)
            {
                variantSwapers.AddRange( tgt.transform.GetComponentsInChildren<PropSwapper>() );
            }

            CreateEditLists( variantSwapers );
        }

        public override void OnInspectorGUI()
        {
            PropSwapperHelper tgt = target as PropSwapperHelper;
            if (targets.Length == 1)
            {
                if (tgt.HasSpawnerComponent() && GUILayout.Button( "Select neighbors" ))
                {
                    Selection.objects = tgt.gameObject.GetComponent<RowPropPlacer>().GetNeighborsWithScript<PropSwapperHelper>().Select( x => x.gameObject ).ToArray();
                }
            }
          
            if (editLists == null || editLists.Count == 0)
                return;
            foreach (EditList list in editLists)
            {
                InternalOnInspectorGUIMultiple( list.variantSwaperList );
            }
        }

    }
}
