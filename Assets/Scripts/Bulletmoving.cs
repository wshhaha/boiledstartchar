using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulletmoving : MonoBehaviour
{
    public float spd;
    public float destime=3;
    public int dmg;
    public GameObject master;
    
    void Start()
    {   
        Destroy(gameObject, destime);
    }
    void Update()
    {
        transform.Translate(Vector3.forward * spd * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Map")
        {   
            Destroy(gameObject);
        }
    }
}