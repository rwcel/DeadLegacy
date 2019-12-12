using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePoolingEvent : AIEvent
{
    // Event함수 대신 실행 : 살아있는 ai들 Unspawn + activeDictionary제거 + 서버 알림
    public override void Play()
    {
        base.Play();

        AIPoolingManager.instance.AllUnSpawnObject();
    }
}
