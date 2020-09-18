using UnityEngine;

//  Не стал вводить дополнительные сущности и выделять базовый класс (EnemyBase). Когда появится нужна, из этого класса
// можно легко выделить базовый функционал
public class GoblinBehaviour : SaveableBehaviour
{
    private readonly int ANIM_DEATH_HASH = Animator.StringToHash("Death");
    
    [SerializeField] private Animator animator;
    [SerializeField] private Transform Waypoint;
    [SerializeField] private float Speed;
    [SerializeField] private int Health;
    public bool IsDead => Health <= 0;

    private bool _canMove;
    private EventManager _eventManager;

    public void Initialize(string name, float speed, int health, Transform wp)
    {
        gameObject.name = name;
        Speed = speed;
        Health = health;
        Waypoint = wp;
    }

    protected override void Start()
    {
        base.Start();
        Type = ObjectType.Goblin;
        
        // В рамках прототипа не указывается, что гоблин будет сходить со своей траектории,
        // т.е. он не должен будет огибать препятствия, не будет отбрасываться взрывами и т.п.
        // Тем самым, направление можно выставить один раз при инициализации
        transform.LookAt(Waypoint);
        _canMove = true;

        _eventManager = GameManager.Instance.GetManager<EventManager>();

        if (_eventManager != null)
        {
            _eventManager.GameResumed.AddListener(OnGameResumed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Думаю, тут не особо нужна физика, поэтому можно обойтись прямым изменением transform.position
        if (_canMove && Waypoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Waypoint.position, Time.deltaTime * Speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            EventManager e = GameManager.Instance.GetManager<EventManager>();
            e.EnemyReachedCastle.Invoke();
            
            Destroy(gameObject);
        }
    }

    public void ApplyDamage(int d)
    {
        if (IsDead)
        {
            return;
        }

        if (Health > 0)
        {
            Health -= d;
        }

        if (IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        _canMove = false;
        animator.SetBool(ANIM_DEATH_HASH, true);

        EventManager e = GameManager.Instance.GetManager<EventManager>();
        e.EnemyDied.Invoke();
            
        // Я позволю тебе проиграть последнюю анимацию перед смертью..
        Destroy(gameObject, 2);
    }

    private void OnGameResumed()
    {
        if (IsDead)
        {
            Die();
        }
    }

    #region SaveableBehaviour implementation

    public struct GoblinState
    {
        // Waypoint будет задаваться из EnemySpawner, и он всегда одинаковый, поэтому пока не вижу смысла его сохранять
        public Vector3 Position;
        public Quaternion Rotation;
        public int Health;
        public bool CanMove;
    }

    public override object GetState()
    {
        GoblinState s = new GoblinState
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Health = this.Health,
            CanMove = _canMove
        };

        return s;
    }

    public override void SetState(object state)
    {
        if (state is GoblinState)
        {
            GoblinState s = (GoblinState)state;

            transform.position = s.Position;
            transform.rotation = s.Rotation;
            Health = s.Health;
            _canMove = s.CanMove;
        }

        else
        {
            Debug.LogError("Unable to restore object state! Something went deeply wrong.. " + gameObject.name);
        }
    }

    #endregion
    
    }
