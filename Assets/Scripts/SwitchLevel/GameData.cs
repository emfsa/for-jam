using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    [Header("Player resources")]
    public int money = 0;
    public int logInventory = 0;
    public int logStorage = 0;

    [Header("levels")]
    public int damageLevel = 0;
    public int logLevel = 0;
    public int logStorageLevel = 0;
    public int fireLevel = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
