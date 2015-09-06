using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonReferenceIndicator : MonoBehaviour {

	RaycastHit hit;
	void Update () {
		Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
     	if(Physics.Raycast(ray,out hit,5)){
			if(hit.transform.tag.Equals("Door")){
				GameObject.FindWithTag("AButton").GetComponent<Image>().enabled = true;
			}
			else{
					GameObject.FindWithTag("AButton").GetComponent<Image>().enabled = false;
			}
		}

	}
}
