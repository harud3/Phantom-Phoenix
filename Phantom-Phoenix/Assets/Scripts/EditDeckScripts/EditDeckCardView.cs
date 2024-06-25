using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// デッキ編成画面のカードの見た目
/// </summary>
public class EditDeckCardView : MonoBehaviour
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
    public void SetCard(EditDeckCardModel cardModel)
    {
        iconImage.sprite = cardModel.character;
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
