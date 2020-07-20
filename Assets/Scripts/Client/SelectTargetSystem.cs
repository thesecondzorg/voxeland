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
        private Nullable<Vector3Int> selectedPoint;
        private Nullable<Vector3Int> processingPoint;
        private Nullable<Vector3Int> placePoint;
        private Vector3 normal;
        [SerializeField] private ChunkLoaderSystem chunkLoaderSystem;
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
            if (!selectedPoint.HasValue) return;
            if (processingPoint.HasValue && processingPoint.Value == selectedPoint.Value) return;
            processingPoint = selectedPoint.Value;
            Vector3 point = selectedPoint.Value;
            // Debug.Log(point);
            Vector2Int chunkPosition = GameSettings.ToChunkPos(point);
            Vector3Int inChunkPos = GameSettings.ToInChunkPos(point);
            if (chunkLoaderSystem.TryUpdateBlock(chunkPosition, inChunkPos, BlockId.AIR, out BlockId oldId))
            {
                BlockUpdateRequest request = new BlockUpdateRequest
                {
                    chunkPosition = chunkPosition,
                    inChunkPosition = inChunkPos,
                    blockId = BlockId.AIR
                };
                NetworkClient.Send(request);
                selectedPoint = null;
            }
        }

        void SecondAction()
        {
            if (!placePoint.HasValue) return;
            if (processingPoint.HasValue && processingPoint.Value == placePoint.Value) return;
            processingPoint = placePoint.Value;
            Vector3 point = placePoint.Value;
            // Debug.Log(point);
            Vector2Int chunkPosition = GameSettings.ToChunkPos(point);
            Vector3Int inChunkPos = GameSettings.ToInChunkPos(point);
            if (chunkLoaderSystem.TryUpdateBlock(chunkPosition, inChunkPos, BlockId.AIR, out BlockId oldId))
            {
                BlockUpdateRequest request = new BlockUpdateRequest
                {
                    chunkPosition = chunkPosition,
                    inChunkPosition = inChunkPos,
                    blockId = BlockId.of(2)
                };
                NetworkClient.Send(request);
                selectedPoint = null;
            }
        }

        void Select()
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, 5, 1 << 8))
            {
                if (hit.collider != null)
                {
                    Vector3 point = hit.point;
                    normal = hit.normal;
                    DrawSelectBox(point, normal);

                    selectedPoint = (point + new Vector3(0.5f, 0.5f, 0.5f) - normal * 0.02f).Floor();
                    placePoint = (point + new Vector3(0.5f, 0.5f, 0.5f) + normal * 0.02f).Floor();

                    Vector2Int chunkPosition = GameSettings.ToChunkPos(selectedPoint.Value);
                    Vector3Int inChunkPos = GameSettings.ToInChunkPos(selectedPoint.Value);
                    text.text = point.ToString() + " \n " + chunkPosition + "\n " + inChunkPos + "\n" +
                                (selectedPoint.Value);
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


        private void DrawSelectBox(Vector3 point, Vector3 normal)
        {
            selectoionTransform.gameObject.SetActive(true);

            Vector3 selectedPoint2 =
                new Vector3(
                    normal.x == 0f ? Mathf.Round(point.x) : (point.x + normal.x * 0.02f),
                    normal.y == 0f ? Mathf.Round(point.y) : (point.y + normal.y * 0.02f),
                    normal.z == 0f ? Mathf.Round(point.z) : (point.z + normal.z * 0.02f));
            selectoionTransform.position = selectedPoint2;

            selectoionTransform.rotation =
                Quaternion.FromToRotation(selectoionTransform.up, normal) * selectoionTransform.rotation;
        }
    }

    public class BlockUpdateRequest : MessageBase
    {
        public Vector2Int chunkPosition;
        public Vector3Int inChunkPosition;
        public BlockId blockId;
    }
}