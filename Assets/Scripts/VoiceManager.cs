
using UnityEngine;
using System.Collections;

public class VoiceManager : MonoBehaviour {

	public AudioClip first;
	public AudioClip[] instructions;

	public AudioClip[] positive, negative;

	public delegate void OnVoiceEvent();
	public static event OnVoiceEvent FinishedInstructions;

	private AudioSource aud;
	private int indexInstruction;
	private int nextDoorNumber;

	int minusCounter = 0;
	public bool started;
	
	void Start () {
		indexInstruction = 3;
		nextDoorNumber = 7;
		aud = GetComponent<AudioSource>();

		MusicDestroy.DestroyMusic();
	}

	public void PlayIntro(){
		aud.clip = first;
		aud.Play();
		Invoke ("StartAudio",first.length);
	}

	void StartAudio(){
		started = true;
	}

	void NextNumber(){
		indexInstruction++;
		if(indexInstruction == 4)
			indexInstruction = 0;

		switch(indexInstruction){
		case 0:
			nextDoorNumber = 19;
			break;
		case 1:
			nextDoorNumber = 6;
			break;
		case 2:
			nextDoorNumber = 13;
			break;
		case 3:
			nextDoorNumber = 7;
			break;

		}
		print(nextDoorNumber);
	}

	public void CheckIfCorrectDoor(string name){
		if(started){
			int number = int.Parse(name);

			if(number == nextDoorNumber){
				NextNumber();
				StopAllCoroutines();
				StartCoroutine(PlayInstruction(true));
				minusCounter = 0;
			}else{
				minusCounter++;
				if(minusCounter == 6){
					Application.LoadLevel("Lvl 3");
				}else{
					StopAllCoroutines();
					StartCoroutine(PlayInstruction(false));
				}
			}
		}
	}

	IEnumerator PlayInstruction(bool good){
		AudioClip feedback;
		int i;
		float time;
		if(good){
			i = Random.Range(0,positive.Length);
			feedback = positive[i];
			time = positive[i].length;
		}else{
			i = Random.Range(0,negative.Length);
			feedback = negative[i];
			time = negative[i].length;
		}
		aud.clip = feedback;
		aud.Play();
		yield return new WaitForSeconds(time);
		aud.clip = instructions[indexInstruction];
		aud.Play();
	}
}
