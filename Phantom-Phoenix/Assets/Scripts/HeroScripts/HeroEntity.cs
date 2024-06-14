using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroEntity", menuName = "Create HeroEntity")]
//カードデータ
public class HeroEntity : ScriptableObject
{
    public new string name;
    public int hp;
    public Sprite icon;
}
