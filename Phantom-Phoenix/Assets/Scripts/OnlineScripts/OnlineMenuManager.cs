using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    bool inRoom;
    bool isMatching = false;
    [SerializeField]
    AudioClip audioClip;
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
        Invoke("ChangeOnlineScene", 0.5f);
        this.GetComponent<AudioSource>().PlayOneShot(audioClip);
    }

    //������2�l�Ȃ�V�[����ς���
    private void Update()
    {
        if(!isMatching && inRoom && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            isMatching = true;
            Invoke("ChangeBattleScene", 0.5f);
            this.GetComponent<AudioSource>().PlayOneShot(audioClip);
        }
    }
    private void ChangeBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
    }
    private void ChangeOnlineScene()
    {
        SceneManager.LoadScene("OnlineScene");
    }
}
