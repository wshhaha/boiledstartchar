using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playermoving : MonoBehaviour 
{
    public UIJoystick stick;
    public float spd;
    Vector3 stickpos;
    private void Start()
    {
        this.enabled = false;
    }
    void Update () 
	{
        stickpos.x = stick.position.x;
        stickpos.y = 0;
        stickpos.z = stick.position.y;
        Vector3 dir = stickpos - transform.position;
        
        dir.y = 0;
        
        dir.Normalize();
        
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(dir),10 * Time.deltaTime);
    }
   
}
