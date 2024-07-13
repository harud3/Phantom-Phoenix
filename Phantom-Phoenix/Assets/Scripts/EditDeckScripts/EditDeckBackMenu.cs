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
            StopAllCoroutines();
            if (Deck.childCount == 30) //�f�b�L��30���Ȃ�Z�[�u����
            {
                //json�t�@�C���ɋL�^
                List<int> cardIDs = new List<int>();
                foreach (Transform card in Deck)
                {
                    cardIDs.Add(card.GetComponent<EditDeckCardController>().model.cardID);
                }
                DeckData data = new DeckData()
                {
                    useHeroID = GameDataManager.instance.DeckHeroID,
                    deck = cardIDs
                };
                string json = JsonUtility.ToJson(data, true);
                PlayerPrefs.SetString($"PlayerDeckData{data.useHeroID}", json);
                PlayerPrefs.Save();
            }
            StartCoroutine(ChangeMenuScene());
        });

    }
    IEnumerator ChangeMenuScene()
    {
        AudioManager.instance.SoundButtonClick3();
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene("MenuScene");
    }

}
