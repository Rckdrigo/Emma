using UnityEngine;
using System.Collections.Generic;

using Ferr;

[AddComponentMenu("Ferr SuperCube/SuperPlane"), RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SuperPlane : MonoBehaviour, IProceduralMesh {
	#region Fields
	[SerializeField]
	Vector3 mExtents       = Vector3.one;
	[SerializeField]
	UVType  mUVType        = UVType.Unit;
	[SerializeField]
	Vector2 mUVOffset      = new Vector2(0, 0);
	[SerializeField]
	Vector2 mUVTile        = new Vector2(1, 1);
	[SerializeField]
	bool    mSliceFaces    = false;
	[SerializeField, RangeAttribute(0.25f, 10)]
	float   mSliceDistance = 1;
	
	[SerializeField, HideInInspector]
	int mPrevSlicesX = 0;
	[SerializeField, HideInInspector]
	int mPrevSlicesY = 0;
	#endregion
	
	#region Properties
	/// <summary>
	/// The total size of the plane mesh on each axis. (local Unity units)
	/// </summary>
	public Vector3    Extents   {get{return mExtents;} set{mExtents = value;}}
	/// <summary>
	/// This IProceduralMesh property returns the sharedMesh of the object's MeshFilter component
	/// </summary>
	public Mesh       MeshData  {get{return GetComponent<MeshFilter>().sharedMesh;}}
	/// <summary>
	/// This IProceduralMesh property returns the object's MeshFilter component
	/// </summary>
	public MeshFilter MeshFilter{get{return GetComponent<MeshFilter>();}}

	public UVType  UVType     {get{return mUVType;     } set{mUVType=value;     }}
	public Vector2 UVTile     {get{return mUVTile;     } set{mUVTile=value;     }}
	public Vector2 UVOffset   {get{return mUVOffset;   } set{mUVOffset=value;   }}
	#endregion
	
	#region Unity events
	private void Start() {
		if (GetComponent<MeshFilter>().sharedMesh == null) {
			Debug.LogWarning("Ferr SuperPlane " + name + " had no mesh, rebuilt itself during runtime!");
			Build(true);
		}
	}
	#endregion
	
	#region Public methods
	/// <summary>
	/// Forces SuperPlane to create a completely new mesh object with a standard SuperPlane name, and rebuild
	/// </summary>
	public void ForceNewMesh() {
		MeshFilter filter = GetComponent<MeshFilter>();
		Mesh       m      = new Mesh();
		m.name = "FerrSuperPlane_" + gameObject.name + "_" + gameObject.GetInstanceID();
		filter.sharedMesh = m;
		
		Build(true);
	}
	/// <summary>
	/// Creates the SuperPlane mesh based on the existing parameters. aFullBuild tells it whether or not to calculate some of the more time expensive items, such as lightmap UVs.
	/// </summary>
	/// <param name="aFullBuild">Do time consuming calculations (lightmap UVs), or just the bare essentials?</param>
	public void Build       (bool aFullBuild) {
		MeshFilter filter   = GetComponent<MeshFilter>();
		Renderer   r        = GetComponent<Renderer>();
		Mesh       m        = filter.sharedMesh;
		string     thisName = "FerrSuperPlane_" + gameObject.name + "_" + gameObject.GetInstanceID();
		
		// Make sure there's a mesh for us to build into!
		if (m      == null) m      = new Mesh();
		if (m.name == ""  ) m.name = thisName;
		else if (m.name.Contains("FerrSuperPlane_") && m.name != thisName) {
			m      = new Mesh();
			m.name = thisName;
		}
		
		// figure out how many slices go onto the mesh
		int sliceX = 2;
		int sliceY = 2;
		if (mSliceFaces) {
			sliceX = Mathf.Max(2, (int)(mExtents.x / mSliceDistance) + 1);
			sliceY = Mathf.Max(2, (int)(mExtents.z / mSliceDistance) + 1);
		}
		
		int vCount = sliceX * sliceY;
		List<Vector3> verts   = new List<Vector3>(vCount);
		List<Vector2> uvs     = new List<Vector2>(vCount);
		List<Vector3> norms   = new List<Vector3>(vCount);
		List<Vector4> tans    = new List<Vector4>(vCount);
		List<Color  > colors  = new List<Color  >(vCount);
		List<int    > indices = new List<int    >((sliceX-1) * (sliceY-1) * 6);
		
		// calculate the uv step amount based on its aspect ratio
		float uvRatio = 1;
		if (r.sharedMaterial != null && r.sharedMaterial.mainTexture != null)
			uvRatio = r.sharedMaterial.mainTexture.width / r.sharedMaterial.mainTexture.height;
		
		// Create the plane itself
		SuperCubeUtil.AddFace(transform.localToWorldMatrix, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90,0,0), new Vector3(mExtents.x, mExtents.z, 1)), 0, mUVType, mUVOffset, mUVTile, 0, mExtents.z * uvRatio / mExtents.y, sliceX, sliceY, ref verts, ref uvs, ref norms, ref tans, ref indices);

		// TODO: Improve plane color sampling for 1.1
		// Resample mesh colors
		if (m.colors != null && m.colors.Length > 0) {
			SuperCubeUtil.ResampleColors(ref colors, sliceX, sliceY, m.colors, 0, mPrevSlicesX, mPrevSlicesY);
		} else {
			for (int i = 0; i < verts.Count; ++i) {
				colors.Add(Color.white);
			}
		}
		
		if (verts.Count >= 65000) {
			Debug.LogWarning("SuperCube has too many vertices (more than 65,000)! Please increase Slice Distance, or hide unnecessary faces!");
			return;
		}
		#if UNITY_EDITOR
		if (verts.Count > 500 && aFullBuild && UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet( gameObject, UnityEditor.StaticEditorFlags.LightmapStatic )) {
			Debug.LogWarning("Unwrapping for lightmaps may take a long time to calculate ("+verts.Count+" vertices)! You can disable this calculation by unchecking Lightmap Static in your GameObject's static settings! Try enabling it once after your object has been placed properly, or increase the Slice Distance!");
		}
		#endif

		// Set the mesh data
		m.Clear();
		m.vertices = verts .ToArray();
		m.normals  = norms .ToArray();
		m.uv       = uvs   .ToArray();
		m.tangents = tans  .ToArray();
		m.colors   = colors.ToArray();
		m.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
		
		// Additional mesh processing
		m.RecalculateBounds();
		#if UNITY_EDITOR
		if (aFullBuild && UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet( gameObject, UnityEditor.StaticEditorFlags.LightmapStatic )) {
			UnityEditor.Unwrapping.GenerateSecondaryUVSet(m);
		}
		#endif
		
		// Set the mesh, and update any box colliders on the object!
		filter.sharedMesh = m;
		BoxCollider collider = GetComponent<BoxCollider>();
		if (collider != null) {
			collider.center  = Vector3.zero;
			collider.size = new Vector3(mExtents.x, 0, mExtents.z);
		}
		
		// Remember the number of  slices for color resampling!
		mPrevSlicesX = sliceX;
		mPrevSlicesY = sliceY;
	}
	#endregion

	#region Creation methods
	/// <summary>
	/// Creates a SuperPlane game object, and assigns the given data! This function will build the mesh right away.
	/// </summary>
	/// <param name="aAt">Location in world space.</param>
	/// <param name="aSize">The width and height of the SuperPlane.</param>
	/// <param name="aMaterial">The material to assign to it, don't want that awful pink color!</param>
	/// <param name="aType">What type of UV coordinates do you want on the plane?</param>
	/// <param name="aSliceDistance">How far apart should extra verts be spaced out on the surface? 0 for none at all. floor(size/sliceDistance)</param>
	/// <returns>A ready-to-go SuperPlane GameObject named "SuperPlane" with a with a fully built SuperPlane, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject Create      (Vector3 aAt, Vector2 aSize, Material aMaterial, UVType aType = UVType.Unit, float aSliceDistance = 0) {
		GameObject go    = new GameObject("SuperPlane");
		SuperPlane plane = go.AddComponent<SuperPlane>();
		plane.mUVType = aType;
		plane.Extents = new Vector3(aSize.x, 0, aSize.y);
		plane.transform.position = aAt;
		if (aSliceDistance > 0) {
			plane.mSliceDistance = aSliceDistance;
			plane.mSliceFaces    = true;
		}        
		plane.Build(true);                          
		go.GetComponent<Renderer>().sharedMaterial = aMaterial;

		go.AddComponent<BoxCollider>();
		return go;
	}
	/// <summary>
	/// Creates a SuperPlane game object, and assigns the given data! This function will build the mesh right away.
	/// </summary>
	/// <param name="aPivotPt">Location to place the pivot point of the object. This is not the actual position of the object after the pivot is applied.</param>
	/// <param name="aPivotType">A bit flag that represents where the pivot is located. This only accepts Top, Bottom, Left, Right flag options, Top being +Z</param>
	/// <param name="aSize">The width and height of the SuperPlane.</param>
	/// <param name="aMaterial">The material to assign to it, don't want that awful pink color!</param>
	/// <param name="aType">What type of UV coordinates do you want on the plane?</param>
	/// <param name="aSliceDistance">How far apart should extra verts be spaced out on the surface? 0 for none at all. floor(size/sliceDistance)</param>
	/// <returns>A ready-to-go SuperPlane GameObject named "SuperPlane" with a with a fully built SuperPlane, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject CreatePivot (Vector3 aPivotPt, PivotType aPivotType, Vector2 aSize, Material aMaterial, UVType aType = UVType.Unit, float aSliceDistance = 0) {
		Vector2 halfSize = aSize/2;
		Vector3 point    = aPivotPt;
		
		if ((aPivotType & PivotType.Top   ) > 0) {
			point.z -= halfSize.y;
		}
		if ((aPivotType & PivotType.Bottom) > 0) {
			point.z += halfSize.y;
		}
		if ((aPivotType & PivotType.Left  ) > 0) {
			point.x += halfSize.x;
		}
		if ((aPivotType & PivotType.Right ) > 0) {
			point.x -= halfSize.x;
		}
		
		return Create(point, aSize, aMaterial, aType, aSliceDistance);
	}
	/// <summary>
	/// Creates a SuperPlane game object on the XZ plane from the given rectangle, Y=0, facing +Y. This function will build the mesh right away.
	/// </summary>
	/// <param name="aRect">A rectangle describing the location and size of the plane on the XZ axis</param>
	/// <param name="aMaterial">The material to assign to it, don't want that awful pink color!</param>
	/// <param name="aType">What type of UV coordinates do you want on the plane?</param>
	/// <param name="aSliceDistance">How far apart should extra verts be spaced out on the surface? 0 for none at all. floor(size/sliceDistance)</param>
	/// <returns>A ready-to-go SuperPlane GameObject named "SuperPlane" with a with a fully built SuperPlane, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject CreateRectXZ(Rect    aRect,              Material aMaterial, UVType aType = UVType.Unit, float aSliceDistance = 0) {
		Vector3 middle  = new Vector3(aRect.xMin + aRect.width/2, 0, aRect.yMin - aRect.height/2);
		Vector2 extents = new Vector2(aRect.width, aRect.height);
		return Create(middle, extents, aMaterial, aType, aSliceDistance);
	}
	/// <summary>
	/// Creates a SuperPlane game object on the XY plane from the given rectangle, Z=0, facing -Z. This function will build the mesh right away.
	/// </summary>
	/// <param name="aRect">A rectangle describing the location and size of the plane on the XY axis</param>
	/// <param name="aMaterial">The material to assign to it, don't want that awful pink color!</param>
	/// <param name="aType">What type of UV coordinates do you want on the plane?</param>
	/// <param name="aSliceDistance">How far apart should extra verts be spaced out on the surface? 0 for none at all. floor(size/sliceDistance)</param>
	/// <returns>A ready-to-go SuperPlane GameObject named "SuperPlane" with a with a fully built SuperPlane, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject CreateRectXY(Rect    aRect,              Material aMaterial, UVType aType = UVType.Unit, float aSliceDistance = 0) {
		Vector3    middle  = new Vector3(aRect.xMin + aRect.width/2, aRect.yMin - aRect.height/2, 0);
		Vector2    extents = new Vector2(aRect.width, aRect.height);
		GameObject go      = Create(middle, extents, aMaterial, aType, aSliceDistance);
		go.transform.localEulerAngles = new Vector3(-90, 0, 0);
		return go;
	}
	#endregion
}