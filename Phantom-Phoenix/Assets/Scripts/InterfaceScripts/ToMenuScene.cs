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
            GameManager.instance.SendConcede();
            GameManager.instance.Concede(true);
        });
    }
}
