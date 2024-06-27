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
                textTension.text = "1�̂�I������\n�����Ȃ�3��\n�G�Ȃ�2�_���[�W";
                break;
        }
    }
    /// <summary>
    /// �e���V�����J�[�h���e���V�����X�y���𓮂����邩�ۂ��̉��� ��{�I��TensionController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        panelSelectable.SetActive(isActive);
    }
}
