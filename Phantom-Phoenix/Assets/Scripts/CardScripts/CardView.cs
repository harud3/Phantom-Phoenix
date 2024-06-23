using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// カードの見た目　Cardプレハブについてる
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] Image imageCharacter;
    [SerializeField] Image imageFrame;
    [SerializeField] Image imageBack;
    [SerializeField] Image imageBackCost;
    [SerializeField] TextMeshProUGUI textCost;
    [SerializeField] Image imageBackATK;
    [SerializeField] TextMeshProUGUI textATK;
    [SerializeField] Image imageBackHP;
    [SerializeField] TextMeshProUGUI textHP;
    [SerializeField] TextMeshProUGUI textCard;
    [SerializeField] GameObject panelSelectable;
    [SerializeField] GameObject frameTaunt;
    /// <summary>
    /// カードの初期表示　基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="cardModel"></param>
    public void SetCard(CardModel cardModel)
    {
        imageCharacter.sprite = cardModel.character;
        imageFrame.sprite = Resources.Load<Sprite>($"Frames/{cardModel.category}{cardModel.rarity}");

        //敵カードは裏面表示にする
        if (!cardModel.isPlayerCard)
        {
            Show(false);
        }

        //スペルカードはATKとHPを非表示
        if(cardModel.category == CardEntity.Category.spell)
        {
            imageBackATK.gameObject.SetActive(false);
            imageBackHP.gameObject.SetActive(false);
        }

        //ステータス表示
        textATK.text = cardModel.atk.ToString();
        textHP.text = cardModel.hp.ToString();
        textCost.text = cardModel.cost.ToString();
        textCard.text = cardModel.textCard.Replace(@"\n","\n");　//改行処理
    }
    /// <summary>
    /// コストを隠す 基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="cardModel"></param>
    public void HideCost(bool isActive)
    {
        imageBackCost.gameObject.SetActive(isActive);
    }
    /// <summary>
    /// 表面・裏面の表示切替 基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void Show(bool viewOpenSide)
    {
        imageBack.gameObject.SetActive(!viewOpenSide);

        imageFrame.gameObject.SetActive(viewOpenSide);
        imageBackATK.gameObject.SetActive(viewOpenSide);
        imageBackHP.gameObject.SetActive(viewOpenSide);
    }
    /// <summary>
    /// ステータスの再表示　基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="cardModel"></param>
    public void ReShow(CardModel cardModel)
    {
        textCost.text = cardModel.cost.ToString();
        textATK.text = cardModel.atk.ToString();
        textHP.text = cardModel.hp.ToString();
    }
    /// <summary>
    /// 主にユニットを動かせるか否かの可視化 基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        panelSelectable.SetActive(isActive);
    }
    /// <summary>
    /// 挑発効果発動中のフレームを表示するか否か 基本的にCardControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="isView"></param>
    public void SetViewFrameTaunt(bool isView)
    {
        frameTaunt.SetActive(isView);
    }
}
