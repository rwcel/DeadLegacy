//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Global;

//public class PlayerImageUI : MonoBehaviour
//{
//    public PlayerInfo playerInfo;
//    public Image playerImage;
//    // Start is called before the first frame update
//    void Start()
//    {
//        playerInfo = GameManager.instance.getCurrentPlayer();
//        switch (playerInfo.charCode)
//        {
//            case CharacterCode.Richard:
//                playerImage.sprite = UIManager.instance.playerImage[0];
//                break;
//            case CharacterCode.Chris:
//                playerImage.sprite = UIManager.instance.playerImage[1];
//                break;
//            case CharacterCode.Jason:
//                playerImage.sprite = UIManager.instance.playerImage[2];
//                break;
//            case CharacterCode.Colin:
//                playerImage.sprite = UIManager.instance.playerImage[3];
//                break;
//        }
//    }
//}
