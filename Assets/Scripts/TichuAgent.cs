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
        //현재 어떤 상태인가? 1 of 8.
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
        Debug.Log("마스킹");
        actionMask.SetActionEnabled(2, 1, false);
    }
}
