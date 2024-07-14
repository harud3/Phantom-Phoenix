using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Menu�Ŏg���@Menu����̑J��
/// </summary>
public class ChangeSceneFromMenu : MonoBehaviour
{
    [SerializeField]
    Button buttonVSAI, buttonVSPlayer, buttonDeck;
    public void Awake()
    {
        buttonVSAI.onClick.AddListener(() => { //AI��
            GameDataManager.instance.isOnlineBattle = false;
            GameDataManager.instance.scene = GameDataManager.Scene.battle;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
        buttonVSPlayer.onClick.AddListener(() => { //�ΐl��
            GameDataManager.instance.isOnlineBattle = true;
            GameDataManager.instance.scene = GameDataManager.Scene.battle;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
        buttonDeck.onClick.AddListener(() => { //�f�b�L�Ґ�
            GameDataManager.instance.scene = GameDataManager.Scene.deck;
            Invoke("ChangeSelectHeroScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
    }
    public void Start()
    {
        GameDataManager.instance.scene = GameDataManager.Scene.menu; //���ꂪ�ǂݍ��܂��̂́A���j���[��ʂɗ�����������āA���݂̏�Ԃ�menu�ƂȂ�
    }
    private void ChangeSelectHeroScene()
    {
        SceneManager.LoadScene("SelectHeroScene");
    }
}
