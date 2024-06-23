using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �J�[�h�̌����ځ@Card�v���n�u�ɂ��Ă�
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] Image imageCharacter;
    [SerializeField] Image imageFrame;
    [SerializeField] Image imageBack;
    [SerializeField] Image imageBackCost;
    [SerializeField] TextMeshProUGUI textCost;
    [SerializeField] Image imageBackATK;
    [SerializeField] TextMeshProUGUI textATK;
    [SerializeField] Image imageBackHP;
    [SerializeField] TextMeshProUGUI textHP;
    [SerializeField] TextMeshProUGUI textCard;
    [SerializeField] GameObject panelSelectable;
    [SerializeField] GameObject frameTaunt;
    /// <summary>
    /// �J�[�h�̏����\���@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void SetCard(CardModel cardModel)
    {
        imageCharacter.sprite = cardModel.character;
        imageFrame.sprite = Resources.Load<Sprite>($"Frames/{cardModel.category}{cardModel.rarity}");

        //�G�J�[�h�͗��ʕ\���ɂ���
        if (!cardModel.isPlayerCard)
        {
            Show(false);
        }

        //�X�y���J�[�h��ATK��HP���\��
        if(cardModel.category == CardEntity.Category.spell)
        {
            imageBackATK.gameObject.SetActive(false);
            imageBackHP.gameObject.SetActive(false);
        }

        //�X�e�[�^�X�\��
        textATK.text = cardModel.atk.ToString();
        textHP.text = cardModel.hp.ToString();
        textCost.text = cardModel.cost.ToString();
        textCard.text = cardModel.textCard.Replace(@"\n","\n");�@//���s����
    }
    /// <summary>
    /// �R�X�g���B�� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void HideCost(bool isActive)
    {
        imageBackCost.gameObject.SetActive(isActive);
    }
    /// <summary>
    /// �\�ʁE���ʂ̕\���ؑ� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void Show(bool viewOpenSide)
    {
        imageBack.gameObject.SetActive(!viewOpenSide);

        imageFrame.gameObject.SetActive(viewOpenSide);
        imageBackATK.gameObject.SetActive(viewOpenSide);
        imageBackHP.gameObject.SetActive(viewOpenSide);
    }
    /// <summary>
    /// �X�e�[�^�X�̍ĕ\���@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void ReShow(CardModel cardModel)
    {
        textCost.text = cardModel.cost.ToString();
        textATK.text = cardModel.atk.ToString();
        textHP.text = cardModel.hp.ToString();
    }
    /// <summary>
    /// ��Ƀ��j�b�g�𓮂����邩�ۂ��̉��� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        panelSelectable.SetActive(isActive);
    }
    /// <summary>
    /// �������ʔ������̃t���[����\�����邩�ۂ� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isView"></param>
    public void SetViewFrameTaunt(bool isView)
    {
        frameTaunt.SetActive(isView);
    }
}
