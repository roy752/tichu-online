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
        //���� � �����ΰ�? 1 of 8.
        sensor.AddOneHotObservation((int)GameManager.instance.currentPhase, (int)Util.PhaseType.NumberOfPhase);

        // ���� �н��ߴ°� ���ߴ°�? each of 4.
        for(int idx = player.playerNumber; idx<player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].isTrickPassed);
        }

        // ���� �����ߴ°� ���ߴ°�? each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].isTrickSelected);
        }

        // ���� ���� ������ ž�ΰ�? 1 of 4. (can be null.)
        if (GameManager.instance.trickStack.Count == 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else sensor.AddOneHotObservation(GameManager.instance.trickStack.Peek().playerIdx, Util.numberOfPlayers);

        // ���� ������ �����ΰ�? 1 of 4.
        sensor.AddOneHotObservation(GameManager.instance.currentTrickPlayerIdx, Util.numberOfPlayers);

        // ���� ���� ������ ���� - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowHand = (float)(GameManager.instance.players[nowIdx].cards.Count) / Util.numberOfCardsPlay;
            sensor.AddObservation(nowHand);
        }

        //���� ���� ���� - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowRoundScore = (float)(GameManager.instance.players[nowIdx].roundScore) / (float)Util.maximumRoundScore;
            sensor.AddObservation(nowRoundScore);
        }

        //���� �� ���� - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            float nowTotalScore = (float)(GameManager.instance.players[nowIdx].totalScore) / (float)Util.maximumTotalScore;
            sensor.AddObservation(nowTotalScore);
        }

        //���� ���� Ƽ�� ���� ���� - each of 4
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].largeTichuFlag);
        }

        //���� ���� Ƽ�� ���� ���� - each of 4
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddObservation(GameManager.instance.players[nowIdx].smallTichuFlag);
        }

        //���� ������ �ҿ� - 1 of 15 (0: �ҿ� ����. 2-14: �ҿ��� �ش��ϴ� ����. 1�� ������� ����.)
        sensor.AddOneHotObservation(GameManager.instance.birdWishValue, Util.numberOfBirdWish);

        //���� ���� ī���� ��Ȳ - each of 56        
        for(int idx = 0; idx<GameManager.instance.cardMarking.Length; ++idx)
        {
            sensor.AddObservation(GameManager.instance.cardMarking[idx]);
        }
        
        //���� ���� ���� ī���� ��Ȳ - each of 56
        for(int idx = 0; idx<Util.numberOfCards; ++idx)
        {
            sensor.AddObservation(player.cards.Any(x => x.id == idx)); // idx �� id. id�� �ش��ϴ� ī�带 ����(Any) �ϰ� �ִ°�?
        }

        // ���� ������ ž Ʈ���� �����ΰ�? 1 of 383, �� �κ��� �ٽ�.
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        Debug.Log("����ŷ");
        actionMask.SetActionEnabled(2, 1, false);
    }
}
