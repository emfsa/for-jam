using UnityEngine;

public class Log : MonoBehaviour
{
    [SerializeField] private int _logAmount = 1;

    
    private PlayerStats _playerStats;

   /* private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerStats player))
        {
            player.SetCurrentLog(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out PlayerStats player))
        {
            player.SetCurrentLog(null);
        }
    }*/

    public void pickUP(PlayerStats playerStats)
    {
        playerStats.AddLogs(_logAmount);
        Destroy(gameObject);
    }
}
