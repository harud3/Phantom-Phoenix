using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Menuで使う　Menuからの遷移
/// </summary>
public class ChangeSceneFromMenu : MonoBehaviour
{
    [SerializeField]
    Button buttonVSAI, buttonVSPlayer, buttonDeck;
    public void Awake()
    {
        buttonVSAI.onClick.AddListener(() => {
            GameDataManager.instance.isOnlineBattle = false;
            GameDataManager.instance.scene = GameDataManager.Scene.battle;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
        buttonVSPlayer.onClick.AddListener(() => {
            GameDataManager.instance.isOnlineBattle = true;
            GameDataManager.instance.scene = GameDataManager.Scene.battle;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
        buttonDeck.onClick.AddListener(() => {
            GameDataManager.instance.scene = GameDataManager.Scene.deck;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
    }
    public void Start()
    {
        GameDataManager.instance.scene = GameDataManager.Scene.menu; //これが読み込まれるのは、メニュー画面に来た時
    }
    private void ChangeSelectHeroScene()
    {
        SceneManager.LoadScene("SelectHeroScene");
    }
}
