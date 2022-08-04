using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;
using BuildingMakerToolset;
namespace BuildingMakerToolset.MeshCombiner
{
# if UNITY_EDITOR
    public class MeshExtended
    {
        public int submeshId = 0;
        public MeshRenderer meshRenderer;
        public Transform transform;
        public Mesh mesh;
    }
    public class PropCombiner : MonoBehaviour
    {
        public Transform ogParent;
        public GameObject combineParent;
        public GameObject resultGO;

        public MeshRenderer[] originalRenderers;
        public LODGroup[] originalLODGroups;
        public bool is32bit = true;

        public void UnCombine()
        {
            
            if (resultGO == null)
                return;
            string actionName = "uncombine";
            Undo.RecordObject( Selection.activeObject, actionName );
            Undo.SetTransformParent( combineParent.transform, resultGO.transform.parent, actionName );
            Undo.RecordObject( combineParent, actionName );
            combineParent.transform.SetSiblingIndex( resultGO.transform.GetSiblingIndex() );
            combineParent.SetActive( true );
            if (originalRenderers != null && originalRenderers.Length > 0)
                for (int i = 0; i < originalRenderers.Length; i++)
                    if (originalRenderers[i] != null)
                    {
                        Undo.RecordObject( originalRenderers[i], actionName );
                        originalRenderers[i].enabled = true;
                    }


            originalLODGroups = combineParent.GetComponentsInChildren<LODGroup>().ToArray();
            if (originalLODGroups != null && originalLODGroups.Length > 0)
                for (int i = 0; i < originalLODGroups.Length; i++)
                {
                    Undo.RecordObject( originalLODGroups[i], actionName );
                    originalLODGroups[i].enabled = true;
                }
            Selection.activeObject = combineParent.gameObject;
            Undo.RecordObjects( resultGO.GetComponentsInChildren<MeshRenderer>(), actionName );
            Undo.DestroyObjectImmediate( resultGO );
            combineParent.transform.SetParent(ogParent);
        }
        class LODCollector
        {
            public List<MeshFilter> meshFilterComponents;
            public LODCollector(List<MeshFilter> list)
            {
                meshFilterComponents = list;
                if (meshFilterComponents == null)
                    meshFilterComponents = new List<MeshFilter>();
            }
        }
        public void Combine()
        {
            if (BHUtilities.IsPrefab( gameObject ))
            {
                Debug.LogWarning( "Can�t combine/uncombine a prefab. If you want to combine/uncombine please unpack the prefab: " + gameObject.name );
                return;
            }
            string actionName = "combine";
            if (combineParent == null)
                combineParent = this.gameObject;

            ogParent = combineParent.transform.parent;

            if (resultGO != null)
            {
                UnCombine();
            }

            
            resultGO = new GameObject( "_" + combineParent.name );
            Undo.RecordObject( Selection.activeObject, actionName );
            Undo.RegisterCreatedObjectUndo( resultGO, actionName );
            // Remember the original position of the object. 
            // For the operation to work, the position must be temporarily set to (0,0,0).

            Undo.RecordObject( combineParent.transform, actionName );
            Vector3 originalPosition = combineParent.transform.position;
            combineParent.transform.position = Vector3.zero;

            List<MeshFilter> meshFilterComponents = combineParent.GetComponentsInChildren<MeshFilter>().ToList();
            originalLODGroups = combineParent.GetComponentsInChildren<LODGroup>().Where(x=>x.enabled && x.gameObject.activeInHierarchy && x.gameObject.isStatic).ToArray();

            Undo.RecordObjects( meshFilterComponents.ToArray(), actionName );
            Undo.RecordObjects( originalLODGroups, actionName );

            List<LODCollector> LODCollectors = new List<LODCollector>();
            for (int i = 0; i< originalLODGroups.Length; i++)
            {
                LOD[] lods = originalLODGroups[i].GetLODs();
                bool deactivate = true;
                for (int j = 0; j< lods.Length;j++)
                {
                    Renderer[] renderers = lods[j].renderers;
                    for(int y=0;y< renderers.Length;y++)
                    {
                        if(renderers[y].gameObject.TryGetComponent(out MeshFilter mf ))
                        {
                            if (!mf.gameObject.isStatic)
                            {
                                deactivate = false;
                                continue;
                            }
                            if (LODCollectors.Count < j + 1)
                            {
                                LODCollectors.Add( new LODCollector(null) );
                            }
                            LODCollectors[j].meshFilterComponents.Add( mf );
                            meshFilterComponents.Remove(mf);
                        }
                    }
                }
                if (deactivate)
                    originalLODGroups[i].enabled = false;
            }

            //remove non static objects
            for (int i = 0; i < meshFilterComponents.Count; i++)
            {
                if (!meshFilterComponents[i].gameObject.isStatic)
                {
                    bool parentFound = false;
                    Transform curParent = meshFilterComponents[i].transform;
                    while (!parentFound)
                    {
                        if(curParent.parent.GetComponent<MeshFilter>()!=null && curParent.parent.gameObject.isStatic || curParent.parent == combineParent.transform)
                        {
                            parentFound = true; 
                        }
                        else
                        {
                            curParent = curParent.parent;
                        }
                    }

                    MeshFilter[] removeList = curParent.GetComponentsInChildren<MeshFilter>();

                    meshFilterComponents.Remove( meshFilterComponents[i] );
                    for (int j = 0; j < removeList.Length; j++)
                    {
                        meshFilterComponents.Remove( removeList[j] );
                    }
                    i--;
                }
            }
            //remove disabled
            for (int i = 0; i < meshFilterComponents.Count; i++)
            {
                if (!meshFilterComponents[i].gameObject.activeInHierarchy)
                {
                    meshFilterComponents.RemoveAt(i);
                    i--;
                    continue;
                }
                if(meshFilterComponents[i].gameObject.TryGetComponent(out MeshRenderer mr ))
                {
                    if (!mr.enabled)
                    {
                        meshFilterComponents.RemoveAt( i );
                        i--;
                        continue;
                    }
                }
                else
                {
                    meshFilterComponents.RemoveAt( i );
                    i--;
                    continue;
                }
            }

            List<MeshRenderer> oRendersrs = meshFilterComponents.Select( x => x.GetComponent<MeshRenderer>() ).ToList();

            for (int i = 0; i < LODCollectors.Count; i++) {
                oRendersrs.AddRange( LODCollectors[i].meshFilterComponents.Select( x => x.GetComponent<MeshRenderer>() ).ToList() );
            }
            originalRenderers = oRendersrs.ToArray();
            Undo.RecordObjects( originalRenderers, actionName );
            for (int i = 0; i < originalRenderers.Length; i++)
            {
                originalRenderers[i].enabled = false;
            }
            

            LOD[] newLods = new LOD[LODCollectors.Count];
            List<GameObject> notLods = CombineMeshFilters( meshFilterComponents );
            List<GameObject> withLods = new List<GameObject>();
            string lodToken = "   /LOD - ";
            for (int i = 0; i< LODCollectors.Count; i++)
            {
                List<GameObject> wlod = CombineMeshFilters( LODCollectors[i].meshFilterComponents, lodToken + i ).ToList();
                withLods.AddRange( wlod );
                List<Renderer> renderers= wlod.Select(x=>x.GetComponent<MeshRenderer>() as Renderer).ToList();
                renderers.AddRange( notLods.Select( x => x.GetComponent<MeshRenderer>() as Renderer ).ToList() );
                newLods[i].renderers = renderers.ToArray();
                newLods[i].screenRelativeTransitionHeight =0.8f- ((float)i / (float)LODCollectors.Count)*0.7f;
            }
            if (newLods.Length > 0)
            {
                LODGroup newLod = resultGO.AddComponent<LODGroup>();
                newLod.SetLODs( newLods );
            }

            ReOrganize( withLods.Union( notLods ).ToList(), lodToken );

            combineParent.transform.position = originalPosition;
            resultGO.transform.position = originalPosition;
            resultGO.transform.SetSiblingIndex( combineParent.transform.GetSiblingIndex() );
            Undo.SetTransformParent( combineParent.transform, resultGO.transform, actionName );
            PropCombiner combiner = resultGO.AddComponent<PropCombiner>();
            combiner.ogParent = ogParent;
            combiner.combineParent = combineParent;
            combiner.originalRenderers = originalRenderers;
            combiner.resultGO = resultGO;
            Selection.activeObject = resultGO.gameObject;
            resultGO.transform.SetParent(ogParent);
        }
        void ReOrganize(List<GameObject> objects, string lodToken)
        {
            List<GameObject> objectsWithLODs = objects.Where( x => x.name.Contains( lodToken ) ).ToList();
            Dictionary<string, List<GameObject>> lodRelativeObjects = new Dictionary<string, List<GameObject>>();


            string[] separator = new string[] { lodToken };
            for (int i = 0; i < objectsWithLODs.Count; i++)
            {
                string objectName = objectsWithLODs[i].name.Split( separator, System.StringSplitOptions.None )[0];
                if (lodRelativeObjects.ContainsKey( objectName ))
                    lodRelativeObjects[objectName].Add( objectsWithLODs[i] );
                else
                    lodRelativeObjects.Add( objectName, new List<GameObject>() { objectsWithLODs[i] } );
                objects.Remove( objectsWithLODs[i] );
            }
           
            foreach (var entry in lodRelativeObjects)
            {
                GameObject anchorOverride = new GameObject( "AnchorOverride " + entry.Key );
                Transform parentTransform = entry.Value[0].transform;
                anchorOverride.transform.SetParent( parentTransform );
                for(int i = 0; i< entry.Value.Count; i++)
                {
                    MeshRenderer mr = entry.Value[i].GetComponent<MeshRenderer>();
                    if (mr == null)
                        continue;

                    mr.probeAnchor = anchorOverride.transform;
                    if (i == 0)
                        anchorOverride.transform.position = mr.bounds.center;
                    else
                        mr.transform.SetParent( parentTransform );
                }
            }

            foreach( var obj in objects)
            {
                MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                if (mr == null)
                    continue;
                GameObject anchorOverride = new GameObject( "AnchorOverride " + obj.name );
                anchorOverride.transform.SetParent( obj.transform );
                anchorOverride.transform.position = mr.bounds.center;
                mr.probeAnchor = anchorOverride.transform;

            }




        }


        List<GameObject> CombineMeshFilters(List<MeshFilter> meshFilterComponents, string name = "")
        {
            List<MeshExtended> meshInfos = new List<MeshExtended>();
            List<GameObject> combinedObjects = new List<GameObject>();
            Dictionary<Material, List<MeshExtended>> materialToMeshFilterList = new Dictionary<Material, List<MeshExtended>>();
            for (int i = 0; i < meshFilterComponents.Count; i++)
            {
                if (meshFilterComponents[i].sharedMesh == null)
                    continue;
                MeshExtended newMesh = new MeshExtended();
                newMesh.mesh = (Mesh)Object.Instantiate( meshFilterComponents[i].sharedMesh );
                newMesh.transform = meshFilterComponents[i].transform;
                newMesh.meshRenderer = meshFilterComponents[i].GetComponent<MeshRenderer>();

                if (newMesh.mesh.subMeshCount > 1)
                {
                    for (int sm = 0; sm < newMesh.mesh.subMeshCount; sm++)
                    {
                        MeshExtended subMesh = new MeshExtended();
                        subMesh.submeshId = sm;
                        subMesh.mesh = newMesh.mesh.GetSubmesh( sm );
                        subMesh.transform = newMesh.transform;
                        subMesh.meshRenderer = newMesh.meshRenderer;
                        meshInfos.Add( subMesh );
                    }
                    continue;
                }

                meshInfos.Add( newMesh );
            }



            // Go through all mesh filters and establish the mapping between the materials and all mesh filters using it.
            for (int i = 0; i < meshInfos.Count; i++)
            {

                var meshRenderer = meshInfos[i].meshRenderer;
                if (meshRenderer == null)
                {
                    continue;
                }

                var materials = meshRenderer.sharedMaterials;
                if (materials == null)
                {
                    continue;
                }

                var material = materials[meshInfos[i].submeshId];
                if (material == null)
                    continue;
                // Add material to mesh filter mapping to dictionary
                if (materialToMeshFilterList.ContainsKey( material )) materialToMeshFilterList[material].Add( meshInfos[i] );
                else materialToMeshFilterList.Add( material, new List<MeshExtended>() { meshInfos[i] } );
            }

            foreach (var entry in materialToMeshFilterList)
            {
                List<MeshExtended> meshesWithSameMaterial = entry.Value;
                // Create a convenient material name
                string materialName = entry.Key.ToString().Split( ' ' )[0];

                CombineInstance[] combine = new CombineInstance[meshesWithSameMaterial.Count];
                for (int i = 0; i < meshesWithSameMaterial.Count; i++)
                {
                    combine[i].mesh = meshesWithSameMaterial[i].mesh;
                    combine[i].transform = meshesWithSameMaterial[i].transform.localToWorldMatrix;
                }

                // Create a new mesh using the combined properties
                var format = is32bit ? IndexFormat.UInt32 : IndexFormat.UInt16;
                Mesh combinedMesh = new Mesh { indexFormat = format };
                combinedMesh.CombineMeshes( combine );
                // Create game object
                string goName = "combined " +( (materialToMeshFilterList.Count > 1) ?  materialName : combineParent.name)  +name;
                GameObject combinedObject = new GameObject( goName );
                combinedObject.isStatic = true;
                var filter = combinedObject.AddComponent<MeshFilter>();
                UnityEditor.Unwrapping.GenerateSecondaryUVSet( combinedMesh );
                filter.sharedMesh = combinedMesh;
                var renderer = combinedObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = entry.Key;
                var pmc = combinedObject.AddComponent<ProcedualMeshCache>();
                pmc.SaveMesh( combinedMesh, filter, null );
                combinedObjects.Add( combinedObject );
            }

            // If there were more than one material, and thus multiple GOs created, parent them and work with result
            foreach (var combinedObject in combinedObjects)
            {
                combinedObject.transform.parent = resultGO.transform;
            }

            return combinedObjects;

        }

    }
#endif
}