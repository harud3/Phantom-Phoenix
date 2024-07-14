using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneFromSelectHeroScene : MonoBehaviour
{
    [SerializeField]
    Button buttonOK;
    public void Start()
    {
        if (GameDataManager.instance.scene == GameDataManager.Scene.deck) //�f�b�L�Ґ�
        {
            buttonOK.onClick.AddListener(() =>
            {
                Invoke("ChangeDeckScene", 0.5f);
                AudioManager.instance.SoundButtonClick1();
            });
        }
        else if (GameDataManager.instance.scene == GameDataManager.Scene.battle)
        {
            if (GameDataManager.instance.isOnlineBattle) //�ΐl��
            {
                buttonOK.onClick.AddListener(() =>
                {
                    Invoke("ChangeOnlineScene", 0.5f);
                    AudioManager.instance.SoundButtonClick1();
                });
            }
            else //AI��
            {
                buttonOK.onClick.AddListener(() =>
                {
                    Invoke("ChangeAIScene", 0.5f);
                    AudioManager.instance.SoundButtonClick1();
                });
            }
        }
        

    }
    private void ChangeDeckScene()
    {
        SceneManager.LoadScene("DeckScene");
    }
    private void ChangeOnlineScene()
    {
        SceneManager.LoadScene("OnlineScene");
    }
    private void ChangeAIScene()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
