using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TensionView : MonoBehaviour
{
    [SerializeField]
    private Image imageTensionCard;
    [SerializeField]
    private Sprite tension0, tension1, tension2, tension3;
    [SerializeField] TextMeshProUGUI textCost;
    [SerializeField] TextMeshProUGUI textTension;
    [SerializeField] GameObject panelSelectable;
    public void ReShow(TensionModel model)
    {
        switch (model.tension)
        {
            case 0:
                imageTensionCard.sprite = tension0;
                textCost.text = "1";
                textTension.text = "";
                break;
            case 1:
                imageTensionCard.sprite = tension1;
                textCost.text = "1";
                textTension.text = "";
                break;
            case 2:
                imageTensionCard.sprite = tension2;
                textCost.text = "1";
                textTension.text = "";
                break;
            case 3:
                imageTensionCard.sprite = tension3;
                textCost.text = "0";
                textTension.text = "1体を選択する\n味方なら3回復\n敵なら2ダメージ";
                break;
        }
    }
    /// <summary>
    /// テンションカードかテンションスペルを動かせるか否かの可視化 基本的にTensionControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        panelSelectable.SetActive(isActive);
    }
}
