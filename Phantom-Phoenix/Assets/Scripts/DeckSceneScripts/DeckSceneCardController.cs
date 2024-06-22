using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// �f�b�L�V�[����CC�͂�邱�Ƃ����Ȃ�
/// </summary>
public class DeckSceneCardController : Controller
{
    DeckSceneCardView view;
    public DeckSceneCardModel model {  get; private set; }
    public DeckSceneCardMovement movement {  get; private set; }
    private void Awake()
    {
        view = GetComponent<DeckSceneCardView>();
        movement = GetComponent<DeckSceneCardMovement>();
    }

    public void Init(int CardID, bool isPlayer = true)
    {
        model = new DeckSceneCardModel(CardID, isPlayer);
        view.SetCard(model);
    }
}
