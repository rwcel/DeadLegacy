using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndCheck : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UIManager.instance.ClosePanel();
            GameManager.instance.StartCoroutine(GameManager.instance.AsyncLoadScene("RoomScene"));
            // GameManager.instance.
        }
    }

}
