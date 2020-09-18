using UnityEngine;

public class InputManager : MonoBehaviour
{
    private IInput _input;

    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);
    }
    
    void Start()
    {
        // Вот здесь мы, якобы, устраиваем проверку, на какой платформе играет игрок, чтобы подсунуть нужный интерпретатор ввода
        // И после долгих и мучительных проверок: voila!
        _input = new PCInput();
        _input.Initialize();
    }

    void Update()
    {
        _input.WaitAndHandleInput();
    }
}

//==============================

public interface IInput
{
    void Initialize();
    void WaitAndHandleInput();
}

public class PCInput : IInput
{
    private EventManager _eventManager;

    public void Initialize()
    {
        _eventManager = GameManager.Instance.GetManager<EventManager>();
    }
    
    public void WaitAndHandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int mask = 1 << 8; // raycastable layer

            if (Physics.Raycast(ray, out hit, 100.0f, mask))
            {
                if (hit.transform != null && hit.collider.gameObject.CompareTag("Road"))
                {
                    if (_eventManager != null)
                    {
                        _eventManager.MouseClickOnRoad.Invoke(hit.point);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _eventManager.TimeScrolled.Invoke(36);
        }
    }
}


