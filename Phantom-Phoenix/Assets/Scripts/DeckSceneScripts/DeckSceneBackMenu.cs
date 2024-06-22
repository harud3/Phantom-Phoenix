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
public class DeckScenebackmenu : MonoBehaviour
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
                cardIDs.Add(card.GetComponent<DeckSceneCardController>().model.cardID);
            }
            DeckData data = new DeckData()
            {
                useHeroID = 1, deck = cardIDs //TODO:�q�[���[ID���ςɂ���
            };
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PlayerDeckData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MenuScene");
        });

    }
    IEnumerator ChangeText()
    {
        textHint.text = "�f�b�L��30�������ł�";
        yield return new WaitForSeconds(2f);
        textHint.text = "�J�[�h���f�b�L�Ƀh���b�O&�h���b�v";
        StopAllCoroutines();
    }
}
