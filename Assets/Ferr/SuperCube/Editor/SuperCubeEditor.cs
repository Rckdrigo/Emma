using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Ferr {
	[AddComponentMenu("Mesh/SuperCube"), CustomEditor(typeof(SuperCube)), CanEditMultipleObjects()]
	public class SuperCubeEditor : Editor {
		
		
		#region Fields
		static bool      mFacesFoldout     = false;
		static PivotType mDragPrevSide     = PivotType.None;
		static Material  mDragPrevMaterial = null;
		static EditorTools.MultiType mWasStatic = EditorTools.MultiType.None;
		
		SerializedProperty mExtents;
		SerializedProperty mWallUVType;
		SerializedProperty mWallUVTile;
		SerializedProperty mWallUVOffset;
		SerializedProperty mTopBottomUVType;
		SerializedProperty mTopBottomUVTile;
		SerializedProperty mTopBottomUVOffset;
		SerializedProperty mSliceFaces;
		SerializedProperty mSliceDistance;
		
		SerializedProperty mFaceBottom;
		SerializedProperty mFaceTop;
		SerializedProperty mFaceFront;
		SerializedProperty mFaceBack;
		SerializedProperty mFaceLeft;
		SerializedProperty mFaceRight;
		
		SerializedProperty mOverrideBottom;
		SerializedProperty mOverrideTop;
		SerializedProperty mOverrideFront;
		SerializedProperty mOverrideBack;
		SerializedProperty mOverrideLeft;
		SerializedProperty mOverrideRight;
		#endregion
		
		#region Enable/Disable
		private void OnEnable () {
			mExtents         = serializedObject.FindProperty("mExtents"       );
			mWallUVType      = serializedObject.FindProperty("mWallUVType"    );
			mWallUVTile      = serializedObject.FindProperty("mWallUVTile"    );
			mWallUVOffset    = serializedObject.FindProperty("mWallUVOffset"  );
			mTopBottomUVType = serializedObject.FindProperty("mTopBottomUVType");
			mTopBottomUVTile = serializedObject.FindProperty("mTopBottomUVTile");
			mTopBottomUVOffset = serializedObject.FindProperty("mTopBottomUVOffset");
			mSliceFaces      = serializedObject.FindProperty("mSliceFaces"    );
			mSliceDistance   = serializedObject.FindProperty("mSliceDistance" );
			
			mFaceBottom      = serializedObject.FindProperty("mFaceBottom"    );
			mFaceTop         = serializedObject.FindProperty("mFaceTop"       );
			mFaceFront       = serializedObject.FindProperty("mFaceFront"     );
			mFaceBack        = serializedObject.FindProperty("mFaceBack"      );
			mFaceLeft        = serializedObject.FindProperty("mFaceLeft"      );
			mFaceRight       = serializedObject.FindProperty("mFaceRight"     );
			
			mOverrideBottom  = serializedObject.FindProperty("mOverrideBottom");
			mOverrideTop     = serializedObject.FindProperty("mOverrideTop"   );
			mOverrideFront   = serializedObject.FindProperty("mOverrideFront" );
			mOverrideBack    = serializedObject.FindProperty("mOverrideBack"  );
			mOverrideLeft    = serializedObject.FindProperty("mOverrideLeft"  );
			mOverrideRight   = serializedObject.FindProperty("mOverrideRight" );
			
			mWasStatic = EditorTools.IsStatic(targets);
		}
		private void OnDisable() {
			if (Selection.gameObjects.Length <= 0 || Selection.activeGameObject.GetComponents<SuperPlane>() == null) {
				SuperCubeEditorUtil.mGizmoTool = Tool.None;
			}
		}
		#endregion
		
		#region GUI
		public override void OnInspectorGUI        () {
			EditorTools.MultiType isStatic = EditorTools.IsStatic(targets);
			
			// Check if the scale has been modified
			for (int i = 0; i < targets.Length; ++i) {
				CheckScale(targets[i] as SuperCube);
			}

			DrawInspectorFields   ();
			ForceNewMeshButton    ();
			
			// Check for changes and undo/redo, and update the mesh if so
			if (mWasStatic != isStatic || serializedObject.ApplyModifiedProperties() || GUI.changed || (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")) {
				for (int i = 0; i < targets.Length; ++i) {
					((SuperCube)targets[i]).Build(true);
				}
			}
			
			// must go last, since it can delete this object
			ReplaceWithCubesButton();
			
			// for tracking changes in static-ness
			mWasStatic = isStatic;
		}
		private         void DrawInspectorFields   () {
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorTools.Box(2, () => {
				EditorGUILayout.PropertyField(mExtents,         new GUIContent("Extents",            "The total size of the cube mesh on each axis. (local Unity units)"));
				EditorGUILayout.PropertyField(mWallUVType,      new GUIContent("Wall UV Type",       "UV mode for the Front, Back, Left and Right faces."));
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(mWallUVTile,      new GUIContent("Tiling",             "UV tiling for the Front, Back, Left and Right faces."));
				EditorGUILayout.PropertyField(mWallUVOffset,    new GUIContent("Offset",             "UV offset for the Front, Back, Left and Right faces."));
				EditorGUI.indentLevel = 0;
				EditorGUILayout.PropertyField(mTopBottomUVType, new GUIContent("Top Bottom UV Type", "UV mode for the Top and Bottom."));
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(mTopBottomUVTile, new GUIContent("Tiling",             "UV tiling for the Top and Bottom."));
				EditorGUILayout.PropertyField(mTopBottomUVOffset, new GUIContent("Offset",           "UV offset for the Top and Bottom."));
				EditorGUI.indentLevel = 0;
				EditorGUILayout.PropertyField(mSliceFaces,      new GUIContent("Slice Faces",        "Enables adding extra verts on the surface of the cube's faces."));
				if (mSliceFaces.hasMultipleDifferentValues || mSliceFaces.boolValue == true) {
					EditorGUILayout.PropertyField(mSliceDistance, new GUIContent("Slice Distance", "How far apart should the slices be? (local Unity units)"));
				}
			});
			EditorGUILayout.Space();
			EditorTools.Box(2, () => {
				EditorGUI.indentLevel = 1;
				mFacesFoldout = EditorGUILayout.Foldout(mFacesFoldout, "Faces");
				EditorGUI.indentLevel = 0;
				if (mFacesFoldout) {
					DrawFaceProperty("X+:", mFaceRight,  mOverrideRight );
					DrawFaceProperty("X-:", mFaceLeft,   mOverrideLeft  );
					DrawFaceProperty("Y+:", mFaceTop,    mOverrideTop   );
					DrawFaceProperty("Y-:", mFaceBottom, mOverrideBottom);
					DrawFaceProperty("Z+:", mFaceBack,   mOverrideBack  );
					DrawFaceProperty("Z-:", mFaceFront,  mOverrideFront );
				}
			});
		}
		private         void ReplaceWithCubesButton() {
			if (GUILayout.Button(new GUIContent("Replace with boring cubes", "This will replace all selected SuperCube objects with Unity's standard cube. This will remove any extra components you may have added. Can undo!"))) {
				Object[] objs = new Object[targets.Length];
				for (int i = 0; i < targets.Length; ++i) {
					SuperCube box = targets[i] as SuperCube;
					if (box == null) continue;

					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					go.transform.parent        = box.transform.parent;
					go.transform.localScale    = box.Extents;
					go.transform.position      = box.transform.TransformPoint(Vector3.zero);
					go.transform.localRotation = box.transform.localRotation;
					objs[i] = go;
					Undo.RegisterCreatedObjectUndo(go, "Replaced SuperCube");
				}
				Selection.objects = objs;
				for (int i = 0; i < targets.Length; ++i) {
					SuperCube box = targets[i] as SuperCube;
					Undo.DestroyObjectImmediate(box.gameObject);
				}
			}
		}
		private         void ForceNewMeshButton    () {
			if (GUILayout.Button(new GUIContent("Force new mesh", "For use with prefab meshes! This creates a new mesh with the same settings, disconnecting it from any prefab connections it might have. Can undo!"))) {
				for (int i = 0; i < targets.Length; ++i) {
					SuperCube box = targets[i] as SuperCube;
					if (box == null) continue;

					Undo.RecordObject(box, "Force new mesh");
					box.ForceNewMesh();
				}
			}
		}
		private         void DrawFaceProperty      (string aLabel, SerializedProperty aFace, SerializedProperty aOverride) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField   (aLabel,    GUILayout.Width(50));
			EditorGUILayout.PropertyField(aFace,     new GUIContent(), GUILayout.Width(20));
			EditorGUILayout.PropertyField(aOverride, new GUIContent());
			if (GUILayout.Button(new GUIContent("X", "Clear material"), GUILayout.Width(20))) {
				aOverride.objectReferenceValue = null;
			}
			EditorGUILayout.EndHorizontal();
		}

		private         void OnSceneGUI            () {
			if (Selection.gameObjects.Length > 0 && (target as SuperCube).gameObject != Selection.activeGameObject) return;
			
			SuperCube box     = target as SuperCube;
			CheckScale(box);
			Vector3   rtb     =  box.Extents/2;
			Vector3   lbf     = -box.Extents/2;
			bool      rebuild = false;
			
			// Hide the gizmo if the user holds down Shift
			if (Event.current.shift && SuperCubeEditorUtil.mGizmoTool == Tool.None) {
				SuperCubeEditorUtil.mGizmoTool = Tools.current;
				Tools.current                  = Tool.None;
			} else if (!Event.current.shift && SuperCubeEditorUtil.mGizmoTool != Tool.None) {
				Tools.current                  = SuperCubeEditorUtil.mGizmoTool;
				SuperCubeEditorUtil.mGizmoTool = Tool.None;
			}
			if (Event.current.type == EventType.KeyDown && Event.current.shift) {
				if (Event.current.keyCode == KeyCode.X) {
					SuperCubeSettings.HideXRing = !SuperCubeSettings.HideXRing;
					Event.current.Use();
				} else if (Event.current.keyCode == KeyCode.Y) {
					SuperCubeSettings.HideYRing = !SuperCubeSettings.HideYRing;
					Event.current.Use();
				} else if (Event.current.keyCode == KeyCode.Z) {
					SuperCubeSettings.HideZRing = !SuperCubeSettings.HideZRing;
					Event.current.Use();
				}
			}
			
			Undo.RecordObject(box.transform, "SuperCube transform Edit");
			Undo.RecordObject(box,           "SuperCube Edit");
			
			ShowGuides(box);

			// show the handles
			if (Event.current.alt && Event.current.control) {
				ExtrudeHandles(box, rtb, lbf);
			} else if (Event.current.alt) {
				rebuild = rebuild || FaceVisibilityHandles(box, rtb, lbf);
			} else {
				rebuild = rebuild || TransformHandles     (box, rtb, lbf);
			}
			
			rebuild |= DoDragDrop();
			
			if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") {
				rebuild = true;
			}

			// recreate the mesh
			if (rebuild) {
				box.Build(true);
			}
		}
		private static void ShowGuides(SuperCube aBox) {
			bool showX = SuperCubeSettings.ShowGuideX;
			bool showY = SuperCubeSettings.ShowGuideY;
			bool showZ = SuperCubeSettings.ShowGuideZ;
			if (!(showX || showY || showZ)) return;

			Matrix4x4 mat = aBox.transform.localToWorldMatrix;
			float right =  aBox.Extents.x/2;
			float left  = -aBox.Extents.x/2;
			float top   =  aBox.Extents.y/2;
			float bottom= -aBox.Extents.y/2;
			float back  =  aBox.Extents.z/2;
			float front = -aBox.Extents.z/2;

			if (showX) {
				Color c =  Color.Lerp(Color.red, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;

				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, bottom, front)), mat.MultiplyPoint3x4(new Vector3(1000, bottom, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, bottom, back )), mat.MultiplyPoint3x4(new Vector3(1000, bottom, back )));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, top,    front)), mat.MultiplyPoint3x4(new Vector3(1000, top,    front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, top,    back )), mat.MultiplyPoint3x4(new Vector3(1000, top,    back )));
			}
			if (showY) {
				Color c =  Color.Lerp(Color.green, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;

				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, -1000, front)), mat.MultiplyPoint3x4(new Vector3(right, 1000, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  -1000, front)), mat.MultiplyPoint3x4(new Vector3(left,  1000, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, -1000, back )), mat.MultiplyPoint3x4(new Vector3(right, 1000, back )));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  -1000, back )), mat.MultiplyPoint3x4(new Vector3(left,  1000, back )));
			}
			if (showZ) {
				Color c =  Color.Lerp(Color.blue, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;

				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, bottom, -1000)), mat.MultiplyPoint3x4(new Vector3(right, bottom, 1000)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  bottom, -1000)), mat.MultiplyPoint3x4(new Vector3(left,  bottom, 1000)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, top,    -1000)), mat.MultiplyPoint3x4(new Vector3(right, top,    1000)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  top,    -1000)), mat.MultiplyPoint3x4(new Vector3(left,  top,    1000)));
			}
		}
		private static void ExtrudeHandles(SuperCube aBox, Vector3 rtb, Vector3 lbf) {
			// Bottom
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			Vector3 pos = aBox.transform.TransformPoint(new Vector3(0, lbf.y, 0));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Top, new Vector3(aBox.Extents.x, 1, aBox.Extents.z));
			}
			
			// Right
			Handles.color = Color.Lerp(Color.red, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(rtb.x, 0, 0));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Left, new Vector3(1, aBox.Extents.y, aBox.Extents.z));
			}
			
			// Left
			pos = aBox.transform.TransformPoint(new Vector3(lbf.x, 0, 0));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Right, new Vector3(1, aBox.Extents.y, aBox.Extents.z));
			}
			
			// Back
			Handles.color = Color.Lerp(Color.blue, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(0, 0, rtb.z));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Front, new Vector3(aBox.Extents.x, aBox.Extents.y, 1));
			}
			
			// Front
			pos = aBox.transform.TransformPoint(new Vector3(0, 0, lbf.z));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Back, new Vector3(aBox.Extents.x, aBox.Extents.y, 1));
			}
			
			// Top
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(0, rtb.y, 0));
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, SuperCubeEditorUtil.AddCap)) {
				AddExtrudeCube(aBox, pos, PivotType.Bottom, new Vector3(aBox.Extents.x, 1, aBox.Extents.z));
			}
		}
		private static void AddExtrudeCube(SuperCube aBox, Vector3 aAt, PivotType aPivot, Vector3 aSize) {
			GameObject go = SuperCube.CreatePivot(aBox.transform.InverseTransformPoint(aAt), aPivot, aSize, aBox.GetComponent<Renderer>().sharedMaterial);
			Vector3 pos = go.transform.position;
			go.transform.parent = aBox.transform;
			go.transform.localPosition = pos;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale    = Vector3.one;
			go.transform.parent = aBox.transform.parent;
			go.isStatic         = aBox.gameObject.isStatic;
			go.tag              = aBox.gameObject.tag;
			go.layer            = aBox.gameObject.layer;

			SuperCube box = go.GetComponent<SuperCube>();
			box.SliceFaces      = aBox.SliceFaces;
			box.SliceDistance   = aBox.SliceDistance;
			box.WallUVType      = aBox.WallUVType;
			box.WallUVTile      = aBox.WallUVTile;
			box.WallUVOffset    = aBox.WallUVOffset;
			box.TopBottomUVType = aBox.TopBottomUVType;
			box.TopBottomUVTile = aBox.TopBottomUVTile;
			box.TopBottomUVOffset=aBox.TopBottomUVOffset;
			box.OverrideTop     = aBox.OverrideBack;
			box.OverrideBottom  = aBox.OverrideBottom;
			box.OverrideFront   = aBox.OverrideFront;
			box.OverrideLeft    = aBox.OverrideLeft;
			box.OverrideRight   = aBox.OverrideRight;
			box.OverrideTop     = aBox.OverrideTop;
			box.Build(true);
			
			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
			
		}
		private static  bool TransformHandles      (SuperCube aBox, Vector3 aRtb, Vector3 aLbf) {
			Vector3 rtb     = aRtb;
			Vector3 lbf     = aLbf;
			Vector3 snap    = EditorTools.GetUnitySnap();
			bool    rebuild = false;
			
			EditorTools.capDir = Matrix4x4.TRS(Vector3.zero, aBox.transform.rotation, Vector3.one);
			
			// Bottom
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Bottom, aBox.transform, false, true, false, snap, ref lbf, ref rtb, EditorTools.ArrowCapYN);
			Handles.color = Color.Lerp(Color.red, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Right,  aBox.transform, true, false, false, snap, ref lbf, ref rtb, EditorTools.ArrowCapXP);
			SuperCubeEditorUtil.Handle(PivotType.Left,   aBox.transform, true, false, false, snap, ref lbf, ref rtb, EditorTools.ArrowCapXN);
			Handles.color = Color.Lerp(Color.blue, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Back,   aBox.transform, false, false, true, snap, ref lbf, ref rtb, EditorTools.ArrowCapZP);
			SuperCubeEditorUtil.Handle(PivotType.Front,  aBox.transform, false, false, true, snap, ref lbf, ref rtb, EditorTools.ArrowCapZN);
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Top,    aBox.transform, false, true, false, snap, ref lbf, ref rtb, EditorTools.ArrowCapYP);

			if (!SuperCubeSettings.HideYRing) {
				Handles.color = Color.Lerp( Color.Lerp(Color.red, Color.white, 0.5f), Color.Lerp(Color.blue, Color.white, 0.5f), .5f);
				SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Right, aBox.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapY);
				SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Left,  aBox.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapY);
				SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Right, aBox.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapY);
				SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Left,  aBox.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapY);
			}

			if (!SuperCubeSettings.HideZRing) {
				Handles.color = Color.Lerp( Color.Lerp(Color.red, Color.white, 0.5f), Color.Lerp(Color.green, Color.white, 0.5f), .5f);
				SuperCubeEditorUtil.Handle(PivotType.Left  | PivotType.Top,    aBox.transform, true, true, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
				SuperCubeEditorUtil.Handle(PivotType.Left  | PivotType.Bottom, aBox.transform, true, true, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
				SuperCubeEditorUtil.Handle(PivotType.Right | PivotType.Top,    aBox.transform, true, true, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
				SuperCubeEditorUtil.Handle(PivotType.Right | PivotType.Bottom, aBox.transform, true, true, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
			}

			if (!SuperCubeSettings.HideXRing) {
				Handles.color = Color.Lerp( Color.Lerp(Color.blue, Color.white, 0.5f), Color.Lerp(Color.green, Color.white, 0.5f), .5f);
				SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Top,    aBox.transform, false, true, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
				SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Bottom, aBox.transform, false, true, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
				SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Top,    aBox.transform, false, true, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
				SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Bottom, aBox.transform, false, true, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
			}

			// Check if there was an edit, and notify an update to the mesh!
			Vector3 newExtents = new Vector3(Mathf.Abs(rtb.x - lbf.x), Mathf.Abs(rtb.y - lbf.y), Mathf.Abs(rtb.z - lbf.z));
			if (aBox.SliceFaces) {
				float   maxSize    = SuperCubeSettings.MaxSize;
				if (newExtents.x > maxSize) {newExtents.x = aBox.Extents.x; lbf.x = aLbf.x; rtb.x = aRtb.x;}
				if (newExtents.y > maxSize) {newExtents.y = aBox.Extents.y; lbf.y = aLbf.y; rtb.y = aRtb.y;}
				if (newExtents.z > maxSize) {newExtents.z = aBox.Extents.z; lbf.z = aLbf.z; rtb.z = aRtb.z;}
			}
			if (aBox.Extents != newExtents) {
				aBox.transform.position += aBox.transform.TransformDirection(lbf + new Vector3((rtb.x - lbf.x) / 2, (rtb.y - lbf.y) / 2, (rtb.z - lbf.z) / 2));
				aBox.Extents = newExtents;
				rebuild = true;
			}

			return rebuild;
		}
		private static  bool FaceVisibilityHandles (SuperCube aBox, Vector3 rtb, Vector3 lbf) {
			bool rebuild = false;

			// Bottom
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			Vector3 pos = aBox.transform.TransformPoint(new Vector3(0, lbf.y, 0));
			Handles.DrawCapFunction cap = (aBox.FaceBottom ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceBottom = !aBox.FaceBottom;
				rebuild         = true;
			}

			// Right
			Handles.color = Color.Lerp(Color.red, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(rtb.x, 0, 0));
			cap = (aBox.FaceRight ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceRight = !aBox.FaceRight;
				rebuild        = true;
			}

			// Left
			pos = aBox.transform.TransformPoint(new Vector3(lbf.x, 0, 0));
			cap = (aBox.FaceLeft ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceLeft = !aBox.FaceLeft;
				rebuild       = true;
			}

			// Back
			Handles.color = Color.Lerp(Color.blue, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(0, 0, rtb.z));
			cap = (aBox.FaceBack ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceBack = !aBox.FaceBack;
				rebuild       = true;
			}

			// Front
			pos = aBox.transform.TransformPoint(new Vector3(0, 0, lbf.z));
			cap = (aBox.FaceFront ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceFront = !aBox.FaceFront;
				rebuild        = true;
			}

			// Top
			Handles.color = Color.Lerp(Color.green, Color.white, 0.5f);
			pos = aBox.transform.TransformPoint(new Vector3(0, rtb.y, 0));
			cap = (aBox.FaceTop ? (Handles.DrawCapFunction)SuperCubeEditorUtil.VisibleCap : (Handles.DrawCapFunction)SuperCubeEditorUtil.HiddenCap);
			if (Handles.Button(pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, HandleUtility.GetHandleSize(pos) * SuperCubeEditorUtil.mHandleSize, cap)) {
				aBox.FaceTop = !aBox.FaceTop;
				rebuild      = true;
			}

			return rebuild;
		}
		#endregion

		#region Helper methods
		private void CheckScale(SuperCube aCube) {
			if (aCube != null && aCube.transform.localScale != Vector3.one) {
				aCube.Extents = new Vector3(aCube.Extents.x * aCube.transform.localScale.x, aCube.Extents.y * aCube.transform.localScale.y, aCube.Extents.z * aCube.transform.localScale.z);
				aCube.transform.localScale = Vector3.one;
				aCube.Build(true);
			}
		}
		
		private bool DoDragDrop() {
			bool      rebuild = false;
			SuperCube cube    = target as SuperCube;
			
			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) {// || Event.current.type == EventType.Repaint) {
				Material dragMat = null;
				for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i) {
					if (DragAndDrop.objectReferences[i] is Material) {
						dragMat = DragAndDrop.objectReferences[i] as Material;
					}
				}
				
				if (dragMat != null) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					PivotType side = RaycastSide(cube, HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
					
					if (side != mDragPrevSide) {
						if (mDragPrevSide != PivotType.None) {
							SetSideMaterial(cube, mDragPrevSide, mDragPrevMaterial);
						}
						mDragPrevMaterial = GetSideMaterial(cube, side);
					}
					SetSideMaterial(cube, side, dragMat);
					rebuild = true;
					mDragPrevSide = side;
					
					if (Event.current.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag();
						mDragPrevMaterial = null;
						mDragPrevSide     = PivotType.None;
					}
					if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) {
						Event.current.Use();
					}
				}
			}
			
			return rebuild;
		}
		
		private Material GetSideMaterial(SuperCube aCube, PivotType aSide) {
			if (aSide == PivotType.None) return null;
			else if (aSide == PivotType.All) {
				return aCube.GetComponent<Renderer>().sharedMaterial;
			} else if ((aSide & PivotType.Back) > 0) {
				return aCube.OverrideBack;
			} else if ((aSide & PivotType.Front) > 0) {
				return aCube.OverrideFront;
			} else if ((aSide & PivotType.Top) > 0) {
				return aCube.OverrideTop;
			} else if ((aSide & PivotType.Bottom) > 0) {
				return aCube.OverrideBottom;
			} else if ((aSide & PivotType.Left) > 0) {
				return aCube.OverrideLeft;
			} else if ((aSide & PivotType.Right) > 0) {
				return aCube.OverrideRight;
			}
			return null;
		}
		private void SetSideMaterial(SuperCube aCube, PivotType aSide, Material aMaterial) {
			if (aSide == PivotType.None) return;
			else if (aSide == PivotType.All) {
				aCube.GetComponent<Renderer>().sharedMaterial = aMaterial;
			} else if ((aSide & PivotType.Back) > 0) {
				aCube.OverrideBack = aMaterial;
			} else if ((aSide & PivotType.Front) > 0) {
				aCube.OverrideFront = aMaterial;
			} else if ((aSide & PivotType.Top) > 0) {
				aCube.OverrideTop = aMaterial;
			} else if ((aSide & PivotType.Bottom) > 0) {
				aCube.OverrideBottom = aMaterial;
			} else if ((aSide & PivotType.Right) > 0) {
				aCube.OverrideRight = aMaterial;
			} else if ((aSide & PivotType.Left) > 0) {
				aCube.OverrideLeft = aMaterial;
			}
		}
		
		private PivotType RaycastSide(SuperCube aCube, Ray aRay) {
			aRay.origin    = aCube.transform.InverseTransformPoint(aRay.origin);
			aRay.direction = aCube.transform.InverseTransformDirection(aRay.direction);
			aRay.direction.Normalize();

			Vector3   size    = aCube.Extents / 2;
			PivotType side    = PivotType.None;
			float     closest = float.MaxValue;
			float     dist    = 0;

			Bounds bounds = new Bounds();
			bounds.center = Vector3.zero;
			bounds.extents = size;
			if (bounds.IntersectRay(aRay, out dist)) {
				side = PivotType.All;
				closest = dist + 0.1f;
			}

			if (CheckSide(aRay, new Vector3( 0, 0,-1), size.z, Mathf.Min(size.x, size.y), closest, out dist)) {
				closest = dist;
				side    = PivotType.Back;
			}
			if (CheckSide(aRay, new Vector3( 0, 0, 1), size.z, Mathf.Min(size.x, size.y), closest, out dist)) {
				closest = dist;
				side    = PivotType.Front;
			}
			if (CheckSide(aRay, new Vector3(-1, 0, 0), size.x, Mathf.Min(size.y, size.z), closest, out dist)) {
				closest = dist;
				side    = PivotType.Right;
			}
			if (CheckSide(aRay, new Vector3( 1, 0, 0), size.x, Mathf.Min(size.y, size.z), closest, out dist)) {
				closest = dist;
				side    = PivotType.Left;
			}
			if (CheckSide(aRay, new Vector3( 0,-1, 0), size.y, Mathf.Min(size.x, size.z), closest, out dist)) {
				closest = dist;
				side    = PivotType.Top;
			}
			if (CheckSide(aRay, new Vector3( 0, 1, 0), size.y, Mathf.Min(size.x, size.z), closest, out dist)) {
				closest = dist;
				side    = PivotType.Bottom;
			}

			return side;
		}

		private static bool CheckSide(Ray aRay, Vector3 dir, float aDist, float aRadius, float closest, out float dist) {
			dist = 0;
			Plane   p      = new Plane(dir, aDist);
			Vector3 center = dir * -aDist;
			if (p.Raycast(aRay, out dist)) {
				if (dist <= closest && (center - aRay.GetPoint(dist)).magnitude < aRadius) {
					Handles.SphereCap(0,aRay.GetPoint(dist), Quaternion.identity, 0.2f);
					return true;
				}
			}

			return false;
		}
		#endregion

		#region Menu hook
		[MenuItem("GameObject/3D Object/Super Cube", false, 0), MenuItem("Tools/Ferr SuperCube/Super Cube %&c", false, 0)]
		private static void CreateSuperCube() {
			GameObject obj = new GameObject("SuperCube");
			obj.AddComponent<BoxCollider>();
			obj.AddComponent<SuperCube  >().Build(true);
			obj.GetComponent<Renderer>().sharedMaterial = EditorTools.GetDefaultMaterial();
			
			Vector3 snap = EditorTools.GetUnitySnap();
			obj.transform.position = SuperCubeEditorUtil.SnapVector(SuperCubeEditorUtil.GetSpawnPos() + new Vector3(0,0.5f,0), snap);
			
			Selection.activeGameObject = obj;
			EditorGUIUtility.PingObject(obj);
		}
		#endregion
	}
}
