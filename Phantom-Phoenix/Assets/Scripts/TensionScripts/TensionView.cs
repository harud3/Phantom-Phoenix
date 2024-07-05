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
    private Sprite tensionElf,tensionWitch,tensionKing,tensionDemon,tensionKnight;
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
            case 3: //king
                imageTensionCard.sprite = tensionKing;
                textTension.text = "全ての味方ユニットを\n+1/+1\n味方ユニットが1体なら\n代わりに+2/+2";
                break;
            case 4: //demon
                imageTensionCard.sprite = tensionDemon;
                textTension.text = "全ての敵ユニットを-1/-1\n敵手札の直近で\n引いたカードの\nコスト+1";
                break;
            case 5: //knight
                imageTensionCard.sprite = tensionKnight;
                textTension.text = "敵ユニット1体に1ダメージ\n味方ヒーローの\n最大HPと現在HPの\n差5につきダメージ+1\nさらに対象の後ろにいる\nユニットに同じダメージ";
                break;
        }

        
    }
}
