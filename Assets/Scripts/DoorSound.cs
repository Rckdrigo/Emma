using UnityEngine;
using System.Collections;

public class DoorSound : MonoBehaviour {

	public AudioClip locked, unlocked;

	public void PlayLocked(){
		GetComponent<AudioSource>().clip = locked;
		GetComponent<AudioSource>().Play();
	}

	public void PlayUnlocked(){
		GetComponent<AudioSource>().clip = unlocked;
		GetComponent<AudioSource>().Play();
	}
}
