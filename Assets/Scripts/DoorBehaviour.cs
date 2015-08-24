
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DoorBehaviour : MonoBehaviour {

	public float rotSpeed = 15;
	public bool locked = false;
	public UnityEvent OnDoorOpened;

	private bool opened;
	private Quaternion initialRot, lastRot;
	private Collider coll;

	void Start(){
		initialRot = transform.rotation;
		lastRot = Quaternion.Euler(new Vector3(0,initialRot.eulerAngles.y + 90,00));

		coll = GetComponent<Collider>();
		opened = false;
	}

	public void ToogleDoor(){
		if(Input.GetButtonDown("Fire1")){// && !locked){
			
			print ("AQUI");
			if(opened){
				CloseDoor();
			}
			else{
				OpenDoor();
				OnDoorOpened.Invoke();
			}
			opened = !opened;
		}
	}

	public void OpenAndCloseDoor(){
		if(Input.GetButtonDown("Fire1") && !locked)
			StartCoroutine("RotateAndReturn",initialRot);
	}

	public void UnlockDoor(){
		locked = false;
	}

	public void LockDoor(){
		locked = true;
	}

	public void CloseDoor(){
		StopCoroutine("Rotate");
		StartCoroutine("Rotate",initialRot);
	}
	
	public void OpenDoor(){
		StopCoroutine("Rotate");
		StartCoroutine("Rotate",lastRot);
	}

	IEnumerator Rotate(Quaternion target){
		coll.isTrigger = true;
		while(transform.rotation != target){
			transform.rotation = Quaternion.RotateTowards(transform.rotation, target, Time.deltaTime * rotSpeed);
			yield return null;
		}
		coll.isTrigger = false;
	}

	IEnumerator RotateAndReturn(){
		coll.isTrigger = true;
		while(transform.rotation != lastRot){
			transform.rotation = Quaternion.RotateTowards(transform.rotation, lastRot, Time.deltaTime * rotSpeed);
			yield return null;
		}
		coll.isTrigger = false;

		while(transform.rotation != initialRot){
			transform.rotation = Quaternion.RotateTowards(transform.rotation, initialRot, Time.deltaTime * rotSpeed);
			yield return null;
		}
		LockDoor();
	}
}
