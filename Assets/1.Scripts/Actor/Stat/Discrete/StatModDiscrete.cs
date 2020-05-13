﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModDiscrete
{
    public StatModDiscrete(StatType statType, ModType modType, int value)
    {
        ModType = modType;
        ModValue = value;
        StatType = statType;
    }

    public StatModDiscrete(StatModDiscrete statMod)
    {
        StatType = statMod.StatType;
        ModType = statMod.ModType;
        ModValue = statMod.ModValue;
    }

    public int ModValue
	{
		get
		{
			return _modValue;
		}
		set
		{

		}
	}
	public ModType ModType
	{
		get
		{
			return _modType;
		}
		set
		{

		}
	}
    public StatType StatType { get; set; }

    ModType _modType = ModType.Fixed;
	private int _modValue = 0;
}
