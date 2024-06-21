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
    [SerializeField] private HeroController playerHeroController, enemyHeroController;

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private Transform playerHandTransform, enemyHandTransform;
    
    [SerializeField] private CardController cardPrefab;

    [SerializeField] private UnityEngine.UI.Button ButtonTurn;
    [SerializeField] private GameObject ButtonTurnGuard;
    [SerializeField] private Sprite playerTurnSprite,enemyTurnSprite; //�^�[���I���A����̃^�[���̃X�v���C�g

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
    void Start()
    {
       StartGame();
    }
    bool isWaitBegin = true;
    private void Update()
    {
        if (GameDataManager.instance.isOnlineBattle && isWaitBegin && enemyDeck != null)
        {
            isWaitBegin = false;
            SetTime();
            SettingInitHero();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
    }
    void StartGame()
    {
        playerDeck = new DeckModel().Init();
        if (GameDataManager.instance.isOnlineBattle)
        {
            if (GameDataManager.instance.isMaster)
            {
                isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
                SendSetIsPlayerTurn(isPlayerTurn);

                int seed = int.Parse(DateTime.Now.ToString("ddHHmmss"));
                UnityEngine.Random.InitState(seed);
                SendSetSeed(seed);
            }
            SendSetEnemyDeck(playerDeck);
        }
        else
        {
            enemyDeck = new DeckModel().Init();
            isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
            SetTime();
            SettingInitHero();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
    }
    public void SendSetSeed(int seed)
    {
        photonView.RPC(nameof(PSetSeed), RpcTarget.Others, seed);
    }
    [PunRPC]
    void PSetSeed(int seed)
    {
        UnityEngine.Random.InitState(seed);
    }
    public void SendSetIsPlayerTurn(bool isPlayerTurn)
    {
        photonView.RPC(nameof(PSetIsPlayerTurn), RpcTarget.Others, isPlayerTurn);
    }
    [PunRPC]
    void PSetIsPlayerTurn(bool isPlayerTurn)
    {
        this.isPlayerTurn = !isPlayerTurn;
    }
    public void SendSetEnemyDeck(DeckModel playerDeck)
    {
        photonView.RPC(nameof(PSetEnemyDeck), RpcTarget.Others, playerDeck.useHeroID, playerDeck.deck.ToArray());
    }
    [PunRPC]
    void PSetEnemyDeck(int useheroID, int[] deckIDs)
    {
        enemyDeck = new DeckModel().Init(useheroID, deckIDs);
    }
    //�q�[���[�̐ݒ�
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
        GiveCard(true, 3);
        GiveCard(false, 3);
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
    /// �O������̌Ăяo���p
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="drawCount"></param>
    public void GiveCard(bool isPlayer, int drawCount)
    {
        StartCoroutine(_GiveCard(isPlayer, drawCount));
    }
    private IEnumerator _GiveCard(bool isPlayer, int drawCount)
    {
        if (isPlayer)
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCardToHand(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
                yield return new WaitForSeconds(0.25f);
            }
        }

        else
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCardToHand(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
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
    private void GiveCardToHand(List<int> deck,Transform hand, bool isPlayer)
    {
        if (deck.Count == 0) //TODO:�f�b�L���Ȃ��Ȃ�߂�@�h���[���Ƃ�1�_���[�W��2�_���[�W��3�_���[�W...�ɂ���
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        if (isPlayer) { playerHeroController.ReShowStackCards(deck.Count()); } 
        else { enemyHeroController.ReShowStackCards(deck.Count()); }
        if (hand.childCount >= 10) { Debug.Log($"�J�[�hID{cardID}�̃J�[�h�͔R���܂���"); return; }
        StartCoroutine(CreateCard(cardID, hand, isPlayer));
    }
    /// <summary>
    /// �J�[�h�̐���
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    IEnumerator CreateCard(int cardID, Transform hand, bool isPlayer)
    {

        CardController x = Instantiate(cardPrefab, canvas);
        x.transform.Translate(new Vector3(isPlayer ? -550 : 550, 0, 0), Space.Self);
        x.GetComponent<CardController>().Init(cardID, isPlayer);
        x.transform.DOLocalMove(new Vector3(isPlayer ? -100 : 100, 0, 0), 0.25f);
        yield return new WaitForSeconds(0.25f);
        x.transform.DOMove(hand.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        x.transform.SetParent(hand);
    }
    #endregion

    #region�@�^�[������
    public void SendChangeTurn()
    {
        photonView.RPC(nameof(PChangeTurn), RpcTarget.Others);
    }
    [PunRPC]
    public void PChangeTurn()
    {
        StartCoroutine(ChangeTurn());
    }
    public IEnumerator ChangeTurn(bool isFirst = false)
    {
        if (isFirst)
        {
            yield return new WaitForSeconds(1.6f);
        }
        else
        {
            AudioManager.instance.SoundButtonClick2();
        }

        int unitCount = 0;
        if (isPlayerTurn)
        {
            unitCount =  playerFields.Concat(enemyFields).Count(i => i.childCount != 0);
            foreach (var i in playerFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList())
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.2f);
            }
            foreach (var i in enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList())
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            unitCount = playerFields.Concat(enemyFields).Count(i => i.childCount != 0);
            foreach (var i in enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList())
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.2f);
            }
            foreach (var i in playerFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList())
            {
                i.ExecuteSpecialSkillEndTurn(isPlayerTurn);
                yield return new WaitForSeconds(0.2f);
            }   
        }

        ButtonTurnGuard.gameObject.SetActive(true);
        if (!isFirst)
        {
            isPlayerTurn = !isPlayerTurn;
            yield return new WaitForSeconds(unitCount * 0.2f);
        }

        
        
        if (isPlayerTurn && !isFirst)
        {

            GiveCardToHand(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
        }
        else if(!isFirst)
        {
            GiveCardToHand(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
        }

        if (isPlayerTurn)
        {
            ButtonTurn.image.sprite = playerTurnSprite;
            Debug.Log("�����^�[��");
            playerHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, true, true);
            SetCanAttackAllFieldUnit(enemyFields, false, false);
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("����^�[��");
            enemyHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, false, false);
            SetCanAttackAllFieldUnit(enemyFields, true, true);
        }

        yield return new WaitForSeconds(0.6f);

        ButtonTurnGuard.gameObject.SetActive(false);

        TurnCalc();
    }
    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            if (!GameDataManager.instance.isOnlineBattle)
            {
                StartCoroutine(AIEnemyTurn());
            }
            else
            {
                EnemyTurn();
            }
        }
    }
    #endregion
    #region ����̓I�ȃ^�[������
    void SetCanAttackAllFieldUnit(Transform[] fields, bool CanAttack, bool ResetIsActiveDoubleAciton)
    {
        foreach (var field in fields)
        {
            if (field.childCount != 0)
            {
                field.GetChild(0).GetComponent<CardController>().SetCanAttack(CanAttack, ResetIsActiveDoubleAciton);
            }
        }
    }
    void PlayerTurn()
    {
    }
    void EnemyTurn()
    {
    }
    public IEnumerator MoveToField(int handIndex, int fieldID, int[] targets = null)
    {
        var cc = enemyHandTransform.GetChild(handIndex).GetComponent<CardController>();
        StartCoroutine(cc.movement.MoveToField(enemyFields[fieldID - 1]));
        yield return new WaitForSeconds(0.25f);
        cc.MoveField(fieldID + 6); //PlayerField�Ƃ��ē��͂���Ă��Ă���@����āA+6���Ă���EnemyField�ɂȂ�
        cc.Show(true);
        cc.putOnField(false, targets == null ? null : SkillManager.instance.GetCardsByFieldID(targets).ToArray());
        yield return new WaitForSeconds(0.75f);
    }
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
                        StartCoroutine(canPutCard.movement.MoveToField(enemyField));
                        yield return new WaitForSeconds(0.25f);
                        canPutCard.MoveField(enemyField.GetComponent<DropField>().fieldID);
                        canPutCard.putOnField(false);
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
                    if (playerField.GetComponentInChildren<CardController>() is var pcc && pcc.model.isAlive)
                    {
                        if (!SkillManager.instance.CheckCanAttackUnit(canAttackFieldEnemyCard, pcc)) { continue; }

                        StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerField));
                        yield return new WaitForSeconds(1f);
                        CardsBattle(canAttackFieldEnemyCard, pcc);
                        SkillManager.instance.ExecutePierce(canAttackFieldEnemyCard, pcc);
                        yield return null;
                        break;
                    }

                }
            }
        }
        canAttackFieldEnemyCards
            = enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.canAttack);
        foreach (var canAttackFieldEnemyCard in canAttackFieldEnemyCards)
        { 
            if (canAttackFieldEnemyCard.model.canAttack && playerHeroController.model.isAlive)
            {
                if (!SkillManager.instance.CheckCanAttackHero(canAttackFieldEnemyCard ,playerHeroController)) { continue; }

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
    public void CardsBattle(CardController attacker, CardController target)
    {

        Debug.Log($"{(isPlayerTurn? "�����^�[��" :"����^�[��")} {attacker.model.name}����{target.model.name}�ɍU��");
        
        attacker.Attack(target,true);
        target.Attack(attacker, false);

        Debug.Log($"{target.model.name}��{attacker.model.atk}�_���[�W {target.model.name}�̎c��HP{target.model.hp}");
        Debug.Log($"{attacker.model.name}��{target.model.atk}�_���[�W {attacker.model.name}�̎c��HP{attacker.model.hp}");

        StartCoroutine(attacker.CheckAlive());
        StartCoroutine(target.CheckAlive());
    }
    public void SendCardBattle(int attackerFieldID, int targetFieldID)
    {
        photonView.RPC(nameof(PCardsBattle), RpcTarget.Others, attackerFieldID, targetFieldID);
    }
    [PunRPC]
    IEnumerator PCardsBattle(int attackerFieldID, int targetFieldID)
    {

        StartCoroutine(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>().movement.MoveToTarget(playerFields[ChangeWorldFieldIDToLocalFieldID(targetFieldID) - 1]));
        yield return new WaitForSeconds(1f);
        CardsBattle(
            enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>(),
            playerFields[ChangeWorldFieldIDToLocalFieldID(targetFieldID) - 1].GetChild(0).GetComponent<CardController>()
            );
    }
    /// <summary>
    /// ���[���h��fieldID�ł���fieldID1�`12���A���[�J����fieldID�ł���fieldID1�`6�ɕϊ�����
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    private int ChangeWorldFieldIDToLocalFieldID(int fieldID)
    {
        if (1 <= fieldID && fieldID <= 6){ return fieldID; }
        else if (fieldID <= 12){ return fieldID - 6; }
        return 0;//���Ȃ��Ƃ͎v��
    }
    public void AttackTohero(CardController attacker)
    {
        if (attacker.model.isPlayerCard) {
            attacker.Attack(enemyHeroController, true);
            //���������
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            attacker.Attack(playerHeroController, true);
            //���������
            if (!playerHeroController.model.isAlive) {
                Invoke("ViewResultPanel", 1f);
            }
        }
    }
    public void SendAttackToHero(int attackerFieldID)
    {
        photonView.RPC(nameof(PAttackToHero), RpcTarget.Others, attackerFieldID);
    }
    [PunRPC]
    IEnumerator PAttackToHero(int attackerFieldID)
    {
        StartCoroutine(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>().movement.MoveToTarget(playerHeroController.transform));
        yield return new WaitForSeconds(1f);
        AttackTohero(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>());
    }
    #endregion
    #region MP����
    public int GetHeroMP(bool isPlayer)
    {
        return isPlayer ? playerHeroController.model.mp : enemyHeroController.model.mp;
    }
    public void ReduceMP(int reduce, bool isPlayerCard) {
        if (isPlayerCard)
        {
            playerHeroController.ReduceMP(reduce);
        }
        else
        {
            enemyHeroController.ReduceMP(reduce);
        }
    }

    #endregion
    #region result�ƃ��j���[�ւ̑J��
    public void Concede(bool isPlayerConcede)
    {
        if (isPlayerConcede) { playerHeroController.model.isConcede(); }
        else { enemyHeroController.model.isConcede(); }
        ViewResultPanel();
    }
    public void SendConcede()
    {
        photonView.RPC(nameof(PSendConcede), RpcTarget.Others);
    }
    [PunRPC]
    public void PSendConcede()
    {
        Concede(false); //���肪�R���V�������ɌĂ΂��̂�false�Œ�
    }
    [SerializeField]
    private AudioSource audioSourceBGM;
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
            resultImage.sprite = Resources.Load<Sprite>($"UIs/win");
        }
        else if (!playerHeroController.model.isAlive && enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/lose");
        }
        else if (!playerHeroController.model.isAlive && !enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/draw");
        }
        Invoke("ChangeMenuScene", 3);
    }
    private void ChangeMenuScene()
    {
        StopAllCoroutines();
        if (PhotonNetwork.IsConnected) { PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect(); }
        SceneManager.LoadScene("MenuScene");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        if (PhotonNetwork.IsConnected)
        {
            Concede(false);
            PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect();
        }
    }
    #endregion
    public void SendMoveField(int fieldID, int siblingIndex, int[] targets = null)
    {
        photonView.RPC(nameof(MoveField), RpcTarget.Others, fieldID, siblingIndex, targets);
    }
    [PunRPC]
    void MoveField(int fieldID, int handIndex, int[] targets = null)
    {
        StartCoroutine(GameManager.instance.MoveToField(handIndex, fieldID, targets));
    }
}
