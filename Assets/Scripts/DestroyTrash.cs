using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrash : MonoBehaviour {
	GameObject conveyer;
	float destroyTime = 25.0f;

	void Start(){
		conveyer = GameObject.FindWithTag("ConveyerSpawner");
		StartCoroutine(WaitAndDestroy());
	}

	void Update () {
		// if(!this.gameObject.isGrabbed()){???
		WaitAndDestroy();
			
	}
	IEnumerator WaitAndDestroy(){

		yield return new WaitForSeconds(destroyTime);
		Debug.Log("WAIT");
		Destroy (this.gameObject);
		conveyer.GetComponent<Conveyer>().decreaseTrashNumber();
	}

}
