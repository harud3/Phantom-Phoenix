using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �q�[���[�f�[�^
/// </summary>
[CreateAssetMenu(fileName = "HeroEntity", menuName = "Create HeroEntity")]
public class HeroEntity : ScriptableObject
{
    //����قǐ����Ȃ��̂�ScriptableObject�ł�����
    public Sprite character;
    public new string name;
    public int hp;
}
