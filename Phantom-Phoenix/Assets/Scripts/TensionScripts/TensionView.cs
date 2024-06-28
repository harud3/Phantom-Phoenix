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
    private Sprite tension0, tension1, tension2;
    [SerializeField]    
    private Sprite tensionElf,tensionWitch;
    [SerializeField] TextMeshProUGUI textCost;
    [SerializeField] TextMeshProUGUI textTension;
    [SerializeField] GameObject panelSelectable;
    public void ReShow(TensionModel model)
    {
        switch (model.tension)
        {
            case 0:
                textCost.text = "1";
                imageTensionCard.sprite = tension0;
                textTension.text = "";
                break;
            case 1:
                textCost.text = "1";
                imageTensionCard.sprite = tension1;
                textTension.text = "";
                break;
            case 2:
                textCost.text = "1";
                imageTensionCard.sprite = tension2;
                textTension.text = "";
                break;
            case 3:
                SetTensionSpell(model);
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
    private void SetTensionSpell(TensionModel model)
    {
        textCost.text = "0";
        switch (model.tensionID)
        {
            case 1: //elf
                imageTensionCard.sprite = tensionElf;
                textTension.text = "味方フィールドに\n2/2/2 即撃 を1体出す";
                break;
            case 2: //witch
                imageTensionCard.sprite = tensionWitch;
                textTension.text = "1体を選択する\n味方なら3回復\n敵なら2ダメージ";
                break;
        }

        
    }
}
