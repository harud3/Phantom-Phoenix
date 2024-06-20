using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// カードの見た目
/// </summary>
public class DeckSceneCardView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image frameImage;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Image backATK;
    [SerializeField] TextMeshProUGUI atkText;
    [SerializeField] Image backHP;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI cardText;
    /// <summary>
    /// 基本的にControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="cardModel"></param>
    public void SetCard(DeckSceneCardModel cardModel)
    {
        iconImage.sprite = cardModel.icon;
        frameImage.sprite = Resources.Load<Sprite>($"Frames/{cardModel.category}{cardModel.rarity}");
        if(cardModel.category == CardEntity.Category.spell)
        {
            backATK.gameObject.SetActive(false);
            backHP.gameObject.SetActive(false);
        }
        atkText.text = cardModel.atk.ToString();
        hpText.text = cardModel.hp.ToString();
        costText.text = cardModel.cost.ToString();
        cardText.text = cardModel.cardText.Replace(@"\n","\n");
    }
}
