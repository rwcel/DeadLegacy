using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIJumpState : AIBaseState
{
    public OffMeshLink offLink = null;         // 링크 가지고 있는지 확인
    Vector3 startMatchTarget;                   // 매치타겟 [공중]
    Vector3 endMatchTarget;                     // 매치타겟 [끝]

    /// <summary>
    /// 1. 대각선으로 뛰지않게 위치 조절
    /// ** 시작-끝은 문제 없으나, 끝-시작이 문제가 있다
    /// ** 점프 속도는 애니메이션 자체 속도로 조절함 0.66
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();

        offLink = nav.currentOffMeshLinkData.offMeshLink;
        if (isAction)
            return;

        isAction = true;
        anim.applyRootMotion = true;

        // 1.
        // transform.position = Vector3.Lerp(transform.position, offLink.startTransform.position, 0.2f);
        RaycastHit hit;
        Debug.DrawRay(transform.position+ new Vector3(0, 0.5f, 0), transform.forward*2f, Color.blue, 5f);
        if (Physics.Raycast(transform.position + new Vector3(0,0.5f,0), transform.forward, out hit, 2f, enviroLayer))
        {
            transform.rotation = Quaternion.LookRotation(-hit.normal);
            Debug.Log(hit.normal);
            transform.position = hit.point + (hit.normal * col.bounds.extents.x * 0.8f);
            StartCoroutine(JumpLerp());

            startMatchTarget = new Vector3(hit.point.x, hit.collider.bounds.center.y + 1f * col.bounds.extents.y , hit.point.z);
            // endMatchTarget = offLink.endTransform.position;

            //GameObject _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //_cube.GetComponent<Collider>().isTrigger = true;
            //_cube.transform.position = startMatchTarget;
        }
        // offLink.activated = false;
    }

    IEnumerator JumpLerp()
    {
        //while(true)
        //{
        //    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        //    {
        //        Debug.Log("매치타겟1");
        //        anim.MatchTarget(startMatchTarget, new Quaternion(), AvatarTarget.RightFoot, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0f, 0.3f); // start and stop time 
        //        break;
        //    }
        //    yield return null;
        //}

        while (true)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.3f)
            {
                Debug.Log("매치타겟2");
                if(offLink== null)
                {
                    offLink = nav.currentOffMeshLinkData.offMeshLink;
                    Debug.Log("아앗");
                }
                    
                // anim.MatchTarget(endMatchTarget, new Quaternion(), AvatarTarget.RightFoot, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.3f, 0.65f); // start and stop time 
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("점프 정상종료");
    }
   

    /// <summary>
    /// 애니메이션 이벤트
    /// 1. 위치 이동시키기
    /// 2. 상태 돌려주기
    /// 3. 바로 움직일 수 있게 해주기
    /// </summary>
    /// <param name="_pos"></param>
    public override void EndState(Vector3 _pos)
    {
        base.EndState(_pos);

        Debug.Log("점프 끝");
        StopCoroutine(JumpLerp());

        // 1.
        NavMeshHit navHit;
        Debug.Log(transform.position);
        NavMesh.SamplePosition(transform.GetChild(0).transform.position, out navHit, 2.0f, NavMesh.AllAreas);
        nav.nextPosition = navHit.position;
        // nav.nextPosition = Vector3.Lerp(transform.position, offLink.endTransform.position, 0.2f);
        // offLink.activated = true;

        // 2.
        isAction = false;

        // 3.
        aiCon.EndActionCheckState(AIController.AIState.JUMP);
    }
}
