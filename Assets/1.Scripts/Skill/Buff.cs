﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManUniqueSkill : Skill
{
    private const int TICK_MULT = 20;
    private const float HEAL_RATE = 0.05f;

    public override void InitSkill()
    {
        SetNameAndExplanation("숨 고르기", "매 5초마다 잃은 체력의 5%를 회복합니다.");
    }

    public override IEnumerator OnAlways()
    {
        BattleStat myBattleStat = owner.GetBattleStat();
        float healAmount = 0;
        while (true)
        {
            if (owner.GetSuperState() != SuperState.PassedOut)
            {
                yield return new WaitForSeconds(SkillConsts.TICK_TIME * TICK_MULT);
                healAmount = myBattleStat.MissingHealth * 0.05f;
                myBattleStat.Heal(healAmount);

                if (healAmount > 1)
                    DisplayHeal(healAmount);
            }
        }
    }

    public void DisplayHeal(float healAmount)
    {
        owner.DisplayHeal(healAmount);
    }
}

public class NyangUniqueSkill : Skill
{
    BattleStat myBattleStat;
    StatModContinuous myMod;
    GameObject skillEffect;

    public override void InitSkill()
    {
        myBattleStat = owner.GetBattleStat();
        myMod = new StatModContinuous(StatType.Attack, ModType.Mult, 2.0f);

        skillEffect = Instantiate((GameObject)Resources.Load("EffectPrefabs/Default_BuffEffect"));
        skillEffect.transform.SetParent(owner.GetTransform());
        skillEffect.transform.position = owner.GetPosition();

        SetNameAndExplanation("선수필승!", "비 전투 상태의 적을 공격할 때 공격력이 200% 증가합니다.");
    }

    public override void BeforeAttack()
    {
        SetEnemy();

        if (enemy.GetSuperState() != SuperState.Battle)
        {
            myBattleStat.AddStatModContinuous(myMod);
            DisplaySkillEffect();

        }
    }

    public override void AfterAttack()
    {
        myBattleStat.RemoveStatModContinuous(myMod);
    }

    public override IEnumerator OnAlways()
    {
        yield return null;
    }

    public void DisplaySkillEffect()
    {
        skillEffect.GetComponent<AttackEffect>().StartEffect();
        skillEffect.transform.position = new Vector3(owner.GetPosition().x, owner.GetPosition().y + 0.08f, owner.GetPosition().z);
    }
}

public class WalUniqueSkill : Skill
{
    BattleStat myBattleStat;

    StatModContinuous atkMod;
    const float ATK_BONUS_RATE = 0.4f;
    StatModContinuous atkSpdMod;
    const float ATKSPD_PENALTY_RATE = -0.33f;

    StatModContinuous shieldMod;
    GameObject skillEffect;
    float shieldTimer;
    TemporaryEffect shieldBuff;
    const float DURATION = 3.0f;
    const float SHIELD_RATE = 0.03f;

    public override void InitSkill()
    {
        myBattleStat = owner.GetBattleStat();
        shieldMod = new StatModContinuous(StatType.Shield, ModType.Fixed, 0);
        shieldBuff = new TemporaryEffect(DURATION);
        shieldTimer = -1;

        atkMod = new StatModContinuous(StatType.Attack, ModType.Mult, ATK_BONUS_RATE);
        atkSpdMod = new StatModContinuous(StatType.AttackSpeed, ModType.Mult, ATKSPD_PENALTY_RATE);

        skillEffect = Instantiate((GameObject)Resources.Load("EffectPrefabs/ShieldGaining_BuffEffect"));
        skillEffect.transform.SetParent(owner.GetTransform());
        skillEffect.transform.position = owner.GetPosition();

        SetNameAndExplanation("파비스 석궁병", "공격력이 40% 증가하는 대신 공격속도가 33% 감소합니다. 매 사격마다 최대체력의 3%에 해당하는 방어막을 얻습니다. 방어막은 중첩되지 않습니다.");
    }

    public override IEnumerator OnAlways()
    {
        //while (true)
        //{
        //    yield return new WaitForSeconds(SkillConsts.TICK_TIME);

        //    if (shieldTimer >= 0 - Mathf.Epsilon)
        //    {
        //        shieldTimer += SkillConsts.TICK_TIME;

        //        if (shieldTimer >= DURATION)
        //        {
        //            shieldTimer = -1;
        //            myBattleStat.RemoveStatModContinuous(myMod);
        //        }
        //    }
        //}
        yield return null;
    }

    public override void OnAttack(float actualDamage, bool isCrit, bool isDodged)
    {
        owner.RemoveTemporaryEffect(shieldBuff);

        // TemporaryEffect 생성
        shieldMod.ModValue = myBattleStat.HealthMax * SHIELD_RATE;
        StatModContinuous tempMod = new StatModContinuous(shieldMod);
        shieldBuff = new TemporaryEffect(DURATION);
        shieldBuff.AddContinuousMod(shieldMod);

        owner.AddTemporaryEffect(shieldBuff);
        //shieldTimer = 0;
        DisplaySkillEffect();
    }

    public override void ApplyStatBonuses()
    {
        myBattleStat.AddStatModContinuous(atkMod);
        myBattleStat.AddStatModContinuous(atkSpdMod);
    }

    public override void RemoveStatBonuses()
    {
        myBattleStat.RemoveStatModContinuous(atkMod);
        myBattleStat.RemoveStatModContinuous(atkSpdMod);
    }

    public void DisplaySkillEffect()
    {
        skillEffect.GetComponent<AttackEffect>().StartEffect();
        skillEffect.transform.position = new Vector3(owner.GetPosition().x, owner.GetPosition().y + 0.08f, owner.GetPosition().z);
    }
}

public class CommonSelfBuffSkill : Skill
{
    List<StatModContinuous> continuousMods;
    List<StatModDiscrete> discreteMods;
    BattleStat myBattleStat;

    public CommonSelfBuffSkill(string name, string explanation)
    {
        SetNameAndExplanation(name, explanation);
    }

    public override void InitSkill()
    {
        myBattleStat = owner.GetBattleStat();
    }

    public override void ApplyStatBonuses()
    {
        foreach (StatModContinuous mod in continuousMods)
            myBattleStat.AddStatModContinuous(mod);
        foreach (StatModDiscrete mod in discreteMods)
            myBattleStat.AddStatModDiscrete(mod);
    }

    public override void RemoveStatBonuses()
    {
        foreach (StatModContinuous mod in continuousMods)
            myBattleStat.RemoveStatModContinuous(mod);
        foreach (StatModDiscrete mod in discreteMods)
            myBattleStat.RemoveStatModDiscrete(mod);
    }

    public void InstantiateLists()
    {
        continuousMods = new List<StatModContinuous>();
        discreteMods = new List<StatModDiscrete>();
    }

    public void AddContinuousMod(StatModContinuous mod)
    {
        continuousMods.Add(mod);
    }

    public void AddDiscreteMod(StatModDiscrete mod)
    {
        discreteMods.Add(mod);
    }
}

//public class MaxiUniqueSkill : Skill
//{
//    StatModContinuous atkMod;
//    const float ATK_BONUS_RATE = 0.4f;
//    StatModContinuous atkSpdMod;
//    const float ATKSPD_PENALTY_RATE = -0.33f;

//    public override void InitSkill()
//    {
//        SetNameAndExplanation("쇼맨십", "공격속도가 200% 증가하지만 공격력은 65% 감소합니다.");
//    }
//}