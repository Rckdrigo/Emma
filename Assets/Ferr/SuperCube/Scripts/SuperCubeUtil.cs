using UnityEngine;
using System.Collections.Generic;

namespace Ferr {
	static class SuperCubeUtil {
		#region Internal methods
		public static void ResampleColors(ref List<Color> aColorList, int aNewWidth, int aNewHeight, Color[] aPrevColors, int aStartIndex, int aOldWidth, int aOldHeight) {
			for (int y = 0; y < aNewHeight; ++y) {
				for (int x = 0; x < aNewWidth; ++x) {
					aColorList.Add(Sample(aPrevColors, aStartIndex, aOldWidth, aOldHeight, (float)x/(aOldWidth-1), (float)y/(aOldHeight-1)));
				}
			}
		}
		public static void AddFace       (Matrix4x4 aObjTransform, Matrix4x4 aTransform, float aOffset, UVType aUVType, Vector2 aUVOffset, Vector2 aUVTiling, float aUStart, float aUEnd, int aXSlices, int aYSlices, ref List<Vector3> aVerts, ref List<Vector2> aUVs, ref List<Vector3> aNormals, ref List<Vector4> aTangents, ref List<int> aIndices) {
			aXSlices = Mathf.Max(aXSlices, 2);
			aYSlices = Mathf.Max(aYSlices, 2);
			
			int       startID = aVerts.Count;
			Vector3   normal  = aTransform.MultiplyVector(new Vector3(0, 0, -1)).normalized;
			
			for (int y = 0; y < aYSlices; y++) {
				float percentY = (float)y / (aYSlices-1) - 0.5f;
				
				for (int x = 0; x < aXSlices; x++) {
					float   percentX = (float)x / (aXSlices-1) - 0.5f;
					Vector3 pt       = new Vector3(percentX, percentY, -aOffset);
					
					aVerts   .Add(aTransform.MultiplyPoint3x4(pt));
					aNormals .Add(normal);
					aUVs     .Add(aUVOffset + Vector2.Scale(aUVTiling, GetUV(aObjTransform, aUVType, aVerts[aVerts.Count-1], aNormals[aVerts.Count-1], aUStart, aUEnd, percentX, percentY)));

					if (x > 0 && y > 0) {
						if ((x+y)%2==0) {
							aIndices.Add(startID + (x  ) + (y  ) * aXSlices);
							aIndices.Add(startID + (x  ) + (y-1) * aXSlices);
							aIndices.Add(startID + (x-1) + (y-1) * aXSlices);
							
							aIndices.Add(startID + (x-1) + (y  ) * aXSlices);
							aIndices.Add(startID + (x  ) + (y  ) * aXSlices);
							aIndices.Add(startID + (x-1) + (y-1) * aXSlices);
						} else {
							aIndices.Add(startID + (x  ) + (y  ) * aXSlices);
							aIndices.Add(startID + (x  ) + (y-1) * aXSlices);
							aIndices.Add(startID + (x-1) + (y  ) * aXSlices);
							
							aIndices.Add(startID + (x-1) + (y  ) * aXSlices);
							aIndices.Add(startID + (x  ) + (y-1) * aXSlices);
							aIndices.Add(startID + (x-1) + (y-1) * aXSlices);
						}
					}
				}
			}
			
			// calculate the tangent!
			Vector3 dir1 = aTransform.MultiplyVector(new Vector3(1,  0, 0));
			Vector3 dir2 = aTransform.MultiplyVector(new Vector3(0, -1, 0));
			Vector2 uv1 = aUVs[startID+1       ] - aUVs[startID];
			Vector2 uv2 = aUVs[startID+aXSlices] - aUVs[startID];
			
			float r = 1.0f / uv1.x * uv2.y - uv2.x * uv1.y;
			Vector3 sDir = new Vector3(
				(uv2.y * dir1.x - uv1.y * dir2.x) * r,
				(uv2.y * dir1.y - uv1.y * dir2.y) * r,
				(uv2.y * dir1.z - uv1.y * dir2.z) * r
			);
			Vector3 tDir = new Vector3(
				(uv1.x * dir2.x - uv2.x * dir1.x) * r,
				(uv1.x * dir2.y - uv2.x * dir1.y) * r,
				(uv1.x * dir2.z - uv2.x * dir1.z) * r
			);
			
			Vector3 ttan = (sDir - aNormals[startID] * Vector3.Dot(aNormals[startID], sDir)).normalized;
			Vector4 tan  = new Vector4(ttan.x, ttan.y, ttan.z, (Vector3.Dot(Vector3.Cross(aNormals[startID], ttan), tDir) < 0) ? -1 : 1);
			for (int i = 0; i < aXSlices * aYSlices; ++i) {
				aTangents.Add(tan);
			}
		}
		#endregion

		#region Helper methods
		public static Color   Sample (Color[] aPrevColors, int aStartIndex, int aWidth, int aHeight, float aX, float aY) {
			int x = (int)(aX * (aWidth-1));
			int y = (int)(aY * (aHeight-1));
			float percentX = aX * (aWidth -1) - x;
			float percentY = aY * (aHeight-1) - y;
			
			Color col1 = aPrevColors[aStartIndex + Mathf.Clamp(x,  0,aWidth-1) + Mathf.Clamp(y,  0,aHeight-1) * aWidth];
			Color col2 = aPrevColors[aStartIndex + Mathf.Clamp(x+1,0,aWidth-1) + Mathf.Clamp(y,  0,aHeight-1) * aWidth];
			Color col3 = aPrevColors[aStartIndex + Mathf.Clamp(x,  0,aWidth-1) + Mathf.Clamp(y+1,0,aHeight-1) * aWidth];
			Color col4 = aPrevColors[aStartIndex + Mathf.Clamp(x+1,0,aWidth-1) + Mathf.Clamp(y+1,0,aHeight-1) * aWidth];
			
			return Color.Lerp(Color.Lerp(col1, col2, percentX), Color.Lerp(col3, col4, percentX), percentY);
		}
		public static Vector2 GetUV  (Matrix4x4 aTransform, UVType aUVType, Vector3 aPt, Vector3 aNorm, float aUStart, float aUEnd, float aPercentX, float aPercentY) {
			Vector2 uv = new Vector2(aPercentX + 0.5f, (aPercentY) + 0.5f);
			if (aUVType == UVType.WallSlide) {
				uv = new Vector2(Mathf.Lerp(aUEnd, aUStart, aPercentX + 0.5f), aPercentY + 0.5f);
			} else if (aUVType == UVType.WorldCoordinates) {
				uv = PosToUV(aTransform.MultiplyPoint3x4(aPt), aTransform.MultiplyVector(aNorm));
			} else if (aUVType == UVType.LocalCoordinates) {
				uv = PosToUV(aPt, aNorm);
			}
			return uv;
		}
		public static Vector2 PosToUV(Vector3 aPos, Vector3 aNormal) {
			aNormal.x = Mathf.Abs(aNormal.x);
			aNormal.y = Mathf.Abs(aNormal.y);
			aNormal.z = Mathf.Abs(aNormal.z);
			
			return new Vector2((aNormal.x*aPos.z) + 
								(aNormal.y*aPos.x) +
								(aNormal.z*aPos.x),
				
								(aNormal.x*aPos.y) + 
								(aNormal.y*aPos.z) +
								(aNormal.z*aPos.y));
		}
		#endregion
	}
}