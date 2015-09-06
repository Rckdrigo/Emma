using UnityEngine;
using System.Collections;

public class LineGenerator : MonoBehaviour {

	public Transform[] references;

	void Start(){
		StartCoroutine(Lines ());	
	}

	IEnumerator Lines(){
		GameObject o = ObjectPool.Instance.GetGameObjectOfType("LineOfLight");
		o.transform.position = references[Random.Range(0,references.Length)].position;
		yield return new WaitForSeconds(0.5f);

		StartCoroutine(Lines ());
	}
}
