using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;

public class TichuAgent : Agent 
{
    public override void Initialize()
    {
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("°üÃø");
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        Debug.Log("¸¶½ºÅ·");
        actionMask.SetActionEnabled(2, 1, false);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("ÈÞ¸®½ºÆ½");
    }
}
