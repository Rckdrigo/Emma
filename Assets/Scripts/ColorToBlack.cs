﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ColorToBlack : MonoBehaviour {

	public float intensity;
	private Material material;
	void awake(){
		material = new Material (Shader.Find ("Hidden/BWDiffuse"));
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (intensity == 0) {
			Graphics.Blit(source,destination);
			return;
		}
		material.SetFloat("_bwBlend", intensity);
		Graphics.Blit (source,destination,material);
	}
}
