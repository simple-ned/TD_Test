using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _labelRoot;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Slider _timeSlider;

    private EventManager _eventManager;
    private ObjectsTimelineManager _timelineManager;
    private bool _isPaused;
    private int _currentSliderIntValue;

    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);
    }
    
    private void Start()
    {
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        _timelineManager = GameManager.Instance.GetManager<ObjectsTimelineManager>();
        
        _isPaused = false;
        
        _pauseButton.onClick.AddListener(OnPauseButtonClick);
        _restartButton.onClick.AddListener(OnRestartButtonClick);
        _continueButton.onClick.AddListener(OnContinueButtonClick);
        _timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        _timeSlider.wholeNumbers = true;

        UpdateMenuState();
    }

    private void UpdateMenuState()
    {
        _restartButton.gameObject.SetActive(!_isPaused);
        _pauseButton.gameObject.SetActive(!_isPaused);
        _labelRoot.SetActive(!_isPaused);
        
        _continueButton.gameObject.SetActive(_isPaused);
        _timeSlider.gameObject.SetActive(_isPaused);
    }

    private void OnPauseButtonClick()
    {
        Time.timeScale = 0;
        _isPaused = !_isPaused;
        
        if (_eventManager != null)
        {
            _eventManager.GamePaused.Invoke();
        }
        
        _timeSlider.maxValue = _timelineManager.TimeTick;
        _timeSlider.value = _timeSlider.maxValue;
        _timeSlider.minValue = 1;

        UpdateMenuState();
    }
    
    private void OnContinueButtonClick()
    {
        Time.timeScale = 1;
        _isPaused = !_isPaused;
        
        if (_eventManager != null)
        {
            _eventManager.GameResumed.Invoke();
        }

        UpdateMenuState();
    }

    private void OnRestartButtonClick()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnSliderValueChanged(float value)
    {
        _eventManager.TimeScrolled.Invoke(Mathf.RoundToInt(_timeSlider.value));
    }
}
