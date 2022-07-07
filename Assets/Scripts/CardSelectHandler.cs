using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardSelectHandler : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject edgeObject;
    void Start()
    {
        edgeObject = transform.Find(GlobalInfo.cardEdgeObjectName).gameObject;
        edgeObject.SetActive(false);
    }

    private bool isSelected = false;
    // Update is called once per frame

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        edgeObject.SetActive(isSelected);
    }
}
