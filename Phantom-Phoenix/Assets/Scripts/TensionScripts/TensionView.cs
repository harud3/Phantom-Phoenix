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
    private Sprite tensionElf,tensionWitch,tensionKing,tensionDemon;
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
    /// <summary>
    /// 各種ヒーローのテンションスペルの表示
    /// </summary>
    private void SetTensionSpell(TensionModel model)
    {
        textCost.text = "0";
        switch (model.tensionID)
        {
            case 1: //エルフ
                imageTensionCard.sprite = tensionElf;
                textTension.text = "味方フィールドに\n2/2/2 即撃 を1体出す";
                break;
            case 2: //ウィッチ
                imageTensionCard.sprite = tensionWitch;
                textTension.text = "1体を選択する\n味方なら3回復\n敵なら2ダメージ";
                break;
            case 3: //キング
                imageTensionCard.sprite = tensionKing;
                textTension.text = "全ての味方ユニットを\n+1/+1\n味方ユニットが1体なら\n代わりに+2/+2";
                break;
            case 4: //デーモン
                imageTensionCard.sprite = tensionDemon;
                textTension.text = "全ての敵ユニットを-1/-1\n敵手札の直近で\n引いたカードの\nコスト+1";
                break;
        }

        
    }
}
