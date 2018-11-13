using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecycleScore1 : MonoBehaviour {
	public ScoreBoard scoreBoard1;
	void OnTriggerEnter(Collider col){
		if (col.tag == "MetalTrash" || col.tag == "PlasticTrash" 
		|| col.tag == "PaperTrash" || col.tag == "GlassTrash")
		{
			scoreBoard1.handleScored();
			Destroy(col.gameObject);
		}
		else
			scoreBoard1.handleWrongScored();
	}
}
