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
                
                //0�� �н�, 1�� ����.
                nowAction = actions.DiscreteActions[0];
                
                if (nowAction == 0) player.SkipLargeTichuCall();
                else player.DeclareLargeTichuCall();
                
                break;
            
            case PhaseType.SmallTichuSelectionPhase:
                
                //0�� �н�, 1�� ����.
                nowAction = actions.DiscreteActions[1];

                if (nowAction == 1) player.DeclareSmallTichuCall();

                break;

            case PhaseType.ExchangeSelection1Phase:
                //0~55 ī�� ����.
                nowAction = actions.DiscreteActions[2];

                var nowCard1 = player.cards.Where(x => x.id == nowAction).ToList();

                // �� 1ĭ ���� ������� ����.

                GameManager.instance.players[(player.playerNumber + 1) % numberOfPlayers].AddCardToSlot(nowCard1[0], player);

                // �� ī�� ����Ʈ���� �ش� ī�带 ��.
                player.cards.Remove(nowCard1[0]);

                break;

            case PhaseType.ExchangeSelection2Phase:
                //0~55 ī�� ����.
                nowAction = actions.DiscreteActions[2];

                var nowCard2 = player.cards.Where(x => x.id == nowAction).ToList();

                // �� 2ĭ ���� ������� ����.

                GameManager.instance.players[(player.playerNumber + 2) % numberOfPlayers].AddCardToSlot(nowCard2[0], player);

                // �� ī�� ����Ʈ���� �ش� ī�带 ��.
                player.cards.Remove(nowCard2[0]);


                break;

            case PhaseType.ExchangeSelection3Phase:
                //0~55 ī�� ����.
                nowAction = actions.DiscreteActions[2];

                var nowCard3 = player.cards.Where(x => x.id == nowAction).ToList();

                // �� 2ĭ ���� ������� ����.

                GameManager.instance.players[(player.playerNumber + 3) % numberOfPlayers].AddCardToSlot(nowCard3[0], player);

                // �� ī�� ����Ʈ���� �ش� ī�带 ��.
                player.cards.Remove(nowCard3[0]);

                break;

            case PhaseType.FirstTrickSelectionPhase:
                //1~382 Ʈ�� ����.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == 0) Debug.LogError("����. ù Ʈ������ �н��� �� ����.");
                if (nowAction == numberOfTrickType) Debug.LogError("����. Ʈ�� ���� ������ �Ұ�����.");

                var cardList = DecodeNumberToTrick(nowAction);

                foreach (var card in cardList)
                {
                    player.AddSelection(card);
                }

                player.SelectTrickCall();

                break;

            case PhaseType.TrickSelectionPhase:
                //0�� �н�. 1~382 Ʈ�� ����.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == numberOfTrickType) Debug.LogError("����. Ʈ�� ���� ������ �Ұ�����.");

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
                //0�� �н�. 1~382 Ʈ�� ����.
                nowAction = actions.DiscreteActions[3];
                decodeNumberForDebug = nowAction;

                if (nowAction == numberOfTrickType) Debug.LogError("����. Ʈ�� ���� ������ �Ұ�����.");

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
                //0,2~14 ������ �ҿ� ����.
                nowAction = actions.DiscreteActions[4];

                if(nowAction!=0)
                {
                    GameManager.instance.isBirdWishActivated = true;
                    GameManager.instance.birdWishValue = nowAction;
                    UIManager.instance.ActivateBirdWishNotice(nowAction);
                }

                break;

            case PhaseType.DragonSelectionPhase:
                //0�� 3ĭ ���� ���, 1�� 1ĭ ���� ��� �� Ʈ�� ���� ����.
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
        //�������� isActionEnd �� true ��.
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //���� � �����ΰ�? 1 of 10.
        sensor.AddOneHotObservation((int)currentPhase, (int)Util.PhaseType.NumberOfPhase);

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
        else sensor.AddOneHotObservation(GetRelativePlayerIdx(GameManager.instance.trickStack.Peek().playerIdx, player.playerNumber), Util.numberOfPlayers);

        // (���� ž �÷��̾� �ε��� + 4 - '��'�� �ε���)%4
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

        // ���� ������ �����ΰ�? 1 of 4.
        sensor.AddOneHotObservation(GetRelativePlayerIdx(GameManager.instance.currentTrickPlayerIdx,player.playerNumber), Util.numberOfPlayers);

        // ���� ���� ������ ���� - each of 4.
        for (int idx = player.playerNumber; idx < player.playerNumber + Util.numberOfPlayers; ++idx)
        {
            int nowIdx = idx % Util.numberOfPlayers;
            sensor.AddOneHotObservation(GameManager.instance.players[nowIdx].cards.Count, numberOfCardsPlay+1);
            //int nowIdx = idx % Util.numberOfPlayers;
            //float nowHand = (float)(GameManager.instance.players[nowIdx].cards.Count) / Util.numberOfCardsPlay;
            //sensor.AddObservation(nowHand);
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
        // �׼��� �ϴ��� ��� block. ���þ����� Ȱ��ȭ.
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
        for (int idx = 0; idx < 2; ++idx) actionMask.SetActionEnabled(actionIdx, idx, false); //�� Ʈ�� ���� ���� �׼� 


        switch (currentPhase)
        {
            case Util.PhaseType.LargeTichuSelectionPhase:
                actionIdx = 0;
                actionMask.SetActionEnabled(actionIdx, 0, true); //���� Ƽ�� �н� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 1, true); //���� Ƽ�� ���� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 2, false); //2�� ���� ����. �̸� ��Ȱ��ȭ.
                break;
            
            case Util.PhaseType.SmallTichuSelectionPhase: //���� Ƽ�� ���� �������� RequestDecision �� canDeclareSmallTichu �� true ���� �����.
                actionIdx = 1;
                actionMask.SetActionEnabled(actionIdx, 0, true); // ���� Ƽ�� �н� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 1, true); // ���� Ƽ�� ���� Ȱ��ȭ
                actionMask.SetActionEnabled(actionIdx, 2, false); //2�� ���� ����. �̸� ��Ȱ��ȭ.
                break;
            
            case Util.PhaseType.ExchangeSelection1Phase:
            case Util.PhaseType.ExchangeSelection2Phase:
            case Util.PhaseType.ExchangeSelection3Phase: // ���� 3���� phase �� �������� ���еȴ�.
                actionIdx = 2;
                foreach (var card in player.cards) actionMask.SetActionEnabled(actionIdx, card.id, true);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfCards, false); // 56�� ���� ����. �̸� ��Ȱ��ȭ.
                break;
            
            case Util.PhaseType.FirstTrickSelectionPhase:
                actionIdx = 3;
                ReleaseValidFirstTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false); //383�� ���� ����.�̸� ��Ȱ��ȭ
                break;
            case Util.PhaseType.TrickSelectionPhase:
                actionIdx = 3;
                ReleaseValidTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);  //383�� ���� ����.�̸� ��Ȱ��ȭ
                break;
            case Util.PhaseType.BombSelectionPhase:
                actionIdx = 3;
                ReleaseValidBombTrick(actionIdx, actionMask);
                actionMask.SetActionEnabled(actionIdx, Util.numberOfTrickType, false);  //383�� ���� ����.�̸� ��Ȱ��ȭ
                break;

            case Util.PhaseType.BirdWishSelectionPhase:
                actionIdx = 4;
                for(int idx = 0; idx<Util.numberOfBirdWish; ++idx) // 0 ���� 14 ���� 1�� �����ϰ�
                {
                    if (idx != 1) actionMask.SetActionEnabled(actionIdx, idx, true);
                }
                actionMask.SetActionEnabled(actionIdx, Util.numberOfBirdWish, false); //15�� ���� ����. �̸� ��Ȱ��ȭ.
                break;

            case Util.PhaseType.DragonSelectionPhase:
                actionIdx = 5;
                actionMask.SetActionEnabled(actionIdx, 0, true);
                actionMask.SetActionEnabled(actionIdx, 1, true);
                actionMask.SetActionEnabled(actionIdx, 2, false); //2�� ���� ����. �̸� ��Ȱ��ȭ.
                break;
        }


    }
    public void ReleaseValidTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // ���� ������ Ʈ���� true�� Ǯ���ִ� �޼ҵ�.
        // ù Ʈ���� �ƴ��� ����Ǿ�����.

        GameManager.instance.RestorePhoenixValue();

        if (Util.IsPlayerHaveToFulfillBirdWish(player) != null) // ������ �ҿ��� �ɸ��� ���
        {
            actionMask.SetActionEnabled(actionIdx, 0, false); // ������ �ҿ��� �ɸ��ٸ� �ϴ� �н��� �Ұ���. �̹� block �Ǿ������� ��������� �ѹ� �� ���ش�. 

            var topTrick = GameManager.instance.trickStack.Peek();
            var birdWish = GameManager.instance.birdWishValue;

            // ī���� Ʈ���� ���� ������ Ʈ���� ã���ش�.
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
                    Debug.LogError("�Ұ����� ��Ȳ.");
                    break;
            }
        }
        else //������ �ҿ��� �ɸ��� �ʴ� ���
        {
            actionMask.SetActionEnabled(actionIdx, 0, true); // ������ �ҿ��� �ɸ��� �ʴ� ��� �н��� �� �� ������ �����.

            var topTrick = GameManager.instance.trickStack.Peek();

            switch (GameManager.instance.trickStack.Peek().trickType)
            {
                case TrickType.Single:
                    ReleaseValidSingle(actionIdx, actionMask, topTrick.trickValue/2); // ��Ȳ, ��, ���� ������ ��ȿ �̱��� ����ŷ �������ش�.
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
                    Debug.LogError("�Ұ����� ��Ȳ.");
                    break;
            }
        }
    }


    public void ReleaseValidFirstTrick(int actionIdx, IDiscreteActionMask actionMask)
    {
        // ù Ʈ���� �����ϴ� ��쿡 ���� �޼ҵ�.
        // ��� ��쿡�� �н��� �Ұ���.

        GameManager.instance.RestorePhoenixValue();
        // ������ �ҿ��� �ɸ��� ���
        if (IsPlayerHaveToFulfillBirdWish(player) != null)
        {
            var birdWish = GameManager.instance.birdWishValue;
            // ���� �ʿ�.
            // �ҿ��� �����ϸ鼭 ������ �̱�, ���, Ʈ����, ���� ���, ��Ʈ����Ʈ, Ǯ�Ͽ콺, ��ī�� ��ź, ���� ��ź�� release ���ش�.
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
                actionMask.SetActionEnabled(actionIdx, dogTrickOffset, true); // ���� �Ȱɸ��� ù Ʈ���̸� ���� �� �� ������ �����.

            // ������ �̱�, ���, Ʈ����, ���� ���, ��Ʈ����Ʈ, Ǯ�Ͽ콺, ��ī�� ��ź, ���� ��ź�� release ���ش�.
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
        // ��ź Ȯ�ο� ���� �޼ҵ�.
        // ���� ����ó���� �ʿ����. ��� ��쿡�� �н��� �� �� ������ �����.
        //

        actionMask.SetActionEnabled(actionIdx, 0, true); // �н��� �� �� ������ �����.

        var nowStack = GameManager.instance.trickStack.Peek();

        if(nowStack.trickType==TrickType.StraightFlushBomb)
        {
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, nowStack.trickValue, nowStack.trickLength);
        }
        else if(nowStack.trickType == TrickType.FourCardBomb)
        {
            ReleaseValidFourCardBomb(actionIdx, actionMask, nowStack.trickValue);
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5); // topValue �� 0���� �Ѱ��ְ� ���̸� ������ 5�� ����. ������ ��� ��ī�� ��ź�� release. 
        }
        else
        {
            ReleaseValidFourCardBomb(actionIdx, actionMask, 0); // topValue �� 0���� �Ѱ��� ��� ��ī�� ��ź�� release.
            ReleaseValidStraightFlushBomb(actionIdx, actionMask, 0, 5); // topValue �� 0���� �Ѱ��ְ� ���̸� ������ 5�� ����. ������ ��� ��ī�� ��ź�� release. 
        }
    }

    public void ReleaseValidSingle(int actionIdx, IDiscreteActionMask actionMask, int topValue)
    {
        //������ �ҿ��� �ɸ��� ������ ����Ǿ� ����.

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
        if(player.cards.Any(x=>x.type == CardType.Dragon)) //�� ���� ����ó��. ���� ���� specialCardsValue[3] = 18. �̰� ������ �ִٸ� �� ���� �����ϰ� ����.
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

        int nowOffset = GetConsecutivePairOffset(topLength);
        
        if(havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal <=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //���� ���� 2���� ī��� ����Ʈ
                var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //���� ���� 3���� ī��� ����Ʈ
                var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //���� ���� 4���� ī��� ����Ʈ
                tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3�� �� 2������ pick
                quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4�� �� 2������ pick
                var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat �ϰ� ����
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

            var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //���� ���� 2���� ī��� ����Ʈ
            var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //���� ���� 3���� ī��� ����Ʈ
            var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //���� ���� 4���� ī��� ����Ʈ
            tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3�� �� 2������ pick
            quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4�� �� 2������ pick
            var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat �ϰ� ����
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
                    if (nowList.Count != topLength) break; // ©���µ� ���̰� �ȳ��´ٸ� break.
                    if (IsStraight(nowList) && nowList.Last().value > topValue) //��Ʈ����Ʈ�̰� topValue ���� ũ�ٸ�
                    {
                        actionMask.SetActionEnabled(actionIdx, nowOffset + nowList.Last().value - topLength, true);
                    }
                    cardList = cardList.Skip(1).ToList(); // �� �տ� �ϳ� ¥����.
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
                }
                cardList = cardList.Skip(1).ToList(); // �� �տ� �ϳ� ¥����.
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
            nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue �� 2 ��� 57, 3�̶�� 69, ....
            for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
            {
                var tripleList = player.cards.Where(x=>x.value == topVal).ToList();
                if (tripleList.Count < 3) continue;
                else //������. Ʈ������ ��������. �� Ȯ���� ����.
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
        // ��Ʈ����Ʈ �÷��� ��ź�� ������:
        // ��Ʈ����Ʈ �÷��� ��ź�� 
        // 1. ���� �־��� ���̿� �����鼭 topValue�� �� ���� ���� ã��,
        // 2. �߰��� topValue �� ������� ���̰� �� ����� ã�ƾ� �Ѵ�.
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
        while(length<=13) // ���� ��ź�� �ִ� ���̴� 13.
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
        //������ �ҿ��� �ɷ����� ����Ǿ� ����.
        //birdWish ī�带 �ּ� 1�� ������ ������ ����Ǿ� ����.
        //�̱��̹Ƿ� birdWish�� �ش��ϴ� �̱��� �׳� release ���ָ� ��.

        if(topValue<birdWish)
            actionMask.SetActionEnabled(actionIdx, birdWish, true);
    }

    public void ReleaseValidPairWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //������ �ҿ��� �ɷ����� ����Ǿ� ����.
        // birdWish ī�带 �ּ� 1�� ������ ������ ����Ǿ� ����.
        // ��Ȳ�� ���� ��� birdWish �� ���� �����ְ� �� Ȯ��, ������.
        if (topValue < birdWish)
        {
            if (FindPhoenix(player.cards) != null) GameManager.instance.phoenix.value = birdWish;

            var pairList = player.cards.Where(x => x.value == birdWish).ToList();
            if (pairList.Count >= 2) actionMask.SetActionEnabled(actionIdx, pairTrickOffset + birdWish - 2, true); //2���� offset(31), 3���� offset+1(32)...
        }
    }
    public void ReleaseValidTripleWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //������ �ҿ��� �ɷ����� ����Ǿ� ����.
        // birdWish ī�带 �ּ� 1�� ������ ������ ����Ǿ� ����.
        // ��Ȳ�� ���� ��� birdWish �� ���� �����ְ� Ʈ������ Ȯ��, ������.
        if (topValue < birdWish)
        {
            if (FindPhoenix(player.cards) != null) GameManager.instance.phoenix.value = birdWish;

            var tripleList = player.cards.Where(x => x.value == birdWish).ToList();

            if (tripleList.Count >= 3) actionMask.SetActionEnabled(actionIdx, tripleTrickOffset + birdWish - 2, true);
        }
    }

    public void ReleaseValidFullHouseWithBirdWish(int actionIdx, IDiscreteActionMask actionMask, int topValue, int birdWish)
    {
        //������ �ҿ��� �ɷ����� ����Ǿ� ����.
        // birdWish ī�带 �ּ� 1�� ������ ������ ����Ǿ� ����.
        // 2 �Ǵ� topValue+1 ���� Ʈ���� Ȯ��. 
        // �б�1: Ʈ������ birdWish�� �ƴ� ���� �� birdWish.
        // �б�2: Ʈ������ birdWish�� ��� ���� birdWish ������ ���.
        // ��Ȳ�� ������ ���� ��� �ֿܰ��� ��Ȳ ���� ����.


        bool havePhoenix = FindPhoenix(player.cards)!=null?true:false;
    
        if(havePhoenix)
        {
            for(int phoenixVal = 2; phoenixVal<=14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                int nowOffset = fullHouseTrickOffset;
                nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue�� 2 ��� 3 Ʈ���ú��� �� 69,topValue�� 3�̶�� 4Ʈ���ú��� �� 81, ....

                for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
                {
                    var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                    if (tripleList.Count < 3) continue;
                    else //������. Ʈ������ ��������. �� Ȯ���� ����.
                    {
                        //�б�1: �� Ʈ������ birdWish �ΰ�? �׷� ���� ��� ����.

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

                        //�б�2: �� Ʈ������ birdWish �� �ƴѰ�? �׷� ���� ������ birdWish.
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
            nowOffset += (Mathf.Max(topValue + 1, 2) - 2) * 12; // topValue�� 2 ��� 3 Ʈ���ú��� �� 69,topValue�� 3�̶�� 4Ʈ���ú��� �� 81, ....

            for (int topVal = Mathf.Max(topValue + 1, 2); topVal <= 14; ++topVal, nowOffset += 12)
            {
                var tripleList = player.cards.Where(x => x.value == topVal).ToList();
                if (tripleList.Count < 3) continue;
                else //������. Ʈ������ ��������. �� Ȯ���� ����.
                {
                    //�б�1: �� Ʈ������ birdWish �ΰ�? �׷� ���� ��� ����.

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

                    //�б�2: �� Ʈ������ birdWish �� �ƴѰ�? �׷� ���� ������ birdWish.
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
            //��Ȳ�� �ִ� ��� �ֿܰ��� ��Ȳ ���� ����.

            for (int phoenixVal = 2; phoenixVal <= 14; ++phoenixVal)
            {
                GameManager.instance.phoenix.value = phoenixVal;

                var properList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList(); //�ߺ� ����.

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
            var properList = player.cards.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x => x.type).ToList(); //�ߺ� ����.

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

                var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //���� ���� 2���� ī��� ����Ʈ
                var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //���� ���� 3���� ī��� ����Ʈ
                var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //���� ���� 4���� ī��� ����Ʈ
                tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3�� �� 2������ pick
                quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4�� �� 2������ pick
                var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat �ϰ� ����
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

            var doubleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 2 select n).ToList(); //���� ���� 2���� ī��� ����Ʈ
            var tripleList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 3 select n).ToList(); //���� ���� 3���� ī��� ����Ʈ
            var quadList = (from n in player.cards where player.cards.Count(y => y.value == n.value) == 4 select n).ToList(); //���� ���� 4���� ī��� ����Ʈ
            tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3�� �� 2������ pick
            quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4�� �� 2������ pick
            var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat �ϰ� ����
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
        // ��Ʈ����Ʈ �÷��� ��ź�� ������:
        // ��Ʈ����Ʈ �÷��� ��ź�� 
        // 1. ���� �־��� ���̿� �����鼭 topValue�� �� ���� ���� ã��,
        // 2. �߰��� topValue �� ������� ���̰� �� ����� ã�ƾ� �Ѵ�.
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
        while (length <= 13) // ���� ��ź�� �ִ� ���̴� 13.
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
            case TrickType.Single: //1~29���� �ִ�.
                //����� ���� üũ.
                if (trick.cards[0].type == CardType.Dragon) ret = dragonTrickOffset;
                else
                {
                    //�״��� ��Ȳ üũ.
                    if(trick.cards[0].type==CardType.Phoenix)
                    {
                        //3�̸� 15, 5��� 16.. 29��� 28.
                        ret = phoenixSingleTrickOffset + trick.trickValue / 2 - 1; 
                    }
                    else
                    {
                        //����� �̱�.
                        ret = trick.trickValue / 2;
                    }
                }
                break;

            case TrickType.Dog: //30.
                ret = dogTrickOffset;
                break;

            case TrickType.Pair://31~43���� �ִ�.
                ret = pairTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.Triple://44~56���� �ִ�.
                ret = tripleTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.FullHouse: //57~212���� �ִ�.
                ret = fullHouseTrickOffset + (trick.trickValue - 2) * 12;
                //Ʈ���� �����±��� ���ߴ�. ī�带 ���� �� ã�� ��� �������� ���Ѵ�.
                int pairValue = 0;


                if(FindPhoenix(trick.cards)!=null)
                {
                    //Ǯ�Ͽ콺�� ��Ȳ�� �ְ� ���� ����Ǿ��� �� �ִ�.
                    //���� ������ �ۼ����ش�.
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

            case TrickType.Straight: //213~ 267���� �ִ�.
                ret = GetStraightOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength;
                break;

            case TrickType.ConsecutivePair: //268~324���� �ִ�.
                ret = GetConsecutivePairOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength / 2 - 1;
                break;

            case TrickType.FourCardBomb: //325~337���� �ִ�.
                ret = fourCardTrickOffset + trick.trickValue - 2;
                break;

            case TrickType.StraightFlushBomb: //338~382���� �ִ�.
                ret = GetStraightFlushTrickOffset(trick.trickLength);
                ret += trick.trickValue - trick.trickLength - 1;
                break;

            default:
                Debug.LogError("�Ұ����� ����");
                break;
        }

        return 0;
    }

    public List<Card> DecodeNumberToTrick(int code)
    {
        //�ڵ带 Ʈ������ ���ڵ�.
        //
        //�̽�: ��Ʈ����Ʈ �÷����� �ְ�, �װ��� �ִ��� �μ����� �ʰ� �ؾ��Ѵ�.
        //
        //���̵��: ��Ʈ����Ʈ �÷��� ī�带 �����ϰ� ī�带 ������.
        // �����ϰ� ���� �� �ִٸ�: ok.
        // �����ϰ� ���� �� ���ٸ�:
        // �б�1: 1���� ���ڶ�� ��Ȳ�� ������ �ִٸ�: 1���� ��Ȳ���� ��ü.
        // �б�2: 1���� ���ڶ�µ� ��Ȳ�� ���ų� 2�� �̻� ���ڶ��: ��Ʈ����Ʈ �÷����� ���� Ʈ���� �����.
        

        List<Card> ret = new List<Card>();
        GameManager.instance.RestorePhoenixValue();
        bool havePhoenix = FindPhoenix(player.cards)!=null?true:false;

        List<Card> nowList = player.cards.ToList(); // nowList�� ī�带 �����.

        var sfTrick = GameManager.instance.FindAnyStraightFlushBomb(nowList);
        List<Card> straightFlushCards = new List<Card>();
        if (sfTrick != null)
        {
            foreach (var card in sfTrick.cards) nowList.Remove(card); // ���� ��ź�� �ִٸ� �� ī��� ����.
            straightFlushCards = sfTrick.cards.ToList();
        }

        if (code == 0)
        {
            //�н�. ret �� �������.
            return ret;
        }
        else if (code < phoenixSingleTrickOffset) //�Ϲ� �̱�
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
            if (ret.Count == 0) Debug.LogError("�Ұ����� �̱� ���ڵ�");
        }
        else if (code < dragonTrickOffset) //��Ȳ �̱�
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
        else if (code == dragonTrickOffset) // �� �̱�
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
        else if (code == dogTrickOffset) //�� �̱�
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
        else if (code < tripleTrickOffset) // ���
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

            if (cnt > 0) Debug.LogError("�Ұ����� ��� ���ڵ�");

        }
        else if (code < fullHouseTrickOffset) // Ʈ����
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

            if (cnt > 0) Debug.LogError("�Ұ����� Ʈ���� ���ڵ�");
        }
        else if (code < straightTrickOffset[0]) //Ǯ�Ͽ콺
        {
            int tripleValue = ((code - fullHouseTrickOffset) / 12) + 2; //0~11�� 2, 12~23�� 3, ... 
            int pairValue = GetFullHousePairValue((code - fullHouseTrickOffset) % 12, tripleValue);

            var pairList = nowList.Where(x => x.value == pairValue).Take(2).ToList();
            var tripleList = nowList.Where(x => x.value == tripleValue).Take(3).ToList();

            int totalNumberOfCards = pairList.Count + tripleList.Count;

            if (totalNumberOfCards == 5) //����.
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
                    else Debug.LogError("�Ұ����� Ǯ�Ͽ콺 ��Ȳ ���ڵ�");
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
                        if (restPairCount + restTripleCount > 1) Debug.LogError("�Ұ����� Ǯ�Ͽ콺 ���ڵ�");
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
                        else Debug.LogError("�Ұ����� Ǯ�Ͽ콺 ����.");
                    }
                }
            }
            if (ret.Count != 5) Debug.LogError("Ǯ�Ͽ콺 ���� ����.");
        }
        else if (code < consecutivePairTrickOffset[0]) //��Ʈ����Ʈ.
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

            if (restCnt == 0) // ����.
            {

            }

            else if (restCnt == 1 && havePhoenix) // �ϳ� ������� ��Ȳ�� ����.
            {
                int restIdx = -1;
                for(int idx = bottomValue; idx<=topValue; ++idx) if(checkArr[idx]==false) { restIdx = idx; break; }

                if (GameManager.instance.birdWishValue == restIdx&& 
                    (nowList.Any(x=>x.value == GameManager.instance.birdWishValue||straightFlushCards.Any(y=>y.value == GameManager.instance.birdWishValue)))) 
                    //�ϳ� ���� �װ� ������ �ҿ��̰� �װ� ������ �ִٸ� ��Ȳ���� ��ü �Ұ���. ���� ���� �ִ´�.
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
            else //�ΰ� �̻� ����. �ϴ� ���� ���� �־��.
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

                if (restCnt == 0) //����.
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
                else Debug.LogError("�Ұ����� ��Ʈ����Ʈ ���ڵ�.");

            }
            if (ret.Count != length) Debug.LogError("�Ұ����� ��Ʈ����Ʈ ���ڵ�. ���� �ȸ���.");
        }
        else if (code < fourCardTrickOffset) //�������.
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

            if(restCnt==0) //����.
            {

            }
            else if(restCnt == 1 && havePhoenix) // �ϳ� ��µ� ��Ȳ�� �ִٸ�
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
                else Debug.LogError("�Ұ����� ���� ��� ���ڵ�.");
            }
            if (ret.Count != length) Debug.LogError("�Ұ����� ���� ��� ����. ���� �ȸ���.");

        }
        else if (code < straightFlushTrickOffset[0]) //��ī�� ��ź.
        {
            //��ī�� ��ź�� ���� ��ź�� ������ ����ó���� �ʿ����.
            //nowList�� ��ģ��.
            nowList.AddRange(straightFlushCards);
            int value = code - fourCardTrickOffset + 2;
            var fourCardList = nowList.Where(x => x.value == value).ToList();
            if (fourCardList.Count != 4) Debug.LogError("�Ұ����� ��ī�� ����. ���� �ȸ���.");

            ret.AddRange(fourCardList);

        }
        else // ���� ��ź.
        {
            //���� ��ź�� ���� ��ź�� ������ ����ó���� �ʿ����.
            //nowList�� ��ģ��.
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
            if (ret.Count != length) Debug.LogError("�Ұ����� ���� ��ź. ���� �ȸ���.");
        }

        return ret;
    }
}
