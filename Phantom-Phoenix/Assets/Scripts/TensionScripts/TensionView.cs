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
    /// �e���V�����J�[�h���e���V�����X�y���𓮂����邩�ۂ��̉��� ��{�I��TensionController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        panelSelectable.SetActive(isActive);
    }
    /// <summary>
    /// �e��q�[���[�̃e���V�����X�y���̕\��
    /// </summary>
    private void SetTensionSpell(TensionModel model)
    {
        textCost.text = "0";
        switch (model.tensionID)
        {
            case 1: //�G���t
                imageTensionCard.sprite = tensionElf;
                textTension.text = "�����t�B�[���h��\n2/2/2 ���� ��1�̏o��";
                break;
            case 2: //�E�B�b�`
                imageTensionCard.sprite = tensionWitch;
                textTension.text = "1�̂�I������\n�����Ȃ�3��\n�G�Ȃ�2�_���[�W";
                break;
            case 3: //�L���O
                imageTensionCard.sprite = tensionKing;
                textTension.text = "�S�Ă̖������j�b�g��\n+1/+1\n�������j�b�g��1�̂Ȃ�\n�����+2/+2";
                break;
            case 4: //�f�[����
                imageTensionCard.sprite = tensionDemon;
                textTension.text = "�S�Ă̓G���j�b�g��-1/-1\n�G��D�̒��߂�\n�������J�[�h��\n�R�X�g+1";
                break;
        }

        
    }
}
