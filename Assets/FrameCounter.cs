using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCounter : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnGUI()
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle();
        style.fontSize = 26;
        style.normal.textColor = Color.red;
        GUILayout.Label("FrameRate: " + (1f / Time.deltaTime), style);


        GUILayout.EndVertical();
    }
}
