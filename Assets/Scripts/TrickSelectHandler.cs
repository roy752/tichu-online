using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrickSelectHandler : SelectionHandler
{
    private UnityAction action;
    
    public void AddAction(UnityAction t)
    {
        action = t;
    }
    public void ClearAction()
    {
        action = null;
    }

    public override void ToggleSelection()
    {
        action?.Invoke();
    }
}
