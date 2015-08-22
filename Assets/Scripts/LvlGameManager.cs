using UnityEngine;
using System.Collections;

public class LvlGameManager : MonoBehaviour {

	public void ChangeLvl(int index){
		StartCoroutine(LoadingLvl1(index));
	}

	IEnumerator LoadingLvl1(int index) {
		AsyncOperation async = Application.LoadLevelAsync(index);
		yield return async;
		Debug.Log("Loading complete");
	}

	public void ChangeLvl(string index){
		StartCoroutine(LoadingLvl1(index));
	}
	
	IEnumerator LoadingLvl1(string index) {
		AsyncOperation async = Application.LoadLevelAsync(index);
		yield return async;
		Debug.Log("Loading complete");
	}
}
