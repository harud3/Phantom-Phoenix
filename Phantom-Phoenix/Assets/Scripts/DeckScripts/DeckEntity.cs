using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーデータ
/// </summary>
[CreateAssetMenu(fileName = "DeckEntity", menuName = "Create DeckEntity")]
public class DeckEntity : ScriptableObject
{
    public int useHeroID = 1;
    public List<int> deck = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6};
}
