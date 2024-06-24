
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

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6]; //�����t�B�[���h�A�G�t�B�[���h
    [SerializeField] private Transform playerHandTransform, enemyHandTransform; //������D�A�G��D
    [SerializeField] private Transform playerSelectionTransform; //�I���t�B�[���h
    [SerializeField] private GameObject HintMessage;

    [SerializeField] private CardController cardPrefab; //�J�[�h

    [SerializeField] private UnityEngine.UI.Button ButtonTurn; //�^�[���ύX�{�^��
    [SerializeField] private GameObject ButtonTurnGuard; //�^�[���ύX�{�^���A�ł�΍�
    [SerializeField] private Sprite playerTurnSprite, enemyTurnSprite; //�^�[���I���A����̃^�[���̃X�v���C�g

    [SerializeField] private TextMeshProUGUI timeCountText; //�^�[���̎c�莞�Ԃ̕\����
    [SerializeField] int timeLimit; //�^�[���̎��Ԑ���
    int timeCount; //�^�[���̎c�莞��

    private bool _isPlayerTurn;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;

    DeckModel playerDeck = null;
    DeckModel enemyDeck = null;
    #region �����ݒ�
    /// <summary>
    /// �Q�[���̏�� �ʐM�ΐ�ɂ����āA�ŏ��ɑ��݃f�[�^�ʐM(�f�b�L�A�V�[�h�l)������̂ŊǗ��K�{
    /// </summary>
    private enum eGameState
    {
        isBigin,
        isGotPlayerTurn,
        isWaitMulligan,
        isProcessMulligan,
        isWaitStart,
        isStarted,
    }
    eGameState gameState = eGameState.isBigin;
    void Start()
    {
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
        else //�I�����C���ΐ�ł͂Ȃ�==AI��
        {
            enemyDeck = new DeckModel().Init();
            isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
            gameState = eGameState.isGotPlayerTurn;
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
        gameState = eGameState.isGotPlayerTurn;
    }
    private void Update()
    {
        //�I�����C���ΐ�ł́A����̃f�b�L���Ȃ���Ύn�߂��Ȃ��̂ŁA�����ŊJ�n���� ����ɔ����AAI��������ŊJ�n����
        if (gameState == eGameState.isGotPlayerTurn)
        {
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
            SettingInitHero();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
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

        HintMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"����̃}���K����҂��Ă��܂�";

        //�}���K������
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
        enemyHeroController.Init(enemyDeck.useHeroID);
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
        ButtonTurnGuard.gameObject.SetActive(true);

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
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("����^�[��");
            enemyHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, false);
            SetCanAttackAllFieldUnit(enemyFields, true, true); //�A�����̕������s��
        }

        yield return new WaitForSeconds(0.6f); //�h���[�҂�����

        ButtonTurnGuard.gameObject.SetActive(false); //�^�[���{�^��������悤��

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
                        StartCoroutine(canPutCard.movement.MoveToArea(enemyField));
                        yield return new WaitForSeconds(0.25f);
                        canPutCard.SummonOnField(false, enemyField.GetComponent<DropField>().fieldID);
                        canPutCard.Show(true);
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
        //PlayerField�Ƃ��ē��͂���Ă��Ă���@����āA+6���Ă���EnemyField�ɂȂ�
        cc.SummonOnField(false, fieldID + 6, targetsByReceiver == null ? null : FieldManager.instance.GetUnitsByFieldID(targetsByReceiver).ToArray());
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
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            attacker.Attack(playerHeroController, true);
            //������������Ă���
            if (!playerHeroController.model.isAlive) {
                Invoke("ViewResultPanel", 1f);
            }
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
    #endregion
    #region MP����
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
        SetCanSummonHandCards();
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
