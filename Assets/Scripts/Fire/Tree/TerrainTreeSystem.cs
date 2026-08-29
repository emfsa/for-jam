using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainTreeSystem : MonoBehaviour
{
    [Header("Terrain & Prefabs")]
    [SerializeField] private Terrain _terrain;
    [SerializeField] private GameObject _interactiveTreePrefab;
    [SerializeField] private LayerMask _terrainLayer;

    private TerrainData _terrainData;
    private TerrainCollider _terrainCollider;

    // СТРОГО без static! Свой массив бэкапа для каждой сцены
    private TreeInstance[] _savedOriginalTrees;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitializeAndBackup();
    }

    private void InitializeAndBackup()
    {
        if (_isInitialized) return;

        if (_terrain == null) _terrain = GetComponent<Terrain>() ?? Terrain.activeTerrain;

        if (_terrain != null && _terrain.terrainData != null)
        {
            _terrainData = _terrain.terrainData;
            _terrainCollider = _terrain.GetComponent<TerrainCollider>();

            // Клонируем исходный массив деревьев ТОЛЬКО один раз при загрузке этой сцены
            _savedOriginalTrees = (TreeInstance[])_terrainData.treeInstances.Clone();
            _isInitialized = true;
        }
    }

    public bool TryReplaceTerrainTree(Vector3 hitPoint, float damage)
    {
        if (SceneManager.GetActiveScene().name == "Night") return false;

        if (!_isInitialized || _terrainData == null)
        {
            InitializeAndBackup();
        }

        if (_terrain == null || _terrainData == null) return false;

        Vector3 tempCoord = hitPoint - _terrain.transform.position;

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

            if (distance < minDistance && distance < 3.5f)
            {
                minDistance = distance;
                closestTreeIndex = i;
            }
        }

        if (closestTreeIndex != -1)
        {
            ReplaceTreeAndDamage(closestTreeIndex, damage);
            return true;
        }

        return false;
    }

    private void ReplaceTreeAndDamage(int index, float damage)
    {
        TreeInstance tree = _terrainData.GetTreeInstance(index);
        Vector3 worldPos = GetTreeWorldPosition(tree);

        float treeAngleDegrees = tree.rotation * Mathf.Rad2Deg;
        Quaternion finalRotation = Quaternion.Euler(0f, treeAngleDegrees, 0f);

        // Удаляем дерево только в Runtime-копии массива
        List<TreeInstance> treeList = new List<TreeInstance>(_terrainData.treeInstances);
        treeList.RemoveAt(index);
        _terrainData.treeInstances = treeList.ToArray();

        // Обновляем коллайдер террейна без вызова Flush(), чтобы не пачкать файл ассета
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
            treeLogic.TakeDamage(damage);
        }
    }

    private Vector3 GetTreeWorldPosition(TreeInstance tree)
    {
        float worldX = tree.position.x * _terrainData.size.x + _terrain.transform.position.x;
        float worldZ = tree.position.z * _terrainData.size.z + _terrain.transform.position.z;
        float worldY = _terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + _terrain.transform.position.y;

        return new Vector3(worldX, worldY, worldZ);
    }

    private void RestoreForest()
    {
        // Восстанавливаем деревья только если данные гарантированно принадлежат этому объекту
        if (_isInitialized && _terrainData != null && _savedOriginalTrees != null)
        {
            _terrainData.treeInstances = _savedOriginalTrees;
        }
    }

    private void OnDisable() => RestoreForest();
    private void OnDestroy() => RestoreForest();
    private void OnApplicationQuit() => RestoreForest();
}