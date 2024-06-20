using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using static UnityEngine.Rendering.DebugUI;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using Photon.Pun;
using Photon.Realtime;
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

    [SerializeField] private HeroController playerHeroController, enemyHeroController;

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private Transform playerHandTransform, enemyHandTransform;
    
    [SerializeField] private CardController cardPrefab;

    [SerializeField] private UnityEngine.UI.Button turnButton;
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
            TurnCalc();
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
            TurnCalc();
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
        for (int i = 0; i < 3; i++) //������D��3��
        {
            GiveCardToHand(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
            GiveCardToHand(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
        }
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
        ChangeTurn();
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
        if (isPlayer)
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCardToHand(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
            }
        }

        else
        {
            for (int i = 0; i < drawCount; i++)
            {
                GiveCardToHand(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
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
        CreateCard(cardID, hand, isPlayer);
    }
    /// <summary>
    /// �J�[�h�̐���
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    void CreateCard(int cardID, Transform hand, bool isPlayer)
    {
        if (hand.childCount >= 10) { Debug.Log($"�J�[�hID{cardID}�̃J�[�h�͔R���܂���");  return; }
        //CardController card = Instantiate(cardPrefab, hand, false);
        if (GameDataManager.instance.isOnlineBattle)
        {
            GameObject x = PhotonNetwork.Instantiate("Card", new Vector3(0, 0, 0), Quaternion.identity);
            x.transform.SetParent(hand);
            CardController card = x.GetComponent<CardController>();
            card.Init(cardID, isPlayer);
        }
        else
        {
            Instantiate(cardPrefab, hand).GetComponent<CardController>().Init(cardID, isPlayer);
        }
        
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
        ChangeTurn();
    }
    public void ChangeTurn()
    {
        if (isPlayerTurn) {
            playerFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList().ForEach(i => { i.ExecuteSpecialSkillEndTurn(isPlayerTurn); });
            enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList().ForEach(i => { i.ExecuteSpecialSkillEndTurn(isPlayerTurn); });
        }
        else {
            enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList().ForEach(i => { i.ExecuteSpecialSkillEndTurn(isPlayerTurn); });
            playerFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).ToList().ForEach(i => { i.ExecuteSpecialSkillEndTurn(isPlayerTurn); });
        }

        isPlayerTurn = !isPlayerTurn;
        
        if (isPlayerTurn)
        {
            GiveCardToHand(playerDeck.deck, playerHandTransform, playerHeroController.model.isPlayer);
        }
        else
        {
            GiveCardToHand(enemyDeck.deck, enemyHandTransform, enemyHeroController.model.isPlayer);
        }
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
        turnButton.image.sprite = playerTurnSprite;
        Debug.Log("�����^�[��");
        playerHeroController.ResetMP();
        SetCanAttackAllFieldUnit(playerFields, true, true);
        SetCanAttackAllFieldUnit(enemyFields, false, false);
    }
    void EnemyTurn()
    {
        turnButton.image.sprite = enemyTurnSprite;
        Debug.Log("����^�[��");
        enemyHeroController.ResetMP();
        SetCanAttackAllFieldUnit(playerFields, false, false);
        SetCanAttackAllFieldUnit(enemyFields, true, true);
    }
    public IEnumerator MoveToField(int handIndex, int fieldID)
    {
        var cc = enemyHandTransform.GetChild(handIndex).GetComponent<CardController>();
        StartCoroutine(cc.movement.MoveToField(enemyFields[fieldID - 1]));
        yield return new WaitForSeconds(0.25f);
        cc.MoveField(fieldID + 6); //PlayerField�Ƃ��ē��͂���Ă��Ă���@����āA+6���Ă���EnemyField�ɂȂ�
        cc.Show(true);
        cc.putOnField(false);
        yield return new WaitForSeconds(0.75f);
    }
    IEnumerator AIEnemyTurn()
    {
        
        turnButton.image.sprite = enemyTurnSprite;
        Debug.Log("����^�[��");
        enemyHeroController.ResetMP();
        SetCanAttackAllFieldUnit(playerFields, false, false);
        SetCanAttackAllFieldUnit(enemyFields, true, true);

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
                        yield return new WaitForSeconds(1.01f);
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

        yield return new WaitForSeconds(1f);
        
        ChangeTurn();
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
    private void ViewResultPanel()
    {
        resultPanel.SetActive(true);
        if (playerHeroController.model.isAlive && !enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/win"); }
        else if (!playerHeroController.model.isAlive && enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/lose"); }
        else if (!playerHeroController.model.isAlive && !enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/draw"); }
        StopAllCoroutines();
        Invoke("ChangeMenuScene", 5);
    }
    private void ChangeMenuScene()
    {
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
}
