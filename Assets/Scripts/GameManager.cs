using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public List<GameObject> cards; 
    // Start is called before the first frame update
    void Start()
    {
        float initialCardPosition = -7.0f;
        float heightFactor = -0.01f;
        float summonPositionX = initialCardPosition;
        float summonPositionY = -2.7f;
        float summonPositionZ = -1f;
        int idx = 0;
        float cardScaleFactor = 0.30f;
        Vector3 initialCardScale = new Vector3(cardScaleFactor,cardScaleFactor, cardScaleFactor);
        Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);
        foreach(var item in cards)
        {
            //if ((idx - 1) / 7 != idx / 7)
            {
              //  summonPositionY += 2.7f;
              //  summonPositionX = initialCardPosition;
            }
            ++idx;
            var obj = Instantiate(item, new Vector3(summonPositionX, summonPositionY, summonPositionZ), initialCardRotation, transform);
            obj.transform.localScale = initialCardScale;
            summonPositionZ += heightFactor;
            summonPositionX += 1.1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
