using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// TurnButtonを押したときの処理
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
                GameManager.instance.ChangeTurn();
            }
        });
    }
}
