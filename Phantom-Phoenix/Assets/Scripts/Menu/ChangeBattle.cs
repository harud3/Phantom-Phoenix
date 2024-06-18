using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeBattle : MonoBehaviour
{
    public void ChangeVSAIScene()
    {
        GameDataManager.instance.isOnlineBattle = false;
        SceneManager.LoadScene("BattleScene");
    }
}
