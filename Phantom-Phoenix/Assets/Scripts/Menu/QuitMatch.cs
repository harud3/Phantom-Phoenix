using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class QuitMatch : MonoBehaviour
{
    [SerializeField]
    Button button;
    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (PhotonNetwork.IsConnected) { PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect(); }
            SceneManager.LoadScene("MenuScene");
        });
    }
}
