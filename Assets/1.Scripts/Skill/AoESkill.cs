﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoESkill : Skill
{
    protected List<TileForMove> effectedAreas; // 매 공격시, 효과 대상 TFM
    protected List<Coverage> coverages; // 스킬별 고유의 효과지역 모양. {0, 0}이 타겟 좌표, y축이 중심축(소유자와 타겟의).

    protected class Coverage // 0, 0인 
    {
        public int x, y;

        public Coverage(int xIn, int yIn)
        {
            x = xIn;
            y = yIn;
        }
    }
    protected enum CentralAxis { X, Y, InverseX, InverseY }

    /// <summary>
    /// 효과범위를 저장하는 메서드. 하위 클래스에서 구현해주면 됨.
    /// </summary>
    public abstract void SetCoverage();

    public AoESkill()
    {
        //targets = new List<ICombatant>();
        effectedAreas = new List<TileForMove>();
        coverages = new List<Coverage>();
    }

    /// <summary>
    /// 효과받는 타일을 구해서 effectedAreas에 Add
    /// </summary>
    /// <param name="mainTarget">효과범위의 중심이 되는 Actor</param>
    protected void GetArea(ICombatant mainTarget)
    {
        TileLayer tileLayer = GameManager.Instance.GetTileLayer(0);
        TileForMove mainTargetPos = mainTarget.GetCurTileForMove();

        effectedAreas.Clear();

        CentralAxis centralAxis = GetCentralAxis(owner.GetCurTileForMove(), mainTarget.GetCurTileForMove());

        switch (centralAxis)
        {
            case CentralAxis.Y:
                foreach (Coverage item in coverages)
                    effectedAreas.Add(tileLayer.GetTileForMove(mainTargetPos.GetX() + item.x, mainTargetPos.GetY() + item.y));
                break;
            case CentralAxis.InverseY:
                foreach (Coverage item in coverages)
                    effectedAreas.Add(tileLayer.GetTileForMove(mainTargetPos.GetX() - item.x, mainTargetPos.GetY() - item.y));
                break;
            case CentralAxis.X:
                foreach (Coverage item in coverages)
                    effectedAreas.Add(tileLayer.GetTileForMove(mainTargetPos.GetX() + item.y, mainTargetPos.GetY() + item.x));
                break;
            case CentralAxis.InverseX:
                foreach (Coverage item in coverages)
                    effectedAreas.Add(tileLayer.GetTileForMove(mainTargetPos.GetX() - item.y, mainTargetPos.GetY() - item.x));
                break;
        }
    }

    // 효과범위의 회전을 결정하기 위한 메서드.
    protected CentralAxis GetCentralAxis(TileForMove myPos, TileForMove targetPos)
    {
        int xDisplacement = targetPos.GetX() - myPos.GetX();
        int yDisplacement = targetPos.GetY() - myPos.GetY();

        if (Mathf.Abs(yDisplacement) >= Mathf.Abs(xDisplacement))
        {
            if (yDisplacement >= 0)
                return CentralAxis.Y;
            else
                return CentralAxis.InverseY;
        }
        else
        {
            if (xDisplacement >= 0)
                return CentralAxis.X;
            else
                return CentralAxis.InverseX;
        }
    }

    /// <summary>
    /// 효과 범위 내의 ICombatant 중 적을 targets에 저장.
    /// </summary>
    protected void GetTargets()
    {
        List<Actor> recentActors;
        targets.Clear();

        foreach (TileForMove tfm in effectedAreas)
        {
            if (tfm != null)
            {
                recentActors = tfm.GetRecentActors();
                foreach (Actor actor in recentActors)
                {
                    if (actor is ICombatant)
                        targets.Add(actor as ICombatant);
                }
            }
        }
    }

    /// <summary>
    /// 효과 범위 내의 적을 찾아서 targets에 저장해놓음. GetArea()와 GetTargets()의 캡슐화 메서드
    /// </summary>
    /// <param name="mainTarget">효과범위 중심이 되는 대상</param>
    protected void FindEnemies(ICombatant mainTarget)
    {
        GetArea(mainTarget);
        GetTargets();
    }
}


public class HanaUniqueSkill : AoESkill
{
    float totalDmg;
    const float RATE_SINGLE = 0.15f;
    const float RATE_NORM = 0.1f;
    const float RATE_CHARGED = 1.7f;
    const float CHARGED_TRIGGER = 0.12f;
    const float TICK_MULT = 8;
    GameObject normEffects;
    GameObject chargedEffect;

    public HanaUniqueSkill()
    {
        totalDmg = 0;
        SetCoverage();
    }

    public override void InitSkill()
    {
        normEffects = Instantiate((GameObject)Resources.Load("EffectPrefabs/HanaNorm_SkillEffect"));
        normEffects.transform.SetParent(owner.GetTransform());
        //        normEffects.transform.position = new Vector3(0, 0, 0);
        normEffects.transform.position = owner.GetPosition();
        //normEffects.GetComponent<ToggleEffect>().OffEffect();
        chargedEffect = Instantiate((GameObject)Resources.Load("EffectPrefabs/HanaCharged_SkillEffect"));
    }

    public override void BeforeAttack() { }
    public override void OnAttack(float actualDamage, bool isCrit, bool isDodged) { }
    public override void AfterAttack() { }
    public override void OnStruck(float actualDamage, bool isDodged, ICombatant attacker) { }

    public override IEnumerator OnAlways()
    {
        while (true)
        {
            yield return new WaitForSeconds(TICK_MULT * TICK_TIME);
            FindEnemies(owner);
            BattleStat myBattleStat = owner.GetBattleStat();
            normEffects.GetComponent<AttackEffect>().StopEffect();

            if (owner.GetSuperState() == SuperState.Battle)
            {
                float dmg;
                bool isCrit;

                myBattleStat.CalDamage(out dmg, out isCrit);


                if (targets.Count == 0)
                {
                    // 효과범위 내에 아무도 없을 때는 아무거도 안함.
                }
                else if (totalDmg >= myBattleStat.HealthMax * CHARGED_TRIGGER)
                {

                    dmg *= RATE_CHARGED;

                    AdditionalAttack(targets, dmg, myBattleStat.PenetrationFixed, myBattleStat.PenetrationMult, isCrit);
                    totalDmg -= myBattleStat.HealthMax * CHARGED_TRIGGER;

                    DisplayChargedEffect();
                }
                else
                {
                    if (targets.Count == 1)
                        dmg *= RATE_SINGLE;
                    else
                        dmg *= RATE_NORM;

                    totalDmg += AdditionalAttack(targets, dmg, myBattleStat.PenetrationFixed, myBattleStat.PenetrationMult, isCrit);

                    normEffects.GetComponent<AttackEffect>().StartEffect();
                }
            }
        }
    }

    public override void SetCoverage()
    {
        coverages.Add(new Coverage(1, 0));
        coverages.Add(new Coverage(-1, 0));
        coverages.Add(new Coverage(0, 1));
        coverages.Add(new Coverage(0, -1));
        coverages.Add(new Coverage(0, 0));
    }

    public void DisplayChargedEffect()
    {
        chargedEffect.GetComponent<AttackEffect>().StartEffect();
        chargedEffect.transform.position = new Vector3(owner.GetPosition().x, owner.GetPosition().y + 0.2f, owner.GetPosition().z);
    }
}

public class IrisUniqueSkill : AoESkill
{
    private const float RATE_SINGLE = 2.4f;
    private const float RATE_NORM = 2.1f;
    private const float INVOKE_PERIOD = 7;
    private int attackCnt = 0;
    private BattleStat myBattleStat;

    GameObject skillEffect;

    public IrisUniqueSkill()
    {
        attackCnt = 0;
        SetCoverage();
    }

    public override void InitSkill()
    {
        skillEffect = Instantiate((GameObject)Resources.Load("EffectPrefabs/Iris_SkillEffect"));
        skillEffect.transform.SetParent(owner.GetTransform());
        myBattleStat = owner.GetBattleStat();
        //        normEffects.transform.position = new Vector3(0, 0, 0);
    }

    public override void BeforeAttack() { }

    public override void OnAttack(float actualDamage, bool isCrit, bool isDodged)
    {
        attackCnt++;

        if(attackCnt % INVOKE_PERIOD == 0)
        {
            SetEnemy();
            FindEnemies(enemy);
            if (targets.Count == 1)
                AdditionalAttack(targets, actualDamage * RATE_SINGLE, myBattleStat.PenetrationFixed, myBattleStat.PenetrationMult, isCrit);
            else
                AdditionalAttack(targets, actualDamage * RATE_NORM, myBattleStat.PenetrationFixed, myBattleStat.PenetrationMult, isCrit);

            DisplaySkillEffect(skillEffect, enemy, false);
            skillEffect.transform.position = new Vector3(enemy.GetPosition().x, enemy.GetPosition().y * 0.9f + transform.position.y * 0.1f - 0.13f, enemy.GetPosition().z * 0.5f + transform.position.z * 0.5f);
        }
    }

    public override void AfterAttack() { }
    public override void OnStruck(float actualDamage, bool isDodged, ICombatant attacker) { }

    public override IEnumerator OnAlways()
    {
        yield return null;
    }

    public override void SetCoverage()
    {
        coverages.Add(new Coverage(1, 0));
        coverages.Add(new Coverage(-1, 0));
        coverages.Add(new Coverage(0, 1));
        coverages.Add(new Coverage(0, -1));
        coverages.Add(new Coverage(0, 0));
    }
}