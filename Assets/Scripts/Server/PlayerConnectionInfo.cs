using System;
using System.Collections.Generic;
using Mirror;
using Test.Map;
using UnityEngine;

namespace Test.Netowrker
{
    public class PlayerConnectionInfo
    {
        private Vector3 position;
        public NetworkIdentity netId;
        public List<Action<PlayerConnectionInfo>> chunkPosListeners = new List<Action<PlayerConnectionInfo>>();

        public PlayerConnectionInfo(NetworkIdentity netId)
        {
            this.netId = netId;
        }

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                
                Vector2Int newChunkPos = new Vector2Int(
                    (int) (position.x / GameSettings.CHUNK_SIZE),
                    (int) (position.z / GameSettings.CHUNK_SIZE));
                ChunkPos = newChunkPos;
                if (ChunkPos != newChunkPos)
                {
                    foreach (Action<PlayerConnectionInfo> listener in chunkPosListeners)
                    {
                        listener.Invoke(this);
                    }
                }
            }
        }

        public Vector2Int ChunkPos { get; private set; }
        
        public void SendChunk(ChunkData chunk)
        {
            NetworkServer.SendToClientOfPlayer(netId, chunk);
        }
        
    }
}