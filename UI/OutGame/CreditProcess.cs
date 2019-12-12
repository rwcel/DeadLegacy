using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditProcess : MonoBehaviour
{
    public void CreditEnd()
    {
        DollyCartManager.instance.DollyCameraMove(DollyCartManager.instance.creditTrack, UIManager.instance.creditPanel, UIManager.instance.mainmenuPanel, -1);
    }
}
