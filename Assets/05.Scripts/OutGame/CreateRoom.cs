using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Michsky.UI.Dark;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;



public class CreateRoom : MonoBehaviour
{
    [SerializeField] private TMP_Text serverName;
    [SerializeField] private SliderManager totalPlayerNumSlider;
    [SerializeField] private SwitchManager connectionTypeSwitch; 
    
    

    public void OnClickCreateRoom()
    {
        if(PhotonNetwork.NetworkClientState != ClientState.JoinedLobby &&
            PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            //Debug.LogError("아직 서버에 접속되지 않았습니다.");
            return;
        }

        // 현재 값들을 가지고 사용하면 됩니다.
        string _serverName = serverName.text;
        // 최대 접속 가능 유저수는 slider value입니다.
        int _maxPlayerNum = (int)totalPlayerNumSlider.saveValue;
        // 헷갈릴만한건 connectionType. True면 public입니다.
        bool _connectionType = connectionTypeSwitch.isOn;

        _serverName = _serverName.Trim();
        if (_serverName == "")
            return;
        
        // 이 값들을 이용해서 서버에서 방을 만드는 코드를 여기에 작성하시면, Create Room Button을 누르면 동작합니다.
        // 현재는 동시에 My Room 으로 넘어가도록 되어있습니다. Loading은 추후에 추가하겠습니다.
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = _maxPlayerNum;

        ExitGames.Client.Photon.Hashtable customData = new ExitGames.Client.Photon.Hashtable();
        customData.Add("IsPublic", _connectionType);
        customData.Add("Ping", PhotonNetwork.GetPing());
        customData.Add("Master", PhotonNetwork.LocalPlayer.NickName);
        // Private 방 코드 생성, 등록
        // 방 코드 다른 방들과 겹치지 않게 세팅해야함.
        if (_connectionType)
        {
            customData.Add("AccessCode", 0);
        }
        else
        {
            customData.Add("AccessCode", Random.Range(100000, 999999));
        }

        roomOptions.CustomRoomProperties = customData;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsPublic", "Ping", "AccessCode", "Master" };
        roomOptions.PublishUserId = true;

        //        roomOptions.CustomRoomProperties
        PhotonNetwork.CreateRoom(_serverName, roomOptions);


        // Set Owner로 MyRoomManager에서 Start button을 Active 시킵니다. 다른게 더 필요하면 거기서 설정하면 됩니다.
        FindObjectOfType<MyRoomManager>().OnRoomCreateOrJoin(true);
    }
}
