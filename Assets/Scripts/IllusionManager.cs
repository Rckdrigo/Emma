using UnityEngine;
using UnityEngine.Audio;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class IllusionManager : MonoBehaviour {

	[Header("Illusions")]
	public List<string> illusionList;
	public List<AudioClip> audioList;
	public GameObject audioSourcePrefab;
	
	//public AudioMixerSnapshot newAudioSnapshot;

	public void SelectRandomIllusion(){
		if(Random.value < 0.5f){
			int i = Random.Range(0,illusionList.Count);
			GenerateVisualIllusion(illusionList[i]);
		}else{
			int i = Random.Range(0,audioList.Count);
			GenerateAudioIllusion(audioList[i]);
		}
	}

	public void GenerateVisualIllusion(string illusion){
		Camera.main.GetComponent<CameraIllusionsReference>().SelectVisualIllusion(illusion);
		illusionList.Remove(illusion);
	}

	public void GenerateAudioIllusion(AudioClip audio){
		GameObject temp = Instantiate(audioSourcePrefab) as GameObject;
		temp.transform.parent = GameObject.FindWithTag("Player").transform;
		temp.transform.position = temp.transform.parent.position;
		temp.GetComponent<AudioSource>().loop = true;
		temp.GetComponent<AudioSource>().clip = audio;
		temp.GetComponent<AudioSource>().Play();

		
		audioList.Remove(audio);
	}


	/*void ChangeAudioSnapshot(){
		newAudioSnapshot.TransitionTo(deltaTime * (steps - 2));
	}*/



}
