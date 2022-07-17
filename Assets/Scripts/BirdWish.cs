using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdWish : MonoBehaviour
{
    public bool activateBirdWish;
    public int birdWishValue;

    public void BirdWishSelection()
    {
        GameManager.instance.isBirdWishActivated = activateBirdWish;
        GameManager.instance.birdWishValue       = birdWishValue;
        if (activateBirdWish) UIManager.instance.ActivateBirdWishNotice(birdWishValue);
    }
}
