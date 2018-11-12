using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSteamCamera : MonoBehaviour {
    public GameObject camera_steam;
    public GameObject camera_simulator;
    private Vector3 offset;
    
	void Start ()
    {
        if (camera_steam)
            offset = transform.position - camera_steam.transform.position;
        else if (camera_simulator)
            offset = transform.position - camera_simulator.transform.position;

    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (camera_steam)
            transform.position = camera_steam.transform.position + offset;
        else if (camera_simulator)
            transform.position = camera_simulator.transform.position + offset;
    }
}
