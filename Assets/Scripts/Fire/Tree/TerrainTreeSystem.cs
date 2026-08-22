using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TerrainTreeSystem : MonoBehaviour
{
    [Header("Terrain & Prefabs")]
    [SerializeField] private Terrain _terrain;
    [SerializeField] private GameObject _interactiveTreePrefab;
    [SerializeField] private LayerMask _terrainLayer;

    [Header("Chop Settings")]
    [SerializeField] private float _chopRange = 4f;
    [SerializeField] private float _damagePerHit = 10f;

    private TerrainData _terrainData;
    private TerrainCollider _terrainCollider;

    // Делаем массив статическим, чтобы он хранил САМОЕ ПЕРВОЕ состояние деревьев
    private static TreeInstance[] _savedOriginalTrees;

    private void Awake()
    {
        InitializeAndBackup();
    }

    private void InitializeAndBackup()
    {
        if (_terrain == null) _terrain = Terrain.activeTerrain;

        if (_terrain != null)
        {
            _terrainData = _terrain.terrainData;
            _terrainCollider = _terrain.GetComponent<TerrainCollider>();

            // Бекапим массив ТОЛЬКО один раз при первом запуске игры
            if (_savedOriginalTrees == null && _terrainData != null)
            {
                _savedOriginalTrees = (TreeInstance[])_terrainData.treeInstances.Clone();
            }
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHitTree();
        }
    }

    private void TryHitTree()
    {
        if (SceneManager.GetActiveScene().name == "Night") return;

        if (!_terrain || _terrainData == null)
        {
            InitializeAndBackup();
        }

        if (Camera.main == null || _terrain == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, _chopRange))
        {
            if (hit.collider.TryGetComponent<TreeLogic>(out var treeLogic))
            {
                treeLogic.TakeDamage(_damagePerHit);
                return;
            }

            if (((1 << hit.collider.gameObject.layer) & _terrainLayer) != 0)
            {
                Vector3 tempCoord = hit.point - _terrain.transform.position;

                int closestTreeIndex = -1;
                float minDistance = float.MaxValue;

                for (int i = 0; i < _terrainData.treeInstanceCount; i++)
                {
                    TreeInstance tree = _terrainData.GetTreeInstance(i);

                    Vector3 treeWorldXZ = new Vector3(
                        tree.position.x * _terrainData.size.x,
                        0f,
                        tree.position.z * _terrainData.size.z
                    );

                    Vector3 tempCoordXZ = new Vector3(tempCoord.x, 0f, tempCoord.z);
                    float distance = Vector3.Distance(treeWorldXZ, tempCoordXZ);

                    if (distance < minDistance && distance < 2.5f)
                    {
                        minDistance = distance;
                        closestTreeIndex = i;
                    }
                }

                if (closestTreeIndex != -1)
                {
                    ReplaceTreeAndDamage(closestTreeIndex);
                }
            }
        }
    }

    private void ReplaceTreeAndDamage(int index)
    {
        TreeInstance tree = _terrainData.GetTreeInstance(index);
        Vector3 worldPos = GetTreeWorldPosition(tree);

        float treeAngleDegrees = tree.rotation * Mathf.Rad2Deg;
        Quaternion finalRotation = Quaternion.Euler(0f, treeAngleDegrees, 0f);

        List<TreeInstance> treeList = new List<TreeInstance>(_terrainData.treeInstances);
        treeList.RemoveAt(index);
        _terrainData.treeInstances = treeList.ToArray();
        _terrain.Flush();

        if (_terrainCollider != null)
        {
            _terrainCollider.enabled = false;
            _terrainCollider.enabled = true;
        }

        GameObject spawnedTree = Instantiate(_interactiveTreePrefab, worldPos, finalRotation);

        Vector3 baseScale = _interactiveTreePrefab.transform.localScale;
        spawnedTree.transform.localScale = new Vector3(
            baseScale.x * tree.widthScale,
            baseScale.y * tree.heightScale,
            baseScale.z * tree.widthScale
        );

        if (spawnedTree.TryGetComponent<TreeLogic>(out var treeLogic))
        {
            treeLogic.TakeDamage(_damagePerHit);
        }
    }

    private Vector3 GetTreeWorldPosition(TreeInstance tree)
    {
        float worldX = tree.position.x * _terrainData.size.x + _terrain.transform.position.x;
        float worldZ = tree.position.z * _terrainData.size.z + _terrain.transform.position.z;
        float worldY = _terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + _terrain.transform.position.y;

        return new Vector3(worldX, worldY, worldZ);
    }

    // Восстанавливаем деревья при выходе из Play Mode или уничтожении
    private void RestoreForest()
    {
        if (_terrainData != null && _savedOriginalTrees != null)
        {
            _terrainData.treeInstances = _savedOriginalTrees;
            if (_terrain != null)
            {
                _terrain.Flush();
            }
        }
    }

    private void OnDisable()
    {
        RestoreForest();
    }

    private void OnDestroy()
    {
        RestoreForest();
    }

    private void OnApplicationQuit()
    {
        RestoreForest();
    }
}