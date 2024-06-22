
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
/// <summary>
/// バトルを統括するスクリプト
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
        //オンライン対戦では、相手のデッキがなければ始められないので、ここで開始する
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
            if (GameDataManager.instance.isMaster) //部屋主が音頭を取る
            {
                isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
                SendSetIsPlayerTurn(isPlayerTurn);

                int seed = int.Parse(DateTime.Now.ToString("ddHHmmss")); //ランダム要素をプレイヤー間で揃えるため、シード値を共有することで対応する
                UnityEngine.Random.InitState(seed);
                SendSetSeed(seed);
            }
            SendSetEnemyDeck(playerDeck);
        }
        else //オンライン対戦ではない==AI戦なので、ここで開始する
        {
            enemyDeck = new DeckModel().Init();
            isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
            SetTime();
            SettingInitHero();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
    }
    /// <summary>
    /// シード値を対戦相手に送信する
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
    /// どちらから開始するのかを対戦相手に送信する
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
    }
    /// <summary>
    /// 自身のデッキを対戦相手に送信する
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
    /// ヒーローの設定
    /// </summary>
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
        GivesCard(true, 3);
        GivesCard(false, 3);
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
    /// 手札にカードを加える
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
    /// デッキから手札にカードを配る
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    private void GiveCard(List<int> deck,Transform hand, bool isPlayer)
    {
        if (deck.Count == 0) //TODO:デッキがないなら戻る　ドローごとに1ダメージ→2ダメージ→3ダメージ...にする
        {
            return;
        }
        //一番上のデッキを取得する
        int cardID = deck[0];
        deck.RemoveAt(0);
        //デッキ残り枚数の再表示
        if (isPlayer) { playerHeroController.ReShowStackCards(deck.Count()); } 
        else { enemyHeroController.ReShowStackCards(deck.Count()); }

        //手札の枚数は最大10枚
        if (hand.childCount >= 10) { Debug.Log($"カードID{cardID}のカードは燃えました"); return; }
        StartCoroutine(CreateCard(cardID, hand, isPlayer));
    }
    /// <summary>
    /// カードの生成と手札への移動
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
    /// <summary>
    /// ターン変更を対戦相手に送信する
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
        if (isFirst) //ゲーム開始時はドロー処理があるので待つ
        {
            yield return new WaitForSeconds(1.6f);
        }
        else
        {
            AudioManager.instance.SoundButtonClick2(); //ButtonTurnが押された音
        }

        //ターン終了時処理
        List<CardController> playerCardsHasSSED = FieldManager.instance.GetUnitsByFieldID(new int[] { 1, 2, 3, 4, 5, 6 }).Where(i => i.SpecialSkillEndTurn != null).ToList();
        List<CardController> enemyCardsHasSSED = FieldManager.instance.GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 }).Where(i => i.SpecialSkillEndTurn != null).ToList();
        int unitCount = playerCardsHasSSED.Concat(enemyCardsHasSSED).Count();
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

        //ターン終了時処理待ち時間
        ButtonTurnGuard.gameObject.SetActive(true);
        if (!isFirst)
        {
            isPlayerTurn = !isPlayerTurn;
            yield return new WaitForSeconds(unitCount * 0.1f);
        }

        //最初のターンはドローなしにするため
        if (isPlayerTurn && !isFirst)
        {
            GivesCard(true, 1);
        }
        else if(!isFirst)
        {
            GivesCard(false, 1);
        }

        //表示の変更
        if (isPlayerTurn)
        {
            ButtonTurn.image.sprite = playerTurnSprite;
            Debug.Log("味方ターン");
            playerHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, true, true);　//連撃権の復活も行う
            SetCanAttackAllFieldUnit(enemyFields, false);
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("相手ターン");
            enemyHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, false);
            SetCanAttackAllFieldUnit(enemyFields, true, true); //連撃権の復活も行う
        }

        yield return new WaitForSeconds(0.6f); //ドロー待ち時間

        ButtonTurnGuard.gameObject.SetActive(false); //ターンボタン押せるように

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
    /// フィールド上のユニットの攻撃権(と連撃権)の復活
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
                field.GetChild(0).GetComponent<CardController>().SetCanAttack(CanAttack, ResetIsActiveDoubleAciton);
            }
        }
    }
    #endregion
    #region AI制御

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
                        if (!FieldManager.instance.CheckCanAttackUnit(canAttackFieldEnemyCard, pcc)) { continue; }

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
    #region 行動
    /// <summary>
    /// 手札に出たことを対戦相手に送信
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
        //PlayerFieldとして入力されてきている　よって、+6してやればEnemyFieldになる
        cc.SummonOnField(false, fieldID + 6, targetsByReceiver == null ? null : FieldManager.instance.GetUnitsByFieldID(targetsByReceiver).ToArray());
        yield return new WaitForSeconds(0.75f);
    }
    /// <summary>
    /// カード同士のバトル処理
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public void CardsBattle(CardController attacker, CardController target)
    {   
        attacker.Attack(target,true);
        target.Attack(attacker, false);

        StartCoroutine(attacker.CheckAlive()); //ここで生きてるかを判断
        StartCoroutine(target.CheckAlive());
    }
    /// <summary>
    /// 攻撃情報を対戦相手に送信する
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
        //attackerFieldIDは1～6, targetFieldIDは7～12で渡される
        //受信者から見ると、attackerは敵であり、targetは味方である　
        //targetFieldIDを-6して、味方のフィールドに変換する必要がある
        //Fields[]は0～5 FieldIDは1～6 よって、FieldIDからFieldを取得するには-1する必要がある
        
        //敵のattackerを取得して、味方のtargetまで移動演出を起こす
        StartCoroutine(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>().movement.MoveToTarget(playerFields[targetFieldID - 6 - 1]));
        yield return new WaitForSeconds(0.25f);
        //開戦の儀
        CardsBattle(
            enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>(),
            playerFields[targetFieldID - 6 - 1].GetChild(0).GetComponent<CardController>()
            );
    }
    /// <summary>
    /// ヒーローへの攻撃
    /// </summary>
    /// <param name="attacker"></param>
    public void AttackTohero(CardController attacker)
    {
        //味方ユニットが味方ヒーローを攻撃することはないので
        if (attacker.model.isPlayerCard) {
            attacker.Attack(enemyHeroController, true);
            //勝利判定もしておく
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            attacker.Attack(playerHeroController, true);
            //勝利判定もしておく
            if (!playerHeroController.model.isAlive) {
                Invoke("ViewResultPanel", 1f);
            }
        }
    }
    /// <summary>
    /// ヒーローへの攻撃を対戦相手に送信する
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
        yield return new WaitForSeconds(1f);
        AttackTohero(enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>());
    }
    #endregion
    #region MP操作
    /// <summary>
    /// ヒーローのMPを取得する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public int GetHeroMP(bool isPlayer)
    {
        return isPlayer ? playerHeroController.model.mp : enemyHeroController.model.mp;
    }
    /// <summary>
    /// ヒーローのMPを減らす
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
    }

    #endregion
    #region resultとメニューへの遷移
    /// <summary>
    /// あきらめる
    /// </summary>
    /// <param name="isPlayerConcede"></param>
    public void Concede(bool isPlayerConcede)
    {
        if (isPlayerConcede) { playerHeroController.Concede(); }
        else { enemyHeroController.Concede(); }
        ViewResultPanel();
    }
    /// <summary>
    /// あきらめることを対戦相手に送信する
    /// </summary>
    public void SendConcede()
    {
        photonView.RPC(nameof(RPCSendConcede), RpcTarget.Others);
    }
    [PunRPC]
    public void RPCSendConcede()
    {
        Concede(false); //相手がコンシした時に呼ばれるのでfalse固定
    }
    [SerializeField]
    private AudioSource audioSourceBGM; //結果が決まったらBGMを消したい
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
            resultImage.sprite = Resources.Load<Sprite>($"UIs/win"); //あなたの処理
        }
        else if (!playerHeroController.model.isAlive && enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/lose"); //あなたの敗北
        }
        else if (!playerHeroController.model.isAlive && !enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundLose();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/draw"); //両者敗北
        }
        Invoke("ChangeMenuScene", 3);
    }
    private void ChangeMenuScene()
    {
        StopAllCoroutines();
        if (PhotonNetwork.IsConnected) { PhotonNetwork.LeaveRoom(); PhotonNetwork.Disconnect(); }
        SceneManager.LoadScene("MenuScene");
    }
    /// <summary>
    /// 切断対策
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
