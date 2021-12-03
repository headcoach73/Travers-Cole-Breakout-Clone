using Mirror;
using UnityEngine;

namespace BreakoutClone
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Breakout Network Settings")]
        [SerializeField]
        private PlayerSettings m_playerOneSettings;
        [SerializeField]
        private PlayerSettings m_playerTwoSettings;


        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            var playerSettings = numPlayers == 0 ? m_playerOneSettings : m_playerTwoSettings;

            var spawnPosition = playerSettings.HeightPosition;
            GameObject player = Instantiate(playerPrefab, new Vector3(0,spawnPosition,0), Quaternion.identity);

            var playerManager = player.GetComponent<PlayerManager>();
            playerManager?.SetPlayerSettings(playerSettings);

            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }
}