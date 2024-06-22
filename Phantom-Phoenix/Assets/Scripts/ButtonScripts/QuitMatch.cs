using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// OnlineSceneの処理　メニューに戻る　部屋からも抜ける
/// </summary>
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
