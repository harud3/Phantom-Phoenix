using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")] 
//�J�[�h�f�[�^
public class CardEntity : ScriptableObject
{
    public new string name;
    public int hp;
    public int atk;
    public int cost;
    public Sprite icon;
}
