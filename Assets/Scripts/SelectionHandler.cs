using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    GameObject edgeObject;
    public bool isSelected = false;

    public virtual void OnEnable()
    {
        edgeObject = transform.Find(Global.edgeObjectName).gameObject;
        edgeObject.SetActive(false);
    }

    public virtual void ToggleSelection(){ }

    public void ToggleBase()
    {
        isSelected = !isSelected;
        edgeObject.SetActive(isSelected);
    }
}
