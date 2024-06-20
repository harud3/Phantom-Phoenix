using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckScenebackmenu : MonoBehaviour
{
    [SerializeField]
    Button button;
    [SerializeField]
    Transform Deck;
    [SerializeField]
    TextMeshProUGUI hintText;
    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (Deck.childCount < 30)
            {
                StopAllCoroutines();
                StartCoroutine(ChangeText());
                return;
            }
            StopAllCoroutines();
            List<int> cardIDs = new List<int>();
            foreach (Transform card in Deck)
            {
                cardIDs.Add(card.GetComponent<DeckSceneCardController>().model.cardID);
            }
            DeckData data = new DeckData()
            {
                useHeroID = 1, deck = cardIDs
            };
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PlayerDeckData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MenuScene");
        });

    }
    IEnumerator ChangeText()
    {
        hintText.text = "デッキが30枚未満です";
        yield return new WaitForSeconds(2f);
        hintText.text = "カードをデッキにドラッグ&ドロップ";
        StopAllCoroutines();
    }
}
