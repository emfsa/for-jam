using System.Collections.Generic;
using UnityEngine;

public class LogStorage : MonoBehaviour
{
    [Header("Storage Capacity")]
    [SerializeField] private int _maxCapacity = 10;
    private int _storedLogsCount = 0;

    [Header("Visual Stages")]
    [Tooltip("3D объекты кучи бревен (от минимальной до полностью забитой)")]
    [SerializeField] private List<GameObject> _visualStages = new List<GameObject>();

    public bool IsFull => _storedLogsCount >= _maxCapacity;
    public bool IsEmpty => _storedLogsCount <= 0;
    public int StoredLogsCount => _storedLogsCount;
    public int MaxCapacity => _maxCapacity;

    private void Start()
    {
        UpdateVisuals();
    }

    public void UpgradeStorage(int extraCapacity)
    {
        _maxCapacity += extraCapacity;
        UpdateVisuals();
        Debug.Log($"Склад улучшен! Новая вместимость: {_maxCapacity}");
    }

    public bool TryAddLog()
    {
        if (IsFull) return false;

        _storedLogsCount++;
        UpdateVisuals();
        return true;
    }

    public bool TryRemoveLog()
    {
        if (IsEmpty) return false;

        _storedLogsCount--;
        UpdateVisuals();
        return true;
    }

    private void UpdateVisuals()
    {
        if (_storedLogsCount == 0 || _visualStages == null || _visualStages.Count == 0)
        {
            if (_visualStages != null)
            {
                foreach (var stage in _visualStages)
                {
                    if (stage != null) stage.SetActive(false);
                }
            }
            return;
        }

        float fillPercent = (float)_storedLogsCount / _maxCapacity;

        int targetIndex = Mathf.Clamp(
            Mathf.FloorToInt(fillPercent * _visualStages.Count),
            0,
            _visualStages.Count - 1
        );

        for (int i = 0; i < _visualStages.Count; i++)
        {
            if (_visualStages[i] != null)
            {
                _visualStages[i].SetActive(i == targetIndex);
            }
        }
    }
}