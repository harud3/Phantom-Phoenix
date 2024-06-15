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
    public Sprite icon;
    public new string name;
    public int cost;
    public int atk;
    public int hp;
    public CategoryRarity categoryRarity;
    public skill skill1, skill2, skill3;
    public string cardText;
    

    public enum CategoryRarity { unitR, unitSR, unitSSR, itemR, itemSR, itemSSR}
    [NonSerialized]
    public readonly static string[] strCategoryRarity = new string[] { "unitR", "unitSR", "unitSSR", "itemR", "itemSR", "itemSSR"};

    public enum skill { �Ȃ�, ����, ����, �_��, �ђ�, �A��, �O�A��}
    [NonSerialized]
    public readonly static string[] strskill= new string[] { "", "����", "����", "�_��", "�ђ�", "�A��", "�O�A��" };
}
