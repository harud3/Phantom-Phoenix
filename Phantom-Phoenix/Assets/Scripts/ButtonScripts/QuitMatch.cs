using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;

/// <summary>
/// OnlineScene�̏����@���j���[�ɖ߂�@���������������
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
            //StartCoroutine(ChangeMenuScene());
            SceneManager.LoadScene("MenuScene");
            AudioManager.instance.SoundButtonClick3();
        });
    }
    [Obsolete]
    private IEnumerator ChangeMenuScene() //�g������Photon��null�G���[���N�����̂����c
    {
        AudioManager.instance.SoundButtonClick3();
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene("MenuScene");
    }
}
