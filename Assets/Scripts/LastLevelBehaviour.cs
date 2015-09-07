using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class LastLevelBehaviour : MonoBehaviour {

	public List<AudioClip> audios;
	private AudioSource au;

	public UnityEvent finished;

	void Start () {
		au = GetComponent<AudioSource>();
		CharacterDestroy.SetPosition();
		StartCoroutine(AddNewAudio());
	}

	IEnumerator AddNewAudio(){
		yield return new WaitForSeconds(Random.Range(2,5));
		GameObject o = ObjectPool.Instance.GetGameObjectOfType("AudioSource");
		int r = Random.Range(0,audios.Count);
		o.GetComponent<AudioSource>().clip = audios[r];
		audios.RemoveAt(r);
		o.GetComponent<AudioSource>().loop = true;
		o.GetComponent<AudioSource>().Play();

	
		if(audios.Count > 0){
			StartCoroutine(AddNewAudio());
		}else{
			finished.Invoke();

			yield return new WaitForSeconds(2f);
			Application.LoadLevel("MainMenu");
		}
	}

}
