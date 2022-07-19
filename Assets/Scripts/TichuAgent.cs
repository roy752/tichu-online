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
    public GamePlayer player;
    public override void Initialize()
    {
        isActionEnd = false;
        player = GetComponent<GamePlayer>();
    }

    public override void OnEpisodeBegin()
    {
        isActionEnd = false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //현재 어떤 상태인가? 1 of 10.
        sensor.AddOneHotObservation((int)GameManager.instance.currentPhase, (int)Util.PhaseType.NumberOfPhase);

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
        else sensor.AddOneHotObservation(GameManager.instance.trickStack.Peek().playerIdx, Util.numberOfPlayers);

        // 현재 누구의 차례인가? 1 of 4.
        sensor.AddOneHotObservation(GameManager.instance.currentTrickPlayerIdx, Util.numberOfPlayers);

        // 현재 남은 손패의 개수 - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowHand = (float)(GameManager.instance.players[nowIdx].cards.Count) / Util.numberOfCardsPlay;
            sensor.AddObservation(nowHand);
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
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // 액션을 일단은 모두 prohibit.
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
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, actionIdx, false); //용 트릭 전달 선택 액션 


        switch (GameManager.instance.currentPhase)
        {
            case Util.PhaseType.LargeTichuSelectionPhase:
                actionIdx = 0;
                actionMask.SetActionEnabled(actionIdx, 0, true); //라지 티츄 패스 활성화
                actionMask.SetActionEnabled(actionIdx, 1, true); //라지 티츄 선택 활성화
                actionMask.SetActionEnabled(actionIdx, 2, false);
                break;
            
            case Util.PhaseType.SmallTichuSelectionPhase: //스몰 티츄 선택 페이즈의 RequestDecision 은 canDeclareSmallTichu 가 true 임이 보장됨.
                actionIdx = 1;
                actionMask.SetActionEnabled(actionIdx, 0, true); // 스몰 티츄 패스 활성화
                actionMask.SetActionEnabled(actionIdx, 1, true); // 스몰 티츄 선택 활성화
                actionMask.SetActionEnabled(actionIdx, 2, false);
                break;
            
            case Util.PhaseType.ExchangeSelection1Phase:
            case Util.PhaseType.ExchangeSelection2Phase:
            case Util.PhaseType.ExchangeSelection3Phase: // 다음 3개는 phase 의 관측으로 구분된다.
                actionIdx = 2;
                foreach (var card in player.cards) actionMask.SetActionEnabled(actionIdx, card.id, true);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfCards, false); // 56은 선택 없음. 이를 비활성화.
                break;
            
            case Util.PhaseType.FirstTrickSelectionPhase:
            case Util.PhaseType.TrickSelectionPhase:
            case Util.PhaseType.BombSelectionPhase:
                actionIdx = 3;
                ReleaseValidTrick(actionIdx, actionMask); // 너무 방대함. 분리할 것.
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);
                break;

            case Util.PhaseType.BirdWishSelectionPhase:
                actionIdx = 4;
                for(int idx = 0; idx<Util.numberOfBirdWish; ++idx) // 0 부터 14 까지
                {
                    if (idx != 1) actionMask.SetActionEnabled(actionIdx, idx, true);
                }
                actionMask.SetActionEnabled(actionIdx, Util.numberOfBirdWish, false);
                break;

            case Util.PhaseType.DragonSelectionPhase:
                actionIdx = 5;
                actionMask.SetActionEnabled(actionIdx, 0, true);
                actionMask.SetActionEnabled(actionIdx, 1, true);
                actionMask.SetActionEnabled(actionIdx, 2, false);
                break;
        }


    }
    public void ReleaseValidTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // 선택 가능한 트릭은 true로 풀어주는 메소드.
        //
        // 쟁점: 참새의 소원에 걸리는가? 현재 첫 트릭인가? bomb phase 인가?
        //
        // 분기1: 참새의 소원에 걸리면서 첫 트릭
        // 분기2: 참새의 소원에 걸리면서 첫 트릭이 아님
        // 분기3: 참새의 소원에 안걸리면서 첫 트릭
        // 분기4: 참새의 소원이 안걸리면서 첫 트릭이 아님

        GameManager.instance.RestorePhoenixValue();

        if (Util.IsPlayerHaveToFulfillBirdWish(player) != null) // 참새의 소원에 걸리는 경우
        {
            actionMask.SetActionEnabled(actionIdx, 0, false); // 참새의 소원에 걸린다면 일단 패스는 불가능. 

            if (GameManager.instance.isFirstTrick) // 첫 트릭인 경우
            {
                // 첫 트릭이지만 참새의 소원에 걸리므로 개는 낼 수 없다.
            }
            else
            {

            }
        }
        else //참새의 소원에 걸리지 않는 경우
        {
            if (GameManager.instance.isFirstTrick) // 첫 트릭인 경우
            {
                actionMask.SetActionEnabled(actionIdx, dogTrickOffset, true); // 참새 안걸리고 첫 트릭이면 개를 낼 수 있다.

                // 가능한 싱글, 페어, 트리플, 연속 페어, 스트레이트, 풀하우스, 포카드 폭탄, 스플 폭탄을 release 해준다.

            }
            else
            {
                var topTrick = GameManager.instance.trickStack.Peek();
                switch (GameManager.instance.trickStack.Peek().trickType)
                {
                    case TrickType.Single:
                        ReleaseValidSingle(actionIdx, actionMask, topTrick.cards[0].value); // 봉황, 용, 새를 포함한 유효 싱글을 마스킹 해제해준다.
                        // 폭탄 2개 모두 release.
                        break;
                    case TrickType.Pair:
                        ReleaseValidPair(actionIdx, actionMask, topTrick.trickValue);
                        // 폭탄 2개 모두 release.
                        break;
                    case TrickType.Triple:
                        ReleaseValidTriple(actionIdx, actionMask, topTrick.trickValue);
                        // 폭탄 2개 모두 release.
                        break;
                    case TrickType.ConsecutivePair:
                        ReleaseValidConsecutivePair(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                        // 폭탄 2개 모두 release.
                        break;
                    case TrickType.Straight:
                        ReleaseValidStraight(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                        // 폭탄 2개 모두 release.
                        break;
                    case TrickType.FullHouse:
                        ReleaseValidFullHouse(actionIdx, actionMask, topTrick.trickValue);
                            // 폭탄 2개 모두 release.
                            break;
                    case TrickType.FourCardBomb:
                        ReleaseValidFourCard(actionIdx, actionMask, topTrick.trickValue);
                            // straight flush bomb release.
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
    }

    public void ReleaseValidSingle(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
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
        if(player.cards.Any(x=>x.value == specialCardsValue[3])) //용 관련 예외처리. 용의 값은 specialCardsValue[3] = 18. 이걸 가지고 있다면 용 선택 가능하게 세팅.
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
        if(havePhoenix)
        {

        }
        else
        {

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
                        cardList = cardList.Skip(1).ToList(); // 맨 앞에 하나 짜른다.
                    }
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
                    cardList = cardList.Skip(1).ToList(); // 맨 앞에 하나 짜른다.
                }
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
                        int botOffset = 0;
                        for (int botVal = 2; botVal <= 14; ++botVal)
                        {
                            if (botVal == topVal) continue;
                            else
                            {
                                var pairList = player.cards.Where(x => x.value == botVal).ToList();
                                if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + botOffset, true);

                                ++botOffset;
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
                    int botOffset = 0;
                    for(int botVal = 2; botVal<=14; ++botVal)
                    {
                        if (botVal == topVal) continue;
                        else
                        {
                            var pairList = player.cards.Where(x => x.value == botVal).ToList();
                            if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, nowOffset + botOffset, true);
                               
                            ++botOffset;
                        }
                    }
                }
            }
        }
    }
    public void ReleaseValidFourCard(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {

    }
    public void ReleaseValidStraightFlushBomb(int actionIdx, IDiscreteActionMask actionMask, int topValue, int topLength)
    {

    }
}
