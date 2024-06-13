using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnButtonClick : MonoBehaviour
{
    [SerializeField]
    Button button;
    [SerializeField]
    GameManager gameManager;
    // Start is called before the first frame update
    private void Start()
    {
        button.onClick.AddListener(() => { gameManager.ChangeTurn(); });
    }
}
