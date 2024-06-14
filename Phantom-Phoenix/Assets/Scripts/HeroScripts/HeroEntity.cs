using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroEntity", menuName = "Create HeroEntity")]
//�J�[�h�f�[�^
public class HeroEntity : ScriptableObject
{
    public new string name;
    public int hp;
    public Sprite icon;
}
