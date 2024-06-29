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
    /// �e���V�����J�[�h���e���V�����X�y���𓮂����邩�ۂ��̉��� ��{�I��TensionController��ʂ��ČĂԂ��ƂɂȂ�
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
                textTension.text = "�����t�B�[���h��\n2/2/2 ���� ��1�̏o��";
                break;
            case 2: //witch
                imageTensionCard.sprite = tensionWitch;
                textTension.text = "1�̂�I������\n�����Ȃ�3��\n�G�Ȃ�2�_���[�W";
                break;
        }

        
    }
}