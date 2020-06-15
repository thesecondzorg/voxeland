using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Map;
using Mirror;
using Test.Map;
using UnityEngine;

namespace Server
{
    public class ChunkSaveSystem
    {
        private string folder;
        private ParallelOptions parallelOptions;

        public ChunkSaveSystem(string name)
        {
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 3;
            
            folder = Application.persistentDataPath + "/Worlds";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            folder += "/" + name;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public bool Load(Vector2Int chunkPosition, out ChunkData chunk)
        {
            try
            {
                string chunkPath = ToChunkPath(chunkPosition);
                // if (File.Exists(chunkPath))
                // {
                //     FileStream file = File.OpenRead(chunkPath);
                //     BinaryFormatter bf = new BinaryFormatter();
                //     chunk = bf.Deserialize(file) as ChunkData;
                //     file.Close();
                //     return true;
                // }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            chunk = null;
            return false;
        }

        public void Save(ChunkData chunk)
        {
            Parallel.Invoke(() =>
            {
                try
                {
                    // string chunkPath = ToChunkPath(chunk.chunkPosition);
                    // if (File.Exists(chunkPath))
                    // {
                    //     File.Delete(chunkPath);
                    // }
                    //
                    // FileStream file = File.OpenWrite(chunkPath);
                    //
                    // // foreach (ChunkSlice slice in chunk.Slices)
                    // // {
                    // //     if (slice.isSingleBlock)
                    // //     {
                    // //         file.WriteByte(1);
                    // //         
                    // //     }
                    // //     else
                    // //     {
                    // //         
                    // //     }
                    // // }
                    // BinaryFormatter bf = new BinaryFormatter();
                    // bf.Serialize(file, chunk);
                    // file.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }

        public void RequestChunk(Vector2Int msgPos,
            Action<ChunkData> foundChunkAction,
            Action<Vector2Int> unknownChunkAction)
        {
            unknownChunkAction.Invoke(msgPos);
            // Parallel.Invoke(parallelOptions, () =>
            // {
            //     if (Load(msgPos, out ChunkData chunk))
            //     {
            //         foundChunkAction.Invoke(chunk);
            //     }
            //     else
            //     {
            //         unknownChunkAction.Invoke(msgPos);
            //     }
            // });
        }

        private string ToChunkPath(Vector2Int chunkPosition)
        {
            return folder + $"/[{chunkPosition.x}]_[{chunkPosition.y}].chunk";
        }
    }
}