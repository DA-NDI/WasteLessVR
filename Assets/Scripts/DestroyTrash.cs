using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using VRTK;

public class DestroyTrash : MonoBehaviour {
	public float destroyTime = 30.0f;

	void Update () {
		// if(!this.gameObject.isGrabbed()){
			Destroy(this.gameObject, destroyTime);
	}
}
