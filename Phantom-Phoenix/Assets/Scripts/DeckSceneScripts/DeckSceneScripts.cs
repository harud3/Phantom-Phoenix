using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �f�b�L�Ґ���ʁ@��ɃJ�[�h�ꗗ�̃y�[�W�Ǘ����s���@1�y�[�W�ő�8��
/// </summary>
public class DeckSceneManager : MonoBehaviour
{
    [SerializeField] Button ButtonLeft;
    [SerializeField] Button ButtonRight;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Stock;
    [SerializeField] Transform Deck;
    private int pages = 1;
    private int id = 1;
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
            Instantiate(cardPrefab, Deck).GetComponent<DeckSceneCardController>().Init(i);
        });
    }
    private void GetNewStock()
    {
        
        id = 1;
        //���ꂪ�Ȃ��ꍇ�A�y�[�W��-1���Ē��덇�킹
        if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { pages--; }

        //���A�J�[�h�ꗗ�ɕ\������Ă���J�[�h������
        foreach (Transform child in Stock) { 
           GameObject.Destroy(child.gameObject);
        }

        //�y�[�W������ɂ��ăJ�[�h���擾����
        int viewCount = 0;
        do
        {
            if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { break; } //�擾�ł��Ȃ������甲����
            DeckSceneCardController card = Instantiate(cardPrefab, Stock).GetComponent<DeckSceneCardController>();
            card.Init((id++) + (8 * pages) - 8);
        } while (++viewCount < 8);
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
