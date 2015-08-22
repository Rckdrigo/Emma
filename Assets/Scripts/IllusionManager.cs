using UnityEngine;
using UnityEngine.Audio;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MotionBlur))]
[RequireComponent(typeof(Twirl))]
[RequireComponent(typeof(Fisheye))]
[RequireComponent(typeof(VignetteAndChromaticAberration))]
[RequireComponent(typeof(NoiseAndGrain))]

public class IllusionManager : MonoBehaviour {

	public int steps = 10;
	public float deltaTime = 15f;
	public GameObject audioSourcePrefab;

	//[System.Serializable]
	public List<string> illusionList;

	MotionBlur motionBlur;
	Twirl twirl;
	Fisheye fisheye;
	VignetteAndChromaticAberration vignette;
	NoiseAndGrain noise;

	public AudioMixerSnapshot newAudioSnapshot;
	
	[System.Serializable()]
	public delegate void Action(int i);
	public Action action2;

	// Use this for initialization
	void Start () {
		motionBlur = Camera.main.GetComponent<MotionBlur>();
		twirl = Camera.main.GetComponent<Twirl>();
		fisheye = Camera.main.GetComponent<Fisheye>();
		vignette = Camera.main.GetComponent<VignetteAndChromaticAberration>();
		noise = Camera.main.GetComponent<NoiseAndGrain>();

	}


	public void SelectRandomIllusion(){
		int i = Random.Range(0,illusionList.Count);

		SetIllusion(illusionList[i]);
		illusionList.RemoveAt(i);
	}

	public void SetIllusion(string illusion){
		Action a = null;
		 
		switch(illusion){
		case "Fisheye":
			a = Fisheye;
			break;
		case "MotionBlur":
			a = MotionBlur;
			break;
		case "Noise":
			a = Noise;
			break;
		case "Twirl":
			a = Twirl;
			break;
		case "Vignette":
			a = Vignette;
			break;
		case "Angular":
			a = GreatAngular;
			break;
		case "AudioChange":
			ChangeAudioSnapshot();
			break;
		}

		if(a != null)
			StartCoroutine(RunIllusion(a));
	}

	public void GenerateSoundIllusionLoop(AudioClip audio){
		GameObject temp = Instantiate(audioSourcePrefab) as GameObject;
		temp.transform.parent = transform;
		Action a = delegate {
			temp.GetComponent<AudioSource>().PlayOneShot(audio);
		};
		StartCoroutine(RunIllusion(a));
	}

	public void GenerateSoundIllusion(AudioClip audio){
		GameObject temp = Instantiate(audioSourcePrefab) as GameObject;
		temp.transform.parent = transform;
		temp.GetComponent<AudioSource>().PlayOneShot(audio);
	}

	void ChangeAudioSnapshot(){
		newAudioSnapshot.TransitionTo(deltaTime * (steps - 2));
	}

	void MotionBlur(int i){
		motionBlur.blurAmount = (float)i/20f;
	}

	void Noise(int i){
		noise.intensityMultiplier = (float)i/5f;
	}

	void Fisheye(int i){
		fisheye.strengthX = (float)i/50f;
		fisheye.strengthY = (float)i/50f;
	}

	void Twirl(int i){
		twirl.angle = i * 3;
	}

	void Vignette(int i){
		vignette.intensity = (float)i/2f;
	}

	void GreatAngular(int i){
		Camera.main.fieldOfView = 60 + (float)i*7.5f;
	}

	IEnumerator RunIllusion(Action action){
		for(int i = 0; i < steps; i++){
			action(i);
			yield return new WaitForSeconds(deltaTime);
		}
	}

}
