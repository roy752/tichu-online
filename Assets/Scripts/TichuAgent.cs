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

        // ���� ������ ž Ʈ���� �����ΰ�? 1 of 383(can be null), �� �κ��� �ٽ�.
        
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
                ReleaseValidTrick(actionIdx, actionMask); // �ʹ� �����. �и��� ��.
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);
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
    public void ReleaseValidTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // ���� ������ Ʈ���� true�� Ǯ���ִ� �޼ҵ�.
        //
        // ����: ������ �ҿ��� �ɸ��°�? ���� ù Ʈ���ΰ�? bomb phase �ΰ�?
        //
        // �б�1: ������ �ҿ��� �ɸ��鼭 ù Ʈ��
        // �б�2: ������ �ҿ��� �ɸ��鼭 ù Ʈ���� �ƴ�
        // �б�3: ������ �ҿ��� �Ȱɸ��鼭 ù Ʈ��
        // �б�4: ������ �ҿ��� �Ȱɸ��鼭 ù Ʈ���� �ƴ�

        GameManager.instance.RestorePhoenixValue();

        if (Util.IsPlayerHaveToFulfillBirdWish(player) != null) // ������ �ҿ��� �ɸ��� ���
        {
            actionMask.SetActionEnabled(actionIdx, 0, false); // ������ �ҿ��� �ɸ��ٸ� �ϴ� �н��� �Ұ���. 

            if (GameManager.instance.isFirstTrick) // ù Ʈ���� ���
            {
                // ù Ʈ�������� ������ �ҿ��� �ɸ��Ƿ� ���� �� �� ����.
            }
            else
            {

            }
        }
        else //������ �ҿ��� �ɸ��� �ʴ� ���
        {
            if (GameManager.instance.isFirstTrick) // ù Ʈ���� ���
            {
                actionMask.SetActionEnabled(actionIdx, dogTrickOffset, true); // ���� �Ȱɸ��� ù Ʈ���̸� ���� �� �� �ִ�.

                // ������ �̱�, ���, Ʈ����, ���� ���, ��Ʈ����Ʈ, Ǯ�Ͽ콺, ��ī�� ��ź, ���� ��ź�� release ���ش�.

            }
            else
            {
                var topTrick = GameManager.instance.trickStack.Peek();
                switch (GameManager.instance.trickStack.Peek().trickType)
                {
                    case TrickType.Single:
                        ReleaseValidSingle(actionIdx, actionMask, topTrick.cards[0].value); // ��Ȳ, ��, ���� ������ ��ȿ �̱��� ����ŷ �������ش�.
                        // ��ź 2�� ��� release.
                        break;
                    case TrickType.Pair:
                        ReleaseValidPair(actionIdx, actionMask, topTrick.trickValue);
                        // ��ź 2�� ��� release.
                        break;
                    case TrickType.Triple:
                        ReleaseValidTriple(actionIdx, actionMask, topTrick.trickValue);
                        // ��ź 2�� ��� release.
                        break;
                    case TrickType.ConsecutivePair:
                        ReleaseValidConsecutivePair(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                        // ��ź 2�� ��� release.
                        break;
                    case TrickType.Straight:
                        ReleaseValidStraight(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                        // ��ź 2�� ��� release.
                        break;
                    case TrickType.FullHouse:
                        ReleaseValidFullHouse(actionIdx, actionMask, topTrick.trickValue);
                            // ��ź 2�� ��� release.
                            break;
                    case TrickType.FourCardBomb:
                        ReleaseValidFourCard(actionIdx, actionMask, topTrick.trickValue);
                            // straight flush bomb release.
                            break;
                    case TrickType.StraightFlushBomb:
                        ReleaseValidStraightFlushBomb(actionIdx, actionMask, topTrick.trickValue, topTrick.trickLength);
                        break;
                    default:
                        Debug.LogError("�Ұ����� ��Ȳ.");
                        break;
                } 
            }
        }
    }

    public void ReleaseValidSingle(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        foreach(var card in player.cards) // �÷��̾ ���� ��� ī�忡 ����
        {
            if (card.value >= 1 && card.value <= 14 && card.value > topValue) // topValue ���� ũ�� 1���� A �� �ϳ��� �ش��Ѵٸ�(�� ���� ����)
                actionMask.SetActionEnabled(actionIdx, card.value, true);
        }
        if(Util.FindPhoenix(player.cards)!=null) // ��Ȳ ���� ���� ó��. ���� �÷��̾ ��Ȳ�� ������ �ִٸ�
        {
            if(GameManager.instance.isFirstTrick) // ù Ʈ���̶��
            {
                actionMask.SetActionEnabled(actionIdx, phoenixSingleTrickOffset, true); // 1.5 �� ���� �����ϰ� ����.
            }
            else
            {
                // �ش��ϴ� ��Ȳ �̱� Ʈ���� true �� ����. value�� 1�̶�� 15(1.5), value�� 2��� 16(2.5), ... value�� 14��� 28(14.5).
                if(GameManager.instance.trickStack.Peek().cards[0].value>=1 && GameManager.instance.trickStack.Peek().cards[0].value<=14)
                    actionMask.SetActionEnabled(actionIdx, phoenixSingleTrickOffset + GameManager.instance.trickStack.Peek().cards[0].value - 1, true);
            }
        }
        if(player.cards.Any(x=>x.value == specialCardsValue[3])) //�� ���� ����ó��. ���� ���� specialCardsValue[3] = 18. �̰� ������ �ִٸ� �� ���� �����ϰ� ����.
        {
            actionMask.SetActionEnabled(actionIdx, dragonTrickOffset, true);
        }
    }
    public void ReleaseValidPair(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        GameManager.instance.RestorePhoenixValue();
        // ���̵��: ��Ȳ�� �ִٸ� �ϳ��� �־��, �ش� ���� 2�� �̻� ��Ÿ���� true�� Ǯ���ش�.
        bool havePhoenix = FindPhoenix(player.cards) != null? true:false;
        for(int val = Mathf.Max(topValue+1,2); val<=14; ++val)
        {
            if (havePhoenix) GameManager.instance.phoenix.value = val;
            // val �� �ش��ϴ� ī���� ������ 2��, �Ǵ� �� �̻��̸� �´� action�� Ǯ���ش�. 2 ���� 31, 3���� 32... 14 ���� 43.
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
            // val �� �ش��ϴ� ī���� ������ 3��, �Ǵ� �� �̻��̸� �´� action�� Ǯ���ش�. 2 Ʈ������ 44, 3���� 45... 14 ���� 56.
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
                    if (nowList.Count != topLength) break; // ©���µ� ���̰� �ȳ��´ٸ� break.
                    if (IsStraight(nowList) && nowList.Last().value > topValue) //��Ʈ����Ʈ�̰� topValue ���� ũ�ٸ�
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                        cardList = cardList.Skip(1).ToList(); // �� �տ� �ϳ� ¥����.
                    }
                }
            }
        }
        else
        {
            // ���̵��: �ߺ��� �����ϰ� ����. �տ������� ���̸�ŭ �ڸ���.
            // �ڸ��� ��Ʈ����Ʈ�̰�, �� ���� ī���� value �� topValue ���� ũ�ٸ� ��ȿ�� ��Ʈ����Ʈ. �װ��� true �� ����.
            var cardList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList();
            while (cardList.Count>0)
            {
                var nowList = cardList.Take(topLength).ToList();
                if (nowList.Count != topLength) break; // ©���µ� ���̰� �ȳ��´ٸ� break.
                if(IsStraight(nowList)&&nowList.Last().value>topValue) //��Ʈ����Ʈ�̰� topValue ���� ũ�ٸ�
                {
                    actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                    cardList = cardList.Skip(1).ToList(); // �� �տ� �ϳ� ¥����.
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
                nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue �� 2 ��� 57, 3�̶�� 69, ....
                for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
                {
                    var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                    if (tripleList.Count < 3) continue;
                    else //������. Ʈ������ ��������. �� Ȯ���� ����.
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
            nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue �� 2 ��� 57, 3�̶�� 69, ....
            for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
            {
                var tripleList = player.cards.Where(x=>x.value == topVal).ToList();
                if (tripleList.Count < 3) continue;
                else //������. Ʈ������ ��������. �� Ȯ���� ����.
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
