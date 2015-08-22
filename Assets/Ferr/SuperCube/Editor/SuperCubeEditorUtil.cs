using UnityEngine;
using UnityEditor;

namespace Ferr {
	static class SuperCubeEditorUtil {
		#region Static fields
		static Texture2D mTexVisible = EditorTools.GetGizmo("SuperCube/Gizmos/Visible.png");
		static Texture2D mTexHidden  = EditorTools.GetGizmo("SuperCube/Gizmos/Hidden.png" );
		static Texture2D mTexAdd     = EditorTools.GetGizmo("SuperCube/Gizmos/AddCube.png");

		internal static float mHandleSize = 0.15f;
		internal static Tool  mGizmoTool  = Tool.None;
		#endregion

		#region Position methods
		internal static Vector3  GetSpawnPos       () {
			Plane   plane  = new Plane(new Vector3(0, 1, 0), 0);
			float   dist   = 0;
			Vector3 result = new Vector3(0, 0, 0);
			Ray ray = SceneView.lastActiveSceneView == null ? new Ray(Vector3.up*10,-Vector3.up) :  SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit)) {
				result = ray.GetPoint(hit.distance);
			}
			else if (plane.Raycast(ray, out dist)) {
				result = ray.GetPoint(dist);
			}
			return result;
		}
		internal static Vector3  Get2DPoint        (Vector3 aPoint, float aHeight) {
			return GetPlanePoint(aPoint, new Plane(Vector3.up, new Vector3(0,aHeight,0)));
		}
		internal static Vector3  GetPlanePoint        (Vector3 aPoint, Plane aPlane) {
			Plane p = aPlane;
			Ray   r = SceneView.lastActiveSceneView == null ? new Ray(Vector3.up,-Vector3.up) : new Ray  (SceneView.lastActiveSceneView.camera.transform.position, aPoint - SceneView.lastActiveSceneView.camera.transform.position);
			float d = 0;
			if (p.Raycast(r, out d)) {
				Vector3 result = r.GetPoint(d);
				return result;
			}
			return aPoint;
		}
		internal static Vector3 CreateBillboardNormal(Vector3 aPt, bool aXLock, bool aYLock, bool aZLock) {
			Vector3 dir = SceneView.lastActiveSceneView == null ? Vector3.up : (SceneView.lastActiveSceneView.camera.transform.position - aPt);
			if (aYLock) {
				dir.y = 0;
			}
			if (aXLock) {
				dir.x = 0;
			}
			if (aZLock) {
				dir.z = 0;
			}
			return dir.normalized;
		}
		public static Vector3 SnapVector  (Vector3 aVector, Vector3 snap) {
			if (Event.current != null && (Event.current.control ^ !SuperCubeSettings.SnapAsDefault)) return aVector;
			return new Vector3(
				((int)(aVector.x / snap.x + (aVector.x > 0 ? 0.5f : -0.5f))) * snap.x,
				((int)(aVector.y / snap.y + (aVector.y > 0 ? 0.5f : -0.5f))) * snap.y,
				((int)(aVector.z / snap.z + (aVector.z > 0 ? 0.5f : -0.5f))) * snap.z);
		}
		#endregion

		#region Cap methods
		internal static void Handle(PivotType aSides, Transform aTransform, bool aLockX, bool aLockY, bool aLockZ, Vector3 aSnap, ref Vector3 lbf, ref Vector3 rtb, Handles.DrawCapFunction aCap) {
			Vector3 pt = new Vector3(0,0,0);
			Vector3 mask = new Vector3(aLockX?1:0, aLockY?1:0, aLockZ?1:0);
			if ((aSides & PivotType.Left  ) > 0) pt.x = lbf.x;
			if ((aSides & PivotType.Right ) > 0) pt.x = rtb.x;
			if ((aSides & PivotType.Top   ) > 0) pt.y = rtb.y;
			if ((aSides & PivotType.Bottom) > 0) pt.y = lbf.y;
			if ((aSides & PivotType.Front ) > 0) pt.z = lbf.z;
			if ((aSides & PivotType.Back  ) > 0) pt.z = rtb.z;
			pt = aTransform.TransformPoint(pt);
			
			Vector3 result = Handles.FreeMoveHandle(pt, Quaternion.identity, HandleUtility.GetHandleSize(pt) * mHandleSize, Vector3.zero, aCap);
			if (result != pt) {
				Vector3 dir = SuperCubeEditorUtil.CreateBillboardNormal(pt, aLockX, aLockY, aLockZ);
				result = SuperCubeEditorUtil.SnapVector(Vector3.Scale(SuperCubeEditorUtil.GetPlanePoint(result, new Plane(dir, pt)), mask), aSnap);
				result = aTransform.InverseTransformPoint(result);
				
				if ((aSides & PivotType.Left  ) > 0) lbf.x = result.x;
				if ((aSides & PivotType.Right ) > 0) rtb.x = result.x;
				if ((aSides & PivotType.Top   ) > 0) rtb.y = result.y;
				if ((aSides & PivotType.Bottom) > 0) lbf.y = result.y;
				if ((aSides & PivotType.Front ) > 0) lbf.z = result.z;
				if ((aSides & PivotType.Back  ) > 0) rtb.z = result.z;
			}
		}
		
		internal static void HiddenCap (int aControlID, Vector3 aPosition, Quaternion aRotation, float aSize) {
			EditorTools.ImageCapBase(aControlID, aPosition, aRotation, aSize, mTexHidden);
		}
		internal static void VisibleCap(int aControlID, Vector3 aPosition, Quaternion aRotation, float aSize) {
			EditorTools.ImageCapBase(aControlID, aPosition, aRotation, aSize, mTexVisible);
		}
		internal static void AddCap    (int aControlID, Vector3 aPosition, Quaternion aRotation, float aSize) {
			EditorTools.ImageCapBase(aControlID, aPosition, aRotation, aSize, mTexAdd);
		}
		#endregion
		
		#region Menu hooks
		[MenuItem("Tools/Ferr SuperCube/Optimizer Object", false, 11)]
		private static void CreateOptimizerObject() {
			GameObject go = GameObject.Find("_Ferr SuperCube Optimizer");
			if (go == null) {
				go = new GameObject("_Ferr SuperCube Optimizer");
				go.AddComponent<SuperMeshCombiner>();
			} else {
				if (go.GetComponent<SuperMeshCombiner>() == null) {
					go.AddComponent<SuperMeshCombiner>();
				}
			}
			
			Selection.activeGameObject = go;
			EditorGUIUtility.PingObject(go);
		}
		#endregion
	}
}