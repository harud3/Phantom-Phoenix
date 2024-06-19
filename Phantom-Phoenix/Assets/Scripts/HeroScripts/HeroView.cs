using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// ヒーローの見た目
/// </summary>
public class HeroView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image hpIcon1;
    [SerializeField] Image hpIcon2;
    [SerializeField] Image mpIcon1;
    [SerializeField] Image mpIcon2;
    [SerializeField] TextMeshProUGUI maxMPText;
    [SerializeField] Image stackCardIcon1;
    [SerializeField] Image stackCardIcon2;
    // Start is called before the first frame update
    /// <summary>
    /// 基本的にControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="heroModel"></param>
    public void SetHero(HeroModel heroModel)
    {
        iconImage.sprite = heroModel.icon;
        //ヒーローHP/MPの表示は、桁位置固定の二桁　よって、二桁目が0の時はImageを見えなくする　ex) HP9 → ×09, 〇 9
        int hp1 = heroModel.hp / 10;
        hpIcon1.sprite = Resources.Load<Sprite>($"Numbers/s{hp1}");
        if (hp1 == 0) { hpIcon1.enabled = false; } else { hpIcon1.enabled = true; }
        hpIcon2.sprite = Resources.Load<Sprite>($"Numbers/s{heroModel.hp % 10}");
        int mp1 = heroModel.mp / 10;
        if (mp1 == 0) { mpIcon1.enabled = false; } else { mpIcon1.enabled = true; }
        mpIcon1.sprite = Resources.Load<Sprite>($"Numbers/s{mp1}");
        mpIcon2.sprite = Resources.Load<Sprite>($"Numbers/s{heroModel.mp % 10}");
        maxMPText.text = heroModel.maxMP.ToString();
    }
    /// <summary>
    /// 基本的にControllerを通して呼ぶことになる
    /// </summary>
    /// <param name="heroModel"></param>
    public void ReShowHP(HeroModel heroModel)
    {
        int hp1 = heroModel.hp / 10;
        hpIcon1.sprite = Resources.Load<Sprite>($"Numbers/s{hp1}");
        if (hp1 == 0) { hpIcon1.enabled = false; } else { hpIcon1.enabled = true; }
        hpIcon2.sprite = Resources.Load<Sprite>($"Numbers/s{heroModel.hp % 10}");
    }
    public void ReShowMP(HeroModel heroModel)
    {
        int mp1 = heroModel.mp / 10;
        if (mp1 == 0) { mpIcon1.enabled = false; } else { mpIcon1.enabled = true; }
        mpIcon1.sprite = Resources.Load<Sprite>($"Numbers/s{mp1}");
        mpIcon2.sprite = Resources.Load<Sprite>($"Numbers/s{heroModel.mp % 10}");
        maxMPText.text = heroModel.maxMP.ToString();
    }
    public void ReShowStackCards(int deckNum)
    {
        int stackCards = deckNum / 10;
        if (stackCards == 0) { stackCardIcon1.enabled = false; } else { stackCardIcon1.enabled = true; }
        stackCardIcon1.sprite = Resources.Load<Sprite>($"Numbers/s{stackCards}");
        stackCardIcon2.sprite = Resources.Load<Sprite>($"Numbers/s{deckNum % 10}");
    }
}
