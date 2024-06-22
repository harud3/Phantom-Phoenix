using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �J�[�h�f�[�^
/// </summary>
[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]
public class CardEntity : ScriptableObject
{
    public Sprite charcter;
    public new string name;
    public int cost;
    public int atk;
    public int hp;
    public Category category;
    public Rarity rarity;
    public Skill skill1, skill2, skill3;
    public string textCard;
    public Target target;

    public enum Category { unit, spell, item }
    public enum Rarity { R, SR, SSR}

    public enum Skill { none, fast, taunt, snipe, pierce, doubleAction} //�Ȃ��A�����A�����A�_���A�ђʁA�A���@�����͌��ʎ����J�[�h�������Ȃ肪���Ȍ��ʌQ�Ȃ̂œ��ʘg
    /// <summary>
    /// ���ʔ͈́@���j�b�g�̏������I�����ʂ������ŊǗ�����A�X�y���̌��ʔ͈�
    /// </summary>
    public enum Target { 
        none, //�ΏۂȂ�
        unit, //���j�b�g�P��
        enemyUnit, //�G���j�b�g�P��
        playerUnit, //�������j�b�g�P��
        hero, //�q�[���[�P��
        unitOrHero, //�P��
        enemy, //�G�P��
        player, //�����P��

        area, //�͈�
        //enemyUnits, //�G���j�b�g�S��
        //playerUnits, //�������j�b�g�S��
        //enemys, //�G�S��
        //players, //�����S��
        //units, //���j�b�g�S��
        //enemyhero, //�G�q�[���[�P�́@1�̂������݂����I�ԕK�v���Ȃ�����
        //playerhero, //�����q�[���[�P�� 1�̂������݂����I�ԕK�v���Ȃ�����
        //heros, //���q�[���[
        //all, //�G�����S��

        selectionArea, //�I��͈�
        selectionPlayerArea, //�G�I��͈�
        selectionEnemyArea, //�����I��͈�
        //UnitsV, //���j�b�g�c���
        //UnitsH, //���j�b�g�����
        //enemyUnitsV, //�G���j�b�g�c��� 
        //enemyUnitsH, //�G���j�b�g�����
        //playerUnitsV, //�������j�b�g�c���
        //playerUnitsH, //�������j�b�g�����
    }
}
