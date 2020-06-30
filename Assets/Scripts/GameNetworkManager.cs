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
    [SerializeField] public Material TerrainMaterial;

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
        terrainGenerator.InitAwake();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
   
        BlockSpecification[] blocks = Resources.LoadAll<BlockSpecification>("Blocks");
        Texture2DArray texture2DArray = new Texture2DArray(128, 128, blocks.Length, TextureFormat.RGB24, true);
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].blockId = (uint) (i);
            if (blocks[i].ViewType == ViewType.Block)
            {
                texture2DArray.SetPixels(blocks[i].Texture.GetPixels(0), i);
            }
            BlockId.Blocks[i] = new BlockId(i);
        }

        texture2DArray.Apply();
        TerrainMaterial.SetTexture("_TextureArray", texture2DArray);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        map = new ServerChunkLoader(terrainGenerator);
        map.Start();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        map.Stop();
    }
}