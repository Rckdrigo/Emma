using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Distortion : MonoBehaviour
{
	public Material material;
	private float Strength = 4.7f;


	void Update()
	{
		float dtime = Time.deltaTime;

		if (Strength > 0.05f)
			Strength -= 0.9f * dtime;
		
	}
	// Postprocess the image
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat ("_Strength", Strength);
		Graphics.Blit(source, destination, material);
	}
}