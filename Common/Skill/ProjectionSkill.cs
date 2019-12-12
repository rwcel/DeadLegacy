using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 투사체 스킬 함수
public class ProjectionSkill : MonoBehaviour
{
    // 파티클 오브젝트
    [SerializeField] GameObject spitParticle;
    [SerializeField] GameObject granade;

    Rigidbody rd;

    Skill skill;
    GameObject usingObj;

    float power;
    float jumpPower = 0.5f;

    float dist;                 // 타겟과의 거리
    float velocity;             // 속도
    float yVelocity;            // y축 속도
    float zVelocity;            // z축 속도
    Vector3 global_velocity;    // 월드좌표

    private void OnEnable()
    {
        rd = GetComponent<Rigidbody>();

        // scale에 따른 힘 조절
        power = 1 / transform.localScale.x;
    }

    /// <summary>
    /// 포물선함수
    /// 0. 파티클 실행
    /// 1. 목표 거리, y,z 속도구하기 + 로컬 방향벡터를 월드 방향벡터로 변환
    /// 2. 점프스킬이라면 오브젝트 자체를 addforce하기
    /// 3. guest 좀비들도 발사하는 척 하기
    /// </summary>
    /// <param name="_skill"></param>
    /// <param name="_usingObj"></param>
    /// <param name="_targetPos"></param>
    /// <param name="_damage"></param>
    public void ThrowThing(Skill _skill, GameObject _usingObj, Vector3 _targetPos, float _damage)   
    {
        // Debug.Log(power);
        skill = _skill;
        usingObj = _usingObj;

        // 0.
        ParicleActive(true);
        
        // 1.
        dist = Vector3.Distance(transform.position, _targetPos);
        velocity = Mathf.Sqrt(dist * -Physics.gravity.y / (Mathf.Sin(Mathf.Deg2Rad * skill.judgeAxis * 2)));
        yVelocity = velocity * Mathf.Sin(Mathf.Deg2Rad * skill.judgeAxis);
        zVelocity = velocity * Mathf.Cos(Mathf.Deg2Rad * skill.judgeAxis);
        global_velocity = transform.TransformVector(new Vector3(0f, yVelocity, zVelocity));

        // 2. **JudgeTime이 있다면 여기서 Condition을 바꿔줘야함(안하면 추락)
        if(skill.judgeShape == SkillJudgeShape.JUMP)
        {
            Debug.Log("도약하기");
            // Debug.Log(global_velocity * jumpPower);
            usingObj.GetComponent<AIBaseState>().ConditionRegulate(AIController.AIState.LEAP,true);
            usingObj.GetComponent<Rigidbody>().AddForce(global_velocity * power, ForceMode.Impulse);
            gameObject.SetActive(false);
        }
        else
        {
            rd.AddForce(global_velocity * power , ForceMode.Impulse);
        }

        //
        //InGameManager.Instance.TransmitZombieState();
    }

    /// <summary>
    /// 1. 타겟레이어 비트로 변환해서 비교
    /// 2. 파티클 및 오브젝트 activeFalse
    /// 3. 링크스킬인 경우 스킬매니저에게 스킬 사용(effectPower번호)
    /// 4. 아니면 스킬 효과 적용
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 1.
        LayerMask targetLayer = (int)skill.judgeTarget << (int)SkillJudgeTarget.PLAYER;

        if ((targetLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
			Debug.Log(collision.collider.name);

            // 2.
            ParicleActive(false);
			gameObject.SetActive(false);

            // 3.
            if (skill.effectKind == SkillEffectKind.LINK)
            {
                // Debug.Log("링크로 이동 : " + (int)skill.effectPower);
                SkillManager.instance.Skilling((int)skill.effectPower, usingObj, transform, transform.position, skill.damage);
            }
            // 4.
            else
            {
                collision.collider.gameObject.GetComponent<LivingEntity>().TakeSkillEffect(skill, skill.damage, transform.position, usingObj);
            }

            // velocity 중첩 막기
            rd.velocity = new Vector3(0, 0, 0);
        }
    }

    // 파티클 onoff
    void ParicleActive(bool _bool)
    {
        switch (skill.id)
        {
            case SkillManager.POSION_SPIT:
            case SkillManager.SLOW_SPIT:
            case SkillManager.SLOW_SPIT2:
                spitParticle.SetActive(_bool);
                if(_bool)
                    spitParticle.GetComponent<ParticleSystem>().Play();
                break;
            case SkillManager.THROW_GRENADE:
                granade.SetActive(_bool);
                break;
            default:
                break;
        }
    }
}
