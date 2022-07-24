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

public class AgentManager : MonoBehaviour
{

    [HideInInspector]
    public SimpleMultiAgentGroup[] agentGroup;

    [HideInInspector]
    public static AgentManager instance;

    public void Start()
    {
        agentGroup = new SimpleMultiAgentGroup[numberOfTeam];

        foreach(var player in GameManager.instance.players)
        {
            var nowAgent = player.GetComponent<TichuAgent>();
            if (nowAgent != null) agentGroup[player.playerNumber % 2].RegisterAgent(nowAgent);
        }
        instance = this;
    }

    public void Evaluate(Score[] score)
    {
        int idx = 0;
        foreach (var nowScore in score)
        {
            float reward = (float)(nowScore.tichuScore + nowScore.oneTwoScore + nowScore.trickScore) / (float)maximumTotalScore;
            agentGroup[idx].AddGroupReward(reward);
            idx++;
        } 
    }

    public void EndGroupEpisode(Score[] score)
    {
        if(score[0].previousScore + score[0].oneTwoScore + score[0].tichuScore + score[0].trickScore>
            score[1].previousScore + score[1].oneTwoScore + score[1].tichuScore + score[1].trickScore)
        {
            agentGroup[0].AddGroupReward(0.2f);
            agentGroup[1].AddGroupReward(-0.2f);
        }

        else
        {
            agentGroup[0].AddGroupReward(-0.2f);
            agentGroup[1].AddGroupReward(0.2f);
        }
        foreach(var group in agentGroup) group.EndGroupEpisode();
    }
}
