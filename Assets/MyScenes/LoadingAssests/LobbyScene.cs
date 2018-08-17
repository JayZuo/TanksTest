using ExitGames.UtilityScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class LobbyScene : MonoBehaviour
{

    public InputField RoomNameInputField;


    public GameObject MatchmakerSubPanel;

    public string ErrorDialog
    {
        get { return this.errorDialog; }
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

    private bool connectFailed = false;

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
        PhotonNetwork.automaticallySyncScene = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.9");


        }
        if (!PhotonNetwork.inRoom)
        {
            RoomInfoPanel.gameObject.SetActive(false);

        }

        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(false);


    }

    private void Update()
    {
        if (PhotonNetwork.inRoom)
        {

            string a = "";
            a += "There is/are " + PhotonNetwork.room.PlayerCount + " Player(s) in the room.";
            a += "\n";
            foreach (var item in PhotonNetwork.playerList)
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

            RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.playerList.Length > 1 && PhotonNetwork.isMasterClient);

        }
        else
        {
            RoomsListPanel.gameObject.SetActive(true);
            RoomInfoPanel.gameObject.SetActive(false);
            if (PhotonNetwork.GetRoomList().Length == 0)
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

                roomsIndicator.text = PhotonNetwork.GetRoomList().Length + " rooms available:";

                int roomcount = PhotonNetwork.GetRoomList().Length;
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
                //// Room listing: simply call GetRoomList: no need to fetch/poll whatever!

                for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
                {
                    RoomInfo roomInfo = PhotonNetwork.GetRoomList()[i];
                    RoomsList.GetChild(i).GetComponent<RoomCellInfo>().RoomName.text = roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;

                    RoomsList.GetChild(i).GetComponent<RoomCellInfo>().JoinButton.onClick.RemoveAllListeners();
                    RoomsList.GetChild(i).GetComponent<RoomCellInfo>().JoinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomInfo.Name));
                }



            }
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
    public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.isMasterClient);
    }
    public void OnJoinedRoom()
    {
        RoomsListPanel.gameObject.SetActive(false);
        RoomInfoPanel.gameObject.SetActive(true);

        //RoomInfoPanel.Find("StartButton").gameObject.SetActive(PhotonNetwork.isMasterClient);



        Debug.Log("OnJoinedRoom");
    }
    public void OnLeftRoom()
    {
        RoomsListPanel.gameObject.SetActive(true);
        RoomInfoPanel.gameObject.SetActive(false);
    }
    public void OnPhotonCreateRoomFailed()
    {
        ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        // PhotonNetwork.LoadLevel(SceneNameGame);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
        PhotonNetwork.JoinLobby();
    }
}
