using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// メインメニューに戻る時の処理
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
            if (Deck.childCount < 30) //デッキが30枚未満なら逃がさない
            {
                StopAllCoroutines();
                StartCoroutine(ChangeText());
                return;
            }
            StopAllCoroutines(); 
            
            //jsonファイルに記録
            List<int> cardIDs = new List<int>();
            foreach (Transform card in Deck)
            {
                cardIDs.Add(card.GetComponent<DeckSceneCardController>().model.cardID);
            }
            DeckData data = new DeckData()
            {
                useHeroID = 1, deck = cardIDs //TODO:ヒーローIDも可変にする
            };
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PlayerDeckData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MenuScene");
        });

    }
    IEnumerator ChangeText()
    {
        textHint.text = "デッキが30枚未満です";
        yield return new WaitForSeconds(2f);
        textHint.text = "カードをデッキにドラッグ&ドロップ";
        StopAllCoroutines();
    }
}
