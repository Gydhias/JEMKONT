using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Linq;

namespace DownBelow.Managers
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public UIMenuLobby UILobby;

        public static NetworkManager Instance;

        private void Awake()
        {
            if (NetworkManager.Instance != null)
            {
                if (NetworkManager.Instance != this)
                {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject, false);
#else
                Destroy(this.gameObject);
#endif
                    return;
                }
            }

            NetworkManager.Instance = this;

            Object.DontDestroyOnLoad(this);
        }

        void Start()
        {
            _connect();
        }

        public void UpdateOwnerName(string newName)
        {
            PhotonNetwork.NickName = newName;
        }

        private void _connect()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        #region UI_calls
        public void ClickOnStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.LoadLevel("Killian");
            }
        }

        public void ClickOnLeave()
        {
            PhotonNetwork.LeaveRoom();
        }


        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void CreateRoom()
        {
            if (!string.IsNullOrEmpty(this.UILobby.RoomInput.text))
            {
                PhotonNetwork.CreateRoom(this.UILobby.RoomInput.text, new RoomOptions() { MaxPlayers = 4, BroadcastPropsChangeToAll = true, PublishUserId = true }, null);
            }
        }

        #endregion


        #region Players_Callbacks
        public void EntityAsksForPath(string entityUID, Cell target, string mainGrid, string innerGrid)
        {
            int[] position = new int[2] { target.PositionInGrid.longitude, target.PositionInGrid.latitude };
            // Temporary, definitely need to create a method and list of all entities handled with UID
            EnemyEntity entity = innerGrid != string.Empty ?
                GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids[innerGrid].GridEntities.First(e => e.UID == entityUID) as EnemyEntity :
                GridManager.Instance.WorldGrids[mainGrid].GridEntities.First(e => e.UID == entityUID) as EnemyEntity;

            GridManager.Instance.FindPath(entity, target.PositionInGrid);

            int[] positions = GridManager.Instance.SerializePathData();

            this.photonView.RPC("RPC_RespondWithEntityProcessedPath", RpcTarget.All, entityUID, position, mainGrid, innerGrid);
        }

        [PunRPC]
        public void RPC_RespondWithEntityProcessedPath(object[] pathDatas)
        {
            EnemyEntity entity = pathDatas[3].ToString() != string.Empty ?
                GridManager.Instance.WorldGrids[pathDatas[2].ToString()].InnerCombatGrids[pathDatas[3].ToString()].GridEntities.First(e => e.UID == pathDatas[0].ToString()) as EnemyEntity :
                GridManager.Instance.WorldGrids[pathDatas[2].ToString()].GridEntities.First(e => e.UID == pathDatas[0].ToString()) as EnemyEntity;

            // We manage the fact that 2 players won't obv be on the same grid, so we send the player
            entity.MoveWithPath(GridManager.Instance.DeserializePathData(entity, (int[])pathDatas[1]), string.Empty);
        }

        public void PlayerAsksForPath(PlayerBehavior player, GridSystem.Cell target, string otherGrid)
        {
            GridManager.Instance.FindPath(GameManager.Instance.Players[player.PlayerID], target.PositionInGrid);

            int[] positions = GridManager.Instance.SerializePathData();

            this.photonView.RPC("RPC_RespondWithProcessedPath", RpcTarget.All, player.PlayerID, positions, otherGrid);
        }

        [PunRPC]
        public void RPC_RespondWithProcessedPath(object[] pathDatas)
        {
            PlayerBehavior movingPlayer = GameManager.Instance.Players[pathDatas[0].ToString()];
            // We manage the fact that 2 players won't obv be on the same grid, so we send the player
            movingPlayer.MoveWithPath(GridManager.Instance.DeserializePathData(movingPlayer, (int[])pathDatas[1]), pathDatas[2].ToString());
        }

        public void PlayerAsksToEnterGrid(PlayerBehavior player, WorldGrid mainGrid, string targetGrid)
        {
            this.photonView.RPC("RPC_RespondToEnterGrid", RpcTarget.All, player.PlayerID, mainGrid.UName, targetGrid);
        }

        [PunRPC]
        public void RPC_RespondToEnterGrid(string playerID, string mainGrid, string targetGrid)
        {
            GameManager.Instance.FireEntityExitingGrid(playerID);

            if (!GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids.ContainsKey(targetGrid))
                Debug.LogError("Couldn't find mainGrid's inner grid called : " + targetGrid + ". Count of innerGrids is : " + GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids.Count);

            GameManager.Instance.Players[playerID].EnterNewGrid(GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids[targetGrid] as CombatGrid);

            GameManager.Instance.FireEntityEnteredGrid(playerID);
        }

        // /!\ Only one combat can be active at the moment, that is important
        public void PlayerAsksToStartCombat()
        {
            this.photonView.RPC("RPC_RespondToStartCombat", RpcTarget.All, GameManager.Instance.SelfPlayer.PlayerID);
        }

        [PunRPC]
        public void RPC_RespondToStartCombat(string playerID)
        {
            CombatManager.Instance.StartCombat(GameManager.Instance.Players[playerID].CurrentGrid as CombatGrid);
        }

        public void PlayerAsksToInteract(Cell interaction)
        {
            this.photonView.RPC("RPC_RespondToInteract", RpcTarget.All, GameManager.Instance.SelfPlayer.PlayerID, interaction.PositionInGrid.latitude, interaction.PositionInGrid.longitude);
        }

        [PunRPC]
        public void RPC_RespondToInteract(string playerID, int latitude, int longitude)
        {
            PlayerBehavior player = GameManager.Instance.Players[playerID];
            player.Interact(player.CurrentGrid.Cells[latitude, longitude]);
        }

        public void PlayerCanceledInteract(Cell interaction)
        {
            this.photonView.RPC("RPC_RespondCancelInteract", RpcTarget.All, GameManager.Instance.SelfPlayer.PlayerID, interaction.PositionInGrid.latitude, interaction.PositionInGrid.longitude);
        }

        [PunRPC]
        public void RPC_RespondCancelInteract(string playerID, int latitude, int longitude)
        {
            PlayerBehavior player = GameManager.Instance.Players[playerID];
            player.Interact(player.CurrentGrid.Cells[latitude, longitude]);
        }
        #endregion

        #region Photon_UI_callbacks

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            this.UILobby?.UpdateRoomList(roomList);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            this.UILobby?.UpdatePlayersFromProperties(targetPlayer);
        }

        public override void OnJoinedRoom()
        {
            if (this.UILobby != null)
                this.UILobby?.OnJoinedRoom();
            else
                GameManager.Instance.WelcomePlayers();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            this.UILobby?.UpdatePlayersList();
        }

        public override void OnLeftRoom()
        {
            this.UILobby?.OnSelfLeftRoom();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            this.UILobby?.OnPlayerLeftRoom();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master serv");
            if (this.UILobby != null)
                PhotonNetwork.JoinLobby();
            else
                GameManager.Instance.WelcomePlayerLately();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected");
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("Joined Lobby !");
        }

        #endregion
    }

}
