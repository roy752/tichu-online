using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandBounce : MonoBehaviour
{
    Vector3 originalPosition;
    float elapseTime;
    public bool stopFlag;
    public bool isDone;

    private void Awake()
    {
        originalPosition = gameObject.transform.position;
        isDone = true;
    }

    IEnumerator t;
    public void StartBounce()
    {
        if (t == null)
        {
            t = BounceCoroutine();
            StartCoroutine(t);
        }
    }

    public void EndBounce()
    {
        if (t != null)
        {
            StopCoroutine(t);
            t = null;
        }
    }

    public IEnumerator BounceCoroutine()
    {
        do 
        {
            elapseTime = 0;
            if (gameObject.transform.position.y < originalPosition.y - Util.offsetZ)
                gameObject.transform.position = originalPosition;

            while (gameObject.transform.position.y >= originalPosition.y - Util.offsetZ)
            {
                //v0: util.initialbouncespeed = 초기속도
                //t0 만큼 시간이 지났을 때 거리: v0*t0 + 0.5g*(t0)^2
                //t0+t 만큼 시간이 지났을 때 거리: v0*(t0+t) + 0.5g*(t0+t)^2
                //t0 에서 t0+t 만큼 시간이 지났을 때 거리 차: v0*t + g*t*t0 + 0.5g*t^2
                // = t(v0 + g(t0+0.5t))
                gameObject.transform.position += new Vector3(0, Util.bounceTick * (Util.initialbounceSpeed + Util.gravity * (elapseTime + 0.5f * Util.bounceTick)), 0);
                elapseTime += Util.bounceTick;
                yield return new WaitForSeconds(Util.bounceTick);
            }
        } while (true);
    }
}
