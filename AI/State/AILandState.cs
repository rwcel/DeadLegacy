using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using NetworkBase;

public class AILandState : AIBaseState
{
    [SerializeField] OffMeshLink offLink = null;          // 링크 가지고 있는지 확인


    public override void StartAction()
    {
        base.StartAction();

        offLink = nav.currentOffMeshLinkData.offMeshLink;
        nav.autoTraverseOffMeshLink = false;

        // **문제 생길 수 있는 부분
        nav.isStopped = true;
        isAction = true;

        StartCoroutine(Cliffing());
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
                if (offLink != null && offLink.CompareTag("Cliff"))
                {
                    Debug.Log("컴포넌트:" + _colLink[i].transform.parent.name);
                    Debug.Log(offLink.startTransform.position + "," + offLink.endTransform.position);
                    nav.autoTraverseOffMeshLink = false;
                    nav.enabled = false;
                    isAction = true;
                    StartCoroutine(Cliffing());
                    break;
                }
            }
            catch { };
        }
    }

    /// <summary>
    /// **상태로 따로 만들어야 하나..?
    /// 1. 낙하시작
    /// 2. 2.0초내 강제 이동 + 다음사람도 탈 수 있게
    /// 3. 낙하종료
    /// </summary>
    /// <returns></returns>
    IEnumerator Cliffing()
    {
        yield return new WaitForSeconds(0.3f);
        offLink.costOverride = 1000.0f;
        offLink.activated = false;
        yield return transform.DOMove(offLink.endTransform.position, 2.0f, false).WaitForCompletion();
        // yield return new WaitForSeconds(0.5f);
        Debug.Log("떨어지기 종료");

        EndState(transform.position);
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

        isAction = false;

        if (cNetworkIdentity.isHost)
        {
            nav.isStopped = false;
            nav.CompleteOffMeshLink();
            offLink.activated = true;
            offLink.costOverride = -1;
            offLink = null;
            aiCon.offLink = null;
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.LAND);
        }
        else
        {
            //NavMeshHit hit;
            //nav.SamplePathPosition(groundLayer, 2f, out hit);
            //nav.nextPosition = hit.position;
            Debug.Log(offLink.endTransform.position);
            nav.enabled = true;
            nav.nextPosition = offLink.endTransform.position;
            //nav.CompleteOffMeshLink();
            Debug.Log("뛰어내리기 끝 처리");
            offLink.activated = true;
            offLink.costOverride = -1;
            offLink = null;
            aiCon.offLink = null;
            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
            _aiPacket.DeFlagAction();
            networkAISyncor.CheckSyncEnd(_aiPacket);
            networkAISyncor.EndFullAction();
        }
    }
}
