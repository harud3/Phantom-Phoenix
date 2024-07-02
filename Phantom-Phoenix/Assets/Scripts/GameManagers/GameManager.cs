
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
    [SerializeField] private HeroController playerHeroController, enemyHeroController; //味方ヒーロー、敵ヒーロー
    [SerializeField] private TensionController playerTensionController, enemyTensionController; //味方テンション、敵テンション

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6]; //味方フィールド、敵フィールド
    [SerializeField] private Transform playerHandTransform, enemyHandTransform; //味方手札、敵手札
    [SerializeField] private Transform playerSelectionTransform; //選択フィールド
    [SerializeField] private GameObject HintMessage;

    [SerializeField] private CardController cardPrefab; //カード

    [SerializeField] private UnityEngine.UI.Button ButtonTurn; //ターン変更ボタン
    [SerializeField] private GameObject PanelGuard; //操作をガードするパネル
    [SerializeField] private Sprite playerTurnSprite, enemyTurnSprite; //ターン終了、相手のターンのスプライト

    [SerializeField] private TextMeshProUGUI timeCountText; //ターンの残り時間の表示部
    [SerializeField] int timeLimit; //ターンの時間制限
    int timeCount; //ターンの残り時間

    private bool _isPlayerTurn;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;

    DeckModel playerDeck = null;
    DeckModel enemyDeck = null; int enemyUseHeroID = 0;
    #region 初期設定
    /// <summary>
    /// ゲームの状態 通信対戦において、最初に相互データ通信(デッキ、シード値)があるので管理必須
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
            if (GameDataManager.instance.isMaster) //部屋主が音頭を取る
            {
                isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
                SendSetIsPlayerTurn(isPlayerTurn);
                gameState = eGameState.isGotPlayerTurn;
            }

        }
        else //オンライン対戦ではない→AI戦
        {
            enemyDeck = new DeckModel().Init();
            enemyUseHeroID = enemyDeck.useHeroID;
            isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
            gameState = eGameState.isWaitEnemyHeroID;
        }
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
        if(gameState == eGameState.isBigin)
        {
            gameState = eGameState.isGotPlayerTurn;
        }
    }
    private void Update()
    {
        if(gameState == eGameState.isGotPlayerTurn){ //Ai戦はこの状態を飛ばしているので、オンラインチェックなし
            gameState = eGameState.isWaitEnemyHeroID;
            SendUseHeroID(playerDeck.useHeroID);
        }
        //オンライン対戦では、相手のデッキがなければ始められないので、ここで開始する それに伴い、AI戦もここで開始する
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
            HintMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"対象を選択してください";

            gameState = eGameState.isStarted;
            if (GameDataManager.instance.isOnlineBattle)
            {
                int seed = int.Parse(DateTime.Now.ToString("ddHHmmss")); //ランダム要素をプレイヤー間で揃えるため、シード値を共有することで対応する
                UnityEngine.Random.InitState(seed);
                SendSetSeed(seed); //厳密にはシード値が正しく受信されたかチェックすべきな気もする
            }
            SetTime();
            SettingInitHand();
            StartCoroutine(ChangeTurn(true));
        }
    }
    /// <summary>
    /// どちらから開始するのかを対戦相手に送信する
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
    /// マリガン入力待ち→マリガン処理
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

        //PlayerMulligan → PanelMulligan → FrameMulligan → TextHintMuligan
        playerSelectionTransform.parent.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"マリガン　{(isPlayerTurn ? "先攻" : "後攻")}";

        //入力待ち
        yield return new WaitUntil(() => gameState == eGameState.isProcessMulligan);


        //マリガン処理
        //ex) 後攻でマリガンを行う時
        //Deck 0, 1, 2, 3 が、CardID A, B, C, D であり、 B, Dを残すとする
        //この時、Aから順に残すか返すかの判定を行う 変数keppIndexに0を代入する
        //AのisMulliganを見て、返すことにした　何もしない         A, B, C, D
        //BのisMulliganを見て、残すことにした　ここで、Deck[0]のAと、Bの位置をスワップする そして、keepIndex+1            B, A, C, D
        //CのisMulliganを見て、返すことにした　何もしない         B, A, C, D
        //DのisMulliganを見て、残すことにした　ここで、keepInedxの値から既にスワップしたカードがあると分かるので、その次のDeck[1]のAと、Dの位置をスワップする　そして、keepIndex+1          B, D, C, A
        //現在のkeepIndexは2    Deckの先頭2枚(0番目～1番目)が残したいカードとなる　よって先頭の2枚(B, D)を固定
        //Deckの2番目～29番目までをシャッフルする　これは、「Deckの keepIndex番目 から、30 - keepIndex 枚を並べ替える」 と表せる
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

        //返したカードが戻ってくるケースがあるけど、まあひとまずこれでいいと思います
        playerDeck.deck = playerDeck.deck.GetRange(0, keepIndex).Concat(playerDeck.deck.GetRange(keepIndex, 30 - keepIndex).OrderBy(i => Guid.NewGuid())).ToList();

        if (GameDataManager.instance.isOnlineBattle)
        {
            SendSetEnemyDeck(playerDeck);
        }

        HintMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"相手のマリガンを待っています";

        gameState = eGameState.isWaitStart;
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
    /// ヒーローの設定
    /// </summary>
    void SettingInitHero()
    {
        playerHeroController.Init(playerDeck.useHeroID);
        enemyHeroController.Init(enemyUseHeroID);
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
        PanelGuard.gameObject.SetActive(true);

        if (isFirst) //ゲーム開始時はドロー処理があるので待つ
        {
            yield return new WaitForSeconds(1.6f);
        }
        else
        {
            AudioManager.instance.SoundButtonClick2(); //ButtonTurnが押された音
        }

        //ターン終了時処理
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

        //ターン終了時処理待ち時間
        if (!isFirst)
        {
            isPlayerTurn = !isPlayerTurn;
            yield return new WaitForSeconds(unitCount * 0.1f);
        }

        //ターン開始でのドロー処理　最初のターンはドローなしにするため
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
            SetCanSummonHandCards(); //手札から出せるかどうかの表示
            playerTensionController.CanUsetensionCard(true);
            SetCanUsetension(true);
        }
        else
        {
            ButtonTurn.image.sprite = enemyTurnSprite;
            Debug.Log("相手ターン");
            enemyHeroController.ResetMP();
            SetCanAttackAllFieldUnit(playerFields, false);
            SetCanAttackAllFieldUnit(enemyFields, true, true); //連撃権の復活も行う
            enemyTensionController.CanUsetensionCard(true);
            SetCanUsetension(true,true); //テンションが使えるかどうかの表示をなくす
        }

        yield return new WaitForSeconds(0.6f); //ドロー待ち時間

        PanelGuard.gameObject.SetActive(false); //ターンボタン押せるように

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
    /// 味方のカードを召喚可能か判断し、表示を変更する
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="setCanNotSummon"></param>
    private void SetCanSummonHandCard(CardController cc, bool setCanNotSummon = false)
    {
        if (cc.model.isPlayerCard && cc.model.cost <= GetHeroMP(true) && !setCanNotSummon) //誤って敵手札が通らないように setCanNotSummonがtrueなら、召喚不可にする
        {
            cc.SetCanSummon(true);
        }
        else
        {
            cc.SetCanSummon(false);
        }
    }
    /// <summary>
    /// 味方手札のカードが召喚可能か判断し、表示を変更する
    /// </summary>
    /// <param name="setCanNotSummon"></param>
    public void SetCanSummonHandCards(bool setCanNotSummon = false)
    {
        foreach (Transform handCards in playerHandTransform) //敵手札の情報開示はしないので、味方固定
        {
            SetCanSummonHandCard(handCards.GetComponent<CardController>(), setCanNotSummon);
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
                var cc = field.GetChild(0).GetComponent<CardController>();
                cc.SetCanAttack(CanAttack, ResetIsActiveDoubleAciton);
                cc.SetIsNotSummonThisTurn();
            }
        }
    }
    #endregion
    #region AI制御

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

        //fieldの攻撃可能ユニットで攻撃する 味方ユニットがいるならユニットを　味方ユニットがいないならヒーローを攻撃
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

        //ヒーローを対象にしているとき
        if (targetsByReceiver != null && targetsByReceiver[0] is var i && (i == 13 || i == 14))
        {
            cc.SummonOnField(fieldID + 6, hcTarget : i == 13 ? playerHeroController : enemyHeroController);
        }
        else //ユニットを対象にしているか、対象を取っていない時
        {
            //PlayerFieldとして入力されてきている　よって、+6してやればEnemyFieldになる
            cc.SummonOnField(fieldID + 6, targetsByReceiver == null ? null : FieldManager.instance.GetUnitsByFieldID(targetsByReceiver).ToArray());
        }
        yield return new WaitForSeconds(0.75f);
    }
    /// <summary>
    /// カード同士のバトル処理
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public IEnumerator CardsBattle(CardController attacker, CardController target)
    {
        //開戦の儀
        attacker.Attack(target,true);
        SkillManager.instance.ExecutePierce(attacker, target);
        target.Attack(attacker, false);

        yield return new WaitForSeconds(0.25f);

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
        CardController attacker = enemyFields[attackerFieldID - 1].GetChild(0).GetComponent<CardController>();
        CardController target = playerFields[targetFieldID - 6 - 1].GetChild(0).GetComponent<CardController>();

        StartCoroutine(attacker.movement.MoveToTarget(playerFields[targetFieldID - 6 - 1]));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(CardsBattle( attacker, target ));
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
            CheckIsAlive(false);
        }
        else {
            attacker.Attack(playerHeroController, true);
            //勝利判定もしておく
            CheckIsAlive(true);
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
    #region ヒーロー操作
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
        if (isPlayerTurn)
        {
            SetCanSummonHandCards();
        }
        SetCanUsetension(isPlayerCard);
    }
    /// <summary>
    /// テンションカード及びテンションスペルが使用可能か判断し、表示を変更する
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
    /// テンションカードの使用を対戦相手に送信する 
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
    /// テンションスペルの使用を対戦相手に送信する 0は効果選択なしのテンションスキル 1～12はフィールドのユニット 13は味方ヒーロー 14は敵ヒーローが対象　受信者目線のIDに変換済み
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
        resultPanel.transform.SetSiblingIndex(99); //最前面に表示
        if (playerHeroController.model.isAlive && !enemyHeroController.model.isAlive)
        {
            AudioManager.instance.SoundWin();
            resultImage.sprite = Resources.Load<Sprite>($"UIs/win"); //あなたの勝利
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
        StopAllCoroutines();
        Invoke("ChangeMenuScene", 3);
    }
    private void ChangeMenuScene()
    {
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
