using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour {

	public GameObject conveyor;
	public Transform endpoint;
	float speed=0.3f;

	void OnTriggerStay(Collider other){
		other.transform.position = Vector3.MoveTowards(other.transform.position, endpoint.position, speed*Time.deltaTime);
	}
}
