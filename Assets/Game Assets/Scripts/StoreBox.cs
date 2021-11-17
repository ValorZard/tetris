using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreBox : MonoBehaviour {
    public GameObject storedGroupObject; 

	// Use this for initialization
    void Awake () {
        storedGroupObject = null;
}


    public void storeBlockGroup (GameObject groupObject) {
        storedGroupObject = groupObject;
        // Debug.LogFormat("Object position: {0}", currentGroupObject.transform.position);
        // Debug.LogFormat("Center position: {0}", getBounds(currentGroupObject).center);
        storedGroupObject.GetComponent<Transform>().position = GetComponent<Transform>().position;
        var group = (BlockGroup) storedGroupObject.GetComponent(typeof(BlockGroup));
        // put the group align with its center
        group.AlignCenter();
        group.enabled = false;

    }


    public GameObject getStoredBlockGroup() {
        return storedGroupObject;
    }

    void Start() {
        //createStoppedGroup();
    }
	
	// Update is called once per frame
	void Update () {
	}
}
