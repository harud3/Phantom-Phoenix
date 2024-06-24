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
/// ÉfÉbÉLï“ê¨âÊñ ÇÃCCÇÕÇ‚ÇÈÇ±Ç∆Ç™è≠Ç»Ç¢
/// </summary>
public class EditDeckCardController : Controller
{
    EditDeckCardView view;
    public EditDeckCardModel model {  get; private set; }
    public EditDeckCardMovement movement {  get; private set; }
    private void Awake()
    {
        view = GetComponent<EditDeckCardView>();
        movement = GetComponent<EditDeckCardMovement>();
    }

    public void Init(int CardID, bool isPlayer = true)
    {
        model = new EditDeckCardModel(CardID, isPlayer);
        view.SetCard(model);
    }
}
