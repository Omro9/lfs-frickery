using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailAnimControl : MonoBehaviour {


    public Animator sailAnim;

	// Use this for initialization
	void Start () {
        sailAnim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKeyDown("w"))
        {
            sailAnim.Play("Forward");
        }
        if (Input.GetKeyDown("s"))
        {
            sailAnim.Play("Reverse");
        }

    }
}
