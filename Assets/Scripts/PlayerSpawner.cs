using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic player spawn based on the main shared mode sample.
/// </summary>
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public GameObject PlayerPrefab;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"[0] {player} {Runner.LocalPlayer}");
        if (player == Runner.LocalPlayer)
        {
            Debug.Log("[1]");
            NetworkObject resultingPlayer = Runner.Spawn(_playerPrefab, new Vector3(0, 1, 0), Quaternion.identity);

            FusionConnector connector = GameObject.FindObjectOfType<FusionConnector>();
            if (connector != null)
            {
                var testPlayer = resultingPlayer.GetComponent<HostPlayer>();

                string playerName = connector.LocalPlayerName;

                if (string.IsNullOrEmpty(playerName))
                    testPlayer.PlayerName = "Player " + resultingPlayer.StateAuthority.PlayerId;
                else
                    testPlayer.PlayerName = playerName;
            }
            _spawnedCharacters.Add(player, resultingPlayer);
            Debug.Log("[2]");
        }
        FusionConnector.Instance?.OnPlayerJoin(Runner);
        Debug.Log("[3]");
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (HostPlayer.LocalPlayer != null)
            HostPlayer.LocalPlayer.IsMasterClient = Runner.IsSharedModeMasterClient;
    }
}
