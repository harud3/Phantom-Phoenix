using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMà»äOÇÃå¯â âπÇìùäáÇ∑ÇÈ
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip soundCardMove;
    [SerializeField]
    private AudioClip soundCardAttack;
    [SerializeField]
    private AudioClip soundButtonClick1;
    [SerializeField]
    private AudioClip soundButtonClick2;
    [SerializeField]
    private AudioClip soundButtonClick3;
    [SerializeField]
    private AudioClip soundWin;
    [SerializeField]
    private AudioClip soundLose;
    [SerializeField]
    private AudioClip soundCardFire;
    [SerializeField]
    private AudioClip soundMPHeal;
    [SerializeField]
    private AudioClip soundMPBuff;
    [SerializeField]
    private AudioClip soundMPDeBuff;
    [SerializeField]
    private AudioClip soundcCardHeal;
    [SerializeField]
    private AudioClip soundcCardBuff;
    [SerializeField]
    private AudioClip soundcCardDeBuff;
    [SerializeField]
    private AudioClip soundcCardSeal;
    [SerializeField]
    private AudioClip soundTensionUp;
    [SerializeField]
    private AudioClip soundTensionDown;

    private AudioSource audioSource;

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
    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }
    public void SoundCardMove()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundCardMove);
    }
    public void SoundCardAttack()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundCardAttack);
    }
    public void SoundButtonClick1()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundButtonClick1);
    }
    public void SoundButtonClick2()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundButtonClick2);
    }
    public void SoundButtonClick3()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundButtonClick3);
    }
    public void SoundWin()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundWin);
    }
    public void SoundLose()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundLose);
    }
    public void SoundCardFire()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundCardFire);
    }
    public void SoundMPHeal()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundMPHeal);
    }
    public void SoundMPBuff()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundMPBuff);
    }
    public void SoundMPDeBuff()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundMPDeBuff);
    }
    public void SoundCardHeal()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundcCardHeal);
    }
    public void SoundcCardBuff()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundcCardBuff);
    }
    public void SoundcCardDeBuff()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundcCardDeBuff);
    }
    public void SoundcCardSeal()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundcCardSeal);
    }
    public void SoundcTensionUp()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundTensionUp);
    }
    public void SoundcTensionDown()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundTensionDown);
    }
}
