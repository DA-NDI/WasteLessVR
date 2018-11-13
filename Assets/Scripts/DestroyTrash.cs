using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrash : MonoBehaviour {
	public float destroyTime = 20.0f;

	void Update () {
		// if(!this.gameObject.isGrabbed()){???
			Destroy(this.gameObject, destroyTime);
	}
}
