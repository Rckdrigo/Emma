using UnityEngine;
using UnityEngine.Events;

[System.Serializable()]
public enum Selfdestructable{None, DestroyOnEnter,DestroyOnExit}

[RequireComponent(typeof(Collider))]
public class TriggerBehaviour : MonoBehaviour {

	public Selfdestructable selfdestructable = Selfdestructable.None;

	public UnityEvent OnEnterTrigger, OnStayTrigger, OnExitTrigger;

	Collider coll;

	void Start(){
		coll = GetComponent<Collider>();
		coll.isTrigger = true;
	}

	void OnTriggerEnter(Collider collider){
		OnEnterTrigger.Invoke();
		if(selfdestructable == Selfdestructable.DestroyOnEnter)
			Destroy(gameObject);
	}

	void OnTriggerStay(Collider collider){
		OnStayTrigger.Invoke();
	}

	void OnTriggerExit(Collider collider){
		OnExitTrigger.Invoke();
		if(selfdestructable == Selfdestructable.DestroyOnExit)
			Destroy(gameObject);
	}
}
