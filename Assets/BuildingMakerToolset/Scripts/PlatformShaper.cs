using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BuildingMakerToolset.Geometry;
using System.Linq;
namespace BuildingMakerToolset.PlatformMaker
{
    public class PlatformShaper : MonoBehaviour
    {
#if UNITY_EDITOR
        [System.NonSerialized]
        public bool enableEditing = false;
        [System.NonSerialized]
        public bool meshGenerationError = false;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;


        public int shapeMask = -1;
        [HideInInspector]
        public List<Shape> shapes = new List<Shape>();


        [HideInInspector]
        public bool showShapesList;

        public float handleRadius = .2f;


        List<GameObject> shapeGameObjects;
        public void UpdateMeshDisplay(bool generateUV2 = true)
        {
            SetShapesHight();
            CompositeShape compShape = new CompositeShape( this.shapes );
            CompositeShape.ShapeMeshData[] shapeMeshes = compShape.GetShapeMeshData( transform , generateUV2 );

            if (shapeMeshes == null)
            {
                meshGenerationError = true;
                return;
            }
            meshGenerationError = false;


            if (shapeGameObjects == null)
                shapeGameObjects = new List<GameObject>();
            if(shapeGameObjects.Count > shapeMeshes.Length)
            {
                int removeFor = shapeGameObjects.Count - shapeMeshes.Length;
                shapeGameObjects.RemoveRange( 0, removeFor );
            }

            for (int i = 0; i < shapeMeshes.Length; i++)
            {
                if (i < shapeGameObjects.Count)
                    shapeGameObjects[i] = UpdateGameObjectMesh( shapeGameObjects[i], shapeMeshes[i].backedMesh, shapeMeshes[i].material );
                else
                    shapeGameObjects.Add( UpdateGameObjectMesh( null, shapeMeshes[i].backedMesh, shapeMeshes[i].material ) );
            }

            ClearUnusedChildren();

        }

        GameObject UpdateGameObjectMesh(GameObject go, Mesh mesh, Material material)
        {

            if (go == null)
                return MeshToGameObjcet( mesh, material );

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            MeshFilter mf = go.GetComponent<MeshFilter>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            MeshCollider mc = go.GetComponent<MeshCollider>();
            ProcedualMeshCache pmc = go.GetComponent<ProcedualMeshCache>();

            if (mr == null || mf == null || mc == null || pmc == null)
            {
                DestroyImmediate( go );
                return MeshToGameObjcet( mesh, material );
            }

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            pmc.SaveMesh( mesh, mf, mc );
            go.isStatic = true;
            if (material != null)
                mr.material = material;
            return go;
        }
        void ClearUnusedChildren()
        {
            int curChild = 0;
            while (transform.childCount > 0)
            {
                if (curChild >= transform.childCount)
                    return;
                GameObject curTestChild = transform.GetChild( curChild ).gameObject;
                if (curTestChild.name.Contains( "ShapeMesh" ) && !shapeGameObjects.Contains( curTestChild ))
                    DestroyImmediate( curTestChild );
                else
                    curChild++;
            }
        }
        GameObject MeshToGameObjcet(Mesh mesh, Material material)
        {
            GameObject go = new GameObject();
            go.name = "ShapeMesh(" + (material != null ? material.name : "") + ")";
            go.transform.SetParent( transform );
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();
            ProcedualMeshCache pmc = go.AddComponent<ProcedualMeshCache>();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            pmc.SaveMesh( mesh, mf, mc );

            mr.material = material;

            go.isStatic = true;

            return go;
        }


        void SetShapesHight()
        {

            if (shapes == null || shapes.Count == 0)
                return;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].points == null || shapes[i].points.Count == 0)
                    continue;
                for (int y = 0; y < shapes[i].points.Count; y++)
                {
                    Vector3 tmp = shapes[i].points[y];
                    tmp.y = transform.position.y;
                    shapes[i].points[y] = tmp;
                }
            }
        }

#endif
    }
}