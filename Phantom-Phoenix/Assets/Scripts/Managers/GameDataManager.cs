using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    //AIí‚©Playerí‚©
    public bool isOnlineBattle;
    //•”‰®‚ğ—§‚Ä‚½‘¤‚©”Û‚©
    public bool isMaster;
    public static GameDataManager instance {  get; private set; }
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
}
