using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ヒーローデータ
/// </summary>
[CreateAssetMenu(fileName = "HeroEntity", menuName = "Create HeroEntity")]
public class HeroEntity : ScriptableObject
{
    public Sprite character;
    public new string name;
    public int hp;
}
