using UnityEngine;
using System.Collections;

public class MusicDestroy : MonoBehaviour {

	private static GameObject o;

	void Start(){
		o = gameObject;
	}

	public static void DestroyMusic(){
		Destroy(o);
	}
}
