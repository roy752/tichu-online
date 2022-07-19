using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;
using System;

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

        // 현재 스택의 탑 트릭은 무엇인가? 1 of 383, 이 부분이 핵심.
        
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
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);
                // 추가 필요.
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
}
