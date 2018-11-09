using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour {
	public float speed = 1f;
	private ScoreBoard scoreBoard;

	void Update () {
		if (speed > 0){
			transform.Translate(-speed*Time.deltaTime, 0, 0);
		}
		// if(this.gameObject.position.z <-10){
		// 	DestroySelf();
		// }
	}

	public void setSpeedToZero(){
		speed = 0f;
	}

	void DestroySelf(){
		Object.Destroy(this.gameObject);
	}
}
