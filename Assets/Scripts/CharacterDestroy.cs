using UnityEngine;
using System.Collections;

public class CharacterDestroy : MonoBehaviour {

	private static Transform o;
	
	void Start(){
		o = gameObject.transform;
	}
	
	public static void SetPosition(){
		o.position = new Vector3(0,1,0);
	}

	public static void DestroyMe(){
		Destroy(o.gameObject);
	}
}
