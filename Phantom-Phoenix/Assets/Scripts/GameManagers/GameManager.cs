
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;
using UnityEngine.XR;
/// <summary>
/// �o�g���𓝊�����X�N���v�g
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private Transform canvas;
    [SerializeField] private HeroController playerHeroController, enemyHeroController; //�����q�[���[�A�G�q�[���[
    [SerializeField] private TensionController playerTensionController, enemyTensionController; //�����e���V�����A�G�e���V����

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6]; //�����t�B�[���h�A�G�t�B�[���h
    [SerializeField] private Transform playerHandTransform, enemyHandTransform; //������D�A�G��D
    [SerializeField] private Transform playerSelectionTransform; //�I���t�B�[���h
    [SerializeField] private GameObject HintMessage;

    [SerializeField] private CardController cardPrefab; //�J�[�h

    [SerializeField] private UnityEngine.UI.Button ButtonTurn; //�^�[���ύX�{�^��
    [SerializeField] private GameObject PanelGuard; //������K�[�h����p�l��
    [SerializeField] private Sprite playerTurnSprite, enemyTurnSprite; //�^�[���I���A����̃^�[���̃X�v���C�g

    [SerializeField] private TextMeshProUGUI timeCountText; //�^�[���̎c�莞�Ԃ̕\����
    [SerializeField] int timeLimit; //�^�[���̎��Ԑ���
    int timeCount; //�^�[���̎c�莞��

    private bool _isPlayerTurn;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;

    DeckModel playerDeck = null;
    DeckModel enemyDeck = null; int enemyUseHeroID = 0;
    #region �����ݒ�
    /// <summary>
    /// �Q�[���̏�� �ʐM�ΐ�ɂ����āA�ŏ��ɑ��݃f�[�^�ʐM(�f�b�L�A�V�[�h�l)������̂ŊǗ��K�{
    /// </summary>
    public enum eGameState
    {
        isBigin,
        isGotPlayerTurn,
        isWaitEnemyHeroID,
        isWaitMulligan,
        isProcessMulligan,
        isWaitStart,
        isStarted,
    }
    public eGameState gameState { get; private set; }
    void Start()
    {
        gameState = eGameState.isBigin;
        StartGame();
    }
    void StartGame()
    {
        playerDeck = new DeckModel().Init();
        if (GameDataManager.instance.isOnlineBattle)
        {
            if (GameDataManager.instance.isMaster) //�����傪���������
            {
                isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
                SendSetIsPlayerTurn(isPlayerTurn);
                gameState = eGameState.isGotPlayerTurn;
            }

        }
        else //�I�����C���ΐ�ł͂Ȃ���AI��
        {
            enemyDeck = new DeckModel().Init();
            enemyUseHeroID = enemyDeck.useHeroID;
            isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
            gameState = eGameState.isWaitEnemyHeroID;
        }
    }
    /// <summary>
    /// �ǂ��炩��J�n����̂���ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="isPlayerTurn"></param>
    public void SendSetIsPlayerTurn(bool isPlayerTurn)
    {
        photonView.RPC(nameof(RPCSetIsPlayerTurn), RpcTarget.Others, isPlayerTurn);
    }
    [PunRPC]
    void RPCSetIsPlayerTurn(bool isPlayerTurn)
    {
        this.isPlayerTurn = !isPlayerTurn;
        if(gameState == eGameState.isBigin)
        {
            gameState = eGameState.isGotPlayerTurn;
        }
    }
    private void Update()
    {
        if(gameState == eGameState.isGotPlayerTurn){ //Ai��͂��̏�Ԃ��΂��Ă���̂ŁA�I�����C���`�F�b�N�Ȃ�
            gameState = eGameState.isWaitEnemyHeroID;
            SendUseHeroID(playerDeck.useHeroID);
        }
        //�I�����C���ΐ�ł́A����̃f�b�L���Ȃ���Ύn�߂��Ȃ��̂ŁA�����ŊJ�n���� ����ɔ����AAI��������ŊJ�n����
        else if (gameState == eGameState.isWaitEnemyHeroID &&  enemyUseHeroID != 0)
        {
            SettingInitHero();
            playerTensionController.Init(playerDeck.useHeroID);
            enemyTensionController.Init(enemyUseHeroID);
            if (!isPlayerTurn)
            {
                playerTensionController.SetTension(3);

            }
            else { enemyTensionController.SetTension(3); }
            gameState = eGameState.isWaitMulligan;
            StartCoroutine(WaitMulligan());
        }
        else if (gameState == eGameState.isWaitStart && enemyDeck != null)
        {
            HintMessage.SetActive(false);
            HintMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"�Ώۂ�I�����Ă�������";

            gameState = eGameState.isStarted;
            if (GameDataManager.instance.isOnlineBattle)
            {
                int seed = int.Parse(DateTime.Now.ToString("ddHHmmss")); //�����_���v�f���v���C���[�Ԃő����邽�߁A�V�[�h�l�����L���邱�ƂőΉ�����
                UnityEngine.Random.InitState(seed);
                SendSetSeed(seed); //�����ɂ̓V�[�h�l����������M���ꂽ���`�F�b�N���ׂ��ȋC������
            }
            SetTime();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
    }
    /// <summary>
    /// �ǂ��炩��J�n����̂���ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="isPlayerTurn"></param>
    public void SendUseHeroID(int useHeroID)
    {
        photonView.RPC(nameof(RPCEnemyUseHeroID), RpcTarget.Others, useHeroID);
    }
    [PunRPC]
    void RPCEnemyUseHeroID(int enemyUseHeroID)
    {
        this.enemyUseHeroID = enemyUseHeroID;
    }
    public void FinishMulligan()
    {
        gameState = eGameState.isProcessMulligan;
    }
    /// <summary>
    /// �}���K�����͑҂����}���K������
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitMulligan()
    {
        for(var i = 0; i < (isPlayerTurn ? 3 : 4); i++)
        {
            CardController cc = Instantiate(cardPrefab, playerSelectionTransform);
            cc.Init(playerDeck.deck[i]);
            cc.SetIsMulliganCard();
        }

        //PlayerMulligan �� PanelMulligan �� FrameMulligan �� TextHintMuligan
        playerSelectionTransform.parent.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"�}���K���@{(isPlayerTurn ? "��U" : "��U")}";

        //���͑҂�
        yield return new WaitUntil(() => gameState == eGameState.isProcessMulligan);


        //�}���K������
        //ex) ��U�Ń}���K�����s����
        //Deck 0, 1, 2, 3 ���ACardID A, B, C, D �ł���A B, D���c���Ƃ���
        //���̎��AA���珇�Ɏc�����Ԃ����̔�����s�� �ϐ�keppIndex��0��������
        //A��isMulligan�����āA�Ԃ����Ƃɂ����@�������Ȃ�         A, B, C, D
        //B��isMulligan�����āA�c�����Ƃɂ����@�����ŁADeck[0]��A�ƁAB�̈ʒu���X���b�v���� �����āAkeepIndex+1            B, A, C, D
        //C��isMulligan�����āA�Ԃ����Ƃɂ����@�������Ȃ�         B, A, C, D
        //D��isMulligan�����āA�c�����Ƃɂ����@�����ŁAkeepInedx�̒l������ɃX���b�v�����J�[�h������ƕ�����̂ŁA���̎���Deck[1]��A�ƁAD�̈ʒu���X���b�v����@�����āAkeepIndex+1          B, D, C, A
        //���݂�keepIndex��2    Deck�̐擪2��(0�Ԗځ`1�Ԗ�)���c�������J�[�h�ƂȂ�@����Đ擪��2��(B, D)���Œ�
        //Deck��2�Ԗځ`29�Ԗڂ܂ł��V���b�t������@����́A�uDeck�� keepIndex�Ԗ� ����A30 - keepIndex ������בւ���v �ƕ\����
        int deckIndex = 0, keepIndex=0;
        int[] mulliganCards = new int[isPlayerTurn ? 3 : 4];
        foreach (Transform mc in playerSelectionTransform)
        {
            CardController cc = mc.GetComponent<CardController>();
            if (!cc.model.isMulligan)
            {
                var c = playerDeck.deck[keepIndex];
                playerDeck.deck[keepIndex++] = cc.model.cardID;
                playerDeck.deck[deckIndex] = c;
            }
            deckIndex++;

        }
        playerSelectionTransform.parent.gameObject.SetActive(false);

        //�Ԃ����J�[�h���߂��Ă���P�[�X�����邯�ǁA�܂��ЂƂ܂�����ł����Ǝv���܂�
        playerDeck.deck = playerDeck.deck.GetRange(0, keepIndex).Concat(playerDeck.deck.GetRange(keepIndex, 30 - keepIndex).OrderBy(i => Guid.NewGuid())).ToList();

        if (GameDataManager.instance.isOnlineBattle)
        {
            SendSetEnemyDeck(playerDeck);
        }

        HintMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"����̃}���K����҂��Ă��܂�";

        gameState = eGameState.isWaitStart;
    }
    /// <summary>
    /// ���g�̃f�b�L��ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="playerDeck"></param>
    public void SendSetEnemyDeck(DeckModel playerDeck)
    {
        photonView.RPC(nameof(RPCSetEnemyDeck), RpcTarget.Others, playerDeck.useHeroID, playerDeck.deck.ToArray());
    }
    [PunRPC]
    void RPCSetEnemyDeck(int useheroID, int[] deckIDs)
    {
        enemyDeck = new DeckModel().Init(useheroID, deckIDs);
    }
    /// <summary>
    /// �V�[�h�l��ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="seed"></param>
    public void SendSetSeed(int seed)
    {
        photonView.RPC(nameof(RPCSetSeed), RpcTarget.Others, seed);
    }
    [PunRPC]
    void RPCSetSeed(int seed)
    {
        UnityEngine.Random.InitState(seed);
    }
    /// <summary>
    /// �q�[���[�̐ݒ�
    /// </summary>
    void SettingInitHero()
    {
        playerHeroController.Init(playerDeck.useHeroID);
        enemyHeroController.Init(enemyUseHeroID);
    }
    /// <summary>
    /// ������D�̐ݒ�
    /// </summary>
    void SettingInitHand()
    {
        GivesCard(true, 3);
        GivesCard(false, 3);
    }
    #endregion
    #region�@���ԊǗ�
    void SetTime()
    {
        timeCountText.text = timeCount.ToString();
    }
    IEnumerator CountDown()
    {
        timeCount = timeLimit;
        SetTime();
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1);
            timeCount--;
            SetTime();
        }
        if (GameDataManager.instance.isOnlineBattle)
        {
            SendChangeTurn();
        }
    }
    #endregion
    #region �J�[�h����
    /// <summary>
    /// ��D�ɃJ�[�h��������
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="drawCount"></param>
    public void GivesCard(bool isPlayer, int drawCount)
    {
        StartCoroutine(GivesCardIE(isPlayer, drawCount));
    }
    private IEnumerator GivesCardIE(bool isPlayer, int drawCount)
    {
        if (isPlayer)
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCard(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
                yield return new WaitForSeconds(0.25f);
            }
        }

        else
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCard(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
    /// <summary>
    /// �f�b�L�����D�ɃJ�[�h��z��
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    private void GiveCard(List<int> deck,Transform hand, bool isPlayer)
    {
        if (deck.Count == 0) //TODO:�f�b�L���Ȃ��Ȃ�߂�@�h���[���Ƃ�1�_���[�W��2�_���[�W��3�_���[�W...�ɂ���
        {
            return;
        }
        //��ԏ�̃f�b�L���擾����
        int cardID = deck[0];
        deck.RemoveAt(0);
        //�f�b�L�c�薇���̍ĕ\��
        if (isPlayer) { playerHeroController.ReShowStackCards(deck.Count()); } 
        else { enemyHeroController.ReShowStackCards(deck.Count()); }

        //��D�̖����͍ő�10��
        if (hand.childCount >= 10) { Debug.Log($"�J�[�hID{cardID}�̃J�[�h�͔R���܂���"); return; }
        StartCoroutine(CreateCard(cardID, hand, isPlayer));
    }
    /// <summary>
    /// �J�[�h�̐����Ǝ�D�ւ̈ړ�
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    IEnumerator CreateCard(int cardID, Transform hand, bool isPlayer)
    {
        CardController cc = Instantiate(cardPrefab, canvas);
        cc.transform.Translate(new Vector3(isPlayer ? -550 : 550, 0, 0), Space.Self);
        cc.GetComponent<CardController>().Init(cardID, isPlayer);
        cc.transform.DOLocalMove(new Vector3(isPlayer ? -100 : 100, 0, 0), 0.25f);
        yield return new WaitForSeconds(0.25f);
        cc.transform.DOMove(hand.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        cc.transform.SetParent(hand);
        if (isPlayer) { SetCanSummonHandCard(cc); }
    }
    #endregion
    #region�@�^�[������
    /// <summary>
    /// �^�[���ύX��ΐ푊��ɑ��M����
    /// </summary>
    public void SendChangeTurn()
    {
        photonView.RPC(nameof(RPCChangeTurn), RpcTarget.Others);
    }
    [PunRPC]
    public void RPCChangeTurn()
    {
        StartCoroutine(ChangeTurn());
    }
    public IEnumerator ChangeTurn(bool isFirst = false)
    {
        PanelGuard.gameObject.SetActive(true);

        if (isFirst) //�Q�[���J�n���̓h���[����������̂ő҂�
        {
            yield return new WaitForSeconds(1.6f);
        }
        else
        {
            AudioManager.instance.SoundButtonClick2(); //ButtonTurn�������ꂽ��
        }

        //�^�[���I��������
        List<CardController> playerCardsHasSSED = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray()).Where(i => i.SpecialSkillEndTurn != null).ToList();
        List<CardController> enemyCardsHasSSED = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray()).Where(i => i.SpecialSkillEndTurn != null).ToList();
        int unitCount = playerCardsHasSSED.Concat(enemyCardsHasSSED).Count();

        SetCanSummonHandCards(true);
        if (isPlayerTurn)
        {
            foreach (var i in playerCardsHasSSED)
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.1f);
            }
            foreach (var i in enemyCardsHasSSED)
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            foreach (var i in enemyCardsHasSSED)
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.1f);
            }
            foreach (var i in playerCardsHasSSED)
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.1f);
            }
        }

        //�^�[���I���������҂�����
        if (!isFirst)
        {
            isPlayerTurn = !isPlayerTurn;
            yield return new WaitForSeconds(unitCount * 0.1f);
        }

        //�^�[���J�n�ł̃h���[�����@�ŏ��̃^�[���̓h���[�Ȃ��ɂ��邽��
        if (isPlayerTurn && !isFirst)
        {
            GivesCard(true, 1);
        }
        else if(!isFirst)
        {
            GivesCard(false, 1);
        }

        //�\���̕ύX
        if (isPlayerTurn)
        {
            ButtonTurn.image.sprite = playerTurnSprite;
            Debug.Log("�����^�[��");
            playerHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, true, true);�@//�A�����̕������s��
            SetCanAttackAllFieldUnit(enemyFields, false);
            SetCanSummonHandCards(); //��D����o���邩�ǂ����̕\��
            playerTensionController.CanUsetensionCard(true);
            SetCanUsetension(true);
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("����^�[��");
            enemyHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, false);
            SetCanAttackAllFieldUnit(enemyFields, true, true); //�A�����̕������s��
            enemyTensionController.CanUsetensionCard(true);
            SetCanUsetension(true,true); //�e���V�������g���邩�ǂ����̕\�����Ȃ���
        }

        yield return new WaitForSeconds(0.6f); //�h���[�҂�����

        PanelGuard.gameObject.SetActive(false); //�^�[���{�^��������悤��

        TurnCalc();
    }
    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (!isPlayerTurn && !GameDataManager.instance.isOnlineBattle)
        {
            StartCoroutine(AIEnemyTurn());
        }
    }
    /// <summary>
    /// �����̃J�[�h�������\�����f���A�\����ύX����
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="setCanNotSummon"></param>
    private void SetCanSummonHandCard(CardController cc, bool setCanNotSummon = false)
    {
        if (cc.model.isPlayerCard && cc.model.cost <= GetHeroMP(true) && !setCanNotSummon) //����ēG��D���ʂ�Ȃ��悤�� setCanNotSummon��true�Ȃ�A�����s�ɂ���
        {
            cc.SetCanSummon(true);
        }
        else
        {
            cc.SetCanSummon(false);
        }
    }
    /// <summary>
    /// ������D�̃J�[�h�������\�����f���A�\����ύX����
    /// </summary>
    /// <param name="setCanNotSummon"></param>
    public void SetCanSummonHandCards(bool setCanNotSummon = false)
    {
        foreach (Transform handCards in playerHandTransform) //�G��D�̏��J���͂��Ȃ��̂ŁA�����Œ�
        {
            SetCanSummonHandCard(handCards.GetComponent<CardController>(), setCanNotSummon);
        }
    }
    /// <summary>
    /// �t�B�[���h��̃��j�b�g�̍U����(�ƘA����)�̕���
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="CanAttack"></param>
    /// <param name="ResetIsActiveDoubleAciton"></param>
    void SetCanAttackAllFieldUnit(Transform[] fields, bool CanAttack, bool ResetIsActiveDoubleAciton = false)
    {
        foreach (var field in fields)
        {
            if (field.childCount != 0)
            {
                var cc = field.GetChild(0).GetComponent<CardController>();
                cc.SetCanAttack(CanAttack, ResetIsActiveDoubleAciton);
                cc.SetIsNotSummonThisTurn();
            }
        }
    }
    #endregion
    #region AI����

    IEnumerator AIEnemyTurn()
    {

        yield return new WaitForSeconds(1f);

        if (enemyTensionController.model.tension == 3)
        {

            switch (enemyTensionController.model.tensionID)
            {
                case 1: //elf
                    yield return new WaitForSeconds(0.25f);
                    if (FieldManager.instance.GetEmptyFieldID(false) is var x && x.emptyField != null)
                    {
                        enemyTensionController.UseTensionSpell<Controller>(null);
                    }
                    yield return new WaitForSeconds(0.5f);
                    break;
                case 2: //witch
                    yield return new WaitForSeconds(0.25f);
                    if (FieldManager.instance.GetRandomUnits(true) is CardController cc) { enemyTensionController.UseTensionSpell(cc); }
                    else { enemyTensionController.UseTensionSpell(playerHeroController); }
                    yield return new WaitForSeconds(0.5f);
                    break;
            }


        }

        //enemyAI��������
        //��D����o����J�[�h���o��
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        if (handCardList.Length != 0)
        {
            foreach (var canPutCard in handCardList.Where(i => i.model.cost <= GetHeroMP(false)))
            {
                if(canPutCard.model.cost > GetHeroMP(false)) { continue; }
                foreach (var enemyField in enemyFields)
                {
                    if (enemyField.childCount == 0)
                    {
                        void Movement(Transform targetTransform)
                        {
                            canPutCard.Show(true);
                            StartCoroutine(canPutCard.movement.MoveToArea(targetTransform));
                        }

                        if(canPutCard.model.category == CardEntity.Category.unit)
                        {
                            canPutCard.Show(true);
                            StartCoroutine(canPutCard.movement.MoveToArea(enemyField));
                            yield return new WaitForSeconds(0.25f);
                            canPutCard.SummonOnField(enemyField.GetComponent<DropField>().fieldID);
                        }
                        else
                        {
                            switch (canPutCard.model.target)
                            {
                                case CardEntity.Target.area:
                                    Movement(enemyField);
                                    yield return new WaitForSeconds(0.25f);
                                    canPutCard.ExecuteSpellContents<Controller>(null); 
                                    break;
                                case CardEntity.Target.enemyUnit:
                                    if(FieldManager.instance.GetRandomUnits(true) is var x && x != null) {
                                        Movement(playerFields[x.model.thisFieldID - 1]);
                                        yield return new WaitForSeconds(0.25f);
                                        canPutCard.ExecuteSpellContents(x); 
                                    }
                                    break;
                            }
                        }
                        yield return new WaitForSeconds(0.75f);
                        break;
                    }
                }
            }

        }

        //field�̍U���\���j�b�g�ōU������ �������j�b�g������Ȃ烆�j�b�g���@�������j�b�g�����Ȃ��Ȃ�q�[���[���U��
        IEnumerable<CardController> canAttackFieldEnemyCards 
            = enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.canAttack);
        foreach (var canAttackFieldEnemyCard in canAttackFieldEnemyCards)
        {
            foreach (var playerField in playerFields)
            {
                if (playerField.childCount != 0)
                {
                    if (canAttackFieldEnemyCard.model.isAlive && playerField.GetComponentInChildren<CardController>() is var pcc && pcc.model.isAlive)
                    {
                        if (!FieldManager.instance.CheckCanAttackUnit(canAttackFieldEnemyCard, pcc)) { continue; }

                        StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerField));
                        yield return new WaitForSeconds(1f);
                        StartCoroutine(CardsBattle(canAttackFieldEnemyCard, pcc));
                        yield return new WaitForSeconds(0.3f);
                        break;
                    }

                }
            }
        }
        canAttackFieldEnemyCards
            = enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.canAttack);
        foreach (var canAttackFieldEnemyCard in canAttackFieldEnemyCards)
        { 
            if (canAttackFieldEnemyCard.model.canAttack && canAttackFieldEnemyCard.model.isAlive && playerHeroController.model.isAlive)
            {
                if (!FieldManager.instance.CheckCanAttackHero(canAttackFieldEnemyCard ,playerHeroController)) { continue; }

                StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerHeroController.transform));
                yield return new WaitForSeconds(1f);
                AttackTohero(canAttackFieldEnemyCard);
                yield return null;
            }
        }
        
        if(GetHeroMP(false) > 0 && enemyTensionController.model.tension < 3)
        {
            yield return new WaitForSeconds(0.25f);
            enemyTensionController.UseTensionCard();
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ChangeTurn());
    }
    #endregion
    #region �s��
    /// <summary>
    /// ��D�ɏo�����Ƃ�ΐ푊��ɑ��M
    /// </summary>
    /// <param name="fieldID"></param>
    /// <param name="targets"></param>
    public void SendMoveToField(int fieldID, int siblingIndex, int[] targetsByReceiver = null)
    {
        photonView.RPC(nameof(RPCMoveField), RpcTarget.Others, fieldID, siblingIndex, targetsByReceiver);
    }
    [PunRPC]
    void RPCMoveField(int fieldID, int handIndex, int[] targetsByReceiver = null)
    {
        StartCoroutine(GameManager.instance.RPCMoveToField(handIndex, fieldID, targetsByReceiver));
    }
    public IEnumerator RPCMoveToField(int handIndex, int fieldID, int[] targetsByReceiver = null)
    {
        var cc = enemyHandTransform.GetChild(handIndex).GetComponent<CardController>();
        StartCoroutine(cc.movement.MoveToArea(enemyFields[fieldID - 1]));
        yield return new WaitForSeconds(0.25f);

        //�q�[���[��Ώۂɂ��Ă���Ƃ�
        if (targetsByReceiver != null && targetsByReceiver[0] is var i && (i == 13 || i == 14))
        {
            cc.SummonOnField(fieldID + 6, hcTarget : i == 13 ? playerHeroController : enemyHeroController);
        }
        else //���j�b�g��Ώۂɂ��Ă��邩�A�Ώۂ�����Ă��Ȃ���
        {
            //PlayerField�Ƃ��ē��͂���Ă��Ă���@����āA+6���Ă���EnemyField�ɂȂ�
            cc.SummonOnField(fieldID + 6, targetsByReceiver == null ? null : FieldManager.instance.GetUnitsByFieldID(targetsByReceiver).ToArray());
        }
        yield return new WaitForSeconds(0.75f);
    }
    /// <summary>
    /// �J�[�h���m�̃o�g������
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public IEnumerator CardsBattle(CardController attacker, CardController target)
    {
        //�J��̋V
        attacker.Attack(target,true);
        SkillManager.instance.ExecutePierce(attacker, target);
        target.Attack(attacker, false);

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(attacker.CheckAlive()); //�����Ő����Ă邩�𔻒f
        StartCoroutine(target.CheckAlive());
    }
    /// <summary>
    /// �U������ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="attackerFieldID"></param>
    /// <param name="targetFieldID"></param>
    public void SendCardBattle(int attackerFieldID, int targetFieldID)
    {
        photonView.RPC(nameof(RPCCardsBattle), RpcTarget.Others, attackerFieldID, targetFieldID);
    }
    [PunRPC]
    IEnumerator RPCCardsBattle(int attackerFieldID, int targetFieldID)
    {
        //attackerFieldID��1�`6, targetFieldID��7�`12�œn�����
        //��M�҂��猩��ƁAattacker�͓G�ł���Atarget�͖����ł���@
        //targetFieldID��-6���āA�����̃t�B�[���h�ɕϊ�����K�v������
        //Fields[]��0�`5 FieldID��1�`6 ����āAFieldID����Field���擾����ɂ�-1����K�v������

        //�G��attacker���擾���āA������target�܂ňړ����o���N����
        CardController attacker = enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>();
        CardController target = playerFields[targetFieldID - 6 - 1].GetChild(0).GetComponent<CardController>();

        StartCoroutine(attacker.movement.MoveToTarget(playerFields[targetFieldID - 6 - 1]));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(CardsBattle( attacker, target ));
    }
    /// <summary>
    /// �q�[���[�ւ̍U��
    /// </summary>
    /// <param name="attacker"></param>
    public void AttackTohero(CardController attacker)
    {
        //�������j�b�g�������q�[���[���U�����邱�Ƃ͂Ȃ��̂�
        if (attacker.model.isPlayerCard) {
            attacker.Attack(enemyHeroController, true);
            //������������Ă���
            CheckIsAlive(false);
        }
        else {
            attacker.Attack(playerHeroController, true);
            //������������Ă���
            CheckIsAlive(true);
        }
    }
    /// <summary>
    /// �q�[���[�ւ̍U����ΐ푊��ɑ��M����
    /// </summary>
    /// <param name="attackerFieldID"></param>
    public void SendAttackToHero(int attackerFieldID)
    {
        photonView.RPC(nameof(RPCAttackToHero), RpcTarget.Others, attackerFieldID);
    }
    [PunRPC]
    IEnumerator RPCAttackToHero(int attackerFieldID)
    {
        StartCoroutine(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>().movement.MoveToTarget(playerHeroController.transform));
        yield return new WaitForSeconds(0.6f);
        AttackTohero(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>());
    }
    public void CheckIsAlive(bool isPlayer)
    {
        if (isPlayer)
        {
            if (!playerHeroController.model.isAlive)
            {
                Invoke("ViewResultPanel", 1f);
            }
        }
        else
        {
            if (!enemyHeroController.model.isAlive)
            {
                Invoke("ViewResultPanel", 1f);
            }
        }
    }
    #endregion
    #region �q�[���[����
    /// <summary>
    /// �q�[���[��MP���擾����
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public int GetHeroMP(bool isPlayer)
    {
        return isPlayer ? playerHeroController.model.mp : enemyHeroController.model.mp;
    }
    /// <summary>
    /// �q�[���[��MP�����炷
    /// </summary>
    /// <param name="reduce"></param>
    /// <param name="isPlayerCard"></param>
    public void ReduceMP(int reduce, bool isPlayerCard) {
        if (isPlayerCard)
        {
            playerHeroController.ReduceMP(reduce);
        }
        else
        {
            enemyHeroController.ReduceMP(reduce);
        }
        if (isPlayerTurn)
        {
            SetCanSummonHandCards();
        }
        SetCanUsetension(isPlayerCard);
    }
    /// <summary>
    /// �e���V�����J�[�h�y�уe���V�����X�y�����g�p�\�����f���A�\����ύX����
    /// </summary>
    /// <param name="setCanNotSummon"></param>
    public void SetCanUsetension(bool isPlayerCard, bool canNotUsetension = false)
    {
        if (!isPlayerCard) { return; }
        else if (canNotUsetension) { playerTensionController.SetCanUseTension(false); }
        else if (playerTensionController.model.tension == 3)
        {
            playerTensionController.SetCanUseTension(true);
        }
        else if (playerTensionController.model.isTensionUsedThisTurn || GetHeroMP(true) < 1)
        {
            playerTensionController.SetCanUseTension(false);
        }
        else
        {
            playerTensionController.SetCanUseTension(true);
        }
    }
    /// <summary>
    /// �e���V�����J�[�h�̎g�p��ΐ푊��ɑ��M���� 
    /// </summary>
    /// <param name="attackerFieldID"></param>
    public void SendUseTensionCard()
    {
        photonView.RPC(nameof(RPCSendUseTensionCard), RpcTarget.Others);
    }
    [PunRPC]
    void RPCSendUseTensionCard()
    {
        enemyTensionController.UseTensionCard();
    }
    /// <summary>
    /// �e���V�����X�y���̎g�p��ΐ푊��ɑ��M���� 0�͌��ʑI���Ȃ��̃e���V�����X�L�� 1�`12�̓t�B�[���h�̃��j�b�g 13�͖����q�[���[ 14�͓G�q�[���[���Ώہ@��M�Җڐ���ID�ɕϊ��ς�
    /// </summary>
    /// <param name="attackerFieldID"></param>
    public void SendUseTensionSpell(int targetFieldIDByReceiver)
    {
        photonView.RPC(nameof(RPCUseTensionSpell), RpcTarget.Others, targetFieldIDByReceiver);
    }
    [PunRPC]
    void RPCUseTensionSpell(int targetFieldIDByReceiver)
    {
        if(targetFieldIDByReceiver == 0)
        {
            enemyTensionController.UseTensionSpell<Controller>(null);
        }
        else if(1 <= targetFieldIDByReceiver && targetFieldIDByReceiver <= 12)
        {
            enemyTensionController.UseTensionSpell(FieldManager.instance.GetUnitByFieldID(targetFieldIDByReceiver));
        }
        if(targetFieldIDByReceiver == 13)
        {
            enemyTensionController.UseTensionSpell(playerHeroController);
        }
        else if(targetFieldIDByReceiver == 14)
        {
            enemyTensionController.UseTensionSpell(enemyHeroController);
        }
    }
    public void SendExecuteSpellContents(int handIndex, int targetByReciever)
    {
        photonView.RPC(nameof(RPCExecuteSpellContents), RpcTarget.Others, handIndex, targetByReciever);
    }
    [PunRPC]
    IEnumerator RPCExecuteSpellContents(int handIndex, int targetByReciever)
    {
        var spell = enemyHandTransform.GetChild(handIndex).GetComponent<CardController>();
        spell.Show(true);
        if (targetByReciever == 0) {
            StartCoroutine(spell.movement.MoveToArea(canvas));
            yield return new WaitForSeconds(0.25f);
            spell.ExecuteSpellContents<Controller>(null); 
        }
        else if(1 <= targetByReciever && targetByReciever <= 12) {
            var target = FieldManager.instance.GetUnitByFieldID(targetByReciever);
            StartCoroutine(spell.movement.MoveToArea(targetByReciever <= 6 ? playerFields[targetByReciever - 1] : enemyFields[targetByReciever - 7]));
            yield return new WaitForSeconds(0.25f);
            spell.ExecuteSpellContents(target); 
        }
        else if(targetByReciever == 13 || targetByReciever == 14){
            StartCoroutine(spell.movement.MoveToArea(targetByReciever == 13 ? playerHeroController.transform : enemyHeroController.transform));
            spell.ExecuteSpellContents(targetByReciever == 13 ? playerHeroController : enemyHeroController); 
        }
    }
    #endregion
    #region result�ƃ��j���[�ւ̑J��
    /// <summary>
    /// ������߂�
    /// </summary>
    /// <param name="isPlayerConcede"></param>
    public void Concede(bool isPlayerConcede)
    {
        if (isPlayerConcede) { playerHeroController.Concede(); }
        else { enemyHeroController.Concede(); }
        ViewResultPanel();
    }
    /// <summary>
    /// ������߂邱�Ƃ�ΐ푊��ɑ��M����
    /// </summary>
    public void SendConcede()
    {
        photonView.RPC(nameof(RPCSendConcede), RpcTarget.Others);
    }
    [PunRPC]
    public void RPCSendConcede()
    {
        Concede(false); //���肪�R���V�������ɌĂ΂��̂�false�Œ�
    }
    [SerializeField]
    private AudioSource audioSourceBGM; //���ʂ����܂�����BGM����������
    private void ViewResultPanel()
    {
        if (resultPanel.activeSelf)
        {
            return;
        }
       audioSourceBGM.Stop();
        resultPanel.SetActive(true);
        resultPanel.transform.SetSiblingIndex(99); //�őO�ʂɕ\��
        if (playerHeroController.model.isAlive && !enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundWin();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/win"); //���Ȃ��̏���
        }
        else if (!playerHeroController.model.isAlive && enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/lose"); //���Ȃ��̔s�k
        }
        else if (!playerHeroController.model.isAlive && !enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/draw"); //���Ҕs�k
        }
        StopAllCoroutines();
        Invoke("ChangeMenuScene", 3);
    }
    private void ChangeMenuScene()
    {
        if (PhotonNetwork.IsConnected) { PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect(); }
        SceneManager.LoadScene("MenuScene");
    }
    /// <summary>
    /// �ؒf�΍�
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        if (PhotonNetwork.IsConnected)
        {
            Concede(false);
            PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect();
        }
    }
    #endregion
}
