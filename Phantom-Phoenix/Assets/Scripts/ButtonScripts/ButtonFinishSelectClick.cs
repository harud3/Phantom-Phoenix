using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// バトル画面　ButtonFinishSelectを押したときの処理
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
