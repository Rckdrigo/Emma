using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
public class VoiceManagerLvl0 : MonoBehaviour {

	public AudioClip voiceClip;
	AudioSource audioSource;

	void Start () {
		audioSource = GetComponent<AudioSource>();
	}

	public void StartAudioClip () {
		audioSource.PlayOneShot(voiceClip);

		StartCoroutine("PlayVoiceLoop");
	}

	public void StopAudioClip(){
		StopCoroutine("PlayVoiceLoop");
	}

	IEnumerator PlayVoiceLoop(){
		yield return new WaitForSeconds(Random.Range(25,40));
		audioSource.PlayOneShot(voiceClip);

		StartCoroutine("PlayVoiceLoop");
	}
}
