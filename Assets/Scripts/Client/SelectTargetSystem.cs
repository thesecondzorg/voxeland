using System;
using Mirror;
using Test;
using Test.Map;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class SelectTargetSystem : MonoBehaviour
    {
        private Camera playerCamera;
        [SerializeField] private SelectionCube selectionCube;
        private Transform selectoionTransform;
        [SerializeField] private GameObject aim;
        [SerializeField] private Text text;
        private Nullable<Vector3> selectedPoint;

        public ChunksHolder worldHolder;

        void Start()
        {
            worldHolder = GetComponent<ChunkLoaderSystem>().worldHolder;
            aim = GameObject.Find("AimImage");
            text = GameObject.Find("UnderCursorInfo").GetComponent<Text>();
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
                Vector3Int inChunkPos = GameSettings.ToInChunkPos(selectedPoint.Value);
                if (worldHolder.TryGet(chunkPosition, out Chunk chunk))
                {
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
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, 10, 1 << 8))
            {
                if (hit.collider != null)
                {
                    Vector3 point = hit.point;
                    selectoionTransform.gameObject.SetActive(true);
                 
                    selectedPoint =
                        new Vector3(
                            hit.normal.x == 0f ? Mathf.Round(point.x) : (point.x + hit.normal.x * 0.02f), 
                            hit.normal.y == 0f ? Mathf.Round(point.y) : (point.y + hit.normal.y * 0.02f), 
                            hit.normal.z == 0f ? Mathf.Round(point.z) : (point.z + hit.normal.z * 0.02f));
                    selectoionTransform.position = selectedPoint.Value;

                    Debug.DrawLine(selectedPoint.Value, selectedPoint.Value + hit.normal);
                    // -0.5,70,0 : 1,0,0
                    // 0,70,0    : 0,90,0

                    // 0,70,-0.5  : 0,0,-1
                    // 0,70,0    : 0,0,0
                    selectoionTransform.rotation =
                        Quaternion.FromToRotation(selectoionTransform.up, hit.normal) * selectoionTransform.rotation;
                    Vector2Int chunkPosition = GameSettings.ToChunkPos(selectedPoint.Value);
                    Vector3Int inChunkPos = GameSettings.ToInChunkPos(selectedPoint.Value);
                    text.text = point.ToString() + " \n " + hit.normal + "\n " + selectedPoint.Value;
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