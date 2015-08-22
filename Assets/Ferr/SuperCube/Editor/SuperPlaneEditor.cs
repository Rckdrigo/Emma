using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Ferr {
	[AddComponentMenu("Mesh/SuperPlane"), CustomEditor(typeof(SuperPlane)), CanEditMultipleObjects()]
	public class SuperPlaneEditor : Editor {
		#region Fields
		static EditorTools.MultiType mWasStatic = EditorTools.MultiType.None;
		
		SerializedProperty mExtents;
		SerializedProperty mUVType;
		SerializedProperty mUVOffset;
		SerializedProperty mUVTile;
		SerializedProperty mSliceFaces;
		SerializedProperty mSliceDistance;
		#endregion
		
		#region Enable/Disable
		private void OnEnable () {
			mExtents       = serializedObject.FindProperty("mExtents");
			mUVType        = serializedObject.FindProperty("mUVType");
			mUVOffset      = serializedObject.FindProperty("mUVOffset");
			mUVTile        = serializedObject.FindProperty("mUVTile");
			mSliceFaces    = serializedObject.FindProperty("mSliceFaces");
			mSliceDistance = serializedObject.FindProperty("mSliceDistance");
			
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
				CheckScale(targets[i] as SuperPlane);
			}

			DrawInspectorFields();
			ForceNewMeshButton ();

			// Check for changes and undo/redo, and update the mesh if so
			if (mWasStatic != isStatic || serializedObject.ApplyModifiedProperties() || GUI.changed || (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")) {
				for (int i = 0; i < targets.Length; ++i) {
					((SuperPlane)targets[i]).Build(true);
				}
			}
			
			// must go last, since it can delete this object
			ReplaceWithPlaneButton();
			
			// for tracking changes in static-ness
			mWasStatic = isStatic;
		}
		private         void DrawInspectorFields   () {
			serializedObject.Update();

			// Draw and update all the fields in the inspector
			EditorGUILayout.Space();
			Ferr.EditorTools.Box(2, () => {
				EditorGUILayout.PropertyField(mExtents,    new GUIContent("Extents",     "The total size of the cube mesh on each axis. (local Unity units)"));
				EditorGUILayout.PropertyField(mUVType,     new GUIContent("UV Type",     "UV mode for the plane."));
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(mUVTile,     new GUIContent("Tiling",      "UV tiling for the plane."));
				EditorGUILayout.PropertyField(mUVOffset,   new GUIContent("Offset",      "UV offset for the plane."));
				EditorGUI.indentLevel = 0;
				EditorGUILayout.PropertyField(mSliceFaces, new GUIContent("Slice Faces", "Enables adding extra verts on the surface of the cube's faces."));
				if (mSliceFaces.hasMultipleDifferentValues || mSliceFaces.boolValue == true) {
					EditorGUILayout.PropertyField(mSliceDistance, new GUIContent("Slice Distance", "How far apart should the slices be? (local Unity units)"));
				}
			});
		}
		private         void ReplaceWithPlaneButton() {
			if (GUILayout.Button(new GUIContent("Replace with boring planes", "This will replace all selected SuperPlane objects with Unity's standard plane. This will remove any extra components you may have added. Can undo!"))) {
				Object[] objs = new Object[targets.Length];
				for (int i = 0; i < targets.Length; ++i) {
					SuperPlane plane = targets[i] as SuperPlane;
					if (plane == null) continue;

					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
					go.transform.parent        = plane.transform.parent;
					go.transform.localScale    = plane.Extents / 10;
					go.transform.position      = plane.transform.TransformPoint(Vector3.zero);
					go.transform.localRotation = plane.transform.localRotation;
					objs[i] = go;
					Undo.RegisterCreatedObjectUndo(go, "Replaced SuperPlane");
				}
				Selection.objects = objs;
				for (int i = 0; i < targets.Length; ++i) {
					SuperPlane plane = targets[i] as SuperPlane;
					Undo.DestroyObjectImmediate(plane.gameObject);
				}
			}
		}
		private         void ForceNewMeshButton    () {
			if (GUILayout.Button(new GUIContent("Force new mesh", "For use with prefab meshes! This creates a new mesh with the same settings, disconnecting it from any prefab connections it might have. Can undo!"))) {
				for (int i = 0; i < targets.Length; ++i) {
					SuperPlane plane = targets[i] as SuperPlane;
					if (plane == null) continue;
					
					Undo.RecordObject(plane, "Force new mesh");
					plane.ForceNewMesh();
				}
			}
		}

		private         void OnSceneGUI            () {
			if (Selection.gameObjects.Length > 0 && (target as SuperPlane).gameObject != Selection.activeGameObject) return;

			SuperPlane box = target as SuperPlane;
			CheckScale(box);
			Vector3    rtb =  box.Extents / 2;
			Vector3    lbf = -box.Extents / 2;

			// Hide the gizmo if the user holds down Shift
			if (Event.current.shift && SuperCubeEditorUtil.mGizmoTool == Tool.None) {
				SuperCubeEditorUtil.mGizmoTool = Tools.current;
				Tools.current                  = Tool.None;
			} else if (!Event.current.shift && SuperCubeEditorUtil.mGizmoTool != Tool.None) {
				Tools.current                  = SuperCubeEditorUtil.mGizmoTool;
				SuperCubeEditorUtil.mGizmoTool = Tool.None;
			}

			Undo.RecordObject(box.transform, "SuperPlane transform Edit");
			Undo.RecordObject(box, "SuperPlane Edit");

			ShowGuides(box);
			TransformHandles(box, rtb, lbf);
		}
		private static void ShowGuides(SuperPlane aBox) {
			bool showX = SuperCubeSettings.ShowGuideX;
			bool showY = SuperCubeSettings.ShowGuideY;
			bool showZ = SuperCubeSettings.ShowGuideZ;
			if (!(showX || showY || showZ)) return;

			Matrix4x4 mat = aBox.transform.localToWorldMatrix;
			Vector3   pos = aBox.transform.position;
			float right =  aBox.Extents.x/2;
			float left  = -aBox.Extents.x/2;
			float back  =  aBox.Extents.z/2;
			float front = -aBox.Extents.z/2;
			
			if (showX) {
				Color c =  Color.Lerp(Color.red, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, pos.y, front)), mat.MultiplyPoint3x4(new Vector3(1000, pos.y, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(-1000, pos.y, back )), mat.MultiplyPoint3x4(new Vector3(1000, pos.y, back )));
			}
			
			if (showY) {
				Color c =  Color.Lerp(Color.green, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  -1000, front)), mat.MultiplyPoint3x4(new Vector3(left,  1000, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  -1000, back )), mat.MultiplyPoint3x4(new Vector3(left,  1000, back )));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, -1000, front)), mat.MultiplyPoint3x4(new Vector3(right, 1000, front)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, -1000, back )), mat.MultiplyPoint3x4(new Vector3(right, 1000, back )));
			}
			
			if (showZ) {
				Color c = Color.Lerp(Color.blue, Color.white, 0.5f);
				c.a = SuperCubeSettings.GuideAlpha;
				Handles.color = c;
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(left,  pos.y, -1000)), mat.MultiplyPoint3x4(new Vector3(left,  pos.y, 1000)));
				EditorTools.DrawDepthLine(mat.MultiplyPoint3x4(new Vector3(right, pos.y, -1000)), mat.MultiplyPoint3x4(new Vector3(right, pos.y, 1000)));
			}
		}
		private static  void TransformHandles      (SuperPlane aPlane, Vector3 rtb, Vector3 lbf) {
			Vector3 snap = EditorTools.GetUnitySnap();
	        
			EditorTools.capDir = Matrix4x4.TRS(Vector3.zero, aPlane.transform.rotation, Vector3.one);
	        
			Handles.color = Color.Lerp(Color.red, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Right, aPlane.transform, true, false, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
			SuperCubeEditorUtil.Handle(PivotType.Left,  aPlane.transform, true, false, false, snap, ref lbf, ref rtb, EditorTools.BarCapZ);
	        
			Handles.color = Color.Lerp(Color.blue, Color.white, 0.5f);
			SuperCubeEditorUtil.Handle(PivotType.Front, aPlane.transform, false, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
			SuperCubeEditorUtil.Handle(PivotType.Back,  aPlane.transform, false, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapX);
	        
			Handles.color = Color.Lerp( Color.Lerp(Color.red, Color.white, 0.5f), Color.Lerp(Color.blue, Color.white, 0.5f), .5f);
			SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Right, aPlane.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapXZ);
			SuperCubeEditorUtil.Handle(PivotType.Back  | PivotType.Left,  aPlane.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapXZ);
			SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Right, aPlane.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapXZ);
			SuperCubeEditorUtil.Handle(PivotType.Front | PivotType.Left,  aPlane.transform, true, false, true, snap, ref lbf, ref rtb, EditorTools.BarCapXZ);

			// Check if there was an edit, and update the mesh!
			Vector3 newExtents = new Vector3(Mathf.Abs(rtb.x - lbf.x), Mathf.Abs(rtb.y - lbf.y), Mathf.Abs(rtb.z - lbf.z));
			if (aPlane.Extents != newExtents) {
				aPlane.transform.position += aPlane.transform.TransformDirection(lbf + new Vector3((rtb.x - lbf.x) / 2, (rtb.y - lbf.y) / 2, (rtb.z - lbf.z) / 2));
				aPlane.Extents = newExtents;
				aPlane.Build(true);
			}
		}
		#endregion

		#region Helper methods
		private void CheckScale(SuperPlane aPlane) {
			if (aPlane != null && aPlane.transform.localScale != Vector3.one) {
				aPlane.Extents = new Vector3(aPlane.Extents.x * aPlane.transform.localScale.x, aPlane.Extents.y * aPlane.transform.localScale.y, aPlane.Extents.z * aPlane.transform.localScale.z);
				aPlane.transform.localScale = Vector3.one;
				aPlane.Build(true);
			}
		}
		#endregion
		
		#region Menu hook
		[MenuItem("GameObject/3D Object/Super Plane", false, 0), MenuItem("Tools/Ferr SuperCube/Super Plane %&l", false, 0)]
		private static void CreateSuperPlane() {
			GameObject obj = new GameObject("SuperPlane");
			obj.AddComponent<BoxCollider>();
			obj.AddComponent<SuperPlane >().Build(true);
			obj.GetComponent<Renderer>().sharedMaterial = EditorTools.GetDefaultMaterial();
			
			Vector3 snap = new Vector3(EditorPrefs.GetFloat("MoveSnapX"), EditorPrefs.GetFloat("MoveSnapY"), EditorPrefs.GetFloat("MoveSnapZ"));
			obj.transform.position = SuperCubeEditorUtil.SnapVector(SuperCubeEditorUtil.GetSpawnPos(), snap);
			
			Selection.activeGameObject = obj;
			EditorGUIUtility.PingObject(obj);
		}
		#endregion
	}
}