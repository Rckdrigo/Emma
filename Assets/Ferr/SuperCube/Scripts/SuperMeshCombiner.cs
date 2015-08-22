using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Ferr SuperCube/SuperMesh Combiner")]
public class SuperMeshCombiner : MonoBehaviour {
	public const int MaxVerts = 65534;

	class Subset {
		public List<Vector3> mPoints     = new List<Vector3>();
		public List<Vector3> mNormals    = new List<Vector3>();
		public List<Vector2> mUVs        = new List<Vector2>();
		public List<Vector2> mLightUVs   = new List<Vector2>();
		public List<Vector4> mTangents   = new List<Vector4>();
		public List<Color>   mColors     = new List<Color>();
		public List<int    > mIndices    = new List<int>();
		public Material      mMaterial   = null;
		public int           mLightmapID = -1;
		public int           mMeshGroup  = -1;

		public int Count { get {return mPoints.Count;} }

		public void Add        (Subset aOther) {
			CheckArrays(aOther);

			mPoints  .AddRange(aOther.mPoints);
			mNormals .AddRange(aOther.mNormals);
			mUVs     .AddRange(aOther.mUVs);
			mLightUVs.AddRange(aOther.mLightUVs);
			mTangents.AddRange(aOther.mTangents);
			mColors  .AddRange(aOther.mColors);
		}
		public void CheckArrays(Subset aSubset = null) {
			if (aSubset == null) aSubset = this;

			if (aSubset.mNormals .Count > 0) CheckFilled<Vector3>(ref mNormals,  Vector3.zero, mPoints.Count);
			if (aSubset.mUVs     .Count > 0) CheckFilled<Vector2>(ref mUVs,      Vector2.zero, mPoints.Count);
			if (aSubset.mLightUVs.Count > 0) CheckFilled<Vector2>(ref mLightUVs, Vector2.zero, mPoints.Count);
			if (aSubset.mTangents.Count > 0) CheckFilled<Vector4>(ref mTangents, Vector4.zero, mPoints.Count);
			if (aSubset.mColors  .Count > 0) CheckFilled<Color  >(ref mColors,   Color.white,  mPoints.Count);
		}
		static void CheckFilled<T>(ref List<T> aList, T aValue, int aCount) {
			for (int i = aList.Count; i < aCount; i++) {
				aList.Add(aValue);
			}
		}
	}
	
	[SerializeField]
	bool mOnlyCombineChildren = false;
#if UNITY_5
	[SerializeField, HideInInspector]
#else
	[SerializeField]
#endif
	bool mOnlyStaticObjects   = false;
	[SerializeField]
	bool mOnlySuperObjects    = false;
	[SerializeField]
	bool mLogTimeCost         = false;
	
	public void Start() {
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();
		
		bool isPro = Application.HasProLicense();
#if UNITY_5
		isPro = true;
#endif

		// Find all the meshes in our domain
		MeshFilter[] meshes = null;
		if (mOnlyCombineChildren) {
			meshes = GetComponentsInChildren     <MeshFilter>();
		} else {
			meshes = GameObject.FindObjectsOfType<MeshFilter>();
		}
		
		List<Subset>         subsets    = new List<Subset>();
		Dictionary<int, int> layers     = new Dictionary<int, int>();
		int                  validCount = 0;
		for (int i = 0; i < meshes.Length; i+=1) {
			// determine if the mesh is valid, and check it with our settings
			if (meshes[i] == null  ||  meshes[i].sharedMesh == null) continue;
			if (meshes[i].GetComponent<SuperMeshCombiner>() != null) continue;
			if (mOnlySuperObjects  &&  meshes[i].GetComponent(typeof(Ferr.IProceduralMesh)) == null) continue;
			if (isPro              &&  meshes[i].gameObject.isStatic) continue; // Unity pro is already handling this object, we'll only crash the program if we try this.
			if (mOnlyStaticObjects && !meshes[i].gameObject.isStatic) continue;
			
			// just tracking count =D
			validCount += 1;
			
			// add the material subsets of the valid mesh
			Renderer  r   = meshes[i].GetComponent<Renderer>();
			Matrix4x4 mat = meshes[i].transform.localToWorldMatrix;
			for (int s = 0; s < meshes[i].sharedMesh.subMeshCount; s+=1) {
#if UNITY_5
				AddSubset(ref subsets, meshes[i].sharedMesh, r.lightmapIndex, s,  r.sharedMaterials[s], mat,  r.realtimeLightmapScaleOffset);
#else
				AddSubset(ref subsets, meshes[i].sharedMesh, r.lightmapIndex, s,  r.sharedMaterials[s], mat,  r.lightmapTilingOffset);
#endif
			}
			r.enabled = false;
			
			// keep track what layer this is on, and how many are on it
			if (!layers.ContainsKey(meshes[i].gameObject.layer)) layers.Add(meshes[i].gameObject.layer, 0);
			layers[meshes[i].gameObject.layer] += 1;
		}
		
		// figure out the most common layer type, and use that for our layer overall
		if (gameObject.layer == 0) {
			int commonLayer = gameObject.layer;
			int max         = 0;
			foreach (KeyValuePair<int, int> pair in layers) {
				if (pair.Value > max) {
					commonLayer = pair.Key;
					max         = pair.Value;
				}
			}
			gameObject.layer = commonLayer;
		}

		// create meshes based on the info we've collected
		int lightmapCount = LightmapCount(subsets);
		for (int i=0; i<lightmapCount; i+=1) {
			CreateSizedMeshes(gameObject, subsets, LightmapID(subsets, i));
		}
		
		sw.Stop();
		if (mLogTimeCost) Debug.Log(string.Format("Merging geometry [{1} objects scanned: {2} valid, {3} subsets]: {0}ms", System.Math.Round((float)sw.Elapsed.TotalMilliseconds, 2), meshes.Length, validCount, subsets.Count));
	}

	static void CreateSizedMeshes(GameObject aParent, List<Subset> aSubsets, int aLightmapID) {
		List<List<Subset>> sizedMeshes   = new List<List<Subset>>();
		List<int>          meshVertCount = new List<int>();

		// this should reduce extra draw calls that might happen when mesh subsets get split too small
		aSubsets.Sort((a,b)=> {return a.mMaterial==null?0:a.mMaterial.GetInstanceID().CompareTo(b.mMaterial==null?0:b.mMaterial.GetInstanceID());});

		sizedMeshes  .Add(new List<Subset>());
		meshVertCount.Add(0);
		for (int i = 0; i < aSubsets.Count; i++) {
			if (aSubsets[i].mLightmapID != aLightmapID) continue;

			// see if we can find room in one of the meshes we're about to create
			bool found = false;
			for (int m = 0; m < sizedMeshes.Count; m++) {
				if (meshVertCount[m] + aSubsets[i].Count < SuperMeshCombiner.MaxVerts) {
					found = true;
					meshVertCount[m] += aSubsets[i].Count;
					sizedMeshes  [m].Add(aSubsets[i]);
				}
			}

			// if there's no room, add a new mesh to the end!
			if (!found) {
				List<Subset> newMesh = new List<Subset>();
				newMesh.Add(aSubsets[i]);
				meshVertCount.Add(aSubsets[i].Count);
				sizedMeshes  .Add(newMesh);
			}
		}

		// create a mesh object for each collection of subsets
		for (int i = 0; i < sizedMeshes.Count; i++) {
			GameObject   go   = new GameObject("Lightmap_" + aLightmapID + "_mesh_" + i);
			MeshFilter   mesh = go.AddComponent<MeshFilter  >();
			MeshRenderer r    = go.AddComponent<MeshRenderer>();
			mesh.sharedMesh   = MergeSubsets(sizedMeshes[i], aLightmapID);
			r.sharedMaterials = GetMaterials(sizedMeshes[i], aLightmapID);
			r.lightmapIndex   = aLightmapID;
			
			go.layer = aParent.layer;
			go.transform.parent     = aParent.transform;
			go.transform.position   = Vector3.zero;
			go.transform.localScale = Vector3.one;
			go.transform.rotation   = Quaternion.identity;
		}
	}
	
	static void AddSubset(ref List<Subset> aSubsets, Mesh aMesh, int aLightmapID, int aMeshSubsetID, Material aMaterial, Matrix4x4 aTransform, Vector4 aLightmapOffset) {
		Vector3[] meshVerts    = aMesh.vertices;
		Vector3[] meshNorms    = aMesh.normals;
		Vector4[] meshTans     = aMesh.tangents;
		Vector2[] meshUVs      = aMesh.uv;
		Color  [] meshColors   = aMesh.colors;
		int    [] inds         = aMesh.GetIndices( aMeshSubsetID );
#if UNITY_5
		Vector2[] meshLightUVs = aMesh.uv2;
#else
		Vector2[] meshLightUVs = aMesh.uv1;
#endif
		
		SortedList remapIndices = new SortedList();
		List<Vector3> verts  = new List<Vector3>();
		List<Vector3> norms  = new List<Vector3>();
		List<Vector2> uvs    = new List<Vector2>();
		List<Vector2> lights = new List<Vector2>();
		List<Vector4> tans   = new List<Vector4>();
		List<Color>   cols   = new List<Color>  ();
		List<int>     setInds= new List<int>    ();
		
		// gather all mesh data in this subset, and remap the indices to match the new index of each vert.
		for (int i = 0; i < inds.Length; ++i) {
			int id = inds[i];
			int remap = remapIndices.IndexOfKey(id);
			if (remap == -1) {
				remapIndices.Add(id, verts.Count);
				setInds     .Add(    verts.Count);
				
				verts .Add(aTransform.MultiplyPoint3x4(meshVerts[id]));
				norms .Add(aTransform.MultiplyVector  (meshNorms[id]));
				uvs   .Add(meshUVs     [id]);
				if (meshColors.Length > 0) {
					cols.Add(meshColors  [id]);
				} else {
					cols.Add(Color.white);
				}
				if (meshLightUVs.Length >= meshUVs.Length) {
					lights.Add(new Vector2(aLightmapOffset.z, aLightmapOffset.w) + Vector2.Scale(new Vector2(aLightmapOffset.x, aLightmapOffset.y), meshLightUVs[id]));
				}
				tans.Add(meshTans[id]);
			} else {
				setInds.Add((int)remapIndices.GetByIndex(remap));
			}
		}

		// find a subset that matches material, lightmap, and has room for this mesh!
		int subsetID = -1;
		for (int i = 0; i < aSubsets.Count; i+=1) {
			if (aSubsets[i].mMaterial == aMaterial && aSubsets[i].mLightmapID == aLightmapID && aSubsets[i].Count+verts.Count < SuperMeshCombiner.MaxVerts) {
				subsetID = i;
			}
		}
		// if none was found, add one
		if (subsetID == -1) {
			Subset s = new Subset();
			s.mMaterial   = aMaterial;
			s.mLightmapID = aLightmapID;
			aSubsets.Add(s);
			subsetID = aSubsets.Count-1;
		}

		// fill the subset with the data we just gathered
		Subset set     = aSubsets[subsetID];
		int    startID = set.mPoints.Count;
		set.mPoints  .AddRange(verts );
		set.mNormals .AddRange(norms );
		set.mColors  .AddRange(cols  );
		set.mUVs     .AddRange(uvs   );
		set.mLightUVs.AddRange(lights);
		set.mTangents.AddRange(tans  );
		for (int i=0; i<setInds.Count; i++) set.mIndices.Add(setInds[i] + startID);
	}
	
	static Mesh MergeSubsets(List<Subset> aSubsets, int aLightmapID) {
		Mesh            result       = new Mesh();
		Subset          resultSubset = new Subset();
		List<List<int>> indices      = new List<List<int>>();

		for (int i = 0; i < aSubsets.Count; ++i) {
			if (aSubsets[i].mLightmapID != aLightmapID) continue;

			int startID = resultSubset.Count;
			resultSubset.Add(aSubsets[i]);
			
			List<int> subIndices = new List<int>();
			for (int t = 0; t < aSubsets[i].mIndices.Count; t+=1) {
				subIndices.Add(startID + aSubsets[i].mIndices[t]);
			}
			indices.Add(subIndices);
		}
		// cap it off if we used that category of data at all!
		resultSubset.CheckArrays();

		result.vertices = resultSubset.mPoints  .ToArray();
		result.normals  = resultSubset.mNormals .ToArray();
		result.uv       = resultSubset.mUVs     .ToArray();
		result.tangents = resultSubset.mTangents.ToArray();
		result.colors   = resultSubset.mColors  .ToArray();
#if UNITY_5
		result.uv2      = resultSubset.mLightUVs.ToArray();
#else
		result.uv1      = resultSubset.mLightUVs.ToArray();
#endif

		result.subMeshCount = aSubsets.Count;
		for (int i = 0; i < indices.Count; ++i) {
			result.SetIndices(indices[i].ToArray(), MeshTopology.Triangles, i);
		}
		result.RecalculateBounds();

		return result;
	}
	static Material[] GetMaterials(List<Subset> aSubsets ,int aLightmapID) {
		List<Material> result = new List<Material>(aSubsets.Count);
		for (int i=0; i<aSubsets.Count; i+=1) {
			if (aSubsets[i].mLightmapID == aLightmapID) {
				result.Add(aSubsets[i].mMaterial);
			}
		}
		return result.ToArray();
	}
	
	static int LightmapCount(List<Subset> aSubsets) {
		List<int> lightmaps = new List<int>();
		for (int i = 0; i < aSubsets.Count; i+=1) {
			if (!lightmaps.Contains(aSubsets[i].mLightmapID))
				lightmaps.Add(aSubsets[i].mLightmapID);
		}
		return lightmaps.Count;
	}
	static int LightmapID(List<Subset> aSubsets, int aIndex) {
		List<int> lightmaps = new List<int>();
		for (int i = 0; i < aSubsets.Count; i+=1) {
			if (!lightmaps.Contains(aSubsets[i].mLightmapID)) {
				if (lightmaps.Count == aIndex) {
					return aSubsets[i].mLightmapID;
				}
				lightmaps.Add(aSubsets[i].mLightmapID);
			}
		}
		return -1;
	}
}