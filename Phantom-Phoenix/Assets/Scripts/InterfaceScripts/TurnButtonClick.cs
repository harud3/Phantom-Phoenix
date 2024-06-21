using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// TurnButton‚ğ‰Ÿ‚µ‚½‚Æ‚«‚Ìˆ—
/// </summary>
public class TurnButtonClick : MonoBehaviour
{
    [SerializeField]
    Button button;
    // Start is called before the first frame update
    private void Start()
    {
        button.onClick.AddListener(() => {
            if (GameManager.instance.isPlayerTurn)
            {
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendChangeTurn();
                }
                StartCoroutine(GameManager.instance.ChangeTurn());
                
            }
        });
    }
}
