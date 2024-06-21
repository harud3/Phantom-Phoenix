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
    [SerializeField] private Sprite playerTurnSprite,enemyTurnSprite; //ターン終了、相手のターンのスプライト

    [SerializeField] private TextMeshProUGUI timeCountText; //ターンの残り時間の表示部
    [SerializeField] int timeLimit; //ターンの時間制限
    int timeCount; //ターンの残り時間

    private bool _isPlayerTurn;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;

    DeckModel playerDeck = null;
    DeckModel enemyDeck = null;
    #region 初期設定
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
    //ヒーローの設定
    void SettingInitHero()
    {
        playerHeroController.Init(playerDeck.useHeroID);
        enemyHeroController.Init(enemyDeck.useHeroID);
    }
    /// <summary>
    /// 初期手札の設定
    /// </summary>
    void SettingInitHand()
    {
        GiveCard(true, 3);
        GiveCard(false, 3);
    }
    #endregion
    #region　時間管理
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
    #region カード生成
    /// <summary>
    /// 外部からの呼び出し用
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
    /// デッキから手札にカードを配る
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    private void GiveCardToHand(List<int> deck,Transform hand, bool isPlayer)
    {
        if (deck.Count == 0) //TODO:デッキがないなら戻る　ドローごとに1ダメージ→2ダメージ→3ダメージ...にする
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        if (isPlayer) { playerHeroController.ReShowStackCards(deck.Count()); } 
        else { enemyHeroController.ReShowStackCards(deck.Count()); }
        if (hand.childCount >= 10) { Debug.Log($"カードID{cardID}のカードは燃えました"); return; }
        StartCoroutine(CreateCard(cardID, hand, isPlayer));
    }
    /// <summary>
    /// カードの生成
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

    #region　ターン制御
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
            Debug.Log("味方ターン");
            playerHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, true, true);
            SetCanAttackAllFieldUnit(enemyFields, false, false);
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("相手ターン");
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
    #region より具体的なターン制御
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
        cc.MoveField(fieldID + 6); //PlayerFieldとして入力されてきている　よって、+6してやればEnemyFieldになる
        cc.Show(true);
        cc.putOnField(false, targets == null ? null : SkillManager.instance.GetCardsByFieldID(targets).ToArray());
        yield return new WaitForSeconds(0.75f);
    }
    IEnumerator AIEnemyTurn()
    {

        yield return new WaitForSeconds(1f);

        //enemyAIが動かす
        //手札から出せるカードを出す
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

        //fieldの攻撃可能ユニットで攻撃する 味方ユニットがいるならユニットを　味方ユニットがいないならヒーローを攻撃
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
    #region 行動
    public void CardsBattle(CardController attacker, CardController target)
    {

        Debug.Log($"{(isPlayerTurn? "味方ターン" :"相手ターン")} {attacker.model.name}から{target.model.name}に攻撃");
        
        attacker.Attack(target,true);
        target.Attack(attacker, false);

        Debug.Log($"{target.model.name}に{attacker.model.atk}ダメージ {target.model.name}の残りHP{target.model.hp}");
        Debug.Log($"{attacker.model.name}に{target.model.atk}ダメージ {attacker.model.name}の残りHP{attacker.model.hp}");

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
    /// ワールドなfieldIDであるfieldID1～12を、ローカルなfieldIDであるfieldID1～6に変換する
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    private int ChangeWorldFieldIDToLocalFieldID(int fieldID)
    {
        if (1 <= fieldID && fieldID <= 6){ return fieldID; }
        else if (fieldID <= 12){ return fieldID - 6; }
        return 0;//来ないとは思う
    }
    public void AttackTohero(CardController attacker)
    {
        if (attacker.model.isPlayerCard) {
            attacker.Attack(enemyHeroController, true);
            //勝利判定も
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            attacker.Attack(playerHeroController, true);
            //勝利判定も
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
    #region MP操作
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
    #region resultとメニューへの遷移
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
        Concede(false); //相手がコンシした時に呼ばれるのでfalse固定
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
