using Client;
using Map;
using Mirror;
using Server;
using Test;
using Test.Map;
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
        //player.GetComponent<SelectTargetSystem>().worldHolder = terrainGenerator.World;
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log("New player connected");
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
   
        terrainGenerator.InitAwake(true);

    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        terrainGenerator.InitAwake(false);
        map = new ServerChunkLoader(terrainGenerator);
        map.Start();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        map.Stop();
    }
}