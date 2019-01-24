using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class Playercnt : MonoBehaviour
{
    public float spd = 5;
    public float rotspd = 120;
    public Transform t;
    PhotonView pv;
    Vector3 curpos;
    Quaternion currot;
    public Transform firepos;
    public GameObject bullet;
    void Start()
    {
        pv = GetComponent<PhotonView>();
        t = GetComponent<Transform>();
        pv.ObservedComponents[0] = this;
        if (pv.isMine)
        {
            GameObject.Find("Main Camera").GetComponent<SmoothFollow>().target = t;
        }
    }
    void Update()
    {
        if (pv.isMine)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            t.Translate(Vector3.forward * v * Time.deltaTime * spd);
            if (v >= 0)
            {
                t.Rotate(Vector3.up * h * Time.deltaTime * rotspd);
            }
            else
            {
                t.Rotate(Vector3.up * h * Time.deltaTime * -rotspd);
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Fire();
            }
        }
        else
        {
            t.position = Vector3.Lerp(t.position, curpos, Time.deltaTime * 10);
            t.rotation = Quaternion.Lerp(t.rotation, currot, Time.deltaTime * 10);
        }
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(t.position);
            stream.SendNext(t.rotation);
        }
        else
        {
            curpos = (Vector3)stream.ReceiveNext();
            currot = (Quaternion)stream.ReceiveNext();
        }
    }
    void Fire()
    {
        StartCoroutine(Createbullet());
        pv.RPC("FireRPC", PhotonTargets.Others);
    }
    IEnumerator Createbullet()
    {
        Instantiate(bullet, firepos.position, firepos.rotation);
        yield return null;
    }
    [PunRPC]
    void FireRPC()
    {
        StartCoroutine(Createbullet());
    }
}