using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// �q�[���[�̌�����
/// </summary>
public class HeroView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image hpIcon1;
    [SerializeField] Image hpIcon2;
    [SerializeField] Image mpIcon1;
    [SerializeField] Image mpIcon2;
    [SerializeField] TextMeshProUGUI maxMPText;
    // Start is called before the first frame update
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="heroModel"></param>
    public void Show(HeroModel heroModel)
    {
        iconImage.sprite = heroModel.icon;
        //�q�[���[HP/MP�̕\���́A���ʒu�Œ�̓񌅁@����āA�񌅖ڂ�0�̎���Image�������Ȃ�����@ex) HP9 �� �~09, �Z 9
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
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="heroModel"></param>
    public void ReShow(HeroModel heroModel)
    {
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
}
