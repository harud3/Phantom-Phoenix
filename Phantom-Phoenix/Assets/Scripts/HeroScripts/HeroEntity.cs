using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ヒーローデータ
/// </summary>
[CreateAssetMenu(fileName = "HeroEntity", menuName = "Create HeroEntity")]
public class HeroEntity : ScriptableObject
{
    public new string name;
    public int hp;
    public Sprite icon;
}
