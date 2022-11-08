using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    GameObject edgeObject;
    public bool isSelected = false;

    public virtual void OnEnable()
    {
        if (transform.Find(Util.edgeObjectName) != null)
        {
            edgeObject = transform.Find(Util.edgeObjectName).gameObject;
            edgeObject.SetActive(false);
        }
        isSelected = false;
    }

    public virtual void ToggleSelection(){ }

    public void ToggleBase()
    {
        isSelected = !isSelected;
        edgeObject.SetActive(isSelected);
    }
}
