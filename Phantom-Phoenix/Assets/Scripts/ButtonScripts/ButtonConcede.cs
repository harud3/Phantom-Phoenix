using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// �o�g����ʁ@ButtonConcede�̏����@�R���V�������N����
/// </summary>
public class ButtonConcede : MonoBehaviour
{
    [SerializeField]
    Button buttonConcede;
    public void Start()
    {
        buttonConcede.onClick.AddListener(() =>
        {
            if (GameDataManager.instance.isOnlineBattle)
            {
                GameManager.instance.SendConcede();
            }
            GameManager.instance.Concede(true);
        });
    }
}
