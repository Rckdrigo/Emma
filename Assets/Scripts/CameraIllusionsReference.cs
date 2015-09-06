using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using UnityEngine.Serialization;

public class CameraIllusionsReference : MonoBehaviour {
	
	[System.Serializable()]
	delegate void Action(int i);
	
	public int steps = 10;
	public float deltaTime = 15f;

	MotionBlur motionBlur;
	Twirl twirl;
	Fisheye fisheye;
	VignetteAndChromaticAberration vignette;
	NoiseAndGrain noise;

	void Start () {
		motionBlur = Camera.main.GetComponent<MotionBlur>();
		twirl = Camera.main.GetComponent<Twirl>();
		fisheye = Camera.main.GetComponent<Fisheye>();
		vignette = Camera.main.GetComponent<VignetteAndChromaticAberration>();
		noise = Camera.main.GetComponent<NoiseAndGrain>();

		motionBlur.enabled = false;
		twirl.enabled = false;
		fisheye.enabled = false;
		vignette.enabled = false;
		noise.enabled = false;

	}

	public void SelectVisualIllusion(string illusion){
		Action a = null;
		
		switch(illusion){
		case "Fisheye":
			a = FisheyeIllusion;
			fisheye.enabled = true;
			break;
		case "MotionBlur":
			a = MotionBlurIllusion;
			motionBlur.enabled = true;
			break;
		case "Noise":
			a = NoiseIllusion;
			noise.enabled = true;
			break;
		case "Twirl":
			a = TwirlIllusion;
			twirl.enabled = true;
			break;
		case "Vignette":
			a = VignetteIllusion;
			vignette.enabled = true;
			break;
		case "Angular":
			a = GreatAngularIllusion;
			break;
		}
		
		if(a != null)
			StartCoroutine(RunIllusion(a));
	}
	void MotionBlurIllusion(int i){
		motionBlur.blurAmount = (float)i/20f;
	}
	
	void NoiseIllusion(int i){
		noise.intensityMultiplier = (float)i/2f;
	}
	
	void FisheyeIllusion(int i){
		fisheye.strengthX = (float)i/50f;
		fisheye.strengthY = (float)i/50f;
	}
	
	void TwirlIllusion(int i){
		twirl.angle = i * 3;
	}
	
	void VignetteIllusion(int i){
		vignette.intensity = (float)i/2f;
	}
	
	void GreatAngularIllusion(int i){
		Camera.main.fieldOfView = 60 + (float)i*7.5f;
	}
	
	IEnumerator RunIllusion(Action action){
		for(int i = 0; i < steps; i++){
			action(i);
			yield return new WaitForSeconds(deltaTime);
		}
	}
}
