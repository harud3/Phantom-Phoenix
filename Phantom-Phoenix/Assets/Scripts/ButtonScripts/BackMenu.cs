using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// ƒƒjƒ…[‚É–ß‚é
/// </summary>
public class BackMenu : MonoBehaviour
{
    [SerializeField]
    Button buttonBack;
    public void Start()
    {
        buttonBack.onClick.AddListener(() =>
        {
            Invoke("ChangeMenuScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
    }
    private void ChangeMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
