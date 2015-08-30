using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DistortionPause : MonoBehaviour
{
	public Material material;
	private float Strength = 0.043f;

	// Postprocess the image
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat ("_Strength", Strength);
		Graphics.Blit(source, destination, material);
	}
}