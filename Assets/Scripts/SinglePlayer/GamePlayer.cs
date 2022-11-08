using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class GamePlayer : MonoBehaviour
{
    public List<Card> cards             = new List<Card>();
    public List<Card> selectCardList    = new List<Card>();
    public Util.PlayerReceiveCardSlot[] slot = new Util.PlayerReceiveCardSlot[Util.numberOfSlots];

    public string playerName;
    public int    playerNumber;
    public int    roundScore;
    public int    totalScore;

    public bool chooseFlag          = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag      = false;
    public bool smallTichuFlag      = false;
    public bool isTrickPassed       = false;
    public bool isBombPassed        = false;
    public bool isTrickSelected     = false;
    public int  ranking;

    public bool canDeclareSmallTichu = true;

    public bool isFinished = false;

    public string previousTrick = null;

    public bool hasBomb = false;

    public Util.PlayerType playerType;

    public TichuAgent agent = null;

    private void Awake()
    {
        agent = GetComponent<TichuAgent>();
        if (agent.enabled == false) playerType = Util.PlayerType.Player;
        else
        {
            if (GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.HeuristicOnly) playerType = Util.PlayerType.Heuristic;
            else                                                                               playerType = Util.PlayerType.Inference;
        }
    }

    /// <summary>
    /// Ʈ���� ���������� ������ ������.
    /// </summary>
    public void ResetPerTrick()
    {
        
        isBombPassed  = false;
        isTrickPassed = false;
        isTrickSelected = false;
        previousTrick = null;
        isDogTrick = false;
        dragonChooseFlag = false;
        getTrickScoreFlag = false;
        dragonChoosePlayerIdx = -1;
    }
    /// <summary>
    /// ���尡 ����������(1,2,3,4��) ������ ������.
    /// </summary>
    public void ResetPerRound()
    {
        hasBomb = false;
        roundScore = 0;
        chooseFlag = false;
        coroutineFinishFlag = false;
        largeTichuFlag = false;
        smallTichuFlag = false;
        ranking = -1;
        canDeclareSmallTichu = true;
        isFinished = false;
        cards.Clear();
        selectCardList.Clear();
        ResetPerTrick();
    }
      
    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public void AddCards(List<Card> cardList)
    {
        cards.AddRange(cardList);
    }
    public void AddCardToSlot(Card card, GamePlayer cardGiver)
    {
        slot[Util.GetCardGiverIdx(cardGiver, this)].player = cardGiver;
        slot[Util.GetCardGiverIdx(cardGiver, this)].card   = card;
    }
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }

    public void AddSelection(Card card)
    {
        selectCardList.Add(card);
        cards.Remove(card);
        Util.SortCard(ref selectCardList);
    }

    public void RemoveSelection(Card card)
    {
        selectCardList.Remove(card);
        cards.Add(card);
        Util.SortCard(ref cards);
    }

    public void DisableSelection()
    {
        cards.AddRange(selectCardList);
        ClearSelection();
    }

    public void ClearSelection()
    {
        foreach (var selectedCard in selectCardList) selectedCard.ResetByToggle();
        selectCardList.Clear();
        GameManager.instance.RestorePhoenixValue();
        Util.SortCard(ref cards);
    }

    public bool CalculateHandRunOut()
    {
        if (cards.Count == 0) { ranking = GameManager.instance.CountFinishedPlayer() + 1; isFinished = true; }
        return isFinished;
        //���� Ƽ��, ���� Ƽ�� ���� ������ ������ ���� ��.
    }

    public void FindNextPlayer(Util.Trick nowTrick)
    {
        int startIdx = GameManager.instance.startPlayerIdx + 1;
        if (nowTrick?.trickType == Util.TrickType.Dog) 
        {
            startIdx++;
            for (int i = 0; i < Util.numberOfPlayers; ++i)
            {
                if (GameManager.instance.players[startIdx % Util.numberOfPlayers].isFinished == false) break;
                ++startIdx;
            }
        }
        GameManager.instance.startPlayerIdx = startIdx;
    }

    public void CalculateIsRoundEnd()
    {
        if(isFinished == true&&GameManager.instance.players[(playerNumber+2)%Util.numberOfPlayers].isFinished == true)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // ������ ����. ���� ���� �¾�.
        }
        else if(GameManager.instance.CountFinishedPlayer()==3)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // 1,2,3���� ������.
        }
    }

    private bool getTrickScoreFlag;
    private bool dragonChooseFlag;
    private int dragonChoosePlayerIdx;
    public IEnumerator GetTrickScore()
    {
        getTrickScoreFlag = false;
        dragonChooseFlag = false;
        if (GameManager.instance.IsDragonOnTop())
        {
            if (playerType != Util.PlayerType.Player)
            {
                yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentDragonSelectWaitDuration, Util.agentDragonSelectRandomFactor));

                agent.currentPhase = Util.PhaseType.DragonSelectionPhase;
                agent.RequestDecision();
                yield return new WaitUntil(() => agent.isActionEnd);
                agent.isActionEnd = false;
            }
            else
            {
                UIManager.instance.ActivateDragonSelection(DragonChooseNextOpponentCall, DragonChoosePreviousOpponentCall);
                yield return new WaitUntil(() => dragonChooseFlag || UIManager.instance.IsTimeOut());
                UIManager.instance.DeactivateDragonSelection();
                if (dragonChooseFlag == false) RandomDragonChooseCall();
            }
            var trickTaker = GameManager.instance.players[dragonChoosePlayerIdx % Util.numberOfPlayers];
            foreach (var trick in GameManager.instance.trickStack)
            {
                int score = 0;
                foreach (var card in trick.cards)
                {
                    score += card.score;
                    card.transform.position = Util.hiddenCardPosition;
                }
                trickTaker.roundScore += score;
            }

            // ���� �� �� ����� ���� Give.
        }
        else
        {
            foreach (var trick in GameManager.instance.trickStack)
            {
                int score = 0;
                foreach (var card in trick.cards)
                {
                    score += card.score;
                    card.transform.position = Util.hiddenCardPosition;
                }
                roundScore += score;
            }
        }
        GameManager.instance.trickStack.Clear();
        getTrickScoreFlag = true;
    }

    private bool isDogTrick;
    public void ProgressIfDog(Util.Trick nowTrick)
    {
        if (nowTrick.trickType == Util.TrickType.Dog)
        {
            isDogTrick = true;
        }
        else isDogTrick = false;
    }

    private bool isBirdTrick;
    public void ProgressIfBird(Util.Trick nowTrick)
    {
        if (nowTrick.cards.Contains(GameManager.instance.bird)) isBirdTrick = true;
        else isBirdTrick = false;
    }

    public void DeclareLargeTichuCall()
    {
        chooseFlag = true;
        canDeclareSmallTichu = false;
        largeTichuFlag = true;
    }
    public void SkipLargeTichuCall()
    {
        chooseFlag = true;
        largeTichuFlag = false;
    }
    public void DeclareSmallTichuCall()
    {
        smallTichuFlag = true;
        canDeclareSmallTichu = false;
        UIManager.instance.DeactivateAlertPopup();
        UIManager.instance.RenderPlayerInfo();
    }
    public void ExchangeCardCall()
    {
        if (UIManager.instance.IsAllSlotSelected())
        {
            GameManager.instance.currentCard?.ToggleSelection();
            GameManager.instance.currentSlot?.ToggleSelection();
            chooseFlag = true;
        }
        else
        {
            UIManager.instance.Massage(Util.slotSelectErrorMsg);
            return;
        }
    }

    public void RandomExchangeCardCall()
    {
        for (int i = 0; i < Util.numberOfSlots; ++i)
        {
            if (UIManager.instance.exchangeCard.slots[i].slot.card == null)
            {
                cards[Random.Range(0, cards.Count)].ToggleSelection();
                UIManager.instance.exchangeCard.slots[i].slot.ToggleSelection();
            }
        }
        ExchangeCardCall();
    }

    public void ReceiveCardCall()
    {
        for (int idx = 0; idx < slot.Length; ++idx)// var cardSlot in slot)
        {
            AddCard(slot[idx].card);
            slot[idx].card.isFixed = false;
            slot[idx].card = null;
        }
        Util.SortCard(ref cards);
        hasBomb = GameManager.instance.IsBombExist(this);
        chooseFlag = true;
        //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //���ۿ� �ִ� ī�带 AddCard() �ϰ�, isFixed Ǯ��, �����ϰ�, ����.
    }

    public void SelectTrickCall()
    {
        Util.Trick nowTrick = Util.MakeTrick(selectCardList);
        if (GameManager.instance.currentPhase != Util.PhaseType.BombSelectionPhase&&Util.IsPlayerHaveToFulfillBirdWish(this)!=null)
        {
            if(GameManager.instance.IsTrickValidAndFulfillBirdWish(nowTrick))
            {
                previousTrick = Util.GetTrickInfo(nowTrick);
                UIManager.instance.RenderTrickCard(selectCardList);
                nowTrick.playerIdx = playerNumber;
                GameManager.instance.trickStack.Push(nowTrick); // ���� �ʿ�.
                GameManager.instance.AddCardMarking(selectCardList);
                ClearSelection();
                canDeclareSmallTichu = false;
                CalculateHandRunOut();
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
                isTrickSelected = true;
                //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
                UIManager.instance.DeactivateBirdWishNotice(); // �� �κ� �߰�.

                hasBomb = GameManager.instance.IsBombExist(this);
                
                chooseFlag = true;
            }
            else
            {
                //DebugSelection(nowTrick); //��ȭ�н��� ����.
                UIManager.instance.Massage(Util.fulfillBirdWishErrorMsg);
                return;
            }
        }
        else
        {
            if (GameManager.instance.isTrickValid(nowTrick))
            {
                previousTrick = Util.GetTrickInfo(nowTrick);
                UIManager.instance.RenderTrickCard(selectCardList);
                nowTrick.playerIdx = playerNumber;
                GameManager.instance.AddCardMarking(selectCardList);
                GameManager.instance.trickStack.Push(nowTrick); // ���� �ʿ�.
                ClearSelection();
                canDeclareSmallTichu = false;

                CalculateHandRunOut();
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
                isTrickSelected = true;

                hasBomb = GameManager.instance.IsBombExist(this);
                //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
                chooseFlag = true;
            }
            else
            {
                //DebugSelection(nowTrick); //��ȭ�н��� ����.
                UIManager.instance.Massage(Util.trickSelectErrorMsg);
                return;
            }
        }
    }

    public void SelectBombCall()
    {
        Util.Trick nowTrick = Util.MakeTrick(selectCardList);

        if(nowTrick.trickType==Util.TrickType.FourCardBomb || nowTrick.trickType==Util.TrickType.StraightFlushBomb)
        {
            isBombPassed = false;
            SelectTrickCall();
        }
        else
        {
            UIManager.instance.Massage(Util.bombSelectErrorMsg);
            return;
        }
    }

    public void PassTrickCall()
    {
        DisableSelection();
        if (isFinished == false) previousTrick = Util.trickPassInfo;
        isTrickPassed = true;
        isTrickSelected = false;
        chooseFlag = true;
    }

    public void PassBombCall()
    {
        DisableSelection();
        isBombPassed = true;
        chooseFlag = true;
    }

    public void PassOrPickRandomTrickCall()
    {
        //������ �ҿ��� ������ �� �ִµ� Ÿ�Ӿƿ��̶�� �����ϴ� Ʈ�� �ƹ��ų� ����.
        Util.Trick birdWishFulfillTrick = null;
        if ((birdWishFulfillTrick = Util.IsPlayerHaveToFulfillBirdWish(this)) != null)
        {
            DisableSelection();
            foreach (var card in birdWishFulfillTrick.cards) card.ToggleSelection();
            SelectTrickCall();
        }
        else
        {
            if (GameManager.instance.isFirstTrick)
            {
                DisableSelection();
                cards[Random.Range(0, cards.Count)].ToggleSelection();
                SelectTrickCall();
            }
            else
            {
                PassTrickCall();
            }
        }
    }

    public void DragonChooseNextOpponentCall()
    {
        dragonChoosePlayerIdx = playerNumber + 1;
        dragonChooseFlag = true;
    }

    public void DragonChoosePreviousOpponentCall()
    {
        dragonChoosePlayerIdx = playerNumber + 3;
        dragonChooseFlag = true;
    }

    public void RandomDragonChooseCall()
    {
        var rnd = new UnityAction[] { DragonChooseNextOpponentCall, DragonChoosePreviousOpponentCall };
        rnd[Random.Range(0, 2)]();
    }

    public void BirdWishChooseCall()
    {
        chooseFlag = true;
    }



    public void ChooseLargeTichu()
    {
        Util.SortCard(ref cards);
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public void ExchangeCards()
    {
        Util.SortCard(ref cards);
        StartCoroutine(ExchangeCardsCoroutine());
    }

    public void ReceiveCard()
    {
        StartCoroutine(ReceiveCardCoroutine());
    }

    public void SelectTrick()
    {
        StartCoroutine(SelectTrickCoroutine());
    }



    public IEnumerator ChooseLargeTichuCoroutine()
    {
        coroutineFinishFlag = false;

        if(playerType != Util.PlayerType.Player)
        {
            yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentLargeTichuWaitDuration, Util.agentLargeTichuRandomFactor));
            agent.currentPhase = Util.PhaseType.LargeTichuSelectionPhase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
        }
        else
        {
            chooseFlag = false;
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInLargeTichuPhase, cards);

            UIManager.instance.ActivateLargeTichu(DeclareLargeTichuCall, SkipLargeTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) SkipLargeTichuCall();
            UIManager.instance.DeactivateLargeTichu();

            UIManager.instance.ActivateWaitingInfo();
        }

        coroutineFinishFlag = true;
        UIManager.instance.RenderPlayerInfo();
    }

    public IEnumerator ExchangeCardsCoroutine()
    {
        coroutineFinishFlag = false;

        if (playerType != Util.PlayerType.Player)
        {

            if (canDeclareSmallTichu)
            {
                yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentSmallTichuWaitDuration, Util.agentSmallTichuRandomFactor));
                agent.currentPhase = Util.PhaseType.SmallTichuSelectionPhase;
                agent.RequestDecision();
                yield return new WaitUntil(() => agent.isActionEnd);
                agent.isActionEnd = false;
            }

            yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentExchangeCardWaitDuration, Util.agentExchangeCardRandomFactor));

            agent.currentPhase = Util.PhaseType.ExchangeSelection1Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;

            agent.currentPhase = Util.PhaseType.ExchangeSelection2Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;

            agent.currentPhase = Util.PhaseType.ExchangeSelection3Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
        }
        else
        {
            chooseFlag = false;
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

            UIManager.instance.ActivateExchangeCardsPopup(ExchangeCardCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) RandomExchangeCardCall();
            UIManager.instance.DeactivateExchangeCardsPopup();

            UIManager.instance.ActivateWaitingInfo();
        }

        coroutineFinishFlag = true;
        UIManager.instance.RenderPlayerInfo();
    }

    public IEnumerator ReceiveCardCoroutine()
    {
        
        coroutineFinishFlag = false;

        if (playerType != Util.PlayerType.Player)
        {
            yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentReceiveCardWaitDuration, Util.agentReceiveCardRandomFactor));
            ReceiveCardCall();


            if (canDeclareSmallTichu)
            {
                yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentSmallTichuWaitDuration, Util.agentSmallTichuRandomFactor));
                agent.currentPhase = Util.PhaseType.SmallTichuSelectionPhase;
                agent.RequestDecision();
                yield return new WaitUntil(() => agent.isActionEnd);
                agent.isActionEnd = false;
            }
        }
        else
        {
            chooseFlag = false;
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

            UIManager.instance.ActivateReceiveCardPopup(ReceiveCardCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) ReceiveCardCall();
            UIManager.instance.DeactivateReceiveCardPopup();

            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
            UIManager.instance.ActivateWaitingInfo();
        }

        coroutineFinishFlag = true;
        UIManager.instance.RenderPlayerInfo();
    }

    public IEnumerator SelectTrickCoroutine()
    {
        coroutineFinishFlag = false;
        GameManager.instance.currentTrickPlayerIdx = playerNumber;

        if (playerType != Util.PlayerType.Player) UIManager.instance.ActivateWaitingInfo();

        if (GameManager.instance.IsTrickAllPassed(this)) //��� �н��� �ߴٸ� ��. �ٽ� �� ����.
        {
            StartCoroutine(GetTrickScore());
            yield return new WaitUntil(() => getTrickScoreFlag);
            GameManager.instance.trickFinishFlag = true;
            GameManager.instance.startPlayerIdx = playerNumber;
            if (isFinished == true) FindNextPlayer(null);
            if (dragonChooseFlag)
            {
                UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(GameManager.instance.players[dragonChoosePlayerIdx % Util.numberOfPlayers].playerName));
                dragonChooseFlag = false;
            }
            else UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(playerName));
            yield return new WaitForSeconds(Util.trickTakeDuration);
        }
        else
        {
            if (isFinished == true)
            {
                previousTrick = null;
                PassTrickCall();
                FindNextPlayer(null);
            }
            else
            {
                if (playerType != Util.PlayerType.Player)
                {

                    if (canDeclareSmallTichu)
                    {
                        yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentSmallTichuWaitDuration, Util.agentSmallTichuRandomFactor));
                        agent.currentPhase = Util.PhaseType.SmallTichuSelectionPhase;
                        agent.RequestDecision();
                        yield return new WaitUntil(() => agent.isActionEnd);
                        agent.isActionEnd = false;
                    }
                    if (GameManager.instance.trickStack.Count > 0 && GameManager.instance.trickStack.Peek().trickLength <= cards.Count)
                    {
                        yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentTrickSelectWaitDuration, Util.agentTrickSelectRandomFactor));
                    }
                    if (GameManager.instance.isFirstTrick) agent.currentPhase = Util.PhaseType.FirstTrickSelectionPhase;
                    else agent.currentPhase = Util.PhaseType.TrickSelectionPhase;
                    agent.RequestDecision();
                    yield return new WaitUntil(() => agent.isActionEnd);
                    agent.isActionEnd = false;
                }
                else
                {
                    GameManager.instance.isSelectionEnabled = true;
                    GameManager.instance.isMultipleSelectionEnabled = true;
                    chooseFlag = false;

                    UIManager.instance.ActivateTrickSelection(SelectTrickCall, PassTrickCall, DeclareSmallTichuCall, SelectBombCall);
                    yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                    if (chooseFlag == false) PassOrPickRandomTrickCall();
                    UIManager.instance.DeactivateTrickSelection();

                    UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

                    GameManager.instance.isSelectionEnabled = false;
                    GameManager.instance.isMultipleSelectionEnabled = false;
                }

                if (isDogTrick) //���� �� ���
                {
                    FindNextPlayer(GameManager.instance.trickStack.Peek());
                    isDogTrick = false;
                    UIManager.instance.ShowInfo(GameManager.instance.players[GameManager.instance.startPlayerIdx % Util.numberOfPlayers].playerName + Util.selectDogInfo);
                    yield return new WaitForSeconds(Util.selectDogDuration);

                    StartCoroutine(GetTrickScore()); //���� ��� �뵵��.
                    yield return new WaitUntil(() => getTrickScoreFlag);
                    GameManager.instance.trickFinishFlag = true;
                }
                else
                {
                    FindNextPlayer(null);
                    if (isBirdTrick)
                    {
                        isBirdTrick = false;

                        if (playerType != Util.PlayerType.Player)
                        {
                            yield return new WaitForSeconds(Util.GetAgentWaitDuration(Util.agentBirdWishSelectWaitDuration, Util.agentBirdWishSelectRandomFactor));

                            agent.currentPhase = Util.PhaseType.BirdWishSelectionPhase;
                            agent.RequestDecision();
                            yield return new WaitUntil(() => agent.isActionEnd);
                            agent.isActionEnd = false;
                        }
                        else
                        {
                            chooseFlag = false;
                            UIManager.instance.ActivateBirdWishSelection(BirdWishChooseCall);
                            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                            UIManager.instance.DeactivateBirdWishSelection();
                        }
                    }
                }
            }
        }

        coroutineFinishFlag = true;
    }


    public void DebugSelection(Util.Trick nowTrick)
    {
        Debug.Log("���� ������: " + GameManager.instance.currentPhase);
        if (GameManager.instance.trickStack.Count > 0)
        {
            var nowStack = GameManager.instance.trickStack.Peek();


            Debug.Log("���� ������ ž - Ÿ��: " + nowStack.trickType + " ���: " + nowStack.trickValue + " ����: " + nowStack.trickLength);
            Debug.Log("���� ������ ī��: ");

            foreach (var card in nowStack.cards) Debug.Log(card);
        }
        else Debug.Log("���� ������ �������.");
        
        if (GameManager.instance.isBirdWishActivated)
        {
            Debug.Log("���� ������ �ҿ� Ȱ��ȭ�Ǿ�����. ��: " + GameManager.instance.birdWishValue);
        }
        else Debug.Log("���� ������ �ҿ� ��Ȱ��ȭ");

        Debug.Log("���� ������ ī��: ");
        foreach (var card in cards) Debug.Log(card);
        foreach (var card in nowTrick.cards) Debug.Log(card);


        Debug.Log("���� ���� Decision: " + agent.decodeNumberForDebug);
        Debug.Log("���� ������ ī�� ");
        foreach (var card in nowTrick.cards) Debug.Log(card);

        Debug.Log("���� ���� Ʈ���� - Ÿ��: " + nowTrick.trickType + " ���: " + nowTrick.trickValue + " ����: " + nowTrick.trickLength);
        
        Debug.LogError("���� ����Ʈ ����.");
    }
}