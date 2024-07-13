using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// �o�g����ʁ@ButtonConcede�̏����@�R���V�������Ƃɂ��ăV�[���J�ڂ��N����
/// </summary>
public class ChangeSceneFromBattleScene : MonoBehaviour
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
