using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<string> cardNames = new List<string>();
    [HideInInspector]
    public List<GameObject> cards;
    // Start is called before the first frame update

    private void Awake()
    {
        foreach(string cardName in System.Enum.GetNames(typeof(GlobalStats.GeneralCardName)))
        {
            for (int i = 1; i <= GlobalStats.numberOfCards; ++i) cardNames.Add(cardName + i.ToString("D2"));
        }
        foreach(string cardName in System.Enum.GetNames(typeof(GlobalStats.SpecialCardName)))
        {
            cardNames.Add(cardName);
        }
    }

    void Start()
    {
        float initialCardPosition = -1.6f;
        float heightFactor = -0.001f;
        float summonPositionX = initialCardPosition;
        float summonPositionY = 3.3f; //-2.9°¡ Àû´ç.
        float summonPositionZ = -1f;
        int idx = 0;
        float cardScaleFactor = 0.2f;
        Vector3 initialCardScale = new Vector3(cardScaleFactor,cardScaleFactor, cardScaleFactor);
        Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);
        foreach(var item in cardNames)
        {
            if ((idx - 1) / 7 != idx / 7)
            {
                summonPositionY -= 0.9f;
                summonPositionX = initialCardPosition;
            }
            ++idx;
            Debug.Log("Prefab/Cards/" + item);
            var obj = Instantiate(Resources.Load("Prefab/Cards/" + item), 
                                  new Vector3(summonPositionX, summonPositionY, summonPositionZ), 
                                  initialCardRotation,
                                  transform) as GameObject;

            obj.transform.localScale = initialCardScale;
            summonPositionZ += heightFactor;
            summonPositionX += 0.52f;
            cards.Add(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
