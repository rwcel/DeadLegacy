#region ENUM
// 다음 스폰 즉시 발동여부
public enum SpawnActionTime
{
    // 즉시   끝나고
    SYNCHRO, SEQUENCE
}

/// SkillManager
public enum SkillType
{
    JUDGE, PROJECTION, SUMMON, DOT,
}

// 스킬 판정 형태  **
public enum SkillJudgeShape
{   // 타겟   원형  부채꼴  사각형  레이 지속(트리거)
    TARGET, CIRCLE, ARC, SQUARE, RAY, SUSTAIN_TRIGGER,
    JUMP = 20,
}

// 스킬 판정 대상 (비트연산)
public enum SkillJudgeTarget
{
    PLAYER = 9, MONSTER, ENVIRO, GROUND, DIE
}

// 스킬 종류
public enum SkillEffectKind
{// 기본 슬로우 스턴	  독	넉백    유인
    NOT, SLOW, STUN, POISON, KNOCKBACK, DECOY,
// 힐          진통힐
    HEAL = 10, PAINKILLERHEAL,                  
    SUMMON = 98, LINK           // 소환 98, 링크스킬 99번
}


/// AIController
// AI 방식의 종류
public enum AIType
{
    // 스폰   필드  죽음(안씀)
    SPAWN, FIELD, DIE
}

// AI성별 -> SoundManager
public enum Gender
{
    //    남    여		중성
    MALE, FEMALE, NEUTRAL
}

// 대기상태 행동
public enum IdleState
{
    // 없음	 먹기
    NON, EAT,
}

// 기본 행동
public enum PatrolState
{
    // 대기행동  근처		순서/역순(**작업 안할듯)
    IDLE, CLOSE, ORDER, REVERSE,
}


#endregion