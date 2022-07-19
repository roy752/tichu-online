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
        //���� � �����ΰ�? 1 of 10.
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
        // �׼��� �ϴ��� ��� prohibit.
        //
        // 1. ���� Ƽ��, ������ 3 (���� Ƽ�� �н�, ���� Ƽ�� ����, ���� ����)
        // 2. ���� Ƽ��, ������ 3 (���� Ƽ�� �н�, ���� Ƽ�� ����, ���� ����)
        // 3. ������ ī��, ������ 57. (ī�� ����, ���� ����)
        // 4. ������ Ʈ��, ������ 384. (Ʈ�� ����, ���� ����) 
        // 5. �ҿ� ����, ������ 16. (�ҿ� ����, ���� ����)
        // 6. �� Ʈ�� ���� ����, ������ 3. (���� ����, ���� ����)
        
        // 1.
        int actionIdx = 0;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //���� Ƽ�� �׼� 

        // 2.
        actionIdx++;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //���� Ƽ�� �׼�

        // 3.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfCards; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //ī�� �����ִ� �׼�

        // 4.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfTrickType; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //Ʈ�� ���� �׼�

        // 5.
        actionIdx++;
        for (int idx = 0; idx < Util.numberOfBirdWish; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //������ �ҿ� ���� �׼�

        // 6.
        actionIdx++;
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, actionIdx, false); //�� Ʈ�� ���� ���� �׼� 


        switch (GameManager.instance.currentPhase)
        {
            case Util.PhaseType.LargeTichuSelectionPhase:
                actionIdx = 0;
                actionMask.SetActionEnabled(actionIdx, 0, true); //���� Ƽ�� �н� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 1, true); //���� Ƽ�� ���� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 2, false);
                break;
            
            case Util.PhaseType.SmallTichuSelectionPhase: //���� Ƽ�� ���� �������� RequestDecision �� canDeclareSmallTichu �� true ���� �����.
                actionIdx = 1;
                actionMask.SetActionEnabled(actionIdx, 0, true); // ���� Ƽ�� �н� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 1, true); // ���� Ƽ�� ���� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 2, false);
                break;
            
            case Util.PhaseType.ExchangeSelection1Phase:
            case Util.PhaseType.ExchangeSelection2Phase:
            case Util.PhaseType.ExchangeSelection3Phase: // ���� 3���� phase �� �������� ���еȴ�.
                actionIdx = 2;
                foreach (var card in player.cards) actionMask.SetActionEnabled(actionIdx, card.id, true);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfCards, false); // 56�� ���� ����. �̸� ��Ȱ��ȭ.
                break;
            
            case Util.PhaseType.FirstTrickSelectionPhase:
            case Util.PhaseType.TrickSelectionPhase:
            case Util.PhaseType.BombSelectionPhase:
                actionIdx = 3;
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);
                // �߰� �ʿ�.
                break;

            case Util.PhaseType.BirdWishSelectionPhase:
                actionIdx = 4;
                for(int idx = 0; idx<Util.numberOfBirdWish; ++idx) // 0 ���� 14 ����
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
