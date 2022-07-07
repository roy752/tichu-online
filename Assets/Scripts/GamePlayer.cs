using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<GameManager.Card> cards = new List<GameManager.Card>();

    public bool chooseFlag = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag = false;
    
    float       duration;

    public void AddCards(List<GameManager.Card> cardList)
    {
        cards.AddRange(cardList);
    }

    public IEnumerator DurationCoroutine()
    {
        duration = GlobalInfo.largeTichuDuration;
        while(duration>=0)
        {
            duration -= GlobalInfo.tick;
            yield return new WaitForSeconds(GlobalInfo.tick);
        }
    }

    public void DeclareLargeTichu()
    {
        chooseFlag = true;
        largeTichuFlag = true;
    }

    public void SkipLargeTichu()
    {
        chooseFlag = true;
        largeTichuFlag = false;
    }

    public void ChooseLargeTichu()
    {
        StartCoroutine(DurationCoroutine());
        StartCoroutine(ChooseLargeTichuCoroutine());
    }


    public IEnumerator ChooseLargeTichuCoroutine()
    {

        coroutineFinishFlag = false;
        chooseFlag = false;

        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 4, cards);

        UIManager.instance.ActivateLargeTichu(DeclareLargeTichu, SkipLargeTichu);

        yield return new WaitUntil(()=>chooseFlag == true || duration<0);

        StopCoroutine(DurationCoroutine());

        UIManager.instance.DeactivateLargeTichu();

        coroutineFinishFlag = true;
    }
}
