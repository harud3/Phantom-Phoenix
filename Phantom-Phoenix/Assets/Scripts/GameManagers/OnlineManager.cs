using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineManager : MonoBehaviourPunCallbacks
{
    bool inRoom = false;
    bool isMatching = false;
    public void Start()
    {
        if (!PlayerPrefs.HasKey("PlayerDeckData"))
        {
            Debug.Log("PlayerDeckDataが存在しません");
            return;
        }
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
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2}, TypedLobby.Default);
        GameDataManager.instance.isMaster = true;
    }

    //部屋が2人ならシーンを変える
    private void Update()
    {
        if(!isMatching && inRoom && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            isMatching = true;
            SceneManager.LoadScene("BattleScene");
        }
    }
}
