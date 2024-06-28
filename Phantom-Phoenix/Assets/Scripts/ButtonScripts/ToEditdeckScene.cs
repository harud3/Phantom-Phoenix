using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToEditdeckScene : MonoBehaviour
{
    [SerializeField]
    Button button;
    public void Awake()
    {
        button.onClick.AddListener(() =>
        {
            Invoke("ChangeDeckScene", 0.5f);
            AudioManager.instance.SoundButtonClick1();
        });
    }
    public void ChangeDeckScene()
    {
        SceneManager.LoadScene("DeckScene");
    }
}
