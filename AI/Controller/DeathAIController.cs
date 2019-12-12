using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAIController : MonoBehaviour
{ 
    public Renderer[] renderObject;                 // 머리티얼 넣어줄 오브젝트 

    float ragDollRegulateTime = 3.0f;               // 랙돌 취소하는시간
    Vector3 takeForce;                              // 받는 힘

    protected LayerMask groundLayer = (1 << (int)SkillJudgeTarget.GROUND);
    protected float groundHeight = 0.2f;            // 땅 인식 최소 거리

    // 랙돌 변수
    [HideInInspector] public Rigidbody[] ragRd;     // 래그돌 리지드바디
    [HideInInspector] public Collider[] ragCol;     // 래그돌 콜라이더
    Transform[] ragTr;                              // 래그돌 트랜스폼
    Vector3[] startPos;                             // 초기 좌표  **다시 정상으로 돌려놔야함
    Quaternion[] startRot;                          // 초기 회전

    bool isFirst;

    /// <summary>
    /// 힘 정하기
    /// </summary>
    /// <param name="_force">받는 힘</param>
    public void SettingForce(Vector3 _force)
    {
        if(!isFirst)
        {
            ragRd = GetComponentsInChildren<Rigidbody>();
            ragCol = GetComponentsInChildren<Collider>();
            ragTr = new Transform[ragRd.Length];
            startPos = new Vector3[ragRd.Length];
            startRot = new Quaternion[ragRd.Length];

            for (int i = 0; i < ragRd.Length; i++)
            {
                ragTr[i] = ragRd[i].gameObject.transform;
                startPos[i] = ragRd[i].gameObject.transform.position;
                startRot[i] = ragRd[i].gameObject.transform.rotation;
            }
        }
        takeForce = _force;
    }

    // obj 그대로 베끼기
    public void CopyAppearance(GameObject obj)
	{
        // 트랜스폼 조절
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
        transform.localScale = obj.transform.localScale;

        // 색상 조절
        AILiving _aiLiving = obj.GetComponent<AILiving>();
        for (int i = 0; i < renderObject.Length; i++)
        {
            // 켜져있는거라면 색상 조절하고 아니라면 안보이게
            if (_aiLiving.renderObject[i].gameObject.activeSelf)
            {
                renderObject[i].gameObject.SetActive(true);
                renderObject[i].material.color = _aiLiving.renderObject[i].material.color;
            }
            else
                renderObject[i].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < ragRd.Length; i++)
        {
            ragRd[i].AddForce(takeForce);
        }

        StartCoroutine(TakeRagDoll());
    }

    IEnumerator TakeRagDoll()
    {
        yield return new WaitForSeconds(ragDollRegulateTime);
        // 1.
        while (true)
        {
            if (Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer))
            {
                // Debug.Log("차가운 바닥에서 죽음");
                // 상태 재설정
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // this.enabled = false;
    }

    public void DeadAIReset()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        for (int i = 0; i < ragRd.Length; i++)
        {
            ragRd[i].velocity = Vector3.zero;
            ragRd[i].angularVelocity = Vector3.zero;
            ragTr[i].position = startPos[i];
            ragTr[i].rotation = startRot[i];
        }

        isFirst = true;
    }
}
