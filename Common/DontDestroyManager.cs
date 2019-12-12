using System.Collections.Generic;
using UnityEngine;

// 매니저들 dontdestroy하는 함수
public class DontDestroyManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}