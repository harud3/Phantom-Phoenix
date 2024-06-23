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
    private AudioClip soundCardDeBuff;
    [SerializeField]
    private AudioClip soundcCardHeal;

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
    public void SoundCardDeBuff()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundCardDeBuff);
    }
    public void SoundCardHeal()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(soundcCardHeal);
    }
}
