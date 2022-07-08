using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject edgeObject;
    public virtual void OnEnable()
    {
        edgeObject = transform.Find(GlobalInfo.edgeObjectName).gameObject;
        edgeObject.SetActive(false);
    }

    protected bool isSelected = false;
    // Update is called once per frame

    public virtual void ToggleSelection()
    {
    }

    public void ToggleBase()
    {
        isSelected = !isSelected;
        edgeObject.SetActive(isSelected);
    }
}
