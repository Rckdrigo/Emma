using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DoorBehaviour))]
public class RandomDoorLocking : MonoBehaviour {

	float pivotValue = 0.3f;

	void Start () {
		if(Random.value < pivotValue)
			GetComponent<DoorBehaviour>().LockDoor();
	}

}
