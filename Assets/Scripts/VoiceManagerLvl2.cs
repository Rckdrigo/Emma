using UnityEngine;
using System.Collections;

public class VoiceManagerLvl2 : MonoBehaviour {
	
	public AudioClip intro, tip, price;
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
	
	public void PlayPrice(){
		audioSource.PlayOneShot(price);
	}

}
