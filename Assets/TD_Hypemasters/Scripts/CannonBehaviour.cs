using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//  Не стал вводить дополнительные сущности и выделять базовый класс (CannnonBase). Когда появится нужна, из этого класса
// можно легко выделить базовый функционал
public class CannonBehaviour : SaveableBehaviour
{
    // Оставлю доступ через Inspector
    [SerializeField] private Transform CannonPivot;
    [SerializeField] private Transform ShootPosition;
    [SerializeField] private float ShootingDelay;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private int Damage;
    [SerializeField] private float AngleToHorizont;
    [SerializeField] private float AttackRadius;
    [SerializeField] private SphereCollider _attackZone;
    
    [SerializeField] private CannonData _cannonData;

    private Quaternion _baseLocalRotation;
    private List<GoblinBehaviour> _enemies;
    private GoblinBehaviour _currentEnemyComponent;
    
    private bool _isShooting;
    
    protected override void Start()
    {
        base.Start();
        Type = ObjectType.Cannon;
        
        // Пока так, но в перспективе можно эту логику использовать при динамическом создании башен
        if (_cannonData != null)
        {
            Damage = _cannonData.Damage;
            ShootingDelay = _cannonData.AttackDelay;
            AttackRadius = _cannonData.AttackDistance;
            AngleToHorizont = _cannonData.AngleToHorizon;
        }
        
        // В модели pivot находится не в геометрическом центре, а где-то под пушкой,
        // поэтому вращение получилось немного кривым. Плюс, диапазон значений
        // вышел от 15 до -75, где 15 это параллельно горизонту, а -75 это вертикально вверх..
        float a = Mathf.Clamp(AngleToHorizont, 0, 90) * -1 + 15f;
        CannonPivot.localRotation = Quaternion.Euler(a, 0, 0);

        if (_attackZone != null)
        {
            _attackZone.radius = AttackRadius;
        }

        _baseLocalRotation = CannonPivot.localRotation;
        _enemies = new List<GoblinBehaviour>();
        _isShooting = false;
    }

    void Update()
    {
        // Есть ли вообще в кого целиться?
        if (_enemies.Count == 0)
        {
            return;
        }

        // Есть ли у нас кто-нибудь на прицеле?
        if (_currentEnemyComponent == null)
        {
            _currentEnemyComponent = ChooseTarget();

            if (_currentEnemyComponent == null)
            {
                if (_isShooting)
                {
                    StopCoroutine(Shoot());
                }

                if (!CannonPivot.localRotation.Equals(_baseLocalRotation))
                {
                    CannonPivot.localRotation = _baseLocalRotation;
                }

                return;
            }
        }

        // А если есть, он не мёртв?
        if (_currentEnemyComponent.IsDead)
        {
            _enemies.Remove(_currentEnemyComponent);
            _currentEnemyComponent = null;
            CannonPivot.localRotation = _baseLocalRotation;

            return;
        }

        // Finally.. Можно и пострелять
        if (CannonPivot != null)
        {
            Vector3 targetPosition = _currentEnemyComponent.transform.position;
            Vector3 pivot = CannonPivot.position;
            Vector3 lookDirection = new Vector3(targetPosition.x - pivot.x,
                0,
                targetPosition.z - pivot.z);
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            CannonPivot.rotation = rotation;
        }

        if (!_isShooting)
        {
            StartCoroutine(Shoot());
        }

    }

    private GoblinBehaviour ChooseTarget()
    {
        if (_enemies.Count > 0)
        {
            // Почистим коллекцию от нулевых ссылок. Бывает, что гоблин может исчезнуть, не покинув триггер башни
            // Особенно если триггер захватывает зону замка, где исчезадют гоблины
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null)
                {
                    _enemies.RemoveAt(i);
                }
            }
            
            _currentEnemyComponent = _enemies[0];
            return _enemies[0];
        }

        return null;
    }

    IEnumerator Shoot()
    {
        _isShooting = true;

        while (_currentEnemyComponent != null)
        {
            GameObject go = Instantiate(BulletPrefab, ShootPosition.position, Quaternion.identity);
            BulletBehaviour b = go.GetComponent<BulletBehaviour>();
            b.Initialize(_currentEnemyComponent, _currentEnemyComponent.transform, Damage);
            
            yield return new WaitForSeconds(ShootingDelay);
        }

        _isShooting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        GoblinBehaviour enemy = other.GetComponent<GoblinBehaviour>();
        if (enemy != null && !_enemies.Contains(enemy))
        {
            _enemies.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GoblinBehaviour enemy = other.GetComponent<GoblinBehaviour>();
        if (enemy != null)
        {
            _enemies.Remove(enemy);

            if (_currentEnemyComponent == enemy)
            {
                _currentEnemyComponent = null;
            }
        }
    }

    #region SaveableBehaviour implementation

    // Не самое изящное решение, но как-то надо восстановить ссылки на targets
    private int _targetGlobalId;
    private List<int> _enemiesGlobalId;
    
    public struct CannonState
    {
        // Не буду тут сохранять позицию и вращение самой башни, т.к. в рамках прототипа не подразумевается,
        // что башни будут создаваться динамически. Только пушка.
        public Vector3 PivotPosition;
        public Quaternion PivotRotation;
        public int CurrentTargetGlobalId;
        public List<int> EnemiesGlobalIds;
        public bool IsShooting;
    }

    public override object GetState()
    {
        CannonState state = new CannonState
        {
            PivotPosition = CannonPivot.position,
            PivotRotation = CannonPivot.rotation,
            CurrentTargetGlobalId = _currentEnemyComponent != null ? _currentEnemyComponent.GlobalId : -1,
            IsShooting = _isShooting
        };

        state.EnemiesGlobalIds = new List<int>();

        foreach (GoblinBehaviour e in _enemies)
        {
            state.EnemiesGlobalIds.Add(e.GlobalId);
        }

        return state;
    }

    public override void SetState(object state)
    {
        if (state is CannonState)
        {
            CannonState s = (CannonState) state;
            CannonPivot.position = s.PivotPosition;
            CannonPivot.rotation = s.PivotRotation;
            _isShooting = s.IsShooting;
            _targetGlobalId = s.CurrentTargetGlobalId;
            _enemiesGlobalId = s.EnemiesGlobalIds;
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
            if (_targetGlobalId > 0)
            {
                GameObject currentEnemy = _t.GetGameObjectByGlobalId(_targetGlobalId);
                _currentEnemyComponent = currentEnemy.GetComponent<GoblinBehaviour>();
            }
            else
            {
                _currentEnemyComponent = null;
            }

            _enemies.Clear();
            
            foreach (int id in _enemiesGlobalId)
            {
                GameObject enemy = _t.GetGameObjectByGlobalId(id);
                _enemies.Add(enemy.GetComponent<GoblinBehaviour>());
            }
        }
    }

    #endregion
    
}
