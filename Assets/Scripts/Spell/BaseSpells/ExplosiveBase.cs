﻿using System;
using UnityEngine;

public class ExplosiveBase : SpellBase
{
    // todo: scale with dmg and scale
    public float radius = 5.0F;
    public float power = 100.0F;

    protected override void SetValues()
    {

        radius = 3.0F;
        power = 100.0F;
        _offset = Vector3.up  + _player.forward * 1.3f;
    }

    /// <summary>
    /// todo: use the following properties:
    /// _direction: yes
    /// _objectForSpell: yes
    /// _speed: yes
    /// _damage: yes
    /// _offset: yes
    /// _objectsCollided: 
    /// _trigger: yes
    /// </summary>
    public override void SpellBehaviour(Spell spell)
    {
        var p = Instantiate(_objectForSpell, _player.position + _offset,
            Quaternion.Euler(_direction));
        
        Explosive explosive = p.GetComponent<Explosive>();

        radius *= _scale;

        explosive.radius = radius;
        explosive._damage = _damage;
        explosive.power = power * _damage / properties._damage + power * radius;
        explosive.Launch(_direction * 2 + _offset, _speed);
        _objectForSpell = p;
    }
    
    public override Tooltip GetTooltip()
    {
        return new Tooltip($"Bomb {DefaultBaseTitle()}", $"Creates an explosive that detonates on contact, dealing {_damage} to entities in a radius of {radius}. {DefaultBaseBody()}");
    }
}