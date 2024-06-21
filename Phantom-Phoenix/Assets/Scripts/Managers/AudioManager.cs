using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip soundCardMove;
    [SerializeField]
    private AudioClip soundCardAttack;
    [SerializeField]
    private AudioClip soundButtonClick;
    [SerializeField]
    private AudioClip soundWin;
    [SerializeField]
    private AudioClip soundLose;
    [SerializeField]
    private AudioClip soundButtonClick2;
    [SerializeField]
    private AudioClip soundCardFire;
    [SerializeField]
    private AudioClip soundCardDeBuff;
    [SerializeField]
    private AudioClip soundcCardHeal;

    public static AudioManager instance {  get; private set; }
    private void Awake()
    {
            if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SoundCardMove()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundCardMove);
    }
    public void SoundCardAttack()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundCardAttack);
    }
    public void SoundButtonClick()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundButtonClick);
    }
    public void SoundWin()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundWin);
    }
    public void SoundLose()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundLose);
    }
    public void SoundButtonClick2()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundButtonClick2);
    }
    public void SoundCardFire()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundCardFire);
    }
    public void SoundCardDeBuff()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundCardDeBuff);
    }
    public void SoundCardHeal()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundcCardHeal);
    }
}
