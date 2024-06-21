using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    Button buttonVSAI, buttonDeck;
    public void Awake()
    {
        buttonVSAI.onClick.AddListener(() => {
            GameDataManager.instance.isOnlineBattle = false;
            Invoke("ChangeBattleScene", 0.5f);
            AudioManager.instance.SoundButtonClick();
        });
        buttonDeck.onClick.AddListener(() => {
            Invoke("ChangeDeckScene", 0.5f);
            AudioManager.instance.SoundButtonClick();
        });
    }
    private void ChangeBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }
    private void ChangeDeckScene()
    {
        SceneManager.LoadScene("DeckScene");
    }
}
