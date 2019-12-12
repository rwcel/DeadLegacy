using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LivingEntity : MonoBehaviour
{
    [BoxGroup("Status")]
    [ProgressBar]
    [SerializeField] [MinValue(0)] public float currentHp;  // 현재 체력
    [HideInInspector] public float maxHp = 100;             // 최대 체력
    [BoxGroup("Status")]
    [SerializeField] [MinValue(0)] public float speed;      // 속도

    // 스턴상태
    protected bool m_stun;
    public bool bStun
    {
        get { return m_stun; }
        set { m_stun = value; }
    }
    // 실드상태
    protected bool m_shield;
    public bool bShield
    {
        get { return m_shield; }
        set { m_shield = value; }
    }
    // 넉백상태
    protected bool m_knockback;
    public bool bKnockback
    {
        get { return m_knockback; }
        set { m_knockback = value; }
    }

    protected SkillProcess skillProcess;        // 스킬프로세스

    protected Skill activateSkill;              // 사용중인 스킬 
    Vector3 m_force;                            // 공격한 쪽의 위치
    GameObject m_attackObject;                  // 공격한 오브젝트
    protected float m_knockForce = 4f;          // 기본 넉백힘
    protected float m_knockYForce = 6f;         // y축 넉백 힘

    /// <summary>
    /// 스킬 효과 처리
    /// </summary>
    /// <param name="skill">스킬정보</param>
    /// <param name="damage">데미지</param>
    /// <param name="forcePos">공격방향</param>
    /// <param name="attackObj">공격오브젝트</param>
    public void TakeSkillEffect(Skill skill, float damage, Vector3 forcePos, GameObject attackObj)
    {
        activateSkill = skill;
        m_force = forcePos;
        m_attackObject = attackObj;

        //Debug.Log(activateSkill.id);

        switch (activateSkill.effectKind)
        {
            case SkillEffectKind.NOT:
                // 여기서 보내..?
                TakeDamage(skill, damage, m_force, m_attackObject);
                break;
            case SkillEffectKind.SLOW:
                if(gameObject.GetComponent<SlowDebuff>() == null)
                {
                    gameObject.AddComponent<SlowDebuff>();
                    gameObject.GetComponent<SlowDebuff>().SettingSkill(activateSkill, attackObj);
                }
                break;
            case SkillEffectKind.STUN:
                StartCoroutine(TakeStun(skill));
                break;
            case SkillEffectKind.POISON:
                if(gameObject.GetComponent<PosionDebuff>() == null)
                {
                    gameObject.AddComponent<PosionDebuff>();
                    gameObject.GetComponent<PosionDebuff>().SettingSkill(activateSkill, attackObj);
                }
                break;
            case SkillEffectKind.KNOCKBACK:
                if (m_knockback)
                    return;

                KnockBack(m_force);
                m_knockback = true;
                // 넉백에 데미지 처리하기
                if (damage != 0)
                {
                    TakeDamage(skill, damage, m_force, m_attackObject);
                }
                break;
            case SkillEffectKind.HEAL:
                if (gameObject.GetComponent<HealBuff>() == null)
                {
                    gameObject.AddComponent<HealBuff>();
                    gameObject.GetComponent<HealBuff>().SettingSkill(activateSkill, attackObj);
                }
                break;
            case SkillEffectKind.PAINKILLERHEAL:
                if (gameObject.GetComponent<PainKillerBuff>() == null)
                {
                    gameObject.AddComponent<PainKillerBuff>();
                    gameObject.GetComponent<PainKillerBuff>().SettingSkill(activateSkill, attackObj);
                }
                break;
        }
    }

    /// <summary>
    /// 데미지 받는 함수
    /// </summary>
    /// <param name="damage">데미지</param>
    /// <param name="force">때린 위치</param>
    /// <param name="attackObject">공격한 오브젝트</param>
    public virtual void TakeDamage(Skill skill, float damage, Vector3 force, GameObject attackObject) { }

    // 스턴
    public IEnumerator TakeStun(Skill skill)
    {
        m_stun = true;
        yield return new WaitForSeconds(skill.effectDuration);
        m_stun = false;
    }

    /// <summary>
    /// 스킬매니저 안거치고 가는 힐 : 힐총
    /// </summary>
    /// <param name="time"> 힐마다 쉬는 시간 </param>
    /// <param name="cycle"> 싸이클 횟수 </param>
    /// <param name="healValue"> 한번 힐 량 </param>
    /// <returns></returns>
    public IEnumerator TakeHeal(float time, float cycle, float healValue)
    {
        for (int i = 0; i < cycle; i++)
        {
            //Debug.Log("회복전:"+currentHp);
            currentHp += healValue;
            yield return new WaitForSeconds(time);
        }
        // Debug.Log("회복:" + currentHp);
    }


    // 넉백
    public virtual void KnockBack(Vector3 pos) { }

    // 실드 확인
    public virtual bool ShieldCheck(Vector3 targetPos) { return true; }
}
