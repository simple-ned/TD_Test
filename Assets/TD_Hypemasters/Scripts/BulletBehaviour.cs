using UnityEngine;

public class BulletBehaviour : SaveableBehaviour
{
    [SerializeField] public float _speed;
    
    public GameObject ImpactVFX;

    private Transform _targetTransform;
    private GoblinBehaviour _enemy;
    private int _damage;

    protected override void Start()
    {
        base.Start();
        Type = ObjectType.Bullet;
    }

    public void Initialize(GoblinBehaviour enemy, Transform target, int damage)
    {
        _targetTransform = target;
        _damage = damage;
        _enemy = enemy;
    }

    private void Update()
    {
        if (_targetTransform != null)
        {
            transform.LookAt(_targetTransform);
            transform.position = Vector3.MoveTowards(transform.position, _targetTransform.position, _speed * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == _targetTransform)
        {
            // Нанеси урон...
            if (_enemy != null)
            {
                _enemy.ApplyDamage(_damage);
            }

            // ... и красиво исчезни. "Со взрывом всё становится лучше" ©
            GameObject impact = GameObject.Instantiate(ImpactVFX, transform.position, Quaternion.identity);
            Destroy(impact, 0.5f);
            Destroy(gameObject);
        }
    }
    
    #region SabeableBehaviour implementation

    public struct BulletState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public int TargetId;
        public int Damage;
        public float Speed;
    }

    public override object GetState()
    {
        BulletState s = new BulletState
        {
            Position = transform.position,
            Rotation = transform.rotation,
            TargetId = _enemy.GlobalId,
            Damage = _damage,
            Speed = _speed
        };

        return s;
    }

    // Не самое изящное решение, но надо как-то восстановить ссылки на Target
    private int _targetGlobalId;
    
    public override void SetState(object state)
    {
        if (state is BulletState)
        {
            BulletState s = (BulletState)state;

            transform.position = s.Position;
            transform.rotation = s.Rotation;
            _damage = s.Damage;
            _speed = s.Speed;
            _targetGlobalId = s.TargetId;
        }

        else
        {
            Debug.LogError("Unable to restore object state! Something went deeply wrong.. " + gameObject.name);
        }
    }

    public override void LateSetState()
    {
        ObjectsTimelineManager _t = GameManager.Instance.GetManager<ObjectsTimelineManager>();
        if (_t != null)
        {
            GameObject target = _t.GetGameObjectByGlobalId(_targetGlobalId);
            if (target != null)
            {
                _enemy = target.GetComponent<GoblinBehaviour>();
                _targetTransform = target.transform;
            }
        }
    }

    #endregion
}
