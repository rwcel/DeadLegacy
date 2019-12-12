using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 판정스킬 함수
public class JudgingSkill : MonoBehaviour
{
    // 파티클 오브젝트
    [SerializeField] GameObject bombParticle;
    [SerializeField] GameObject posionParticle;

    // 스킬 정보 변수(넘겨주기)
    protected Skill skill;
    GameObject usingObj;                // 스킬사용 오브젝트(**판정타입이 Target이면 스킬 받는 대상)
    float damage;
    Vector3 targetPos;

    float recycleTime = 1.0f;           // 트리거 지속시간 : 스킬지속시간 없을경우

    int layerNum;                       // 레이어 이름
    protected LayerMask targetLayer;	// 타겟 레이어마스크(비트)

    /// <summary>
    /// 스킬 판정 전 세팅
    /// 1.스킬 정보 데이터 적용
    /// 2. 파티클 오브젝트 관련 설정
    /// </summary>
    /// <param name="_skill">스킬 정보</param>
    /// <param name="_usingObj">스킬 사용 오브젝트</param>
    /// <param name="_targetPos">대상 위치</param>
    /// <param name="_damage">데미지</param>
    public void SkillJudgement(Skill _skill, GameObject _usingObj, Vector3 _targetPos, float _damage)
    {
        // 1.
        skill = _skill;
        usingObj = _usingObj;
        damage = _damage;
        targetPos = _targetPos;
        // Debug.Log(skill.id);

        // 2.
        switch (skill.id)
        {
            // **바닥, 플레이어 맞는거 전부 y 위치 다르게 조절해야함
            case SkillManager.JUDGE_GRENADE:
            case SkillManager.SELF_DESTRUCT:
                bombParticle.SetActive(true);
                bombParticle.GetComponent<ParticleSystem>().Play();
                break;
            case SkillManager.JUDGE_LEAP:
                posionParticle.SetActive(true);
                posionParticle.GetComponent<ParticleSystem>().Play();
                break;
            case SkillManager.RUSH_ATTACK:
            case SkillManager.SHIELD_RUSH:
                GetComponent<SphereCollider>().enabled = true;
                break;
        }

        Judgement();
    }

    /// <summary>
    /// 판정하는 함수
    /// 1. 타겟레이어 비트연산자로 변환
    /// 2. 판정 : 스킬형태에 따라
    /// </summary>
    void Judgement()
    {
        // 1.
        targetLayer = (int)skill.judgeTarget << (int)SkillJudgeTarget.PLAYER;

        Collider[] _target = null;

        // 2. 
        switch (skill.judgeShape)
        {
            // 타겟
            case SkillJudgeShape.TARGET:
                SkillCheck(usingObj.gameObject);
                break;
            // 원 : 범위 내 확인
            case SkillJudgeShape.CIRCLE:
                _target = Physics.OverlapSphere(transform.position, skill.judgeRange, targetLayer);
                for (int i = 0; i < _target.Length; i++)
                {
                    // Debug.Log(_target[i].name);
                    SkillCheck(_target[i].gameObject);
                }
                break;
            // 부채꼴 : 높이 맞추기, 범위 각도 내 확인
            case SkillJudgeShape.ARC:
                //Debug.Log("부채꼴");
                _target = Physics.OverlapSphere(transform.position, skill.judgeRange, targetLayer);
                for (int i = 0; i < _target.Length; i++)
                {
                    Vector3 _direction = (_target[i].gameObject.transform.position - transform.position).normalized;
                    _direction.y = 0;
                    float _angle = Vector3.Angle(_direction, transform.forward);
                    // Debug.Log(_angle + ", " + skill.axisRange);
                    if (_angle < skill.judgeAxis * 0.5f)
                    {
                        SkillCheck(_target[i].gameObject);
                    }
                }
                break;
            // 사각형 : 범위 내 확인
            case SkillJudgeShape.SQUARE:
                //Debug.Log("사각형");
                _target = Physics.OverlapBox(transform.position, new Vector2(skill.judgeRange, skill.judgeAxis), transform.rotation, targetLayer);
                for (int i = 0; i < _target.Length; i++)
                {
                    SkillCheck(_target[i].gameObject);
                }
                break;
            // 레이 : 맞은 타겟 확인
            case SkillJudgeShape.RAY:
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, skill.judgeRange, targetLayer))
                {
                    Debug.DrawLine(transform.position, targetPos, Color.magenta, 3.0f);
                    //Debug.Log("성공 : " + hit.collider.name);
                    // InGameManager.Instance.TransmitPlayerHit(hit.collider.gameObject.transform.root.GetComponent<CNetworkIdentity>().Netcode, (int)20);

                    Debug.Log(hit.collider.gameObject.transform.root.name);
                    SkillCheck(hit.collider.gameObject);
                }
                break;
            // 지속 트리거 : 몸에 부착
            case SkillJudgeShape.SUSTAIN_TRIGGER:
                Debug.Log("부착");
                transform.SetParent(usingObj.transform);

                // AI, Player다르게
                switch(usingObj.layer)
                {
                    case (int)SkillJudgeTarget.MONSTER:
                        usingObj.transform.GetComponent<AIController>().judgeObj = this.gameObject;
                        break;
                    case (int)SkillJudgeTarget.PLAYER:
                        usingObj.transform.GetComponent<PlayerController>().judgingObj = this.gameObject;
                        break;
                }
                break;
            default:
                break;
        }
        // 스킬 사라지게 하기
        if (skill.effectDuration != 0)
            StartCoroutine(GameObjectActiveFalse(skill.effectDuration));
        else
            StartCoroutine(GameObjectActiveFalse(recycleTime));
    }

    /// <summary>
    /// 스킬대상 확인
    /// 1. 지속스킬사용 오브젝트 + 본인이라면 무시하기
    /// 2. LivingEntity의 스킬 효과 적용 (데미지 슬로우 등등)
    /// </summary>
    /// <param name="obj"></param>
    public void SkillCheck(GameObject obj)
    {
        Debug.Log(obj.name);
        //Debug.Log(transform.parent.name);
        //Debug.Log(skill.effectKind);

        // 1.
        if (transform.parent == obj.transform)
            return;

        // 2.
        obj.gameObject.GetComponent<LivingEntity>().TakeSkillEffect(skill, damage, transform.position, usingObj);
    }

    // 스킬 사용 후, 오브젝트 지우기
    IEnumerator GameObjectActiveFalse(float _time)
    {
        yield return new WaitForSeconds(_time);

        // **모든 연산에서 하기때문에 주의
        switch (skill.id)
        {
            case SkillManager.JUDGE_GRENADE:
            case SkillManager.SELF_DESTRUCT:
                bombParticle.SetActive(false);
                bombParticle.GetComponent<ParticleSystem>().Play();
                // 일정시간 뒤, 스킬 사라지게 하기
                break;
            case SkillManager.JUDGE_LEAP:
                posionParticle.SetActive(false);
                posionParticle.GetComponent<ParticleSystem>().Play();
                break;
            case SkillManager.RUSH_ATTACK:
            case SkillManager.SHIELD_RUSH:
                ResetObj();
                break;
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 지속 사용 오브젝트 제자리에 넣기
    /// </summary>
    public void ResetObj()
    {
        Debug.Log("제자리로!");
        transform.SetParent(SkillManager.instance.transform);
        GetComponent<SphereCollider>().enabled = false;
    }

    /// <summary>
    /// 지속 트리거에 사용
    /// 각도 맞으면 스킬처리
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // **최상위 객체..?
        Debug.Log(other.gameObject.name);
        if (other.gameObject != usingObj)
        {
            if (other.gameObject.CompareTag("Fluid"))
                return;

            if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                Vector3 _direction = (other.gameObject.transform.position - transform.position).normalized;
                _direction.y = 0;
                float _angle = Vector3.Angle(_direction, transform.forward);
                //Debug.Log(_angle + ", " + skill.judgeAxis);
                //Debug.Log(other.name);
                if (_angle < skill.judgeAxis * 0.5f)
                {
                    SkillCheck(other.gameObject);
                }
            }
            // **수정
            if((int)SkillJudgeTarget.ENVIRO == other.gameObject.layer || (int)SkillJudgeTarget.PLAYER == other.gameObject.layer)
            {
                //usingObj.transform.GetChild(0).GetComponent<AIAnim>().RushRebound();
                usingObj.GetComponent<AIRushState>().RushRebound();
                ResetObj();
            }
        }
    }
}
