using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Photoninit : MonoBehaviour
{
    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings("woophotonex");
    }
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("connect master");
        PhotonNetwork.JoinRandomRoom();
    }
    public virtual void OnJoinedLobby()
    {
        Debug.Log("join lobby");
        PhotonNetwork.JoinRandomRoom();
    }
    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.Log("no room");
        PhotonNetwork.CreateRoom("room");
    }
    public virtual void OnCreatedRoom()
    {
        Debug.Log("create");
    }
    public virtual void OnJoinedRoom()
    {
        Debug.Log("join room");
        StartCoroutine(Createplayer());
    }
   
    IEnumerator Createplayer()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity, 0);
        yield return null;
    }
}
