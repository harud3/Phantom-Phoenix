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
            Debug.Log("PlayerDeckData�����݂��܂���");
            return;
        }
        GameDataManager.instance.isOnlineBattle = true;
        //PhotonServerSettings�̐ݒ���e���g���ă}�X�^�T�[�o�֐ڑ�
        PhotonNetwork.ConnectUsingSettings();
    }
    //�}�X�^�T�[�o�ւ̐ڑ�������������Ă΂��R�[���o�b�N
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
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2}, TypedLobby.Default); //TODO:�l������
        GameDataManager.instance.isMaster = true;
        SceneManager.LoadScene("OnlineScene");
        AudioManager.instance.SoundButtonClick();
    }

    //������2�l�Ȃ�V�[����ς���
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
