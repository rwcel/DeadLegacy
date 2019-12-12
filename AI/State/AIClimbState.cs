using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NetworkBase;

public class AIClimbState : AIBaseState
{
    float wallClimbY;                           // 벽 오를 수 있는 높이

    // 오프메시 링크관련 변수
    [SerializeField] OffMeshLink offLink = null;    // 링크 가지고 있는지 확인
    private float oldLinkCost = -1f;                  // 원래 코스트

    bool isClimb;                               // 벽 타고 오르기


    public override void StartAction()
    {
        base.StartAction();

        offLink = nav.currentOffMeshLinkData.offMeshLink;
        ClimbSetting();

        StartCoroutine(Climbing());
    }

    public override void GuestAction()
    {
        base.GuestAction();
        Collider[] _colLink = Physics.OverlapSphere(transform.position, 0.5f);
        for (int i = 0; i < _colLink.Length; i++)
        {
            try
            {
                offLink = _colLink[i].transform.parent.GetComponent<OffMeshLink>();
                if (offLink != null)
                {
                    ClimbSetting();
                    Debug.Log("설정완료");
                    StartCoroutine(Climbing());
                    break;
                }
            }
            catch { };
        }
    }

    /// <summary>
    /// 1. 오프메시 벽쪽으로 몸 돌리기  **위치 강제로 맞춰서 제대로 레이쏘게
    /// 2. 타고 올라갈 벽 높이 지정하기 : wallClimbY
    /// 3. 타고 있는 오프메시를 다른 ai가 못하게 하기
    /// 4. 벽 높이에 도달하면 오르기 실행
    /// </summary>
    void ClimbSetting()
    {
        // 1.
        transform.position = offLink.startTransform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 5f, enviroLayer))
        {
            transform.rotation = Quaternion.LookRotation(-hit.normal);
            transform.position = hit.point + (hit.normal * col.bounds.extents.x * 0.8f);

            // 2.
            wallClimbY = hit.collider.bounds.size.y + transform.position.y - gameObject.GetComponent<Collider>().bounds.size.y;
        }

        // 3.
        offLink.costOverride = 1000.0f;
        offLink.activated = false;

        nav.enabled = false;
        isAction = true;
    }

    IEnumerator Climbing()
    {
        while(isAction && !isClimb)
        {
            transform.Translate(Vector3.up * Time.deltaTime);
            yield return null;
            // 4.
            if (transform.position.y >= wallClimbY)
            {
                aiAnim.ClimbRail();
                isClimb = true;
            }
        }
    }

    /// <summary>
    /// 1. 벽 다 오르면 위치 이동시키기
    /// 2. 상태 돌려주기
    /// 3. 바로 움직일 수 있게 해주기
    /// </summary>
    /// <param name="_pos"></param>
    public override void EndState(Vector3 _pos)
    {
        base.EndState(_pos);

        Debug.Log("클라이밍 끝");

        // 1.
        transform.position = offLink.endTransform.position;

        // 2.
        isAction = false;
        isClimb = false;

        offLink.activated = true;
        offLink.costOverride = oldLinkCost;
        offLink = null;

        aiCon.offLink = null;

        if (cNetworkIdentity.isHost)
        {
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.CLIMB);
        }
        else
        {
            Debug.Log("뛰어내리기 끝 처리");
            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
            _aiPacket.DeFlagAction();
            networkAISyncor.CheckSyncEnd(_aiPacket);
            networkAISyncor.EndFullAction();
        }
    }
}
