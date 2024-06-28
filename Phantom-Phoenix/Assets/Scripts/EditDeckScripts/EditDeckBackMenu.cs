using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ���C�����j���[�ɖ߂鎞�̏���
/// </summary>
public class EditDeckbackmenu : MonoBehaviour
{
    [SerializeField]
    Button button;
    [SerializeField]
    Transform Deck;
    [SerializeField]
    TextMeshProUGUI textHint;
    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (Deck.childCount < 30) //�f�b�L��30�������Ȃ瓦�����Ȃ�
            {
                StopAllCoroutines();
                StartCoroutine(ChangeText());
                return;
            }
            StopAllCoroutines(); 
            
            //json�t�@�C���ɋL�^
            List<int> cardIDs = new List<int>();
            foreach (Transform card in Deck)
            {
                cardIDs.Add(card.GetComponent<EditDeckCardController>().model.cardID);
            }
            DeckData data = new DeckData()
            {
                useHeroID = GameDataManager.instance.editDeckHeroID, deck = cardIDs //TODO:�q�[���[ID���ςɂ���
            };
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PlayerDeckData", json);
            PlayerPrefs.Save();
            StartCoroutine(ChangeMenuScene());
        });

    }
    IEnumerator ChangeText()
    {
        textHint.text = "�f�b�L��30�������ł�";
        yield return new WaitForSeconds(2f);
        textHint.text = "�J�[�h���f�b�L�Ƀh���b�O&�h���b�v";
        StopAllCoroutines();
    }
    IEnumerator ChangeMenuScene()
    {
        AudioManager.instance.SoundButtonClick3();
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene("MenuScene");
    }

}
