using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

// 마우스로 이동시키기
public class AllMonsterController : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    Vector3 startPoint;
    Vector3 endPoint;
    Vector3 centerPoint;

    List<GameObject> monsterList = new List<GameObject>();
    Collider[] colHit;

    LayerMask targetMask;
    LayerMask groundMask;

    private void Start()
    {
        targetMask = 1 << 10;
        groundMask = 1 << 12;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit,targetMask))
            {
                startPoint = hit.point;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                endPoint = hit.point;
            }
            centerPoint = (endPoint + startPoint) * 0.5f;

            float rad = Mathf.Sqrt(Mathf.Pow(endPoint.x - centerPoint.x, 2) + Mathf.Pow(endPoint.z - centerPoint.z, 2));

            colHit = Physics.OverlapSphere(centerPoint, rad, targetMask);
            for (int i = 0; i < colHit.Length; i++)
            {
                colHit[i].gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // Debug.Log(hit.point);
                for (int i = 0; i < colHit.Length; i++)
                {
                    colHit[i].GetComponent<NavMeshAgent>().SetDestination(hit.point);
                }
            }
        }
    }
}