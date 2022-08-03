using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
public class Launcher : MonoBehaviourPunCallbacks
{
    
    public static Launcher instance;

    private void Awake(){
        instance = this; 
    }
    
    
    public GameObject PenisMfs;
    public GameObject MenuButtons;
    public TMP_Text LoadingText;
    
    public GameObject RoomScreen;
    public TMP_Text RoomNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;

    public GameObject ErrorScreen;
    public TMP_Text ErrorText;

    public GameObject RoomBrowserScreen;
    public RoomButton theRoomButton;
    public List<RoomButton> allRoomButtons = new List<RoomButton>();


    public GameObject nameInputScreen;
    public TMP_InputField nameInput;

    private bool hasSetNick;

    public string LevelToPlay;


    public GameObject StartButton;
    void Start()
    {
        closeMenus();
        LoadingText.text = "connecting to da server";  


        PhotonNetwork.ConnectUsingSettings();
    }


    void closeMenus(){
        
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);

    }
    public override void OnConnectedToMaster(){
        PenisMfs.SetActive(true);
        closeMenus();
        MenuButtons.SetActive(true);
        LoadingText.text = "connected to server, but at what cost?";

        PhotonNetwork.JoinLobby();
        LoadingText.text = "joining lobby for your gay ass";
   
   
        PhotonNetwork.AutomaticallySyncScene = true;
   
   
    }

    public override void OnJoinedLobby(){
        closeMenus();
        MenuButtons.SetActive(true);
        LoadingText.text = "why do we keep doing this shit";
    
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!hasSetNick){
            closeMenus();
            nameInputScreen.SetActive(true);

            if(PlayerPrefs.HasKey("playerName")){
                nameInput.text = PlayerPrefs.GetString("playerName");
            }

        }
    
        else{
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }

    }
    
    public void openRoomCreate(){
        closeMenus();
        CreateRoomScreen.SetActive(true);

    }
    public void closeRoomCreate(){
        closeMenus();
        CreateRoomScreen.SetActive(false);
        MenuButtons.SetActive(true);
    }
    public void CreateRoom(){
        if(!string.IsNullOrEmpty(RoomNameInput.text)){
            
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 10; 
            PhotonNetwork.CreateRoom(RoomNameInput.text, options);
            closeMenus();
            LoadingText.text = "creating da room 4 u";
        }
    }
    public override void OnJoinedRoom(){
        closeMenus();
        RoomScreen.SetActive(true);
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if(PhotonNetwork.IsMasterClient){
            StartButton.SetActive(true);
        }else{
            StartButton.SetActive(false);
        }
    }


       private void ListAllPlayers()
    {
        foreach(TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();
        // playerNameLabel.SetActive(false);
        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i <players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){

        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);

    }
    public override void OnPlayerLeftRoom(Player otherPlayer){

        ListAllPlayers();

    }


    public override void OnCreateRoomFailed(short returnCode, string message){
        ErrorText.text = "creating room, failed(nothing to do with u being gay): " + message;
        closeMenus();
        ErrorScreen.SetActive(true);
    
    }
    public void CloseRoomError(){
        ErrorScreen.SetActive(false);
        MenuButtons.SetActive(true);
    }
    public void LeaveRoom(){
        PhotonNetwork.LeaveRoom();
        closeMenus();
        LoadingText.text = "leaving.............";
        
        
    }
    public override void OnLeftRoom(){
        closeMenus();
        MenuButtons.SetActive(true);
    }

    public void OpenRoomBrowser(){
        closeMenus();
        RoomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser(){
        closeMenus();
        MenuButtons.SetActive(true);

    }


public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }


    public void JoinRoom(RoomInfo inputInfo){
     
        PhotonNetwork.JoinRoom(inputInfo.Name);
        closeMenus();
        LoadingText.text = "joining room......";

    }

    public void SetNickname(){

        if(!string.IsNullOrEmpty(nameInput.text)){
            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString("playerName", nameInput.text);

            closeMenus();
            MenuButtons.SetActive(true);
            hasSetNick = true;
        }
 
    }
    
    public void StartGame(){

        PhotonNetwork.LoadLevel(LevelToPlay);

    }

    public override void OnMasterClientSwitched(Player newMasterClient){

        if(PhotonNetwork.IsMasterClient){    
            StartButton.SetActive(true);
        
        }else{
            
            StartButton.SetActive(false);
        }

    }
    
    
    public void ExitGame(){
        Application.Quit();
    }
   
}
