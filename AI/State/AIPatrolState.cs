using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrolState : AIBaseState
{
    float patrolRange = 8.0f;                       // 순찰 범위
    float patrolDist = 4.0f;                        // 네비게이션 거리 측정 범위 (제곱)
    float patrolSquareRange = 10f;                  // 주위 네브매쉬 원 범위

    float nextParolWaitTime = 2.0f;                 // 다음 위치를 찾기까지 기다리는 시간
    float patrolCheckTime = 0.5f;                   // 확인하는 시간

    int patrolTick = 30;                            // 처음에 바로 탐색할 수 있게 하기. 이후 0 (1.8초)
    int nextPatrolTickCount = 30;

    /// <summary>
    /// 첫 탐색 위치 지정(바로 다음 구문 실행하게 하기 위해 자기 위치로 지정)
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;
        if (aiCon.aiInfo.patrolState > PatrolState.IDLE)
        {
            nav.isStopped = false;
        }
    }

    private void FixedUpdate()
    {
        tick++;
        patrolTick++;

        if (tick >= aiTickCount)
        {
            tick = 0;

            switch (aiCon.aiInfo.patrolState)
            {
                // 1.
                case PatrolState.IDLE:
                    switch (aiCon.aiInfo.idleState)
                    {
                        case IdleState.NON:
                            break;
                        case IdleState.EAT:
                            break;
                    }
                    break;
                // 2.
                case PatrolState.CLOSE:
                    PatrolCloseCheck();
                    // StartCoroutine(PatrolClose());
                    break;
                // 순서대로 : 1-2-3-1-2-...
                case PatrolState.ORDER:
                    break;
                // 되돌아가기 : 1-2-3-2-1-...
                case PatrolState.REVERSE:
                    break;
            }
        }
    }


    void PatrolCloseCheck()
    {
        if ((transform.position - patrolDest).sqrMagnitude < patrolDist)
        {
            if (patrolTick >= nextPatrolTickCount)
            {
                // 1-1.
                patrolDest = createPos + Random.insideUnitSphere * patrolRange;
                NavMeshHit navHit;
                NavMesh.SamplePosition(patrolDest, out navHit, patrolSquareRange, NavMesh.AllAreas);

                RaycastHit _rayHit;
                Vector3 _direction = (navHit.position - transform.position);
                // 1-2.
                if (Physics.Raycast(transform.position, _direction, out _rayHit, patrolRange))
                {
                    //Debug.Log("실패:"+ patrolDest);
                    patrolDest = transform.position;
                }
                // 2.
                else
                {
                    patrolDest = navHit.position;
                    nav.SetDestination(patrolDest);
                    // Debug.Log("성공:" + patrolDest);

                    patrolTick = 0;
                }
            }

        }
    }

    public override void EndAction()
    {
        base.EndAction();
        this.enabled = false;
    }
}
