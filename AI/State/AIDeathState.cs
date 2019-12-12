using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIDeathState : AIBaseState
{
    //float ragDollRegulateTime = 3.0f;             // 랙돌 취소하는시간
    //float respawnTime = 3.0f;                     // 리스폰시간
    Vector3 takeForce;                              // 밀려나는 힘
    float baseTakeForce = 300f;                     // 기본 힘 : 데미지 비례예정(20~34)

    public void SetTakeForce(Vector3 _force)
    {
        takeForce = (gameObject.transform.position - _force).normalized * baseTakeForce;
        // Debug.Log(_force + "==" + gameObject.transform.position);
    }

    public override void StartAction()
    {
        base.StartAction();

        aiCon.StopAllCoroutines();

        AIObjectUnSpawn();
    }

    /// <summary>
    /// 풀링매니저에게 죽은애 꺼내달라고 요청하기
    /// </summary>
    void AIObjectUnSpawn()
    {
        if(!gameObject.CompareTag("Fluid"))
            AIPoolingManager.instance.ChangeDeathAI(gameObject, aiCon.aiInfo.id, takeForce);

        CheckSpawnType();
    }

    // 스폰타입만 재사용 : 오브젝트 풀
    public void CheckSpawnType()
    {
        if (aiCon.aiInfo.type == AIType.SPAWN)
        {
            GetComponent<AIController>().DieAction();
            GetComponent<AIController>().DieAction = null;
            AIPoolingManager.instance.UnspawnObject(cNetworkIdentity.Netcode);
        }
        else
            gameObject.SetActive(false);
    }

    // 현재 AI DieAction 실행
    public void CurrentDieAction()
    {
        GetComponent<AIController>().DieAction();
        GetComponent<AIController>().DieAction = null;
    }

    public override void EndAction()
    {
        base.EndAction();
    }
}
