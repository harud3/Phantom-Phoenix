using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Menuで使う　Menuからの遷移
/// </summary>
public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    Button buttonVSAI, buttonDeck;
    public void Awake()
    {
        buttonVSAI.onClick.AddListener(() => {
            GameDataManager.instance.isOnlineBattle = false;
            Invoke("ChangeBattleScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
        buttonDeck.onClick.AddListener(() => {
            Invoke("ChangeDeckScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
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
