using TMPro;
using UnityEngine;

public class LabelCounters : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _killedLabel;
    [SerializeField] private TextMeshProUGUI _missedLabel;
    [SerializeField] private TextMeshProUGUI _timeCountLabel;
    
    private StatisticManager _statistic;
    private ObjectsTimelineManager _timelineManager;
    
    void Start()
    {
        _statistic = GameManager.Instance.GetManager<StatisticManager>();
        _timelineManager = GameManager.Instance.GetManager<ObjectsTimelineManager>();

        if (_statistic == null)
        {
            Debug.LogError("Failed to get StatisticManager!");
        }

        EventManager e = GameManager.Instance.GetManager<EventManager>();

        if (e != null)
        {
            e.StatisticChanged.AddListener(OnUnitKilled);
            e.StatisticChanged.AddListener(OnUnitMissed);
            e.GameTimeIncreased.AddListener(OnTimeIncreased);
            e.GameResumed.AddListener(OnGameResumed);
        }
    }

    public void OnUnitKilled()
    {
        UpdateKilledLabel();
    }

    public void OnUnitMissed()
    {
        UpdateMissedLabel();
    }

    public void OnTimeIncreased()
    {
        UpdateTimeLabel();
    }

    public void OnGameResumed()
    {
        // Обновим показатели статистики, они могли измениться во время перемотки времени
        UpdateKilledLabel();
        UpdateMissedLabel();
        UpdateTimeLabel();
    }

    private void OnDestroy()
    {
        EventManager e = GameManager.Instance.GetManager<EventManager>();

        if (e != null)
        {
            e.StatisticChanged.RemoveListener(OnUnitKilled);
            e.StatisticChanged.RemoveListener(OnUnitMissed);
            e.GameTimeIncreased.RemoveListener(OnTimeIncreased);
            e.GameResumed.RemoveListener(OnGameResumed);
        }
    }

    private void UpdateMissedLabel()
    {
        if (_missedLabel != null)
        {
            _missedLabel.text = _statistic.UnitsMissed.ToString();
        }
        
        else
        {
            Debug.LogError("TextMeshPro component didn't found");
        }
    }

    private void UpdateKilledLabel()
    {
        if (_killedLabel != null)
        {
            _killedLabel.text = _statistic.UnitsKilled.ToString();
        }
        
        else
        {
            Debug.LogError("TextMeshPro component didn't found");
        }
    }

    private void UpdateTimeLabel()
    {
        if (_timeCountLabel != null)
        {
            _timeCountLabel.text = _timelineManager.TimeTick.ToString();
        }
        
        else
        {
            Debug.LogError("TextMeshPro component didn't found");
        }
    }
}
