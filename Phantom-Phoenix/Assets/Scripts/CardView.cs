using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image hpIcon;
    [SerializeField] Image atkIcon;
    [SerializeField] Image costIcon;
    [SerializeField] GameObject selectablePanel;
    // Start is called before the first frame update
    public void Show(CardModel cardModel)
    {
        iconImage.sprite = cardModel.icon;
        hpIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.hp}");
        atkIcon.sprite =  Resources.Load<Sprite>($"Numbers/s{cardModel.atk}");
        costIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.cost}");
    }
    public void ReShow(CardModel cardModel)
    {
        hpIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.hp}");
        atkIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.atk}");
    }
    public void SetActiveSelectablePanel(bool flg)
    {
        selectablePanel.SetActive(flg);
    }
}
