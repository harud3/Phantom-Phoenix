using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ƒoƒgƒ‹‰æ–Ê@ButtonTurn‚ğ‰Ÿ‚µ‚½‚Æ‚«‚Ìˆ—
/// </summary>
public class ButtonFinishSelectClick : MonoBehaviour
{
    [SerializeField]
    Button button;
    // Start is called before the first frame update
    private void Start()
    {
        button.onClick.AddListener(() => {
            AudioManager.instance.SoundButtonClick3();
            GameManager.instance.FinishMulligan();
        });
    }
}
