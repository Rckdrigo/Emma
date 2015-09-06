using UnityEngine;
using System.Collections;

public class MoveLines : MonoBehaviour {

	public AudioClip[] scream;
	public TrailRenderer line;

	void OnEnable(){
		line.material.color = new Color(
			Random.value,Random.value,Random.value);
		Invoke ("Pool",3);
		GetComponent<AudioSource>().clip = scream[Random.Range(0,scream.Length)];
		GetComponent<AudioSource>().Play();
	}

	void Update () {
		transform.Translate(Vector3.back * 500 * Time.deltaTime);
	}

	void Pool(){
		GetComponent<AudioSource>().Stop();
		ObjectPool.Instance.PoolGameObject(gameObject);
	}
}
