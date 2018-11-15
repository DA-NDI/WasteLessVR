using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyer : MonoBehaviour {
	public GameObject[] trashArrayPaper;
	public GameObject[] trashArrayPlastic;
	public GameObject[] trashArrayMetal;
	public GameObject[] trashArrayGlass;
	// public GameObject[] trashArrayWaste;
	public Vector3 trashValues;
	public float trashWait;
	public float trashMostWait;
	public float trashLeastWait;
	public int startWait;
	public bool stop;
	int randomTrashArray;
	int randomTrashNum;
	public int trashNumberInScene = 0;
	public bool isPlaying = true;
	
	void Start () {
		StartCoroutine(waitTrash());	
	}
	
	
	void Update () {
		if(isPlaying && trashNumberInScene<=15){
			trashWait = Random.Range(trashLeastWait, trashMostWait);
		}
	}

	public void stopConveyer(){
		isPlaying = false;
	}

	public void increaseTrashNumber(){
		trashNumberInScene++;
	}

	public void decreaseTrashNumber(){
		trashNumberInScene--;
	}

	IEnumerator waitTrash(){
		yield return new WaitForSeconds(startWait);
		while(isPlaying){
			if(trashNumberInScene<=15){
				randomTrashArray = Random.Range(0, 4);
				randomTrashNum = Random.Range(0, 3); 
				Vector3 trashPosition = new Vector3(Random.Range(-trashValues.x, trashValues.x), 1, Random.Range(-trashValues.z, trashValues.z));
				switch (randomTrashArray)
				{
						case 1:
							Instantiate(trashArrayPaper[randomTrashNum], trashPosition + transform.TransformPoint(0,0,0), gameObject.transform.rotation);
							break;
						case 2:
							Instantiate(trashArrayPlastic[randomTrashNum], trashPosition + transform.TransformPoint(0,0,0), gameObject.transform.rotation);
							break;
						case 3:
							Instantiate(trashArrayMetal[randomTrashNum], trashPosition + transform.TransformPoint(0,0,0), gameObject.transform.rotation);
							break;
						case 4:
							Instantiate(trashArrayGlass[randomTrashNum], trashPosition + transform.TransformPoint(0,0,0), gameObject.transform.rotation);
							break;
						// case 5:
						// 	Instantiate(trashArrayWaste[randomTrashNum], trashPosition + transform.TransformPoint(0,0,0), gameObject.transform.rotation);
						// 	break;
				}
				increaseTrashNumber();
			}
			
			yield return new WaitForSeconds(trashWait);
		
		}
	}
}
