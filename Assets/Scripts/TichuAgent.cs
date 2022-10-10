using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;
using System;
using static Util;

public class TichuAgent : Agent 
{
    public bool isActionEnd = false;
    public PhaseType currentPhase;
    public int decodeNumberForDebug;
    
    public GamePlayer player;
    public override void Initialize()
    {
        isActionEnd = false;
        player = GetComponent<GamePlayer>();
        currentPhase = PhaseType.NumberOfPhase;
    }

    public override void OnEpisodeBegin()
    {
        isActionEnd = false;
        currentPhase = PhaseType.NumberOfPhase;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int nowAction = 0;
        switch(currentPhase)
        {
            case PhaseType.LargeTichuSelectionPhase:
                
                //0은 패스, 1은 선언.
                nowAction = actions.DiscreteActions[0];
                
                if (nowAction == 0) player.SkipLargeTichuCall();
                else player.DeclareLargeTichuCall();
                
                break;
            
            case PhaseType.SmallTichuSelectionPhase:
                
                //0은 패스, 1은 선언.
                nowAction = actions.DiscreteActions[1];

                if (nowAction == 1) player.DeclareSmallTichuCall();

                break;

            case PhaseType.ExchangeSelection1Phase:
                //0~55 카드 선택.
                nowAction = actions.DiscreteActions[2];

                var nowCard1 = player.cards.Where(x => x.id == nowAction).ToList();

                // 내 1칸 다음 사람에게 전달.

                GameManager.instance.players[(player.playerNumber + 1) % numberOfPlayers].AddCardToSlot(nowCard1[0], player);

                // 내 카드 리스트에서 해당 카드를 뺌.
                player.cards.Remove(nowCard1[0]);

                break;

            case PhaseType.ExchangeSelection2Phase:
                //0~55 카드 선택.
                nowAction = actions.DiscreteActions[2];

                var nowCard2 = player.cards.Where(x => x.id == nowAction).ToList();

                // 내 2칸 다음 사람에게 전달.

                GameManager.instance.players[(player.playerNumber + 2) % numberOfPlayers].AddCardToSlot(nowCard2[0], player);

                // 내 카드 리스트에서 해당 카드를 뺌.
                player.cards.Remove(nowCard2[0]);


                break;

            case PhaseType.ExchangeSelection3Phase:
                //0~55 카드 선택.
                nowAction = actions.DiscreteActions[2];

                var nowCard3 = player.cards.Where(x => x.id == nowAction).ToList();

                // 내 2칸 다음 사람에게 전달.

                GameManager.instance.players[(player.playerNumber + 3) % numberOfPlayers].AddCardToSlot(nowCard3[0], player);

                // 내 카드 리스트에서 해당 카드를 뺌.
                player.cards.Remove(nowCard3[0]);

                break;

            case PhaseType.FirstTrickSelectionPhase:
                //1~382 트릭 선택.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == 0) Debug.LogError("에러. 첫 트릭에서 패스할 수 없음.");
                if (nowAction == numberOfTrickType) Debug.LogError("에러. 트릭 선택 없음은 불가능함.");

                var cardList = DecodeNumberToTrick(nowAction);

                foreach (var card in cardList)
                {
                    player.AddSelection(card);
                }

                player.SelectTrickCall();

                break;

            case PhaseType.TrickSelectionPhase:
                //0은 패스. 1~382 트릭 선택.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == numberOfTrickType) Debug.LogError("에러. 트릭 선택 없음은 불가능함.");

                if (nowAction == 0) player.PassTrickCall();
                else
                {
                    var cardList2 = DecodeNumberToTrick(nowAction);

                    foreach (var card in cardList2)
                    {
                        player.AddSelection(card);
                    }

                    player.SelectTrickCall();
                }
                break;

            case PhaseType.BombSelectionPhase:
                //0은 패스. 1~382 트릭 선택.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == numberOfTrickType) Debug.LogError("에러. 트릭 선택 없음은 불가능함.");

                if (nowAction == 0) player.PassBombCall();
                else
                {
                    var cardList3 = DecodeNumberToTrick(nowAction);

                    foreach (var card in cardList3)
                    {
                        player.AddSelection(card);
                    }

                    player.SelectBombCall();
                }
                break;

            case PhaseType.BirdWishSelectionPhase:
                //0,2~14 참새의 소원 선택.
                nowAction = actions.DiscreteActions[4];

                if(nowAction!=0)
                {
                    GameManager.instance.isBirdWishActivated = true;
                    GameManager.instance.birdWishValue = nowAction;
                    UIManager.instance.ActivateBirdWishNotice(nowAction);
                }

                break;

            case PhaseType.DragonSelectionPhase:
                //0은 3칸 다음 사람, 1은 1칸 다음 사람 용 트릭 전달 선택.
                nowAction = actions.DiscreteActions[5];

                if(nowAction==0)
                {
                    player.DragonChooseNextOpponentCall();
                }
                else
                {
                    player.DragonChoosePreviousOpponentCall();
                }

                break;
        }

        isActionEnd = true;
        //마지막에 isActionEnd 를 true 로.
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //현재 어떤 상태인가? 1 of 10.
        sensor.AddOneHotObservation((int)currentPhase, (int)Util.PhaseType.NumberOfPhase);

        // 현재 패스했는가 안했는가? each of 4.
        for(int idx = player.playerNumber; idx<player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].isTrickPassed);
        }

        // 현재 선택했는가 안했는가? each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].isTrickSelected);
        }

        // 현재 누가 스택의 탑인가? 1 of 4. (can be null.)
        if (GameManager.instance.trickStack.Count == 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else sensor.AddOneHotObservation(GetRelativePlayerIdx(GameManager.instance.trickStack.Peek().playerIdx, player.playerNumber), Util.numberOfPlayers);

        // (스택 탑 플레이어 인덱스 + 4 - '나'의 인덱스)%4
        //0
        //0: 0 = 4 - 0 % 4
        //1: 1 = 5 - 0 % 4
        //2: 2 = 6 - 0 % 4
        //3: 3 = 7 - 0 % 4

        //1
        //0: 3 = 4 - 1 % 4
        //1: 0 = 5 - 1 % 4
        //2: 1 = 6 - 1 % 4
        //3: 2 = 7 - 1 % 4

        //2
        //0: 2 = 4 - 2 % 4
        //1: 3 = 5 - 2 % 4
        //2: 0 = 6 - 2 % 4
        //3: 1 = 7 - 2 % 4

        //3
        //0: 1 = 4 - 3 % 4
        //1: 2 = 5 - 3 % 4
        //2: 3 = 6 - 3 % 4
        //3: 0 = 7 - 3 % 4
        ///

        // 현재 누구의 차례인가? 1 of 4.
        sensor.AddOneHotObservation(GetRelativePlayerIdx(GameManager.instance.currentTrickPlayerIdx,player.playerNumber), Util.numberOfPlayers);

        // 현재 남은 손패의 개수 - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddOneHotObservation(GameManager.instance.players[nowIdx].cards.Count, numberOfCardsPlay+1);
            //int nowIdx = idx % Util.numberOfPlayers;
            //float nowHand = (float)(GameManager.instance.players[nowIdx].cards.Count) / Util.numberOfCardsPlay;
            //sensor.AddObservation(nowHand);
        }

        //현재 라운드 점수 - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowRoundScore = (float)(GameManager.instance.players[nowIdx].roundScore) / (float)Util.maximumRoundScore;
            sensor.AddObservation(nowRoundScore);
        }

        //현재 총 점수 - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowTotalScore = (float)(GameManager.instance.players[nowIdx].totalScore) / (float)Util.maximumTotalScore;
            sensor.AddObservation(nowTotalScore);
        }

        //현재 라지 티츄 선언 여부 - each of 4
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].largeTichuFlag);
        }

        //현재 스몰 티츄 선언 여부 - each of 4
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].smallTichuFlag);
        }

        //현재 참새의 소원 - 1 of 15 (0: 소원 없음. 2-14: 소원에 해당하는 숫자. 1은 사용하지 않음.)
        sensor.AddOneHotObservation(GameManager.instance.birdWishValue, Util.numberOfBirdWish);

        //현재 빠진 카드의 현황 - each of 56        
        for(int idx = 0; idx<GameManager.instance.cardMarking.Length; ++idx)
        {
            sensor.AddObservation(GameManager.instance.cardMarking[idx]);
        }
        
        //현재 내가 가진 카드의 현황 - each of 56
        for(int idx = 0; idx<Util.numberOfCards; ++idx)
        {
            sensor.AddObservation(player.cards.Any(x => x.id == idx)); // idx 는 id. id에 해당하는 카드를 보유(Any) 하고 있는가?
        }

        // 현재 스택의 탑 트릭은 무엇인가? 1 of 383(can be null), 이 부분이 핵심.
        if(GameManager.instance.trickStack.Count==0)
        {
            for (int idx = 0; idx < numberOfTrickType; ++idx) sensor.AddObservation(0);
        }
        else
        {
            sensor.AddOneHotObservation(EncodeTrickToNumber(GameManager.instance.trickStack.Peek()),numberOfTrickType);
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // 액션을 일단은 모두 block. 선택없음만 활성화.
        //
        // 1. 라지 티츄, 사이즈 3 (라지 티츄 패스, 라지 티츄 선언, 선택 없음)
        // 2. 스몰 티츄, 사이즈 3 (스몰 티츄 패스, 스몰 티츄 선언, 선택 없음)
        // 3. 나눠줄 카드, 사이즈 57. (카드 선택, 선택 없음)
        // 4. 선택할 트릭, 사이즈 384. (트릭 선택, 선택 없음) 
        // 5. 소원 선택, 사이즈 16. (소원 선택, 선택 없음)
        // 6. 용 트릭 전달 선택, 사이즈 3. (전달 선택, 선택 없음)
        
        // 1.
        int actionIdx = 0;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //라지 티츄 액션 

        // 2.
        actionIdx++;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //스몰 티츄 액션

        // 3.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfCards; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //카드 나눠주는 액션

        // 4.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfTrickType; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //트릭 선택 액션

        // 5.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfBirdWish; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //참새의 소원 선택 액션

        // 6.
        actionIdx++;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //용 트릭 전달 선택 액션 


        switch (currentPhase)
        {
            case Util.PhaseType.LargeTichuSelectionPhase:
                actionIdx = 0;
                actionMask.SetActionEnabled(actionIdx, 0, true); //라지 티츄 패스 활성화
                actionMask.SetActionEnabled(actionIdx, 1, true); //라지 티츄 선택 활성화
                actionMask.SetActionEnabled(actionIdx, 2, false); //2는 선택 없음. 이를 비활성화.
                break;
            
            case Util.PhaseType.SmallTichuSelectionPhase: //스몰 티츄 선택 페이즈의 RequestDecision 은 canDeclareSmallTichu 가 true 임이 보장됨.
                actionIdx = 1;
                actionMask.SetActionEnabled(actionIdx, 0, true); // 스몰 티츄 패스 활성화
                actionMask.SetActionEnabled(actionIdx, 1, true); // 스몰 티츄 선택 활성화
                actionMask.SetActionEnabled(actionIdx, 2, false); //2는 선택 없음. 이를 비활성화.
                break;
            
            case Util.PhaseType.ExchangeSelection1Phase:
            case Util.PhaseType.ExchangeSelection2Phase:
            case Util.PhaseType.ExchangeSelection3Phase: // 다음 3개는 phase 의 관측으로 구분된다.
                actionIdx = 2;
                foreach (var card in player.cards) actionMask.SetActionEnabled(actionIdx, card.id, true);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfCards, false); // 56은 선택 없음. 이를 비활성화.
                break;
            
            case Util.PhaseType.FirstTrickSelectionPhase:
                actionIdx = 3;
                ReleaseValidFirstTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false); //383은 선택 없음.이를 비활성화
                break;
            case Util.PhaseType.TrickSelectionPhase:
                actionIdx = 3;
                ReleaseValidTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);  //383은 선택 없음.이를 비활성화
                break;
            case Util.PhaseType.BombSelectionPhase:
                actionIdx = 3;
                ReleaseValidBombTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);  //383은 선택 없음.이를 비활성화
                break;

            case Util.PhaseType.BirdWishSelectionPhase:
                actionIdx = 4;
                for(int idx = 0; idx<Util.numberOfBirdWish; ++idx) // 0 부터 14 까지 1을 제외하고
                {
                    if (idx != 1) actionMask.SetActionEnabled(actionIdx, idx, true);
                }
                actionMask.SetActionEnabled(actionIdx, Util.numberOfBirdWish, false); //15는 선택 없음. 이를 비활성화.
                break;

            case Util.PhaseType.DragonSelectionPhase:
                actionIdx = 5;
                actionMask.SetActionEnabled(actionIdx, 0, true);
                actionMask.SetActionEnabled(actionIdx, 1, true);
                actionMask.SetActionEnabled(actionIdx, 2, false); //2는 선택 없음. 이를 비활성화.
                break;
        }


    }
    public void ReleaseValidTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // 선택 가능한 트릭은 true로 풀어주는 메소드.
        // 첫 트릭이 아님이 보장되어있음.

        GameManager.instance.RestorePhoenixValue();

        if (Util.IsPlayerHaveToFulfillBirdWish(player) != null) // 참새의 소원에 걸리는 경우
        {
            actionMask.SetActionEnabled(actionIdx, 0, false); // 참새의 소원에 걸린다면 일단 패스는 불가능. 이미 block 되어있지만 명시적으로 한번 더 해준다. 

            var topTrick = GameManager.instance.trickStack.Peek();
            var birdWish = GameManager.instance.birdWishValue;

            // 카드의 트릭에 따라 가능한 트릭을 찾아준다.
            switch (GameManager.instance.trickStack.Peek().trickType)
            {
                case TrickType.Single:
                    ReleaseValidSingleWithBirdWish(actionIdx, actionMask, topTrick.trickValue / 2,birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0, birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5, birdWish);
                    }
                    break;
                case TrickType.Pair:
                    ReleaseValidPairWithBirdWish(actionIdx, actionMask, topTrick.trickValue, birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0, birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    }
                    break;
                case TrickType.Triple:
                    ReleaseValidTripleWithBirdWish(actionIdx, actionMask, topTrick.trickValue,birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0, birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    }
                    break;
                case TrickType.ConsecutivePair:
                    ReleaseValidConsecutivePairWithBirdWish(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength,birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0,birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    }
                    break;
                case TrickType.Straight:
                    ReleaseValidStraightWithBirdWish(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength,birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0,birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    }
                    break;
                case TrickType.FullHouse:
                    ReleaseValidFullHouseWithBirdWish(actionIdx, actionMask, topTrick.trickValue,birdWish);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0,birdWish);
                        ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    }
                    break;
                case TrickType.FourCardBomb:
                    ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, topTrick.trickValue,birdWish);
                    ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
                    break;
                case TrickType.StraightFlushBomb:
                    ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength,birdWish);
                    break;
                default:
                    Debug.LogError("불가능한 상황.");
                    break;
            }
        }
        else //참새의 소원에 걸리지 않는 경우
        {
            actionMask.SetActionEnabled(actionIdx, 0, true); // 참새의 소원에 걸리지 않는 경우 패스를 할 수 있음이 보장됨.

            var topTrick = GameManager.instance.trickStack.Peek();

            switch (GameManager.instance.trickStack.Peek().trickType)
            {
                case TrickType.Single:
                    ReleaseValidSingle(actionIdx, actionMask, topTrick.trickValue/2); // 봉황, 용, 새를 포함한 유효 싱글을 마스킹 해제해준다.
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.Pair:
                    ReleaseValidPair(actionIdx, actionMask, topTrick.trickValue);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.Triple:
                    ReleaseValidTriple(actionIdx, actionMask, topTrick.trickValue);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.ConsecutivePair:
                    ReleaseValidConsecutivePair(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.Straight:
                    ReleaseValidStraight(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.FullHouse:
                    ReleaseValidFullHouse(actionIdx, actionMask, topTrick.trickValue);
                    if (player.hasBomb)
                    {
                        ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    }
                    break;
                case TrickType.FourCardBomb:
                    ReleaseValidFourCardBomb(actionIdx, actionMask, topTrick.trickValue);
                    ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
                    break;
                case TrickType.StraightFlushBomb:
                    ReleaseValidStraightFlushBomb(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                    break;
                default:
                    Debug.LogError("불가능한 상황.");
                    break;
            }
        }
    }


    public void ReleaseValidFirstTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // 첫 트릭을 내야하는 경우에 대한 메소드.
        // 어떠한 경우에도 패스는 불가능.

        GameManager.instance.RestorePhoenixValue();
        // 참새의 소원에 걸리는 경우
        if (IsPlayerHaveToFulfillBirdWish(player) != null)
        {
            var birdWish = GameManager.instance.birdWishValue;
            // 구현 필요.
            // 소원을 만족하면서 가능한 싱글, 페어, 트리플, 연속 페어, 스트레이트, 풀하우스, 포카드 폭탄, 스플 폭탄을 release 해준다.
            ReleaseValidSingleWithBirdWish(actionIdx, actionMask, 0, birdWish);
            ReleaseValidPairWithBirdWish(actionIdx, actionMask, 0,birdWish);
            ReleaseValidTripleWithBirdWish(actionIdx, actionMask, 0, birdWish);
            ReleaseValidFullHouseWithBirdWish(actionIdx, actionMask, 0,birdWish);
            for (int len = 5; len <= 14; ++len) ReleaseValidStraightWithBirdWish(actionIdx, actionMask, 0, len,birdWish);
            for (int len = 4; len <= 14; len += 2) ReleaseValidConsecutivePairWithBirdWish(actionIdx, actionMask, 0, len,birdWish);
            ReleaseValidFourCardBombWithBirdWish(actionIdx, actionMask, 0,birdWish);
            ReleaseValidStraightFlushBombWithBirdWish(actionIdx, actionMask, 0, 5,birdWish);
        }
        else
        {
            if(player.cards.Any(x=>x.type == CardType.Dog))
                actionMask.SetActionEnabled(actionIdx, dogTrickOffset, true); // 참새 안걸리고 첫 트릭이면 개를 낼 수 있음이 보장됨.

            // 가능한 싱글, 페어, 트리플, 연속 페어, 스트레이트, 풀하우스, 포카드 폭탄, 스플 폭탄을 release 해준다.
            ReleaseValidSingle(actionIdx, actionMask, 0);
            ReleaseValidPair(actionIdx, actionMask, 0);
            ReleaseValidTriple(actionIdx, actionMask, 0);
            ReleaseValidFullHouse(actionIdx, actionMask, 0);
            for(int len = 5; len<=14; ++len) ReleaseValidStraight(actionIdx, actionMask, 0, len);
            for(int len = 4; len<=14; len+=2) ReleaseValidConsecutivePair(actionIdx, actionMask, 0, len);
            ReleaseValidFourCardBomb(actionIdx, actionMask, 0);
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5);
        }
    }

    public void ReleaseValidBombTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // 폭탄 확인에 대한 메소드.
        // 참새 예외처리는 필요없음. 어떠한 경우에도 패스를 할 수 있음이 보장됨.
        //

        actionMask.SetActionEnabled(actionIdx, 0, true); // 패스를 할 수 있음이 보장됨.

        var nowStack = GameManager.instance.trickStack.Peek();

        if(nowStack.trickType==TrickType.StraightFlushBomb)
        {
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, nowStack.trickValue, nowStack.trickLength);
        }
        else if(nowStack.trickType == TrickType.FourCardBomb)
        {
            ReleaseValidFourCardBomb(actionIdx, actionMask, nowStack.trickValue);
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5); // topValue 를 0으로 넘겨주고 길이를 최하인 5로 설정. 가능한 모든 포카드 폭탄을 release. 
        }
        else
        {
            ReleaseValidFourCardBomb(actionIdx, actionMask, 0); // topValue 를 0으로 넘겨줘 모든 포카드 폭탄을 release.
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5); // topValue 를 0으로 넘겨주고 길이를 최하인 5로 설정. 가능한 모든 포카드 폭탄을 release. 
        }
    }

    public void ReleaseValidSingle(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        //참새의 소원에 걸리지 않음이 보장되어 있음.

        foreach(var card in player.cards) // 플레이어가 가진 모든 카드에 대해
        {
            if (card.value >= 1 && card.value <= 14 && card.value > topValue) // topValue 보다 크고 1부터 A 중 하나에 해당한다면(즉 새도 포함)
                actionMask.SetActionEnabled(actionIdx, card.value, true);
        }
        if(Util.FindPhoenix(player.cards)!=null) // 봉황 관련 예외 처리. 만약 플레이어가 봉황을 가지고 있다면
        {
            if(GameManager.instance.isFirstTrick) // 첫 트릭이라면
            {
                actionMask.SetActionEnabled(actionIdx, phoenixSingleTrickOffset, true); // 1.5 를 선택 가능하게 세팅.
            }
            else
            {
                // 해당하는 봉황 싱글 트릭을 true 로 세팅. value가 1이라면 15(1.5), value가 2라면 16(2.5), ... value가 14라면 28(14.5).
                if(GameManager.instance.trickStack.Peek().cards[0].value>=1 && GameManager.instance.trickStack.Peek().cards[0].value<=14)
                    actionMask.SetActionEnabled(actionIdx, phoenixSingleTrickOffset + GameManager.instance.trickStack.Peek().cards[0].value - 1, true);
            }
        }
        if(player.cards.Any(x=>x.type == CardType.Dragon)) //용 관련 예외처리. 용의 값은 specialCardsValue[3] = 18. 이걸 가지고 있다면 용 선택 가능하게 세팅.
        {
            actionMask.SetActionEnabled(actionIdx, dragonTrickOffset, true);
        }
    }
    public void ReleaseValidPair(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        GameManager.instance.RestorePhoenixValue();
        // 아이디어: 봉황이 있다면 하나씩 넣어보고, 해당 값이 2개 이상 나타나면 true로 풀어준다.
        bool havePhoenix = FindPhoenix(player.cards) != null? true:false;
        for(int val = Mathf.Max(topValue+1,2); val<=14; ++val)
        {
            if (havePhoenix) GameManager.instance.phoenix.value = val;
            // val 에 해당하는 카드의 개수가 2개, 또는 그 이상이면 맞는 action을 풀어준다. 2 페어는 31, 3페어는 32... 14 페어는 43.
            if (player.cards.Where(x => x.value == val).ToList().Count >= 2) actionMask.SetActionEnabled(actionIdx, pairTrickOffset + val - 2, true);
        }
    }
    public void ReleaseValidTriple(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards) != null ? true : false;

        for(int val = Mathf.Max(topValue + 1, 2); val<=14; ++val)
        {
            if (havePhoenix) GameManager.instance.phoenix.value = val;
            // val 에 해당하는 카드의 개수가 3개, 또는 그 이상이면 맞는 action을 풀어준다. 2 트리플은 44, 3페어는 45... 14 페어는 56.
            if (player.cards.Where(x => x.value == val).ToList().Count >= 3) actionMask.SetActionEnabled(actionIdx, tripleTrickOffset + val - 2, true);
        }
    }
    public void ReleaseValidConsecutivePair(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength)
    {
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards) != null ? true : false;

        int nowOffset = GetConsecutivePairOffset(topLength);
        
        if(havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal <=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
                var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
                var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
                tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
                quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
                var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
                SortCard(ref properList);

                while (properList.Count > 0)
                {
                    var nowList = properList.Take(topLength).ToList();
                    if (nowList.Count != topLength) break;
                    var nowTrick = MakeTrick(nowList);
                    if (nowTrick.trickType == TrickType.ConsecutivePair && nowTrick.trickValue > topValue)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowTrick.trickValue - topLength / 2 - 1, true);
                    }
                    properList = properList.Skip(2).ToList();
                }
            }
        }
        else
        {

            var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
            var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
            var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
            tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
            quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
            var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
            SortCard(ref properList);

            while(properList.Count>0)
            {
                var nowList = properList.Take(topLength).ToList();
                if (nowList.Count != topLength) break;
                var nowTrick = MakeTrick(nowList);
                if(nowTrick.trickType == TrickType.ConsecutivePair&&nowTrick.trickValue>topValue)
                {
                    actionMask.SetActionEnabled(actionIdx, nowOffset + nowTrick.trickValue - topLength / 2 - 1, true);
                }
                properList = properList.Skip(2).ToList();
            }
        }
    }
    public void ReleaseValidStraight(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength)
    {
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards) != null ? true : false;
        int nowOffset = GetStraightOffset(topLength);
        
        if (havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal <=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;
                var cardList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList();
                while (cardList.Count > 0)
                {
                    var nowList = cardList.Take(topLength).ToList();
                    if (nowList.Count != topLength) break; // 짤랐는데 길이가 안나온다면 break.
                    if (IsStraight(nowList) && nowList.Last().value > topValue) //스트레이트이고 topValue 보다 크다면
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                    }
                    cardList = cardList.Skip(1).ToList(); // 맨 앞에 하나 짜른다.
                }
            }
        }
        else
        {
            // 아이디어: 중복을 제외하고 정렬. 앞에서부터 길이만큼 자른다.
            // 자른게 스트레이트이고, 맨 뒤의 카드의 value 가 topValue 보다 크다면 유효한 스트레이트. 그것을 true 로 세팅.
            var cardList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList();
            while (cardList.Count>0)
            {
                var nowList = cardList.Take(topLength).ToList();
                if (nowList.Count != topLength) break; // 짤랐는데 길이가 안나온다면 break.
                if(IsStraight(nowList)&&nowList.Last().value>topValue) //스트레이트이고 topValue 보다 크다면
                {
                    actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                }
                cardList = cardList.Skip(1).ToList(); // 맨 앞에 하나 짜른다.
            }
        }
    }
    public void ReleaseValidFullHouse(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards) != null ? true : false;

        if(havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal<=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                int nowOffset = fullHouseTrickOffset;
                nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue 가 2 라면 57, 3이라면 69, ....
                for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
                {
                    var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                    if (tripleList.Count < 3) continue;
                    else //오케이. 트리플은 갖춰졌다. 페어를 확인할 차례.
                    {
                        for (int botVal = 2; botVal <= 14; ++botVal)
                        {
                            if (botVal == topVal) continue;
                            else
                            {
                                var pairList = player.cards.Where(x => x.value == botVal).ToList();
                                if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,botVal), true);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            int nowOffset = fullHouseTrickOffset;
            nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue 가 2 라면 57, 3이라면 69, ....
            for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
            {
                var tripleList = player.cards.Where(x=>x.value == topVal).ToList();
                if (tripleList.Count < 3) continue;
                else //오케이. 트리플은 갖춰졌다. 페어를 확인할 차례.
                {
                    for(int botVal = 2; botVal<=14; ++botVal)
                    {
                        if (botVal == topVal) continue;
                        else
                        {
                            var pairList = player.cards.Where(x => x.value == botVal).ToList();
                            if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,botVal), true);
                        }
                    }
                }
            }
        }
    }
    public void ReleaseValidFourCardBomb(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        GameManager.instance.RestorePhoenixValue();

        var fourCardList = (from n in player.cards where player.cards.Count(x => x.value == n.value) == 4 select n).ToList();
        SortCard(ref fourCardList);

        while(fourCardList.Count>0)
        {
            var nowList = fourCardList.Take(4).ToList();
            if (nowList[0].value > topValue) actionMask.SetActionEnabled(actionIdx, fourCardTrickOffset + nowList[0].value - 2, true);
            fourCardList = fourCardList.Skip(4).ToList();
        }

    }
    public void ReleaseValidStraightFlushBomb(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength)
    {
        // 스트레이트 플러쉬 폭탄의 주의점:
        // 스트레이트 플러쉬 폭탄은 
        // 1. 먼저 주어진 길이에 맞으면서 topValue가 더 높은 것을 찾고,
        // 2. 추가로 topValue 와 관계없이 길이가 더 긴것을 찾아야 한다.
        GameManager.instance.RestorePhoenixValue();

        var beanCardList   = player.cards.Where(x => x.type == CardType.Bean  ).ToList();
        var flowerCardList = player.cards.Where(x => x.type == CardType.Flower).ToList();
        var shuCardList    = player.cards.Where(x => x.type == CardType.Shu   ).ToList();
        var moonCardList   = player.cards.Where(x => x.type == CardType.Moon  ).ToList();

        SortCard(ref beanCardList);
        SortCard(ref flowerCardList);
        SortCard(ref shuCardList);
        SortCard(ref moonCardList);

        int length = topLength;
        while(length<=13) // 스플 폭탄의 최대 길이는 13.
        {
            int nowOffset = GetStraightFlushTrickOffset(length);

            int skipIdx = 0;
            while(true)
            {
                var nowList = beanCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if(IsStraight(nowList))
                {
                    if((length == topLength&&nowList.Last().value>topValue)||length>topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while(true)
            {
                var nowList = flowerCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while(true)
            {
                var nowList = shuCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while(true)
            {
                var nowList = moonCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }
            ++length;
        }
    }

    public void ReleaseValidSingleWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //참새의 소원에 걸렸음이 보장되어 있음.
        //birdWish 카드를 최소 1장 가지고 있음이 보장되어 있음.
        //싱글이므로 birdWish에 해당하는 싱글을 그냥 release 해주면 됨.

        if(topValue<birdWish)
            actionMask.SetActionEnabled(actionIdx, birdWish, true);
    }

    public void ReleaseValidPairWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //참새의 소원에 걸렸음이 보장되어 있음.
        // birdWish 카드를 최소 1장 가지고 있음이 보장되어 있음.
        // 봉황이 있을 경우 birdWish 로 값을 맞춰주고 페어를 확인, 릴리즈.
        if (topValue < birdWish)
        {
            if (FindPhoenix(player.cards) != null) GameManager.instance.phoenix.value = birdWish;

            var pairList = player.cards.Where(x => x.value == birdWish).ToList();
            if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, pairTrickOffset + birdWish - 2, true); //2페어는 offset(31), 3페어는 offset+1(32)...
        }
    }
    public void ReleaseValidTripleWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //참새의 소원에 걸렸음이 보장되어 있음.
        // birdWish 카드를 최소 1장 가지고 있음이 보장되어 있음.
        // 봉황이 있을 경우 birdWish 로 값을 맞춰주고 트리플을 확인, 릴리즈.
        if (topValue < birdWish)
        {
            if (FindPhoenix(player.cards) != null) GameManager.instance.phoenix.value = birdWish;

            var tripleList = player.cards.Where(x => x.value == birdWish).ToList();

            if (tripleList.Count >= 3) actionMask.SetActionEnabled(actionIdx, tripleTrickOffset + birdWish - 2, true);
        }
    }

    public void ReleaseValidFullHouseWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //참새의 소원에 걸렸음이 보장되어 있음.
        // birdWish 카드를 최소 1장 가지고 있음이 보장되어 있음.
        // 2 또는 topValue+1 부터 트리플 확인. 
        // 분기1: 트리플이 birdWish가 아닌 경우는 페어가 birdWish.
        // 분기2: 트리플이 birdWish인 경우 페어는 birdWish 제외한 모두.
        // 봉황을 가지고 있을 경우 최외각에 봉황 루프 삽입.


        bool havePhoenix = FindPhoenix(player.cards)!=null?true:false;
    
        if(havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal<=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                int nowOffset = fullHouseTrickOffset;
                nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue가 2 라면 3 트리플부터 즉 69,topValue가 3이라면 4트리플부터 즉 81, ....

                for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
                {
                    var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                    if (tripleList.Count < 3) continue;
                    else //오케이. 트리플은 갖춰졌다. 페어를 확인할 차례.
                    {
                        //분기1: 그 트리플이 birdWish 인가? 그럼 페어는 모두 가능.

                        if(topVal==birdWish)
                        {
                            for (int botVal = 2; botVal <= 14; ++botVal)
                            {
                                if (botVal == topVal) continue;
                                else
                                {
                                    var pairList = player.cards.Where(x => x.value == botVal).ToList();
                                    if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,botVal), true);
                                }
                            }
                        }

                        //분기2: 그 트리플이 birdWish 가 아닌가? 그럼 페어는 무조건 birdWish.
                        else
                        {
                            var pairList = player.cards.Where(x => x.value == birdWish).ToList();
                            if(pairList.Count>=2)
                            {
                                actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,birdWish), true);
                            }
                        }
                    }
                }
            }
        }
        else
        {

            int nowOffset = fullHouseTrickOffset;
            nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue가 2 라면 3 트리플부터 즉 69,topValue가 3이라면 4트리플부터 즉 81, ....

            for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
            {
                var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                if (tripleList.Count < 3) continue;
                else //오케이. 트리플은 갖춰졌다. 페어를 확인할 차례.
                {
                    //분기1: 그 트리플이 birdWish 인가? 그럼 페어는 모두 가능.

                    if (topVal == birdWish)
                    {
                        for (int botVal = 2; botVal <= 14; ++botVal)
                        {
                            if (botVal == topVal) continue;
                            else
                            {
                                var pairList = player.cards.Where(x => x.value == botVal).ToList();
                                if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,botVal), true);
                            }
                        }
                    }

                    //분기2: 그 트리플이 birdWish 가 아닌가? 그럼 페어는 무조건 birdWish.
                    else
                    {
                        var pairList = player.cards.Where(x => x.value == birdWish).ToList();
                        if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + GetFullHousePairOffset(topVal,birdWish), true);
                    }
                }
            }
        }    
    
    }

    public void ReleaseValidStraightWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength, int birdWish)
    {
        int nowOffset = GetStraightOffset(topLength);

        if(FindPhoenix(player.cards)!=null)
        {
            //봉황이 있는 경우 최외각에 봉황 루프 삽입.

            for (int phoenixVal = 2; phoenixVal <= 14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                var properList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList(); //중복 제거.

                while(properList.Count>0)
                {
                    var nowList = properList.Take(topLength).ToList();
                    if (nowList.Count != topLength) break;

                    if(IsStraight(nowList)&&nowList.Any(x=>x.value==birdWish)&&nowList.Last().value>topValue)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                    }
                    properList = properList.Skip(1).ToList();
                }
            }

        }
        else
        {
            var properList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList(); //중복 제거.

            while (properList.Count > 0)
            {
                var nowList = properList.Take(topLength).ToList();
                if (nowList.Count != topLength) break;

                if (IsStraight(nowList) && nowList.Any(x => x.value == birdWish) && nowList.Last().value > topValue)
                {
                    actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                }
                properList = properList.Skip(1).ToList();
            }
        }
    }
    public void ReleaseValidConsecutivePairWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue,int topLength, int birdWish)
    {
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards) != null ? true : false;

        int nowOffset = GetConsecutivePairOffset(topLength);

        if (havePhoenix)
        {
            for (int phoenixVal = 2; phoenixVal <= 14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
                var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
                var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
                tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
                quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
                var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
                SortCard(ref properList);

                while (properList.Count > 0)
                {
                    var nowList = properList.Take(topLength).ToList();
                    if (nowList.Count != topLength) break;
                    var nowTrick = MakeTrick(nowList);
                    if (nowTrick.trickType == TrickType.ConsecutivePair && nowTrick.cards.Any(x=>x.value == birdWish) &&nowTrick.trickValue > topValue)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowTrick.trickValue - topLength / 2 - 1, true);
                    }
                    properList = properList.Skip(2).ToList();
                }
            }
        }
        else
        {

            var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
            var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
            var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
            tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
            quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
            var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
            SortCard(ref properList);

            while (properList.Count > 0)
            {
                var nowList = properList.Take(topLength).ToList();
                if (nowList.Count != topLength) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.ConsecutivePair && nowTrick.cards.Any(x=>x.value == birdWish) &&nowTrick.trickValue > topValue)
                {
                    actionMask.SetActionEnabled(actionIdx, nowOffset + nowTrick.trickValue - topLength / 2 - 1, true);
                }
                properList = properList.Skip(2).ToList();
            }
        }
    }
    public void ReleaseValidFourCardBombWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        if (birdWish > topValue)
        {
            GameManager.instance.RestorePhoenixValue();
            var quadList = player.cards.Where(x => x.value == birdWish).ToList();
            if (quadList.Count == 4) actionMask.SetActionEnabled(actionIdx, fourCardTrickOffset + birdWish - 2, true);
        } 
    }
    public void ReleaseValidStraightFlushBombWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength, int birdWish)
    {
        // 스트레이트 플러쉬 폭탄의 주의점:
        // 스트레이트 플러쉬 폭탄은 
        // 1. 먼저 주어진 길이에 맞으면서 topValue가 더 높은 것을 찾고,
        // 2. 추가로 topValue 와 관계없이 길이가 더 긴것을 찾아야 한다.
        GameManager.instance.RestorePhoenixValue();

        var beanCardList = player.cards.Where(x => x.type == CardType.Bean).ToList();
        var flowerCardList = player.cards.Where(x => x.type == CardType.Flower).ToList();
        var shuCardList = player.cards.Where(x => x.type == CardType.Shu).ToList();
        var moonCardList = player.cards.Where(x => x.type == CardType.Moon).ToList();

        SortCard(ref beanCardList);
        SortCard(ref flowerCardList);
        SortCard(ref shuCardList);
        SortCard(ref moonCardList);

        int length = topLength;
        while (length <= 13) // 스플 폭탄의 최대 길이는 13.
        {
            int nowOffset = GetStraightFlushTrickOffset(length);

            int skipIdx = 0;
            while (true)
            {
                var nowList = beanCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList)&&nowList.Any(x=>x.value == birdWish))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while (true)
            {
                var nowList = flowerCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList)&& nowList.Any(x => x.value == birdWish))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while (true)
            {
                var nowList = shuCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList)&& nowList.Any(x => x.value == birdWish))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }

            skipIdx = 0;
            while (true)
            {
                var nowList = moonCardList.Skip(skipIdx).Take(length).ToList();
                if (nowList.Count != length) break;
                if (IsStraight(nowList)&& nowList.Any(x => x.value == birdWish))
                {
                    if ((length == topLength && nowList.Last().value > topValue) || length > topLength)
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - length - 1, true);
                    }
                }
                skipIdx++;
            }
            ++length;
        }
    }

    public int EncodeTrickToNumber(Trick trick)
    {
        int ret;
        switch(trick.trickType)
        {
            case TrickType.Single: //1~29까지 있다.
                //용부터 먼저 체크.
                if (trick.cards[0].type == CardType.Dragon) ret = dragonTrickOffset;
                else
                {
                    //그다음 봉황 체크.
                    if(trick.cards[0].type==CardType.Phoenix)
                    {
                        //3이면 15, 5라면 16.. 29라면 28.
                        ret = phoenixSingleTrickOffset + trick.trickValue / 2 - 1; 
                    }
                    else
                    {
                        //평범한 싱글.
                        ret = trick.trickValue / 2;
                    }
                }
                break;

            case TrickType.Dog: //30.
                ret = dogTrickOffset;
                break;

            case TrickType.Pair://31~43까지 있다.
                ret = pairTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.Triple://44~56까지 있다.
                ret = tripleTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.FullHouse: //57~212까지 있다.
                ret = fullHouseTrickOffset + (trick.trickValue - 2) * 12;
                //트리플 오프셋까지 구했다. 카드를 보고 페어를 찾아 페어 오프셋을 구한다.
                int pairValue = 0;


                if(FindPhoenix(trick.cards)!=null)
                {
                    //풀하우스에 봉황이 있고 값이 변경되었을 수 있다.
                    //관련 로직을 작성해준다.
                    GameManager.instance.RestorePhoenixValue();
                    var withoutPhoenixList = trick.cards.Where(x => x.type != CardType.Phoenix).ToList();
                    foreach(var card in withoutPhoenixList)
                    {
                        if(card.value != trick.trickValue)
                        {
                            pairValue = card.value;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var card in trick.cards)
                    {
                        if (card.value != trick.trickValue)
                        {
                            pairValue = card.value;
                            break;
                        }
                    }
                }
                ret += GetFullHousePairOffset(trick.trickValue, pairValue);
                break;

            case TrickType.Straight: //213~ 267까지 있다.
                ret = GetStraightOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength;
                break;

            case TrickType.ConsecutivePair: //268~324까지 있다.
                ret = GetConsecutivePairOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength / 2 - 1;
                break;

            case TrickType.FourCardBomb: //325~337까지 있다.
                ret = fourCardTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.StraightFlushBomb: //338~382까지 있다.
                ret = GetStraightFlushTrickOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength - 1;
                break;

            default:
                Debug.LogError("불가능한 관찰");
                break;
        }

        return 0;
    }

    public List<Card> DecodeNumberToTrick(int code)
    {
        //코드를 트릭으로 디코딩.
        //
        //이슈: 스트레이트 플러쉬가 있고, 그것을 최대한 부서지지 않게 해야한다.
        //
        //아이디어: 스트레이트 플러쉬 카드를 제외하고 카드를 만들어본다.
        // 제외하고 만들 수 있다면: ok.
        // 제외하고 만들 수 없다면:
        // 분기1: 1개가 모자라고 봉황을 가지고 있다면: 1개를 봉황으로 대체.
        // 분기2: 1개가 모자라는데 봉황이 없거나 2개 이상 모자라면: 스트레이트 플러쉬를 깨고 트릭을 만든다.
        

        List<Card> ret = new List<Card>();
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards)!=null?true:false;

        List<Card> nowList = player.cards.ToList(); // nowList로 카드를 만든다.

        var sfTrick = GameManager.instance.FindAnyStraightFlushBomb(nowList);
        List<Card> straightFlushCards = new List<Card>();
        if (sfTrick != null)
        {
            foreach (var card in sfTrick.cards) nowList.Remove(card); // 스플 폭탄이 있다면 그 카드는 제외.
            straightFlushCards = sfTrick.cards.ToList();
        }

        if (code == 0)
        {
            //패스. ret 은 비어있음.
            return ret;
        }
        else if (code < phoenixSingleTrickOffset) //일반 싱글
        {
            foreach (var card in nowList)
            {
                if (card.value == code)
                {
                    ret.Add(card);
                    break;
                }
            }
            if (ret.Count == 0)
            {
                foreach (var card in straightFlushCards)
                {
                    if (card.value == code)
                    {
                        ret.Add(card);
                        break;
                    }
                }
            }
            if (ret.Count == 0) Debug.LogError("불가능한 싱글 디코딩");
        }
        else if (code < dragonTrickOffset) //봉황 싱글
        {
            foreach (var card in nowList)
            {
                if (card.type == CardType.Phoenix)
                {
                    card.value = code - phoenixSingleTrickOffset + 1;
                    ret.Add(card);
                    break;
                }
            }
        }
        else if (code == dragonTrickOffset) // 용 싱글
        {
            foreach (var card in nowList)
            {
                if (card.type == CardType.Dragon)
                {
                    ret.Add(card);
                    break;
                }
            }
        }
        else if (code == dogTrickOffset) //개 싱글
        {
            foreach (var card in nowList)
            {
                if (card.type == CardType.Dog)
                {
                    ret.Add(card);
                    break;
                }
            }
        }
        else if (code < tripleTrickOffset) // 페어
        {
            int value = code - pairTrickOffset + 2;
            if (havePhoenix)
            {
                SortCard(ref nowList);
                GameManager.instance.phoenix.value = value;
            }

            int cnt = 2;

            foreach (var card in nowList)
            {
                if (card.value == value)
                {
                    ret.Add(card);
                    --cnt;
                    if (cnt == 0) break;
                }
            }

            if (cnt > 0)
            {
                foreach (var card in straightFlushCards)
                {
                    if (card.value == value)
                    {
                        ret.Add(card);
                        --cnt;
                        if (cnt == 0) break;
                    }
                }
            }

            if (cnt > 0) Debug.LogError("불가능한 페어 디코딩");

        }
        else if (code < fullHouseTrickOffset) // 트리플
        {
            int value = code - tripleTrickOffset + 2;
            if (havePhoenix)
            {
                SortCard(ref nowList);
                GameManager.instance.phoenix.value = value;
            }

            int cnt = 3;

            foreach (var card in nowList)
            {
                if (card.value == value)
                {
                    ret.Add(card);
                    --cnt;
                    if (cnt == 0) break;
                }
            }

            if (cnt > 0)
            {
                foreach (var card in straightFlushCards)
                {
                    if (card.value == value)
                    {
                        ret.Add(card);
                        --cnt;
                        if (cnt == 0) break;
                    }
                }
            }

            if (cnt > 0) Debug.LogError("불가능한 트리플 디코딩");
        }
        else if (code < straightTrickOffset[0]) //풀하우스
        {
            int tripleValue = ((code - fullHouseTrickOffset) / 12) + 2; //0~11은 2, 12~23은 3, ... 
            int pairValue = GetFullHousePairValue((code - fullHouseTrickOffset) % 12, tripleValue);

            var pairList = nowList.Where(x => x.value == pairValue).Take(2).ToList();
            var tripleList = nowList.Where(x => x.value == tripleValue).Take(3).ToList();

            int totalNumberOfCards = pairList.Count + tripleList.Count;

            if (totalNumberOfCards == 5) //만족.
            {
                ret.AddRange(pairList);
                ret.AddRange(tripleList);
            }
            else
            {
                if (totalNumberOfCards == 4 && havePhoenix)
                {

                    if (pairList.Count == 1)
                    {
                        GameManager.instance.phoenix.value = pairValue;
                    }
                    else if (tripleList.Count == 2)
                    {
                        GameManager.instance.phoenix.value = tripleValue;
                    }
                    else Debug.LogError("불가능한 풀하우스 봉황 디코딩");
                    ret.AddRange(pairList);
                    ret.AddRange(tripleList);
                    ret.Add(GameManager.instance.phoenix);
                }
                else
                {
                    int restPairCount = 2 - pairList.Count;
                    int restTripleCount = 3 - tripleList.Count;

                    foreach (var cards in straightFlushCards)
                    {
                        if (cards.value == pairValue && restPairCount>0)
                        {
                            pairList.Add(cards);
                            --restPairCount;
                            if (restPairCount == 0) break;
                        }
                    }

                    foreach (var cards in straightFlushCards)
                    {
                        if (cards.value == tripleValue&&restTripleCount>0)
                        {
                            tripleList.Add(cards);
                            --restTripleCount;
                            if (restTripleCount == 0) break;
                        }
                    }

                    if (restTripleCount + restPairCount == 0)
                    {
                        ret.AddRange(pairList);
                        ret.AddRange(tripleList);
                    }
                    else
                    {
                        if (restPairCount + restTripleCount > 1) Debug.LogError("불가능한 풀하우스 디코딩");
                        else if (havePhoenix)
                        {
                            ret.AddRange(pairList);
                            ret.AddRange(tripleList);
                            ret.Add(GameManager.instance.phoenix);
                            if (restPairCount == 1)
                            {
                                GameManager.instance.phoenix.value = pairValue;
                            }
                            else
                            {
                                GameManager.instance.phoenix.value = tripleValue;
                            }
                        }
                        else Debug.LogError("불가능한 풀하우스 선택.");
                    }
                }
            }
            if (ret.Count != 5) Debug.LogError("풀하우스 선택 오류.");
        }
        else if (code < consecutivePairTrickOffset[0]) //스트레이트.
        {
            int length = GetStraightLength(code);
            int topValue = GetStraightValue(code);
            int bottomValue = topValue - length + 1;
            bool[] checkArr = new bool[15];

            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = true;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = false;

            for (int i = bottomValue; i <= topValue; ++i)
            {
                if (checkArr[i] == false)
                {
                    foreach (var card in nowList)
                    {
                        if (card.value == i)
                        {
                            ret.Add(card);
                            checkArr[i] = true;
                            break;
                        }
                    }
                }
            }

            int restCnt = checkArr.Where(x => x == false).ToList().Count;

            if (restCnt == 0) // 만족.
            {

            }

            else if (restCnt == 1 && havePhoenix) // 하나 비었지만 봉황이 있음.
            {
                int restIdx = -1;
                for(int idx = bottomValue; idx<=topValue; ++idx) if(checkArr[idx]==false) { restIdx = idx; break; }

                if (GameManager.instance.birdWishValue == restIdx&& 
                    (nowList.Any(x=>x.value == GameManager.instance.birdWishValue||straightFlushCards.Any(y=>y.value == GameManager.instance.birdWishValue)))) 
                    //하나 비었어도 그게 참새의 소원이고 그걸 가지고 있다면 봉황으로 대체 불가능. 스플 깨서 넣는다.
                {

                    foreach (var card in straightFlushCards)
                    {
                        if (card.value == restIdx)
                        {
                            ret.Add(card);
                            checkArr[restIdx] = true;
                            break;
                        }
                    }

                }
                else
                {

                    ret.Add(GameManager.instance.phoenix);
                    for (int i = bottomValue; i <= topValue; ++i)
                    {
                        if (checkArr[i] == false)
                        {

                            GameManager.instance.phoenix.value = i;
                            checkArr[i] = true;
                            break;
                        }
                    }
                }
            }
            else //두개 이상 비음. 일단 스플 깨서 넣어본다.
            {
                for (int i = bottomValue; i <= topValue; ++i)
                {
                    if (checkArr[i] == false)
                    {
                        foreach (var card in straightFlushCards)
                        {
                            if (card.value == i)
                            {
                                ret.Add(card);
                                checkArr[i] = true;
                                break;
                            }
                        }
                    }
                }
                restCnt = checkArr.Where(x => x == false).ToList().Count;

                if (restCnt == 0) //만족.
                {

                }
                else if (restCnt == 1 && havePhoenix)
                {
                    ret.Add(GameManager.instance.phoenix);
                    for (int i = bottomValue; i <= topValue; ++i)
                    {
                        if (checkArr[i] == false)
                        {

                            GameManager.instance.phoenix.value = i;
                            checkArr[i] = true;
                            break;
                        }
                    }
                }
                else Debug.LogError("불가능한 스트레이트 디코딩.");

            }
            if (ret.Count != length) Debug.LogError("불가능한 스트레이트 디코딩. 길이 안맞음.");
        }
        else if (code < fourCardTrickOffset) //연속페어.
        {
            int length = GetConsecutivePairLength(code);
            int topValue = GetConsecutivePairValue(code);
            int bottomValue = topValue - length / 2 + 1;

            int[] checkArr = new int[15];
            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = 0;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = 2;

            for(int i=bottomValue; i<=topValue; ++i)
            {
                foreach(var card in nowList)
                {
                    if(card.value == i&&checkArr[i]>0)
                    {
                        --checkArr[i];
                        ret.Add(card);
                        if (checkArr[i] == 0) break;
                    }
                }
            }

            int restCnt = checkArr.Sum();

            if(restCnt==0) //만족.
            {

            }
            else if(restCnt == 1 && havePhoenix) // 하나 비는데 봉황이 있다면
            {
                ret.Add(GameManager.instance.phoenix);
                for(int i = bottomValue; i<=topValue; ++i)
                {
                    if(checkArr[i]>0)
                    {
                        GameManager.instance.phoenix.value = i;
                        break;
                    }
                }
            }
            else
            {
                for(int i = bottomValue; i<=topValue; ++i)
                {
                    if(checkArr[i]>0)
                    {
                        foreach(var card in straightFlushCards)
                        {
                            if(card.value == i&&checkArr[i]>0)
                            {
                                --checkArr[i];
                                ret.Add(card);
                                if (checkArr[i] == 0) break;
                            }
                        }
                    }
                }

                restCnt = checkArr.Sum();
                if(restCnt == 0)
                {

                }
                else if (restCnt == 1 && havePhoenix)
                {
                    ret.Add(GameManager.instance.phoenix);
                    for (int i = bottomValue; i <= topValue; ++i)
                    {
                        if (checkArr[i] > 0)
                        {
                            GameManager.instance.phoenix.value = i;
                            break;
                        }
                    }
                }
                else Debug.LogError("불가능한 연속 페어 디코딩.");
            }
            if (ret.Count != length) Debug.LogError("불가능한 연속 페어 선택. 길이 안맞음.");

        }
        else if (code < straightFlushTrickOffset[0]) //포카드 폭탄.
        {
            //포카드 폭탄은 스플 폭탄이 깨지는 예외처리가 필요없음.
            //nowList에 합친다.
            nowList.AddRange(straightFlushCards);
            int value = code - fourCardTrickOffset + 2;
            var fourCardList = nowList.Where(x => x.value == value).ToList();
            if (fourCardList.Count != 4) Debug.LogError("불가능한 포카드 선택. 길이 안맞음.");

            ret.AddRange(fourCardList);

        }
        else // 스플 폭탄.
        {
            //스플 폭탄은 스플 폭탄이 깨지는 예외처리가 필요없음.
            //nowList에 합친다.
            nowList.AddRange(straightFlushCards);
            
            int length = GetStraightFlushLength(code);
            int topValue = GetStraightFlushValue(code);
            int bottomValue = topValue - length + 1;
            bool[] checkArr = new bool[15];


            var beanCardList = nowList.Where(x => x.type == CardType.Bean).ToList();
            var flowerCardList = nowList.Where(x => x.type == CardType.Flower).ToList();
            var shuCardList = nowList.Where(x => x.type == CardType.Shu).ToList();
            var moonCardList = nowList.Where(x => x.type == CardType.Moon).ToList();

            SortCard(ref beanCardList);
            SortCard(ref flowerCardList);
            SortCard(ref shuCardList);
            SortCard(ref moonCardList);

            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = true;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = false;

            for(int i= bottomValue; i<=topValue; ++i)
            {
                if(checkArr[i]==false)
                {
                    foreach (var card in beanCardList)
                    {
                        if (card.value == i)
                        {
                            checkArr[i] = true;
                            ret.Add(card);
                            break;
                        }
                    }
                    
                }
            }

            if (ret.Count == length) goto StraightFlushEnd;
            ret.Clear();

            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = true;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = false;

            for (int i = bottomValue; i <= topValue; ++i)
            {
                if (checkArr[i] == false)
                {
                    foreach (var card in flowerCardList)
                    {
                        if (card.value == i)
                        {
                            checkArr[i] = true;
                            ret.Add(card);
                            break;
                        }
                    }
                }
            }

            if (ret.Count == length) goto StraightFlushEnd;
            ret.Clear();

            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = true;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = false;

            for (int i = bottomValue; i <= topValue; ++i)
            {
                if (checkArr[i] == false)
                {
                    foreach (var card in shuCardList)
                    {
                        if (card.value == i)
                        {
                            checkArr[i] = true;
                            ret.Add(card);
                            break;
                        }
                    }
                }
            }

            if (ret.Count == length) goto StraightFlushEnd;
            ret.Clear();

            for (int i = 0; i < checkArr.Length; ++i) checkArr[i] = true;
            for (int i = bottomValue; i <= topValue; ++i) checkArr[i] = false;

            for (int i = bottomValue; i <= topValue; ++i)
            {
                if (checkArr[i] == false)
                {
                    foreach (var card in moonCardList)
                    {
                        if (card.value == i)
                        {
                            checkArr[i] = true;
                            ret.Add(card);
                            break;
                        }                   }
                }
            }

            if (ret.Count == length) goto StraightFlushEnd;
            ret.Clear();


        StraightFlushEnd:
            if (ret.Count != length) Debug.LogError("불가능한 스플 폭탄. 길이 안맞음.");
        }

        return ret;
    }
}
