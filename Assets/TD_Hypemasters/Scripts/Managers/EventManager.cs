using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public UnityEvent GamePaused;
    public UnityEvent GameResumed;
    public UnityEventInt TimeScrolled;
    public UnityEvent EnemyDied;
    public UnityEvent EnemyReachedCastle;
    public UnityEvent StatisticChanged;
    public UnityEventVector3 MouseClickOnRoad;
    public UnityEvent GameTimeIncreased;
    
    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);

        GamePaused = new UnityEvent();
        GameResumed = new UnityEvent();
        TimeScrolled = new UnityEventInt();
        EnemyDied = new UnityEvent();
        EnemyReachedCastle = new UnityEvent();
        StatisticChanged = new UnityEvent();
        MouseClickOnRoad = new UnityEventVector3();
    }
}

// ===========================

public class UnityEventVector3 : UnityEvent<Vector3>
{
}

public class UnityEventInt : UnityEvent<int>
{
}
