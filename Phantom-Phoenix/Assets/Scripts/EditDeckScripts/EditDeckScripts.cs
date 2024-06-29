using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
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
    List<CardEntity> stockCards;
    List<int> cardIDs;
    private int pages = 1;
    private void Start()
    {
        var heroID = GameDataManager.instance.editDeckHeroID;
        stockCards = GameDataManager.instance.cardlist.cl.Where(c => c.hero == CardEntity.Hero.common || (int)c.hero == heroID).OrderByDescending(c => c.hero).ThenBy(c => c.ID).ToList();
        cardIDs = stockCards.Select(c => c.ID).ToList();

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

        DeckModel deckmodel = new DeckModel().Init();
        if(deckmodel.useHeroID == GameDataManager.instance.editDeckHeroID)
        {
            deckmodel.deck.OrderBy(i => i).ToList().ForEach(i =>
            {
                Instantiate(cardPrefab, Deck).GetComponent<EditDeckCardController>().Init(i);
            });
        }

        //�ŏ��̃y�[�W��\��
        GetNewStock();

        
    }
    private void GetNewStock()
    {
        int viewCount = 8;
        int id = 0;

        if (cardIDs.Count is int cc && cc < pages * 8)
        {
            if (cc <= (pages - 1) * 8) { pages--; } //�\������J�[�h���Ȃ��̂ŁApage--�őΏ�
            viewCount = cc % 8;
            if(viewCount == 0) { viewCount = 8; } //8�Ŋ���؂��Ȃ�8���\������
        }

        //���A�J�[�h�ꗗ�ɕ\������Ă���J�[�h������
        foreach (Transform child in Stock) { 
           Destroy(child.gameObject);
        }

        //�y�[�W������ɂ��ăJ�[�h���擾����
        do
        {
            EditDeckCardController card = Instantiate(cardPrefab, Stock).GetComponent<EditDeckCardController>();
            card.Init(cardIDs[(id++) + (8 * pages) - 8]);
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
