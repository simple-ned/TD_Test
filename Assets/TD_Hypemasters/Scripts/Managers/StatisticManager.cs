using UnityEngine;

public class StatisticManager : SaveableBehaviour
{
    private int _killed;
    private int _missed;
    private EventManager _eventManager;

    public int UnitsKilled => _killed;
    public int UnitsMissed => _missed;

    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);
    }
    
    protected override void Start()
    {
        base.Start();
        
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        _eventManager.EnemyDied.AddListener(OnUnitKilled);
        _eventManager.EnemyReachedCastle.AddListener(OnUnitMissed);
    }

    public void OnUnitKilled()
    {
        _killed++;
        _eventManager.StatisticChanged.Invoke();
    }

    public void OnUnitMissed()
    {
        _missed++;
        _eventManager.StatisticChanged.Invoke();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        _eventManager.EnemyDied.RemoveListener(OnUnitKilled);
        _eventManager.EnemyReachedCastle.RemoveListener(OnUnitMissed);
    }
    
    #region SaveableBehaviour implementation

    public struct StatisticManagerState
    {
        public int Killed;
        public int Missed;
    }

    public override object GetState()
    {
        StatisticManagerState s = new StatisticManagerState
        {
            Killed = _killed,
            Missed = _missed
        };

        return s;
    }

    public override void SetState(object state)
    {
        if (state is StatisticManagerState)
        {
            StatisticManagerState s = (StatisticManagerState) state;

            _killed = s.Killed;
            _missed = s.Missed;
        }

        else
        {
            Debug.LogError("Unable to restore object state! Something went deeply wrong.. " + gameObject.name);
        }
    }

    #endregion
}
