using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    bool inRoom;
    bool isMatching = false;
    public void OnMatchingButton()
    {
        if (!PlayerPrefs.HasKey("PlayerDeckData"))
        {
            Debug.Log("PlayerDeckDataが存在しません");
            return;
        }
        GameDataManager.instance.isOnlineBattle = true;
        //PhotonServerSettingsの設定内容を使ってマスタサーバへ接続
        PhotonNetwork.ConnectUsingSettings();
    }
    //マスタサーバへの接続が成功したら呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        GameDataManager.instance.isMaster = false;
    }
    public override void OnJoinedRoom()
    {
        inRoom = true;
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2}, TypedLobby.Default); //TODO:人数直す
        GameDataManager.instance.isMaster = true;
        SceneManager.LoadScene("OnlineScene");
        AudioManager.instance.SoundButtonClick();
    }

    //部屋が2人ならシーンを変える
    private void Update()
    {
        if(!isMatching && inRoom && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            isMatching = true;
            SceneManager.LoadScene("BattleScene");
            AudioManager.instance.SoundButtonClick();
        }
    }
}
