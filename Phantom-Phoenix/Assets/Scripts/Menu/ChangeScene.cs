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
            SceneManager.LoadScene("BattleScene");
        });
        buttonDeck.onClick.AddListener(() => {
            SceneManager.LoadScene("DeckScene");
        });
    }
}
