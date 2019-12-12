using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// AI DB 정보 읽어서 데이터 적용하는 것
/// </summary>
public class CSV_AI : MonoBehaviour
{
    public Dictionary<short, AIInfo> aiDictionary;
    public Dictionary<short, AggroData> aggroDictionary;

    short id;                         // AI 번호 : 딕셔너리 키값
    AIInfo aiInfo;                  // 넣을 데이터들 : 딕셔너리 value
    AggroData aiAggro;              // 어그로 데이터 : 딕셔너리 value

    // 임시 변수값들
    int tmpInt;
    string tmpString;
    string[] tmpSplit, tmpSplit2;
    float minScale, maxScale;       // 몸크기 랜덤해서 넣는 임시변수

    public static CSV_AI instance = null;   // 싱글톤

    /// <summary>
    /// 1. 기본 값들 받기
    /// 2. 성별 : string데이터 대문자로 넣기
    /// 3. 기본행동 : int 값 ENUM으로 변환해서 넣기
    /// 4. 크기 : 데이터 / 로 잘라서 랜덤부여해서 넣기 
    /// 5. 스킬 : 스킬개수만큼 배열만들고 id 넣기 
    /// 6. 옷 : 몸은(0번) 무조건 보이게, 1번부터는 % 랜덤
    /// 7. 팔레트 : 팔레트 DB에 해당하는 배열 R,G,B 저장
    /// 8. 어그로 변수 받기
    /// 9. 딕셔너리에 추가
    /// </summary>
    void Awake()
    {
        instance = this;

        aiDictionary = new Dictionary<short, AIInfo>();
        aggroDictionary = new Dictionary<short, AggroData>();

        List<Dictionary<string, object>> data = CSVReader.Read("AIDB");
        //Debug.Log("딕셔너리 수 :" + data.Count);
        List<Dictionary<string, object>> data2 = CSVReader.Read("SkillDB");
        //Debug.Log("딕셔너리 수 :" + data2.Count);
        List<Dictionary<string, object>> data3 = CSVReader.Read("PaletteDB");
        //Debug.Log("딕셔너리 수 :" + data3.Count);

        for (var i = 0; i < data.Count; i++)
        {
            // 1.
            short.TryParse(data[i]["ID"].ToString(), out id);
            aiInfo.id = id;
            // Debug.Log("id : " + id);
            float.TryParse(data[i]["HP"].ToString(), out aiInfo.hp);
            float.TryParse(data[i]["Damage"].ToString(), out aiInfo.damage);
            float.TryParse(data[i]["Armor"].ToString(), out aiInfo.armor);
            float.TryParse(data[i]["MinHitDamage"].ToString(), out aiInfo.minHitDamage);
            float.TryParse(data[i]["NormalSpeed"].ToString(), out aiInfo.normalSpeed);
            float.TryParse(data[i]["ChaseSpeed"].ToString(), out aiInfo.chaseSpeed);
            float.TryParse(data[i]["ChaseDist"].ToString(), out aiInfo.chaseDist);


            // 2.
            tmpString = data[i]["Gender"].ToString();
            tmpString = tmpString.ToUpper();
            aiInfo.gender = (Gender)System.Enum.Parse(typeof(Gender), tmpString);

            // 3.
            int.TryParse(data[i]["IdleState"].ToString(), out tmpInt);
            aiInfo.idleState = (IdleState)tmpInt;
            int.TryParse(data[i]["BasicState"].ToString(), out tmpInt);
            aiInfo.patrolState = (PatrolState)tmpInt;

            // 4.
            tmpString = data[i]["Scale"].ToString();
            tmpSplit = tmpString.Split('/');
            float.TryParse(tmpSplit[0], out minScale);
            float.TryParse(tmpSplit[1], out maxScale);
            aiInfo.scale = Random.Range(minScale, maxScale);


            // 5.
            int.TryParse(data[i]["SkillNum"].ToString(), out aiInfo.skill.skillCount);
            aiInfo.skill.skillIds = new int[aiInfo.skill.skillCount];
            for (int j = 0; j < aiInfo.skill.skillCount; j++)
            {
                int.TryParse(data[i]["SkillID" + (j + 1)].ToString(), out aiInfo.skill.skillIds[j]);
            }

            // 6.
            int.TryParse(data[i]["Material"].ToString(), out aiInfo.clothesNum);
            if (aiInfo.clothesNum > 0)
            {
                aiInfo.looks = new float[aiInfo.clothesNum];
                aiInfo.looks[0] = 1f;

                for (int j = 1; j < aiInfo.looks.Length; j++)
                    float.TryParse(data[i]["Clothes" + (j)].ToString(), out aiInfo.looks[j]);
            }
            else
                aiInfo.looks = new float[0];

            // 7.
            int.TryParse(data[i]["PaletteID"].ToString(), out aiInfo.paletteID);
            for (int j = 0; j < data3.Count; j++)
            {
                if (aiInfo.paletteID == (int)data3[j]["ID"])
                {
                    int.TryParse(data3[j]["ID"].ToString(), out aiInfo.paletteID);
                    int.TryParse(data3[j]["Num"].ToString(), out aiInfo.paletteNum);
                    aiInfo.palette = new float[aiInfo.paletteNum, 3];
                    for (int k = 0; k < aiInfo.paletteNum; k++)
                    {
                        tmpString = data3[j]["Palette" + (k + 1)].ToString();
                        //Debug.Log(tmpString);
                        tmpSplit = tmpString.Split('/');

                        for (int l = 0; l < 3; l++)
                            float.TryParse(tmpSplit[l], out aiInfo.palette[k, l]);
                    }
                    break;
                }
            }

            // 8. 
            float.TryParse(data[i]["RagePoint"].ToString(), out aiAggro.ragePoint);
            float.TryParse(data[i]["RagePointFactor"].ToString(), out aiAggro.ragePointFactor);
            float.TryParse(data[i]["DistanceFactor"].ToString(), out aiAggro.distanceFactor);
            float.TryParse(data[i]["HeightFactor"].ToString(), out aiAggro.heightFactor);
            float.TryParse(data[i]["TargetCountFactor"].ToString(), out aiAggro.targetCountFactor);
            float.TryParse(data[i]["ViewAngle"].ToString(), out aiAggro.viewAngle);
            float.TryParse(data[i]["ViewDist"].ToString(), out aiAggro.viewDistance);
            bool.TryParse(data[i]["ViewDist"].ToString(), out aiAggro.heightRecognition);
            aiAggro.chaseDist = aiInfo.chaseDist;


            // 9.
            aiDictionary.Add(id, aiInfo);
            aggroDictionary.Add(id, aiAggro);
        }
    }
}
