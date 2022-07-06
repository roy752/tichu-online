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

    public IEnumerator DurationCoroutine()
    {
        duration = GlobalInfo.largeTichuDuration;
        while(duration>=0)
        {
            duration -= GlobalInfo.tick;
            yield return new WaitForSeconds(GlobalInfo.tick);
        }
    }

    public IEnumerator ChooseLargeTichuCoroutine()
    {
        coroutineFinishFlag = false;
        chooseFlag = false;
        GameManager.instance.RenderCards(cards.OrderBy(x => x.value).ToList());

        GameObject yesButtonObject = GameManager.instance.uiParent.transform.Find(GlobalInfo.buttons.ltYesButtonName).gameObject;
        GameObject noButtonObject  = GameManager.instance.uiParent.transform.Find(GlobalInfo.buttons.ltNoButtonName).gameObject;

        yesButtonObject.SetActive(true);
        noButtonObject.SetActive(true);

        Button yesButton = yesButtonObject.GetComponent<Button>();
        Button noButton = noButtonObject.GetComponent<Button>();

        yesButton.onClick.AddListener(DeclareLargeTichu);
        noButton.onClick.AddListener(SkipLargeTichu);

        yield return new WaitUntil(()=>chooseFlag == true || duration<0);

        StopCoroutine(DurationCoroutine());

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButtonObject.SetActive(false);
        noButtonObject.SetActive(false);

        coroutineFinishFlag = true;
    }
}
