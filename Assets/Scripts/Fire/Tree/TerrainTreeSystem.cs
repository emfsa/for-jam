using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // 1. Добавили пространство имен для работы со сценами

public class TerrainTreeSystem : MonoBehaviour
{
    [Header("Terrain & Prefabs")]
    [SerializeField] private Terrain _terrain;
    [SerializeField] private GameObject _interactiveTreePrefab; // Префаб с вашим TreeLogic
    [SerializeField] private LayerMask _terrainLayer;

    [Header("Chop Settings")]
    [SerializeField] private float _chopRange = 4f;
    [SerializeField] private float _damagePerHit = 10f;

    private TerrainData _terrainData;
    private TerrainCollider _terrainCollider;
    private TreeInstance[] _initialTrees;

    private void Awake()
    {
        if (_terrain == null)
            _terrain = Terrain.activeTerrain;

        if (_terrain != null)
        {
            _terrainData = _terrain.terrainData;
            _terrainCollider = _terrain.GetComponent<TerrainCollider>();

            // Клонируем исходный массив, чтобы не испортить .asset файл в Unity Editor
            _initialTrees = (TreeInstance[])_terrainData.treeInstances.Clone();
        }
    }

    private void Update()
    {
        // Проверка нажатия ЛКМ через новый Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHitTree();
        }
    }

    private void TryHitTree()
    {
        // 2. Проверка сцены: если срубать деревья пытаются на сцене "Night", выходим из метода
        if (SceneManager.GetActiveScene().name == "Night") return;

        if (Camera.main == null || _terrain == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, _chopRange))
        {
            // 1. Если попали по уже спавненному объекту с TreeLogic
            if (hit.collider.TryGetComponent<TreeLogic>(out var treeLogic))
            {
                treeLogic.TakeDamage(_damagePerHit);
                return;
            }

            // 2. Если попали по слою Terrain — ищем ближайшее дерево на террейне
            if (((1 << hit.collider.gameObject.layer) & _terrainLayer) != 0)
            {
                Vector3 tempCoord = hit.point - _terrain.transform.position;

                int closestTreeIndex = -1;
                float minDistance = float.MaxValue;

                for (int i = 0; i < _terrainData.treeInstanceCount; i++)
                {
                    TreeInstance tree = _terrainData.GetTreeInstance(i);

                    // Перевод X и Z координат из диапазона (0..1) в мировые метры
                    Vector3 treeWorldXZ = new Vector3(
                        tree.position.x * _terrainData.size.x,
                        0f,
                        tree.position.z * _terrainData.size.z
                    );

                    Vector3 tempCoordXZ = new Vector3(tempCoord.x, 0f, tempCoord.z);
                    float distance = Vector3.Distance(treeWorldXZ, tempCoordXZ);

                    if (distance < minDistance && distance < 2.5f) // Радиус поиска
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

        // 1. Рассчитываем точный угол в градусах из радиан
        float treeAngleDegrees = tree.rotation * Mathf.Rad2Deg;

        // 2. Задаем поворот strictly вокруг оси Y
        Quaternion finalRotation = Quaternion.Euler(0f, treeAngleDegrees, 0f);

        // 3. Удаляем дерево из TerrainData
        List<TreeInstance> treeList = new List<TreeInstance>(_terrainData.treeInstances);
        treeList.RemoveAt(index);
        _terrainData.treeInstances = treeList.ToArray();
        _terrain.Flush();

        if (_terrainCollider != null)
        {
            _terrainCollider.enabled = false;
            _terrainCollider.enabled = true;
        }

        // 4. Спавним префаб
        GameObject spawnedTree = Instantiate(_interactiveTreePrefab, worldPos, finalRotation);

        // Масштабирование
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
        // 1. Считаем чистые мировые X и Z
        float worldX = tree.position.x * _terrainData.size.x + _terrain.transform.position.x;
        float worldZ = tree.position.z * _terrainData.size.z + _terrain.transform.position.z;

        // 2. Получаем точную высоту поверхности Terrain именно в этой точке
        float worldY = _terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + _terrain.transform.position.y;

        return new Vector3(worldX, worldY, worldZ);
    }

    private void RestoreForest()
    {
        // Безопасное восстановление террейна без ошибок при закрытии сцены
        if (_terrain != null && _terrainData != null && _initialTrees != null)
        {
            _terrainData.treeInstances = _initialTrees;
            _terrain.Flush();

            if (_terrainCollider != null)
            {
                _terrainCollider.enabled = false;
                _terrainCollider.enabled = true;
            }
        }
    }

    private void OnDisable()
    {
        RestoreForest();
    }

    private void OnApplicationQuit()
    {
        RestoreForest();
    }
}