using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyScene : MonoBehaviourPunCallbacks
{

    public InputField RoomNameInputField;


    public GameObject MatchmakerSubPanel;

    public string ErrorDialog
    {
        get
        {
            return this.errorDialog;
        }
        private set
        {
            this.errorDialog = value;
            if (!string.IsNullOrEmpty(value))
            {
                print(errorDialog);
            }
        }
    }

    public Transform RoomCellPool;

    private Transform RoomsListPanel;
    private Transform RoomInfoPanel;

    private string errorDialog;
    private Text roomsIndicator;
    private Transform RoomsList;

    public void Awake()
    {

        RoomsListPanel = transform.Find("MainPanel/RoomsListPanel");
        RoomInfoPanel = transform.Find("MainPanel/RoomInfoPanel");
        roomsIndicator = transform.Find("MainPanel/RoomsListPanel/RoomsIndicator").GetComponent<Text>();
        RoomsList = transform.Find("MainPanel/RoomsListPanel/RoomsList");
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings();
        }
        if (!PhotonNetwork.InRoom)
        {
            RoomInfoPanel.gameObject.SetActive(false);

        }

        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(false);


    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomcount = roomList.Count;
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                roomcount--;
            }
        }

        RoomsListPanel.gameObject.SetActive(true);
        RoomInfoPanel.gameObject.SetActive(false);
        if (roomcount == 0)
        {
            roomsIndicator.text = "Currently no games are available.";

            for (int i = 0; i < RoomsList.childCount; i++)
            {
                RoomsList.GetChild(i).gameObject.SetActive(false);
                RoomsList.GetChild(i).SetParent(RoomCellPool);

            }
        }
        else
        {
            roomsIndicator.text = roomcount + " rooms available:";

            while (roomcount > RoomsList.childCount)
            {
                RoomCellPool.GetChild(0).gameObject.SetActive(true);
                RoomCellPool.GetChild(0).SetParent(RoomsList);
            }
            while (roomcount < RoomsList.childCount)
            {
                RoomsList.GetChild(0).gameObject.SetActive(false);
                RoomsList.GetChild(0).SetParent(RoomCellPool);
            }

            int i = 0;

            foreach (RoomInfo roomInfo in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
                {
                    continue;
                }

                RoomsList.GetChild(i).GetComponent<RoomCellInfo>().RoomName.text = roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;

                RoomsList.GetChild(i).GetComponent<RoomCellInfo>().JoinButton.onClick.RemoveAllListeners();
                RoomsList.GetChild(i).GetComponent<RoomCellInfo>().JoinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomInfo.Name));

                i++;
            }
        }
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {

            string a = "";
            a += "There is/are " + PhotonNetwork.CurrentRoom.PlayerCount + " Player(s) in the room.";
            a += "\n";
            foreach (var item in PhotonNetwork.PlayerList)
            {
                a += "\n";

                a += item.NickName;

                if (item.IsMasterClient)
                {
                    a += " (Master Client)";
                }

                if (item.IsLocal)
                {
                    a += " (Local User)";
                }
                a += "\n";
            }
            RoomInfoPanel.Find("RoomInfo").GetComponent<Text>().text = a;

            RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.PlayerList.Length > 1 && PhotonNetwork.IsMasterClient);
        }
    }

    public void OnClickRoomCreatedButton()
    {
        PhotonNetwork.CreateRoom(RoomNameInputField.text, new RoomOptions() { MaxPlayers = 4 }, null);
    }
    public void OnClickBackButton()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void OnClickStartButton()
    {
        PhotonNetwork.LoadLevel("_Complete-Game");
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.isMasterClient);
    }
    public override void OnJoinedRoom()
    {
        RoomsListPanel.gameObject.SetActive(false);
        RoomInfoPanel.gameObject.SetActive(true);

        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.isMasterClient);

        Debug.Log("OnJoinedRoom");
    }
    public override void OnLeftRoom()
    {
        RoomsListPanel.gameObject.SetActive(true);
        RoomInfoPanel.gameObject.SetActive(false);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name. Code: " + returnCode + " Message: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ErrorDialog = "Error: Can't join room (full or unknown room name). Code: " + returnCode;
        Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed. Code: " + returnCode + " Message: " + message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnJoinRandomFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms. Code: " + returnCode + " Message: " + message);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon. Cause: " + cause);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
        PhotonNetwork.JoinLobby();
    }
}
