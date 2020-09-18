using UnityEngine;

public enum ObjectType
{
    Undefined = 0,
    Bullet,
    Goblin,
    Cannon,
    StatisticManager
}

public class SaveableBehaviour : MonoBehaviour
{
    public static ObjectType DefineType(object o)
    {
        if (o is GoblinBehaviour.GoblinState)
        {
            return ObjectType.Goblin;
        }

        if (o is CannonBehaviour.CannonState)
        {
            return ObjectType.Cannon;
        }

        if (o is BulletBehaviour.BulletState)
        {
            return ObjectType.Bullet;
        }
        
        if (o is StatisticManager.StatisticManagerState)
        {
            return ObjectType.StatisticManager;
        }

        return ObjectType.Undefined;
    }
    
    
    public int GlobalId;
    public ObjectType Type;
    
    private ObjectsTimelineManager _timeLineManager;

    

    protected virtual void Start()
    {
        _timeLineManager = GameManager.Instance.GetManager<ObjectsTimelineManager>();

        if (_timeLineManager != null && _timeLineManager.IsManipulatingTime)
        {
            _timeLineManager.RegisterObject(this);
        }
    }

    public virtual void Init(int globalId)
    {
        GlobalId = globalId;
    }

    public virtual object GetState()
    {
        return null;
    }

    public virtual void SetState(object state)
    {
        
    }

    // Автоматически вызывается после того, как все у всех текущих объектов был вызван SetState
    public virtual void LateSetState()
    {
    }

    protected virtual void OnDestroy()
    {
        if (_timeLineManager != null && _timeLineManager.IsManipulatingTime)
        {
            _timeLineManager.UnregisterObject(this);
        }
    }
}
