using System;
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
        
        [SerializeField]public WorldHolder worldHolder;

        void Start()
        {
            aim = GameObject.Find("AimImage");
            aim.SetActive(true);
            playerCamera = Camera.main;
            selectoionTransform = Instantiate(selectionCube.gameObject).transform;
            selectoionTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            Select();
            MainAction();
        }

        void MainAction()
        {
            if (Input.GetMouseButton(0))
            {
                if (selectedPoint.HasValue)
                {
                    Vector2Int chunkPosition = GameSettings.ToChunkPos(selectedPoint.Value);
                    if (worldHolder.TryGet(chunkPosition, out LoadedChunk chunk))
                    {
                        Vector3Int inChunkPos = GameSettings.ToInChunkPos(selectedPoint.Value);
                        BlockId blockId = chunk.ChunkData.GetId(inChunkPos);
                        Debug.Log(blockId);
                    }
                }
            }
        }

        
        
        void Select()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10, 1<<8))
            {
                if (hit.collider != null)
                {
                    // Debug.DrawLine(hit.point, hit.point + hit.normal);
                    Vector3 point = hit.point;
                    selectoionTransform.gameObject.SetActive(true);
                    selectedPoint = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z)) -
                                    hit.normal;
                    selectoionTransform.position = selectedPoint.Value;
                    // Debug.Log("hit: " + point + " " + hit.normal);
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
}