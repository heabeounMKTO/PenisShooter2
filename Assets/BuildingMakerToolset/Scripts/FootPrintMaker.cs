using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BuildingMakerToolset.Geometry;
using BuildingMakerToolset.PropPlacer;
using UnityEditor;
namespace BuildingMakerToolset.PlatformMaker
{
    [RequireComponent( typeof(RowPropPlacer) )]
    public class FootPrintMaker : MonoBehaviour
    {
#if UNITY_EDITOR
        string gameObjectName = "Building-footprint-shape";
        [System.NonSerialized]
        public Vector3[] polygonVectorArray;
        RowPropPlacer spawner;
        public RowPropPlacer GetSpawner()
        {
            if(spawner==null)
                spawner = gameObject.GetComponent<RowPropPlacer>();
            return spawner;
        }



        public void SelectNeighbors()
        {
            if (TryGetComponent( out RowPropPlacer spnr ))
                Selection.objects = spnr.GetNeighborsWithScript<FootPrintMaker>().Select(x => x.gameObject).ToArray();
        }
        public bool TryGenerateShapeFromCurrentSelection()
        {
            List<RowPropPlacer> rpList = Selection.objects.Where( x => x.GetType() == typeof( GameObject ) ).Select( x => x as GameObject ).Where( x => x.GetComponent<FootPrintMaker>() != null ).Select( x => x.GetComponent<FootPrintMaker>().GetSpawner() ).ToList();
            return TryGenerateShape( rpList );
        }

        public bool TryGenerateShape(List<RowPropPlacer> rowPropPlasers)
        {
            if (rowPropPlasers == null || rowPropPlasers.Count == 0)
                return false;

            if (TryGetComponent( out RowPropPlacer spnr ))
            {
                polygonVectorArray = SimplifyPointLine( GenerateFootprintPolygonFromSelection( rowPropPlasers ) ).ToArray();
            }
            else
                return false;

            if (polygonVectorArray.Length < 3)
                return false;

            GameObject footprint = new GameObject();
            
            footprint.transform.SetParent( transform.parent );
            Undo.RegisterCreatedObjectUndo( footprint, "generate Shape" );
            footprint.name = gameObjectName;
            footprint.transform.position = BHUtilities.AvaragePosition( polygonVectorArray );
            footprint.transform.rotation = transform.rotation;
            
            PlatformShaper sc = footprint.AddComponent<PlatformShaper>();
            Shape footPrintShape = new Shape();
            footPrintShape.points = polygonVectorArray.ToList();
            for(int i = 0; i< footPrintShape.points.Count; i++)
            {
                footPrintShape.points[i] = footprint.transform.InverseTransformPoint( footPrintShape.points[i] );
            }
            sc.shapes.Add( footPrintShape );
            sc.UpdateMeshDisplay();

            Selection.activeObject = footprint;

            return true;
        }


        struct Point
        {
            public Vector3 position;
            public bool isCorner;
            public bool createCornerPoint;
            public bool altCreateCornerPoint;
            public Point(Vector3 pos, bool createcornerpoint = false, bool iscorner = false, bool altcreatecornerpoint = false)
            {
                position = pos;
                createCornerPoint = createcornerpoint;
                isCorner = iscorner;
                altCreateCornerPoint = altcreatecornerpoint;
            }

            public bool Equals(Point other)
            {
                return other.createCornerPoint == createCornerPoint && other.position == position && other.isCorner == isCorner;
            }
        }

        List<Vector3> GenerateFootprintPolygonFromSelection(List<RowPropPlacer> neighborList)
        {
            if (neighborList == null || neighborList.Count == 0)
                return null;
            neighborList = RowPropPlacer.SortedNeighbors( neighborList );
            List<Point> pointList = new List<Point>();
            Point prevPoint = new Point(Vector3.zero);

            for(int i = 0; i< neighborList.Count; i++)
            {
                RowPropPlacer cur = neighborList[i];
                Point curPoint = new  Point(cur.SpawnPivotWorldPosition);

                if (i == 0 || !prevPoint.Equals(curPoint))
                {

                    float angle = cur.spawnPivot.pivRotation.magnitude;
                    curPoint.createCornerPoint = angle == 90;
                    curPoint.isCorner = angle == 180;
                    pointList.Add( curPoint );
                }
                prevPoint = curPoint;
            }


            AddCornerPoints(ref pointList);

            List<Vector3> refinedVectorList = new List<Vector3>();
            for (int i = 0; i < pointList.Count; i++)
            {
                refinedVectorList.Add(pointList[i].position);
            }


            return refinedVectorList;
        }



        void AddCornerPoints(ref List<Point> pointList)
        {
            int iteration = 0;
        Repeat:
            bool needRepeat = false;
            for (int i = 0; i < pointList.Count; i++)
            {

                Point curPoint = pointList[i];
                if (!curPoint.createCornerPoint )
                    continue;


                Point revPointA = pointList[(i - 1 + pointList.Count) % pointList.Count];
                Point revPointB = pointList[(i - 2 + pointList.Count) % pointList.Count];
                

                float distance;
                Vector3 newCornerVector;

                if (revPointA.isCorner || revPointA.createCornerPoint  || revPointB.createCornerPoint || curPoint.altCreateCornerPoint)
                {
                    needRepeat = true;
                    Point fwdPointA = pointList[(i + 1) % pointList.Count];

                    if (fwdPointA.createCornerPoint)
                        continue;

                    distance = Vector3.Distance(revPointA.position, curPoint.position);
                    newCornerVector = curPoint.position - (fwdPointA.position - curPoint.position).normalized * Mathf.Sqrt((distance * distance) / 2);

                    curPoint.createCornerPoint = false;
                    pointList[i] = curPoint;
                    pointList.Insert(i, new Point(newCornerVector, false, true));
                    continue;
                }

                distance = Vector3.Distance(revPointA.position, curPoint.position);
                newCornerVector = revPointA.position + (revPointA.position - revPointB.position).normalized * Mathf.Sqrt((distance * distance) / 2);

                if (Vector3.Angle(newCornerVector - revPointA.position, curPoint.position - revPointA.position) <= 1f)
                {
                    needRepeat = true;
                    curPoint.altCreateCornerPoint = true;
                    pointList[i] = curPoint;
                    continue;
                }

                curPoint.createCornerPoint = false;
                pointList[i] = curPoint;
                pointList.Insert(i, new Point(newCornerVector, false, true));
            }
            if (needRepeat && iteration< 100)
            {
                iteration++;
                goto Repeat;
            }
        }








        List<Vector3> SimplifyPointLine(List<Vector3> vectorList)
        {
            if (vectorList == null || vectorList.Count == 0)
                return null;
            if (vectorList.Count < 3)
                return vectorList;
            List<Vector3> simplified = new List<Vector3>();
            for (int i = 0; i< vectorList.Count; i++)
            {
                Vector3 curPoint = vectorList[i];
                Vector3 nextPoint = curPoint;
                Vector3 lastPoint = curPoint;
                int iter = 0;
                int testIndex = i;
                while (Vector3.Distance(nextPoint, curPoint) < 0.01f && iter < vectorList.Count + i)
                {
                    if (testIndex != i)
                    {
                        vectorList.RemoveAt(testIndex);
                    }
                    iter++;
                    testIndex = (i + iter) % vectorList.Count;
                    nextPoint = vectorList[testIndex];
                }




                iter = 0;
                testIndex = i;
                while (Vector3.Distance(lastPoint, curPoint) < 0.01f && iter < vectorList.Count + i)
                {
                    if (testIndex != i)
                    {
                        vectorList.RemoveAt(testIndex);
                    }
                    iter++;
                    testIndex = (i - iter + vectorList.Count) % vectorList.Count;
                    lastPoint = vectorList[testIndex];
                }



                float nextAngle = Vector3.Angle( curPoint - lastPoint, nextPoint - lastPoint );
                if (nextAngle < 1 ) {
                    continue;
                }

                
                
                if (simplified.Count == 0 || (Vector3.Distance(simplified[simplified.Count -1], curPoint)>0.1f && Vector3.Distance( simplified[0], curPoint ) > 0.1f))
                    simplified.Add( curPoint );
                
            }

            return simplified;
        }

#endif
    }
}
