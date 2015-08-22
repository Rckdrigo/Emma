using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VoiceManagerLvl1 : MonoBehaviour {
	
	public AudioClip intro, tip;
	AudioSource audioSource;
	
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}
	
	public void PlayTip () {
		audioSource.PlayOneShot(tip);
	}
	
	public void PlayIntro(){
		audioSource.PlayOneShot(intro);
	}

}
