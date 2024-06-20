using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ToMenuScene : MonoBehaviour
{
    [SerializeField]
    Button button;
    public void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (GameDataManager.instance.isOnlineBattle)
            {
                GameManager.instance.SendConcede();
            }
            GameManager.instance.Concede(true);
        });
    }
}
