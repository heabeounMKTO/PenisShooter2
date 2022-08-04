using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BuildingMakerToolset.PropPlacer
{
    public class PropSwapperHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool HasSpawnerComponent()
        {
            return gameObject.GetComponent<RowPropPlacer>() != null;
        }
#endif
    }
}
