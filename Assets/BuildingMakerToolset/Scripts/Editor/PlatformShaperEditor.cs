using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BuildingMakerToolset.Geometry;
namespace BuildingMakerToolset.PlatformMaker
{

    [CustomEditor( typeof( PlatformShaper ) )]
    public class PlatformShaperEditor : Editor
    {

        PlatformShaper shapeCreator;
        SelectionInfo selectionInfo;
        bool redoMesh;
        bool shapeChangedSinceLastRepaint;

        GUIStyle standardStyle;
        GUIStyle selectedStyle;
        GUIContent tmpGuiContent;


        void CreateStyle()
        {
            if (standardStyle == null)
            {
                standardStyle = new GUIStyle( "MiniLabel" );
            }
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle( "BoldLabel" );
            }
            if (tmpGuiContent == null)
            {
                tmpGuiContent = new GUIContent();
            }
        }

        public override void OnInspectorGUI()
        {

            if (BHUtilities.IsPrefab( shapeCreator.gameObject ))
            {
                EditorGUILayout.HelpBox( "Can´t edit a prefab. If you want to edit please unpack the prefab", MessageType.Warning );
                return;
            }
            CreateStyle();

           

            EditorGUIUtility.labelWidth = 85;
            bool enableEditing = EditorGUILayout.Toggle( "Enable Editing", shapeCreator.enableEditing, GUILayout.ExpandWidth( false ) );

            

            if (shapeCreator.enableEditing != enableEditing)
            {
                shapeCreator.enableEditing = enableEditing;
                if (enableEditing)
                {
                    SceneView.lastActiveSceneView.drawGizmos = true;
                    VerifyCorrectRotation();
                    Tools.hidden = true;
                }
                else
                    Tools.hidden = false;
            }
            int shapeDeleteIndex = -1;
            int shapeCloneIndex = -1;

            GUILayout.BeginHorizontal( GUILayout.Width(450));
            /////////////////////////////////////////////////////////////////////////////
            EditorGUILayout.BeginVertical();
            GUILayout.Label( "Shapes", GUILayout.ExpandWidth( false ) );

            int curSelectedShape = selectionInfo.selectedShapeIndex;

            int fieldhight = 18;
            int extraFieldHight = 0;
            Texture txt = (Texture)EditorGUIUtility.Load( "LockIcon-On" );

           

            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                if (i == curSelectedShape)
                {
                    ShpeOptions(shapeCreator.shapes[i], i * (fieldhight + 2) + 60, ref extraFieldHight);
                }
                else if (GUILayout.Button( shapeCreator.shapes[i].name, GUILayout.MaxWidth( 100 ), GUILayout.Height( fieldhight ), GUILayout.ExpandWidth( false ) ))
                {
                    selectionInfo.selectedShapeIndex = i;
                }
            }
            EditorGUILayout.EndVertical();

            
            GUILayout.BeginVertical();
            GUILayout.Label( "Offset" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                EditorGUIUtility.labelWidth = 10;
                float offset = EditorGUILayout.FloatField( "y", shapeCreator.shapes[i].hightOffset, GUILayout.MaxWidth( 50 ), GUILayout.Height( fieldhight ) );
                if (shapeCreator.shapes[i].hightOffset != offset)
                {
                    Undo.RecordObject( shapeCreator, "Shape change" );
                    shapeCreator.shapes[i].hightOffset = offset;
                    ShapeSetDirty();
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();

           

            GUILayout.BeginVertical();
            GUILayout.Label( "Thickness" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                EditorGUIUtility.labelWidth = 10;
                float thickness = EditorGUILayout.FloatField( "y", shapeCreator.shapes[i].thickness, GUILayout.MaxWidth( 50 ), GUILayout.Height( fieldhight ) );
                if (shapeCreator.shapes[i].thickness != thickness)
                {
                    Undo.RecordObject( shapeCreator, "Shape change" );
                    shapeCreator.shapes[i].thickness = thickness;
                    ShapeSetDirty();
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();




            GUILayout.BeginVertical();
            GUILayout.Label( "Edge" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                EditorGUIUtility.labelWidth = 10;
                bool toggle = GUILayout.Toggle( shapeCreator.shapes[i].wall, "", GUILayout.MaxWidth( 50 ), GUILayout.Height( fieldhight ) );
                if (shapeCreator.shapes[i].wall != toggle)
                {
                    Undo.RecordObject( shapeCreator, "Shape change" );
                    shapeCreator.shapes[i].wall = toggle;
                    ShapeSetDirty();
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();

           


            GUILayout.BeginVertical();
            GUILayout.Label( "Flip" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                EditorGUIUtility.labelWidth = 10;
                bool toggle = GUILayout.Toggle( shapeCreator.shapes[i].flip, "", GUILayout.MaxWidth( 50 ), GUILayout.Height( fieldhight ) );
                if (shapeCreator.shapes[i].flip != toggle)
                {
                    Undo.RecordObject( shapeCreator, "Shape change" );
                    shapeCreator.shapes[i].flip = toggle;
                    ShapeSetDirty();
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();




            GUILayout.BeginVertical();
            GUILayout.Label( "Hole" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                EditorGUIUtility.labelWidth = 10;
                bool toggle = GUILayout.Toggle( shapeCreator.shapes[i].hole, "", GUILayout.MaxWidth( 50 ), GUILayout.Height( fieldhight ) );
                if (shapeCreator.shapes[i].hole != toggle)
                {
                    Undo.RecordObject( shapeCreator, "Shape change" );
                    shapeCreator.shapes[i].hole = toggle;
                    ShapeSetDirty();
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();



            GUILayout.BeginVertical();
            GUILayout.Label( "" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                if (GUILayout.Button( "Clone", GUILayout.MaxWidth( 100 ), GUILayout.Height( fieldhight ) ))
                {
                    shapeCloneIndex = i;
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();


            GUILayout.BeginVertical();
            GUILayout.Label( "" );
            for (int i = 0; i < shapeCreator.shapes.Count; i++)
            {
                if (GUILayout.Button( "Delete", GUILayout.MaxWidth( 100 ), GUILayout.Height( fieldhight ) ))
                {
                    shapeDeleteIndex = i;
                }
                if (i == curSelectedShape)
                    GUILayout.Space( extraFieldHight );
            }
            GUILayout.EndVertical();

            /////////////////////////////////////////////////////////////////////////////
            GUILayout.EndHorizontal();
            if (enableEditing)
                EditorGUILayout.HelpBox( "Ctrl-left click to add points.\nCtrl-left click on point to delete.\nShift-left click to create new shape.", MessageType.Info );

            if (shapeCreator.meshGenerationError)
                EditorGUILayout.HelpBox( "Invalid Shape", MessageType.Warning );


            if (shapeDeleteIndex != -1)
            {
                Undo.RecordObject( shapeCreator, "Delete shape" );
                shapeCreator.shapes.RemoveAt( shapeDeleteIndex );
                selectionInfo.selectedShapeIndex = Mathf.Clamp( selectionInfo.selectedShapeIndex, 0, shapeCreator.shapes.Count - 1 );
            }
            if (shapeCloneIndex != -1)
            {
                Undo.RecordObject( shapeCreator, "Clone shape" );
                CloneShape( shapeCreator.shapes[shapeCloneIndex] );
                selectionInfo.selectedShapeIndex = Mathf.Clamp( selectionInfo.selectedShapeIndex, 0, shapeCreator.shapes.Count - 1 );
            }

            if (GUI.changed)
            {
                ShapeSetDirty();
            }

            if (shapeChangedSinceLastRepaint)
            {
                SceneView.RepaintAll();
            }
        }

        void ShpeOptions(Shape curShape, int fieldYpos, ref int extraFieldHight)
        {
            GUIContent linkedIcon = new GUIContent( (Texture)EditorGUIUtility.Load( "Linked" ) );
            if (linkedIcon.image == null)
                linkedIcon.image = (Texture)EditorGUIUtility.Load( "LockIcon-On" );
            GUIContent unlinkedIcon = new GUIContent( (Texture)EditorGUIUtility.Load( "Unlinked" ) );
            if (unlinkedIcon.image == null)
                unlinkedIcon.image = (Texture)EditorGUIUtility.Load( "LockIcon" );
            GUIStyle selected = new GUIStyle( "Button" );
            selected.fontStyle = FontStyle.Bold;
            
            string newName = GUILayout.TextField( curShape.name != null ? curShape.name : "", 14, selected, GUILayout.Width( 100 ) );
            if (curShape.name != newName)
            {
                Undo.RecordObject( shapeCreator, "rename shape" );
                curShape.name = newName;
            }
            extraFieldHight = 81;
      


            if (!curShape.offset2Same)
                extraFieldHight += 18;
            if (curShape.wall)
                extraFieldHight += 18;
         

            GUILayout.Space( extraFieldHight );
            Rect r = new Rect( 30, fieldYpos, 420, extraFieldHight );
            GUILayout.BeginArea( r, new GUIStyle( "Box" ) );
          

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "Shape Material:", GUILayout.MinWidth( 100 ), GUILayout.ExpandWidth( false ) );
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            GUILayout.Space( 20 );
            EditorGUI.BeginChangeCheck();
            Material up = curShape.upMaterial;
            Material down = curShape.downMaterial;
            Material side = curShape.sideMaterial;
            bool flip = curShape.flip;
            EditorGUIUtility.labelWidth =50;
            if(!curShape.hole)
                up = (Material)EditorGUILayout.ObjectField(flip? "down face" : "up face" , up, typeof( Material ),false ,GUILayout.Width( 128) );
            EditorGUIUtility.labelWidth = 35;
            if(!curShape.hole && curShape.thickness !=0)
                down = (Material)EditorGUILayout.ObjectField( "down", down, typeof( Material ), false, GUILayout.Width( 128) );
            if(curShape.wall)
                side = (Material)EditorGUILayout.ObjectField( "edge", side, typeof( Material ), false, GUILayout.Width( 128 ) );
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject( shapeCreator, "Change material" );
                curShape.upMaterial = up;
                curShape.sideMaterial = side;
                curShape.downMaterial = down;
                ShapeSetDirty();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "UV settings:", GUILayout.MinWidth( 80 ), GUILayout.ExpandWidth( false ) );
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            bool dirty = false;
            Vector2 offset = curShape.uvOffset;
            Vector2 scale = curShape.uvScale;

            float fieldWidth = 80;   
            if (!curShape.hole)
            {
                tmpGuiContent.image = (curShape.offset2Same ? linkedIcon : unlinkedIcon).image as Texture2D;
                if (GUILayout.Button( tmpGuiContent.image, GUIStyle.none, GUILayout.Width( 16 ) ))
                    curShape.offset2Same = !curShape.offset2Same;

                EditorGUIUtility.labelWidth = 110;
                offset = EditorGUILayout.Vector2Field( curShape.offset2Same ? "shapes: offset" : "up face:offset", offset, GUILayout.Width( 170 + fieldWidth ) , GUILayout.ExpandWidth( false ) );
                EditorGUIUtility.labelWidth = 40;


              

                tmpGuiContent.image = (curShape.useSimpleScale ? linkedIcon : unlinkedIcon).image as Texture2D;
                if (GUILayout.Button( tmpGuiContent.image, GUIStyle.none, GUILayout.Width( 16 ) ))
                    curShape.useSimpleScale = !curShape.useSimpleScale;

                if (curShape.useSimpleScale)
                {
                    scale.x = scale.y = EditorGUILayout.FloatField( "scale", scale.x, GUILayout.Width( fieldWidth ) );
                }
                else
                    scale = EditorGUILayout.Vector2Field( "scale", scale, GUILayout.Width( fieldWidth*2 ) );
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

            }




                Vector2 offset2 = curShape.uvOffset2;
                Vector2 scale2 = curShape.uvScale2;
                if (!curShape.offset2Same)
                {


                    GUILayout.Space(20);

                    EditorGUIUtility.labelWidth = 110;
                    offset2 = EditorGUILayout.Vector2Field( "down:   offset", offset2, GUILayout.Width( 250 ) );
                    EditorGUIUtility.labelWidth = 40;

                    tmpGuiContent.image = (curShape.useSimpleScale2 ? linkedIcon : unlinkedIcon).image as Texture2D;
                    if (GUILayout.Button( tmpGuiContent.image, GUIStyle.none, GUILayout.Width( 16 ) ))
                        curShape.useSimpleScale2 = !curShape.useSimpleScale2;


                    if (curShape.useSimpleScale2)
                    {
                        scale2.x = scale2.y = EditorGUILayout.FloatField( "scale", scale2.x, GUILayout.Width( fieldWidth ) );
                    }
                    else
                        scale2 = EditorGUILayout.Vector2Field( "scale", scale2, GUILayout.Width( fieldWidth*2 ) );
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                else
                {
                    scale2 = scale;
                    offset2 = offset;
                }

                Vector2 wOffset = curShape.wallUvOffset;
                Vector2 wScale = curShape.wallUvScale;
                if (curShape.wall)
                {

                    GUILayout.Space( 20 );
                    EditorGUIUtility.labelWidth = 110;
                    wOffset = EditorGUILayout.Vector2Field( "edges:  offset", wOffset, GUILayout.Width( 250 ), GUILayout.ExpandWidth( false ) );
                    EditorGUIUtility.labelWidth = 40;


                    tmpGuiContent.image = (curShape.wallUseSimpleScale ? linkedIcon : unlinkedIcon).image as Texture2D;
                    if (GUILayout.Button( tmpGuiContent.image, GUIStyle.none, GUILayout.Width( 16 ) ))
                        curShape.wallUseSimpleScale = !curShape.wallUseSimpleScale;



                    if (curShape.wallUseSimpleScale)
                        wScale.x = wScale.y = EditorGUILayout.FloatField( "scale", wScale.x, GUILayout.Width( fieldWidth ) );
                    else
                        wScale = EditorGUILayout.Vector2Field( "scale", wScale, GUILayout.Width( fieldWidth*2 ) );
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();




                if (offset != curShape.uvOffset || scale != curShape.uvScale || wOffset != curShape.wallUvOffset || wScale != curShape.wallUvScale || offset2 != curShape.uvOffset2 || scale2 != curShape.uvScale2)
                {
                    dirty = true;
                }

                if (dirty)
                {
                    Undo.RecordObject( shapeCreator, "Shape uv change" );
                    curShape.uvOffset = offset;
                    curShape.uvScale = scale;


                    curShape.uvOffset2 = offset2;
                    curShape.uvScale2 = scale2;

                    curShape.wallUvOffset = wOffset;
                    curShape.wallUvScale = wScale;
                    ShapeSetDirty();
                }

        }

        Rect selectionRect;

        void OnSceneGUI()
        {
            Event guiEvent = Event.current;
            if (guiEvent.type == EventType.Repaint)
            {
                Draw( guiEvent );
            }
            if (!shapeCreator.enableEditing)
                return;

         



            if (guiEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl( GUIUtility.GetControlID( FocusType.Passive ) );
            }
            else
            {
                HandleInput( guiEvent );
                if (shapeChangedSinceLastRepaint)
                {
                    HandleUtility.Repaint();
                }
            }
        }

        Shape CreateNewShape()
        {
            Undo.RecordObject( shapeCreator, "Create shape" );
            Shape newShape = null;
            if (shapeCreator.shapes != null && shapeCreator.shapes.Count > 0)
            {
                newShape = NewShapeWithSettings( shapeCreator.shapes[selectionInfo.selectedShapeIndex] );
            }
            else
            {
                newShape = NewShapeWithSettings( null );
            }
            newShape.name = "New Shape";
            newShape.uvOffset = Vector2.zero;
            newShape.uvScale = Vector2.one;
            newShape.wallUvOffset = Vector2.zero;
            newShape.wallUvScale = Vector2.one;
            shapeCreator.shapes.Add( newShape );
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
            return newShape;
        }

        void CloneShape(Shape sample)
        {
            Undo.RecordObject( shapeCreator, "Copy shape" );
            Shape newShape = NewShapeWithSettings( sample );
          
            newShape.points = new List<Vector3>();
            newShape.points.AddRange( sample.points );
            newShape.name = "(copy)" + sample.name;
            
            
            newShape.uvOffset = sample.uvOffset;
            newShape.uvScale = sample.uvScale;
            newShape.wallUvOffset = sample.wallUvOffset;
            newShape.wallUvScale = sample.wallUvScale;

            shapeCreator.shapes.Add( newShape );
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
        }

        Shape NewShapeWithSettings(Shape sample)
        {
            Shape newShape = new Shape();

            if (sample == null)
                return newShape;
            newShape.upMaterial = sample.upMaterial;
            newShape.sideMaterial = sample.sideMaterial;
            newShape.downMaterial = sample.downMaterial;
            newShape.hightOffset = sample.hightOffset;


            return newShape;
        }

        void CreateNewPoint(Vector3 position)
        {
            bool mouseIsOverSelectedShape = selectionInfo.mouseOverShapeIndex == selectionInfo.selectedShapeIndex;
            int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? selectionInfo.lineIndex + 1 : GetBestInsertIndex( position );
            SelectedShape.lastCreatedPointIndex = newPointIndex;
            Undo.RecordObject( shapeCreator, "Add point" );
            SelectedShape.points.Insert( newPointIndex, position );
            selectionInfo.pointIndex = newPointIndex;
            selectionInfo.mouseOverShapeIndex = selectionInfo.selectedShapeIndex;
            ShapeSetDirty();

            SelectPointUnderMouse();
        }

        int GetBestInsertIndex(Vector3 position)
        {
            return Mathf.Min( SelectedShape.points.Count, SelectedShape.lastCreatedPointIndex);
        }

        int GetClosestPointOnShape(Shape shape, Vector3 pos)
        {
            if (shape == null || shape.points == null || shape.points.Count == 0)
                return 0;
            float bestDist = Mathf.Infinity;
            int bestIndex = 0;
            for (int i = 0; i < shape.points.Count; i++)
            {
                float curDist = Vector3.Distance( pos, ToWorld( shape.points[i] ) );
                if (curDist < bestDist)
                {
                    bestDist = curDist;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        void DeletePointUnderMouse()
        {
            Undo.RecordObject( shapeCreator, "Delete point" );
            SelectedShape.points.RemoveAt( selectionInfo.pointIndex );
            selectionInfo.pointIsSelected = false;
            selectionInfo.mouseIsOverPoint = false;
            if (SelectedShape.points.Count == 0)
                shapeCreator.shapes.Remove( SelectedShape );
            ShapeSetDirty();

        }

        Vector3 ToLoc(Vector3 val)
        {
            Vector3 localPos = shapeCreator.transform.InverseTransformPoint( val );
            localPos.y = 0;
            return localPos;
        }
        Vector3 ToWorld(Vector3 val)
        {
            val.y = 0;
            return shapeCreator.transform.TransformPoint( val );
        }
        void SelectPointUnderMouse()
        {
            selectionInfo.pointIsSelected = true;
            selectionInfo.mouseIsOverPoint = true;
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;

            selectionInfo.positionAtStartOfDrag = ToWorld( SelectedShape.points[selectionInfo.pointIndex] );
            ShapeSetDirty();
        }

        void SelectShapeUnderMouse()
        {
            if (selectionInfo.mouseOverShapeIndex != -1)
            {
                selectionInfo.selectedShapeIndex = selectionInfo.mouseOverShapeIndex;
                ShapeSetDirty();
            }
        }

        Vector3 GetMousePositionOnShape(Shape shape)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
            float drawPlaneHeight = shapeCreator.transform.position.y + shape.hightOffset;
            float distToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
            return mouseRay.GetPoint( distToDrawPlane );
        }

        static Texture2D customSplitCursor;
        bool leftMouseHoldDown = false;
        bool rightMouseHoldDown = false;
        void HandleInput(Event guiEvent)
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
                leftMouseHoldDown = true;
            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
                leftMouseHoldDown = false;
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
                rightMouseHoldDown = true;
            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 1)
                rightMouseHoldDown = false;

            if (rightMouseHoldDown)
                return;
            Rect cursorRect = new Rect( SceneView.lastActiveSceneView.position );
            Camera viewCamera = SceneView.currentDrawingSceneView.camera;
            Vector3 cursorWorldPosition = viewCamera.ScreenToWorldPoint( new Vector3(guiEvent.mousePosition.x,Screen.height -guiEvent.mousePosition.y - 70, viewCamera.nearClipPlane * 2 ) );

            if (guiEvent.modifiers == EventModifiers.Shift)
            {
                EditorGUIUtility.AddCursorRect( cursorRect, MouseCursor.ArrowPlus );
                Handles.Label( cursorWorldPosition, "Create new shape" );
            }
            else if (guiEvent.modifiers == EventModifiers.Control)
            {
                if (selectionInfo.mouseIsOverPoint)
                {
                    EditorGUIUtility.AddCursorRect( cursorRect, MouseCursor.ArrowMinus );
                    Handles.Label( cursorWorldPosition, "Remove point" );
                }
                else
                {

                    EditorGUIUtility.AddCursorRect( new Rect( SceneView.lastActiveSceneView.position ), MouseCursor.ArrowPlus );

                    Handles.Label( cursorWorldPosition, "Add point" );
                }
            }
            else if (selectionInfo.mouseIsOverPoint)
            {
                EditorGUIUtility.AddCursorRect( cursorRect , MouseCursor.MoveArrow );
            }



            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
            {
                HandleShiftLeftMouseDown();
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
            {
                HandleControlLeftMouseDown();
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseDown();
            }

            if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
            {
                HandleLeftMouseUp();
            }

            if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                HandleLeftMouseDrag();
            }

            if (!selectionInfo.pointIsSelected)
            {
                UpdateMouseOverInfo();
            }

        }

        Shape lastShapeCreated;

        void HandleShiftLeftMouseDown()
        {
            Shape notUsedShape = null;
            if (lastShapeCreated != null && lastShapeCreated.points.Count < 3)
            {
                notUsedShape = lastShapeCreated;
            }
            lastShapeCreated = CreateNewShape();
            CreateNewPoint( GetMousePositionOnShape( SelectedShape ) );

            if (notUsedShape != null)
            {
                shapeCreator.shapes.Remove( notUsedShape );
                selectionInfo.selectedShapeIndex = Mathf.Clamp( selectionInfo.selectedShapeIndex, 0, shapeCreator.shapes.Count - 1 );
            }

        }

        void HandleControlLeftMouseDown()
        {

            if (selectionInfo.mouseIsOverPoint)
            {
                SelectShapeUnderMouse();
                DeletePointUnderMouse();
                return;
            }
            if (shapeCreator.shapes.Count == 0)
            {
                CreateNewShape();
            }
            SelectShapeUnderMouse();
            CreateNewPoint( GetMousePositionOnShape( SelectedShape ) );

        }

        void HandleLeftMouseDown()
        {


            if (shapeCreator.shapes.Count == 0)
            {
                CreateNewShape();
            }

            SelectShapeUnderMouse();

            if (selectionInfo.mouseIsOverPoint)
            {
                SelectPointUnderMouse();
            }
        }

        void HandleLeftMouseUp()
        {
            if (selectionInfo.pointIsSelected)
            {
                SelectedShape.points[selectionInfo.pointIndex] = ToLoc( selectionInfo.positionAtStartOfDrag );
                Undo.RecordObject( shapeCreator, "Move point" );
                SelectedShape.points[selectionInfo.pointIndex] = ToLoc( GetMousePositionOnShape( SelectedShape ) );

                selectionInfo.pointIsSelected = false;
                selectionInfo.pointIndex = -1;
                ShapeSetDirty();
            }

        }

        void HandleLeftMouseDrag()
        {
            if (selectionInfo.pointIsSelected)
            {
                SelectedShape.points[selectionInfo.pointIndex] = ToLoc( GetMousePositionOnShape( SelectedShape ) - shapeCreator.transform.up * SelectedShape.hightOffset );
                ShapeSetDirty();
            }
        }

        void UpdateMouseOverInfo()
        {
            int mouseOverPointIndex = -1;
            int mouseOverShapeIndex = -1;
            for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
            {
                Shape currentShape = shapeCreator.shapes[shapeIndex];

                for (int i = 0; i < currentShape.points.Count; i++)
                {
                    Vector3 pointPos = ToWorld( currentShape.points[i] ) + shapeCreator.transform.up * currentShape.hightOffset;
                    if (Vector3.Distance( GetMousePositionOnShape( currentShape ), pointPos ) < shapeCreator.handleRadius * HandleUtility.GetHandleSize( pointPos ))
                    {
                        mouseOverPointIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                        break;
                    }
                }

            }



            if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
            {
                selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
                selectionInfo.pointIndex = mouseOverPointIndex;
                selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

                ShapeSetDirty( false );
            }

            if (selectionInfo.mouseIsOverPoint)
            {
                selectionInfo.mouseIsOverLine = false;
                selectionInfo.lineIndex = -1;
            }
            else
            {
                int mouseOverLineIndex = -1;
                float closestLineDst = shapeCreator.handleRadius;
                for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
                {
                    Shape currentShape = shapeCreator.shapes[shapeIndex];

                    for (int i = 0; i < currentShape.points.Count; i++)
                    {
                        Vector3 nextPointInShape = ToWorld( currentShape.points[(i + 1) % currentShape.points.Count] );
                        float dstFromMouseToLine = HandleUtility.DistancePointToLineSegment( Maths2D.ToXZ( GetMousePositionOnShape( currentShape ) ), Maths2D.ToXZ( ToWorld( currentShape.points[i] ) ), Maths2D.ToXZ( nextPointInShape ) );
                        if (dstFromMouseToLine < closestLineDst)
                        {
                            closestLineDst = dstFromMouseToLine;
                            mouseOverLineIndex = i;
                            mouseOverShapeIndex = shapeIndex;
                        }
                    }
                }

                if (selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
                {
                    selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
                    selectionInfo.lineIndex = mouseOverLineIndex;
                    selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                    ShapeSetDirty( false );
                }
            }
        }

        void Draw(Event guiEvent)
        {
            for (int shapeIndex = 0; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
            {

                Shape shapeToDraw = shapeCreator.shapes[shapeIndex];

                Vector3 currnetOffset = shapeCreator.transform.up * shapeToDraw.hightOffset;

                bool shapeIsSelected = shapeIndex == selectionInfo.selectedShapeIndex;
                bool mouseIsOverShape = shapeIndex == selectionInfo.mouseOverShapeIndex;
                Color deselectedShapeColour = Color.grey;
                bool enableEdit = shapeCreator.enableEditing;
                for (int i = 0; i < shapeToDraw.points.Count; i++)
                {
                    Vector3 nextPoint = shapeToDraw.points[(i + 1) % shapeToDraw.points.Count];
                    if (enableEdit && i == selectionInfo.lineIndex && mouseIsOverShape)
                    {
                        Handles.color = Color.red;
                        Handles.DrawLine( ToWorld( shapeToDraw.points[i] ) + currnetOffset, ToWorld( nextPoint ) + currnetOffset );
                    }
                    else
                    {
                        Handles.color = (shapeIsSelected) ? Color.yellow : deselectedShapeColour;
                        Handles.DrawDottedLine( ToWorld( shapeToDraw.points[i] ) + currnetOffset, ToWorld( nextPoint ) + currnetOffset, 4 );
                    }
                    if (shapeToDraw.thickness != 0)
                    {
                        Vector3 thicknessOffset = shapeCreator.transform.up * shapeToDraw.thickness;
                        Handles.DrawDottedLine( ToWorld( shapeToDraw.points[i] ) + currnetOffset + thicknessOffset, ToWorld( nextPoint ) + currnetOffset + thicknessOffset, 4 );
                        Handles.DrawDottedLine( ToWorld( shapeToDraw.points[i] ) + currnetOffset + thicknessOffset, ToWorld( shapeToDraw.points[i] ) + currnetOffset, 4 );
                    }


                    if (enableEdit && i == selectionInfo.pointIndex && mouseIsOverShape)
                    {
                        Handles.color = (selectionInfo.pointIsSelected) ? Color.yellow : Color.red;
                    }
                    else
                    {
                        Handles.color = (shapeIsSelected) ? Color.white : deselectedShapeColour;
                    }
                    Vector3 handlePosition = ToWorld( shapeToDraw.points[i] ) + currnetOffset;

                    float handleRad = shapeCreator.handleRadius * HandleUtility.GetHandleSize( handlePosition ) * (enableEdit ? 1 : 0.25f);
                    Handles.DrawSolidDisc( handlePosition, Vector3.up, handleRad );
                    if (shapeIsSelected)
                    {
                        Handles.color = enableEdit ? Color.black : Color.gray;
                        Handles.DrawWireDisc( handlePosition, Vector3.up, handleRad );
                    }

                }
            }


            if (shapeChangedSinceLastRepaint)
            {
                if (!leftMouseHoldDown)
                {
                    if (redoMesh)
                    {
                        shapeCreator.UpdateMeshDisplay( true );
                        redoMesh = false;
                    }
                    shapeChangedSinceLastRepaint = false;
                }
                else if (redoMesh)
                    shapeCreator.UpdateMeshDisplay( false );

            }


        }
        void ShapeSetDirty(bool recreateMesh = true)
        {
            if (recreateMesh)
                redoMesh = true;
            shapeChangedSinceLastRepaint = true;
        }
        void OnEnable()
        {
            ShapeSetDirty();
            shapeCreator = target as PlatformShaper;
            selectionInfo = new SelectionInfo();
            shapeCreator.handleRadius = 0.1f;

            Undo.undoRedoPerformed += OnUndoOrRedo;
        }

        void VerifyCorrectRotation()
        {
            if (shapeCreator == null)
                return;
            Vector3 fwd = shapeCreator.transform.forward;
            fwd.y = 0;
            shapeCreator.transform.rotation = Quaternion.LookRotation( fwd );
        }

        void OnDisable()
        {
            if (shapeChangedSinceLastRepaint)
            {
                shapeCreator.UpdateMeshDisplay( true );
                shapeChangedSinceLastRepaint = false;
            }

            Undo.undoRedoPerformed -= OnUndoOrRedo;
            Tools.hidden = false;
        }

        void OnUndoOrRedo()
        {
            if (selectionInfo.selectedShapeIndex >= shapeCreator.shapes.Count || selectionInfo.selectedShapeIndex == -1)
            {
                selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
            }
            ShapeSetDirty();
        }

        Shape SelectedShape
        {
            get
            {
                return shapeCreator.shapes[selectionInfo.selectedShapeIndex];
            }
        }

        public class SelectionInfo
        {
            public int selectedShapeIndex;
            public int mouseOverShapeIndex;

            public int pointIndex = -1;
            public bool mouseIsOverPoint;
            public bool pointIsSelected;
            public Vector3 positionAtStartOfDrag;

            public int lineIndex = -1;
            public bool mouseIsOverLine;
        }

    }


}
