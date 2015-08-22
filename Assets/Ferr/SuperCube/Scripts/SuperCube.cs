#if UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0 || UNITY_4_0_1 || UNITY_3_5
#define NO_DEF
#endif

using UnityEngine;
using System.Collections.Generic;

using Ferr;

[AddComponentMenu("Ferr SuperCube/SuperCube"), RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SuperCube : MonoBehaviour, IProceduralMesh {
	#region Fields
	[SerializeField]
	Vector3 mExtents         = Vector3.one;
	[SerializeField]
	UVType  mWallUVType      = UVType.WorldCoordinates;
	[SerializeField]
	Vector2 mWallUVOffset    = new Vector2(0, 0);
	[SerializeField]
	Vector2 mWallUVTile      = new Vector2(1, 1);
	[SerializeField]
	UVType  mTopBottomUVType = UVType.WorldCoordinates;
	[SerializeField]
	Vector2 mTopBottomUVOffset = new Vector2(0, 0);
	[SerializeField]
	Vector2 mTopBottomUVTile = new Vector2(1, 1);
	[SerializeField]
	bool    mSliceFaces      = false;
	[SerializeField, RangeAttribute(0.25f, 10)]
	float   mSliceDistance   = 1;
	
	[SerializeField]
	bool     mFaceBottom     = true;
	[SerializeField]
	Material mOverrideBottom = null;
	[SerializeField]
	bool     mFaceTop        = true;
	[SerializeField]
	Material mOverrideTop    = null;
	[SerializeField]
	bool     mFaceFront      = true;
	[SerializeField]
	Material mOverrideFront  = null;
	[SerializeField]
	bool     mFaceBack       = true;
	[SerializeField]
	Material mOverrideBack   = null;
	[SerializeField]
	bool     mFaceLeft       = true;
	[SerializeField]
	Material mOverrideLeft   = null;
	[SerializeField]
	bool     mFaceRight      = true;
	[SerializeField]
	Material mOverrideRight  = null;
	
	[SerializeField, HideInInspector]
	int[] mPrevSliceX = new int[6];
	[SerializeField, HideInInspector]
	int[] mPrevSliceY = new int[6];
	#endregion
	
	#region Properties
	/// <summary>
	/// The total size of the cube mesh on each axis. (local Unity units)
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
	
	public bool    SliceFaces     {get{return mSliceFaces;     } set{mSliceFaces=value;     }}
	public float   SliceDistance  {get{return mSliceDistance;  } set{mSliceDistance=value;  }}
	public UVType  WallUVType     {get{return mWallUVType;     } set{mWallUVType=value;     }}
	public Vector2 WallUVTile     {get{return mWallUVTile;     } set{mWallUVTile=value;     }}
	public Vector2 WallUVOffset   {get{return mWallUVOffset;   } set{mWallUVOffset=value;   }}
	public UVType  TopBottomUVType{get{return mTopBottomUVType;} set{mTopBottomUVType=value;}}
	public Vector2 TopBottomUVTile  {get{return mTopBottomUVTile;  } set{mTopBottomUVTile=value;  }}
	public Vector2 TopBottomUVOffset{get{return mTopBottomUVOffset;} set{mTopBottomUVOffset=value;}}

	/// <summary>
	/// Enable/Disable the top face (+Y) of the cube
	/// </summary>
	public bool FaceTop    {get{return mFaceTop;   }set{mFaceTop   = value;}}
	/// <summary>
	/// Enable/Disable the bottom face (-Y) of the cube
	/// </summary>
	public bool FaceBottom {get{return mFaceBottom;}set{mFaceBottom= value;}}
	/// <summary>
	/// Enable/Disable the right face (+X) of the cube
	/// </summary>
	public bool FaceRight  {get{return mFaceRight; }set{mFaceRight = value;}}
	/// <summary>
	/// Enable/Disable the left face (-X) of the cube
	/// </summary>
	public bool FaceLeft   {get{return mFaceLeft;  }set{mFaceLeft  = value;}}
	/// <summary>
	/// Enable/Disable the front face (-Z) of the cube
	/// </summary>
	public bool FaceFront  {get{return mFaceFront; }set{mFaceFront = value;}}
	/// <summary>
	/// Enable/Disable the back face (+Z) of the cube
	/// </summary>
	public bool FaceBack   {get{return mFaceBack;  }set{mFaceBack  = value;}}
	
	/// <summary>
	/// Override material for the top face (+Y) of the cube
	/// </summary>
	public Material OverrideTop    {get{return mOverrideTop;   }set{mOverrideTop    = value;}}
	/// <summary>
	/// Override material for the bottom face (-Y) of the cube
	/// </summary>
	public Material OverrideBottom {get{return mOverrideBottom;}set{mOverrideBottom = value;}}
	/// <summary>
	/// Override material for the right face (+X) of the cube
	/// </summary>
	public Material OverrideRight  {get{return mOverrideRight; }set{mOverrideRight  = value;}}
	/// <summary>
	/// Override material for the left face (-X) of the cube
	/// </summary>
	public Material OverrideLeft   {get{return mOverrideLeft;  }set{mOverrideLeft   = value;}}
	/// <summary>
	/// Override material for the front face (-Z) of the cube
	/// </summary>
	public Material OverrideFront  {get{return mOverrideFront; }set{mOverrideFront  = value;}}
	/// <summary>
	/// Override material for the back face (+Z) of the cube
	/// </summary>
	public Material OverrideBack   {get{return mOverrideBack;  }set{mOverrideBack   = value;}}
	
	
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
	/// Forces SuperCube to create a completely new mesh object with a standard SuperCube name, and rebuild
	/// </summary>
	public void ForceNewMesh() {
		MeshFilter filter = GetComponent<MeshFilter>();
		Mesh       m      = new Mesh();
		m.name = "FerrSuperCube_" + gameObject.name + "_" + gameObject.GetInstanceID();
		filter.sharedMesh = m;
		
		Build(true);
	}
	/// <summary>
	/// Creates the SuperCube mesh based on the existing parameters. aFullBuild tells it whether or not to calculate some of the more time expensive items, such as lightmap UVs.
	/// </summary>
	/// <param name="aFullBuild">Do time consuming calculations (lightmap UVs, editor only), or just the bare essentials?</param>
	public void Build       (bool aFullBuild) {
		MeshFilter filter   = GetComponent<MeshFilter>();
		Renderer   r        = GetComponent<Renderer>();
		Mesh       m        = filter.sharedMesh;
		string     thisName = "FerrSuperCube_" + gameObject.name + "_" + gameObject.GetInstanceID();
		
		// Make sure there's a mesh for us to build into!
		if (m      == null) m = new Mesh();
		if (m.name == ""  ) m.name = thisName;
		else if (m.name.Contains("FerrSuperCube_") && m.name != thisName) {
			m      = new Mesh();
			m.name = thisName;
		}
		
		Matrix4x4 transMat = transform.localToWorldMatrix;
		int vCount = GetVertCount();
		List<Vector3> verts = new List<Vector3>(vCount);
		List<Vector2> uvs   = new List<Vector2>(vCount);
		List<Vector3> norms = new List<Vector3>(vCount);
		List<Vector4> tans  = new List<Vector4>(vCount);
		List<Color  > colors= new List<Color  >(vCount);
		
		List<List<int>> indexList = new List<List<int>>();
		List<Material>  materials = new List<Material>();
		CreateIndexArrays(out materials, out indexList);
		
		// figure out how many slices go onto the mesh
		int sliceX  = 2;
		int sliceY  = 2;
		int sliceZ  = 2;
		if (mSliceFaces) {
			sliceX = Mathf.Max(2, (int)(mExtents.x / mSliceDistance) + 1);
			sliceY = Mathf.Max(2, (int)(mExtents.y / mSliceDistance) + 1);
			sliceZ = Mathf.Max(2, (int)(mExtents.z / mSliceDistance) + 1);
		}
		int startID = 0;
		
		// calculate the uv step amount based on its aspect ratio
		float uStart  = 0;
		float uvRatio = 1;
		if (r.sharedMaterial != null && r.sharedMaterial.mainTexture != null)
			uvRatio = (float)r.sharedMaterial.mainTexture.height / r.sharedMaterial.mainTexture.width;
		
		// Front
		List<int> indices = GetIndexArray(mOverrideFront, materials, indexList);
		if (mFaceFront) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,  0), new Vector3(mExtents.x, mExtents.y, mExtents.z)), 0.5f, mWallUVType, mWallUVOffset, mWallUVTile, uStart, uStart + mExtents.x * uvRatio / mExtents.y, sliceX, sliceY, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(0, startID, m, ref colors, sliceX, sliceY);
		}
		uStart += mExtents.x * uvRatio / mExtents.y;
		
		// Left
		indices = GetIndexArray(mOverrideLeft, materials, indexList);
		if (mFaceLeft) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90, 0), new Vector3(mExtents.z, mExtents.y, mExtents.x)), 0.5f, mWallUVType, mWallUVOffset, mWallUVTile, uStart, uStart + mExtents.z * uvRatio / mExtents.y, sliceZ, sliceY, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(1, startID, m, ref colors, sliceZ, sliceY);
		}
		uStart += mExtents.z * uvRatio / mExtents.y;
		
		// Back
		indices = GetIndexArray(mOverrideBack, materials, indexList);
		if (mFaceBack) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,180,0), new Vector3(mExtents.x, mExtents.y, mExtents.z)), 0.5f, mWallUVType, mWallUVOffset, mWallUVTile, uStart, uStart + mExtents.x * uvRatio / mExtents.y, sliceX, sliceY, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(2, startID, m, ref colors, sliceX, sliceY);
		}
		uStart += mExtents.x * uvRatio / mExtents.y;
		
		// Right
		indices = GetIndexArray(mOverrideRight, materials, indexList);
		if (mFaceRight) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,270,0), new Vector3(mExtents.z, mExtents.y, mExtents.x)), 0.5f, mWallUVType, mWallUVOffset, mWallUVTile, uStart, uStart + mExtents.z * uvRatio / mExtents.y, sliceZ, sliceY, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(3, startID, m, ref colors, sliceZ, sliceY);
		}
		uStart += mExtents.z * uvRatio / mExtents.y;
		
		// Top
		indices = GetIndexArray(mOverrideTop, materials, indexList);
		if (mFaceTop   ) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0,0), new Vector3(mExtents.x, mExtents.z, mExtents.y)), 0.5f, mTopBottomUVType, mTopBottomUVOffset, mTopBottomUVTile, uStart, uStart + mExtents.x, sliceX, sliceZ, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(4, startID, m, ref colors, sliceX, sliceZ);
		}
		// Bottom
		indices = GetIndexArray(mOverrideBottom, materials, indexList);
		if (mFaceBottom) {
			SuperCubeUtil.AddFace(transMat, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(270,0,0), new Vector3(mExtents.x, mExtents.z, mExtents.y)), 0.5f, mTopBottomUVType, mTopBottomUVOffset, mTopBottomUVTile, uStart, uStart + mExtents.x, sliceX, sliceZ, ref verts, ref uvs, ref norms, ref tans, ref indices);
			startID = Resample(5, startID, m, ref colors, sliceX, sliceZ);
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
		m.vertices = verts.ToArray();
		m.normals  = norms.ToArray();
		m.uv       = uvs  .ToArray();
		m.tangents = tans .ToArray();
		// TODO: Color resampling for v1.1
		//m.colors   = colors.ToArray();
		
		// Set up the mesh indices and material based sub-meshes
		m.subMeshCount = indexList.Count;
		for (int i = 0; i < indexList.Count; ++i) {
			if (indexList[i].Count <= 0) {
				indexList.RemoveAt(i);
				materials.RemoveAt(i);
				i--;
				continue;
			}
			m.SetIndices(indexList[i].ToArray(), MeshTopology.Triangles, i);
		}
		r.sharedMaterials = materials.ToArray();
		
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
			collider.size = mExtents;
		}
	}
	#endregion
	
	#region Helper methods
	private int GetVertCount() {
		int result = 0;
		int sliceX  = 2;
		int sliceY  = 2;
		int sliceZ  = 2;
		
		if (mSliceFaces) {
			sliceX = Mathf.Max(2, (int)(mExtents.x / mSliceDistance) + 1);
			sliceY = Mathf.Max(2, (int)(mExtents.y / mSliceDistance) + 1);
			sliceZ = Mathf.Max(2, (int)(mExtents.z / mSliceDistance) + 1);
		}
		
		if (mFaceBack  ) result += sliceX * sliceY;
		if (mFaceFront ) result += sliceX * sliceY;
		if (mFaceBottom) result += sliceX * sliceZ;
		if (mFaceTop   ) result += sliceX * sliceZ;
		if (mFaceLeft  ) result += sliceY * sliceZ;
		if (mFaceRight ) result += sliceY * sliceZ;
		
		return result;
	}
	private int       Resample         (int aFaceID, int aStartID, Mesh aMesh, ref List<Color> aColors, int aSliceX, int aSliceY) {
		// TODO: Color resampling for 1.1
		//if (aMesh.colors != null && aMesh.colors.Length > 0) {
		//	SuperCubeUtil.ResampleColors(ref aColors, aSliceX, aSliceY, aMesh.colors, aStartID, mPrevSliceX[aFaceID], mPrevSliceY[aFaceID]);
		//} else {
		for (int i = 0; i < aSliceX*aSliceY; ++i) {
			aColors.Add(Color.white);
		}
		//}
		aStartID += mPrevSliceX[aFaceID] * mPrevSliceY[aFaceID];
		mPrevSliceX[aFaceID] = aSliceX;
		mPrevSliceY[aFaceID] = aSliceY;

		return aStartID;
	}
	private void      CreateIndexArrays(out List<Material> aMaterialList, out List<List<int>> aIndices) {
		aMaterialList = new List<Material>();
		Material defaultMat = GetComponent<Renderer>().sharedMaterial;
		aMaterialList.Add(defaultMat);
		
		Material m = mOverrideFront == null ? defaultMat : mOverrideFront;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideFront  == null ? defaultMat : mOverrideFront;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideBack   == null ? defaultMat : mOverrideBack;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideLeft   == null ? defaultMat : mOverrideLeft;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideRight  == null ? defaultMat : mOverrideRight;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideTop    == null ? defaultMat : mOverrideTop;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		m = mOverrideBottom == null ? defaultMat : mOverrideBottom;
		if (!aMaterialList.Contains(m)) aMaterialList.Add(m);
		
		aIndices = new List<List<int>>();
		for (int i = 0; i < aMaterialList.Count; ++i) {
			aIndices.Add(new List<int>());
		}
	}
	private List<int> GetIndexArray    (Material aMaterial, List<Material> aMaterialList, List<List<int>> aIndices) {
		Material m = aMaterial == null ? GetComponent<Renderer>().sharedMaterial : aMaterial;
		return aIndices[aMaterialList.IndexOf(m)];
	}
	#endregion

	#region Creation methods
	/// <summary>
	/// Creates a SuperCube game object, and assigns the given data! This function will build the cube mesh right away.
	/// </summary>
	/// <param name="aAt">The center of the SuperCube.</param>
	/// <param name="aSize">The width, height, and depth of the SuperCube.</param>
	/// <param name="aMaterial">The default material for the entire SuperCube.</param>
	/// <param name="aWallUV">The type of UV calculations to use for the +X, -X, +Z, -Z faces.</param>
	/// <param name="aTopBottomUV">The type of UV calculations to use for the +Y, -Y faces.</param>
	/// <param name="aSliceDistance">How far apart shall we try and place vertex slices on the faces? floor(size/sliceDistance)</param>
	/// <param name="aHideFaces">A bit mask describing which faces should be hidden.</param>
	/// <param name="aOverrideTop">Material override for the +Y face.</param>
	/// <param name="aOverrideBottom">Material override for the -Y face.</param>
	/// <param name="aOverrideLeft">Material override for the -X face.</param>
	/// <param name="aOverrideRight">Material override for the +X face.</param>
	/// <param name="aOverrideFront">Material override for the -Z face.</param>
	/// <param name="aOverrideBack">Material override for the +Z face.</param>
	/// <returns>A GameObject named "SuperCube" with a fully built SuperCube, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject Create      (Vector3 aAt,                            Vector3 aSize, Material aMaterial, UVType aWallUV = UVType.WorldCoordinates, UVType aTopBottomUV = UVType.WorldCoordinates, float aSliceDistance = 0, PivotType aHideFaces = PivotType.None, Material aOverrideTop = null, Material aOverrideBottom = null, Material aOverrideLeft = null, Material aOverrideRight = null, Material aOverrideFront = null, Material aOverrideBack = null ) {
		GameObject go  = new GameObject("SuperCube");
		SuperCube  box = go.AddComponent<SuperCube>();
		box.mWallUVType        = aWallUV;
		box.mTopBottomUVType   = aTopBottomUV;
		box.Extents            = aSize;
		box.transform.position = aAt;
		if (aSliceDistance > 0) {
			box.mSliceDistance = aSliceDistance;
			box.mSliceFaces    = true;
		}

		if ((aHideFaces & PivotType.Back  ) > 0 ) box.FaceBack   = false;
		if ((aHideFaces & PivotType.Bottom) > 0 ) box.FaceBottom = false;
		if ((aHideFaces & PivotType.Front ) > 0 ) box.FaceFront  = false;
		if ((aHideFaces & PivotType.Left  ) > 0 ) box.FaceLeft   = false;
		if ((aHideFaces & PivotType.Right ) > 0 ) box.FaceRight  = false;
		if ((aHideFaces & PivotType.Top   ) > 0 ) box.FaceTop    = false;

		box.mOverrideBack   = aOverrideBack;
		box.mOverrideFront  = aOverrideFront;
		box.mOverrideTop    = aOverrideTop;
		box.mOverrideBottom = aOverrideBottom;
		box.mOverrideLeft   = aOverrideLeft;
		box.mOverrideRight  = aOverrideRight;

		box.Build(true);
		go.GetComponent<Renderer>().sharedMaterial = aMaterial;

		go.AddComponent<BoxCollider>();
		return go;
	}
	/// <summary>
	/// Creates a SuperCube game object using a pivot point to determine location, and assigns the given data! This function will build the cube mesh right away.
	/// </summary>
	/// <param name="aPivotPt">Location in space for the pivot point to be placed.</param>
	/// <param name="aPivotType">A bit mask defining where on the cube the pivot is placed. Opposing sides indicate to center along that axis.</param>
	/// <param name="aSize">The width, height, and depth of the SuperCube.</param>
	/// <param name="aMaterial">The default material for the entire SuperCube.</param>
	/// <param name="aWallUV">The type of UV calculations to use for the +X, -X, +Z, -Z faces.</param>
	/// <param name="aTopBottomUV">The type of UV calculations to use for the +Y, -Y faces.</param>
	/// <param name="aSliceDistance">How far apart shall we try and place vertex slices on the faces? floor(size/sliceDistance)</param>
	/// <param name="aHideFaces">A bit mask describing which faces should be hidden.</param>
	/// <param name="aOverrideTop">Material override for the +Y face.</param>
	/// <param name="aOverrideBottom">Material override for the -Y face.</param>
	/// <param name="aOverrideLeft">Material override for the -X face.</param>
	/// <param name="aOverrideRight">Material override for the +X face.</param>
	/// <param name="aOverrideFront">Material override for the -Z face.</param>
	/// <param name="aOverrideBack">Material override for the +Z face.</param>
	/// <returns>A GameObject named "SuperCube" with a fully built SuperCube, MeshFilter, Renderer, and BoxCollider component!</returns>
	public static GameObject CreatePivot (Vector3 aPivotPt, PivotType aPivotType, Vector3 aSize, Material aMaterial, UVType aWallUV = UVType.WorldCoordinates, UVType aTopBottomUV = UVType.WorldCoordinates, float aSliceDistance = 0, PivotType aHideFaces = PivotType.None, Material aOverrideTop = null, Material aOverrideBottom = null, Material aOverrideLeft = null, Material aOverrideRight = null, Material aOverrideFront = null, Material aOverrideBack = null ) {
		Vector3 halfSize = aSize/2;
		Vector3 point    = aPivotPt;
		
		if ((aPivotType & PivotType.Top   ) > 0) {
			point.y -= halfSize.y;
		}
		if ((aPivotType & PivotType.Bottom) > 0) {
			point.y += halfSize.y;
		}
		if ((aPivotType & PivotType.Left  ) > 0) {
			point.x += halfSize.x;
		}
		if ((aPivotType & PivotType.Right ) > 0) {
			point.x -= halfSize.x;
		}
		if ((aPivotType & PivotType.Front ) > 0) {
			point.z += halfSize.z;
		}
		if ((aPivotType & PivotType.Back ) > 0) {
			point.z -= halfSize.z;
		}
		
		return Create(point, aSize, aMaterial, aWallUV, aTopBottomUV, aSliceDistance, aHideFaces, aOverrideTop, aOverrideBottom, aOverrideLeft, aOverrideRight, aOverrideFront, aOverrideBack);
	}
	#endregion
}
