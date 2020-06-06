using System;
using Mirror;
using Test;
using Test.Map;
using UnityEngine;

namespace Client
{
    public class SelectTargetSystem : MonoBehaviour
    {
        private Camera playerCamera;
        [SerializeField] private SelectionCube selectionCube;
        private Transform selectoionTransform;
        [SerializeField] private GameObject aim;
        private Nullable<Vector3> selectedPoint;

        public ChunksHolder worldHolder;

        void Start()
        {
            worldHolder = GetComponent<ChunkLoaderSystem>().worldHolder;
            aim = GameObject.Find("AimImage");
            aim.SetActive(true);
            playerCamera = Camera.main;
            selectoionTransform = Instantiate(selectionCube.gameObject).transform;
            selectoionTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            Select();
            if (Input.GetMouseButton(0))
            {
                MainAction();
            }

            if (Input.GetMouseButton(1))
            {
                SecondAction();
            }
        }

        void MainAction()
        {
            if (selectedPoint.HasValue)
            {
                Debug.Log(selectedPoint.Value);

                Vector2Int chunkPosition = GameSettings.ToChunkPos(selectedPoint.Value);
                if (worldHolder.TryGet(chunkPosition, out Chunk chunk))
                {
                    Vector3Int inChunkPos = GameSettings.ToInChunkPos(selectedPoint.Value);
                    BlockId blockId = chunk.chunk.GetId(inChunkPos);
                    Debug.Log(blockId);
                    NetworkClient.Send(new BlockUpdateRequest
                    {
                        chunkPosition = chunkPosition,
                        inChunkPosition = inChunkPos,
                        blockId = BlockId.AIR
                    });
                }
            }
        }

        void SecondAction()
        {
        }

        void Select()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10, 1 << 8))
            {
                if (hit.collider != null)
                {
                    // Debug.DrawLine(hit.point, hit.point + hit.normal);
                    Vector3 point = hit.point;
                    selectoionTransform.gameObject.SetActive(true);
                    selectedPoint = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z)) -
                                    hit.normal;
                    selectoionTransform.position = selectedPoint.Value;
                }
                else
                {
                    selectoionTransform.gameObject.SetActive(false);
                }
            }
            else
            {
                selectoionTransform.gameObject.SetActive(false);
            }
        }
    }

    public class BlockUpdateRequest : MessageBase
    {
        public Vector2Int chunkPosition;
        public Vector3Int inChunkPosition;
        public BlockId blockId;
    }
}