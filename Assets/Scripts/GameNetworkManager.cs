using Mirror;
using Server;
using Test;
using UnityEngine;

public class RequestChunkMessage : MessageBase
{
    public Vector2Int pos;
}
public class GameNetworkManager : NetworkManager
{
    [SerializeField]private TerrainGenerator terrainGenerator;
    
    private ServerChunkLoader map;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // add player at correct spawn position
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log("New player connected");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        map = new ServerChunkLoader(terrainGenerator);
        map.Start();
        // NetworkServer.RegisterHandler<RequestChunkMessage>(OnRequestChunkMessage);
    }

    
}