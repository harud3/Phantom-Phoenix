using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �f�b�L�Ґ���ʁ@��ɃJ�[�h�ꗗ�̃y�[�W�Ǘ����s���@1�y�[�W�ő�8��
/// </summary>
public class EditDeckManager : MonoBehaviour
{
    [SerializeField] Button ButtonLeft;
    [SerializeField] Button ButtonRight;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Stock;
    [SerializeField] Transform Deck;
    private int pages = 1;
    private void Start()
    {
        
        ButtonLeft.onClick.AddListener(() =>
        {
            AudioManager.instance.SoundCardMove();
            ButtonPageOnClick(true);
        });
        ButtonRight.onClick.AddListener(() =>
        {
            AudioManager.instance.SoundCardMove();
            ButtonPageOnClick(false);
        });

        //�ŏ��̃y�[�W��\��
        GetNewStock();

        DeckModel deckmodel = new DeckModel().Init();
        deckmodel.deck.OrderBy(i => i).ToList().ForEach(i =>
        {
            Instantiate(cardPrefab, Deck).GetComponent<EditDeckCardController>().Init(i);
        });
    }
    private void GetNewStock()
    {
        int viewCount = 8;
        int id = 1;

        if (GameDataManager.instance.cardlist.cl.Count is int clc && clc < pages * 8)
        {
            if (clc < (pages - 1) * 8) { pages--; } //�\������J�[�h���Ȃ��̂ŁApage--�őΏ�
            viewCount = clc % 8;
        }

        //���A�J�[�h�ꗗ�ɕ\������Ă���J�[�h������
        foreach (Transform child in Stock) { 
           Destroy(child.gameObject);
        }

        //�y�[�W������ɂ��ăJ�[�h���擾����
        do
        {
            EditDeckCardController card = Instantiate(cardPrefab, Stock).GetComponent<EditDeckCardController>();
            card.Init((id++) + (8 * pages) - 8);
        } while (--viewCount > 0);
    }
    private void ButtonPageOnClick(bool isLeft)
    {
        //���{�^���͑O�y�[�W
        if (isLeft)
        {
            if (--pages <= 0) { pages = 1; }
        }
        else //�E�{�^���͎��y�[�W
        {
            ++pages;
        }
        GetNewStock();
    }
}
