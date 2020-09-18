using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectsTimelineManager : MonoBehaviour
{
    private const int MAXIMUM_FRAMES = 10000; // Максимальное количество фреймов (состояний игры), которое будет хранить наш timeline
    
    private int _timeTick;
    private float _timeInterval; // Как часто сохранять состояния
    private EventManager _eventManager;
    private EnemyManager _enemyManager;
    private bool _isPaused;
    private int _objectCount;

    // Массив хранит фреймы, в свою очередь фрейм это словарь из global_id и состояний каждого объекта
    private Dictionary<int, object>[] _timeline;
    
    // Список "живых" объектов на сцене в данный момент
    private Dictionary<int, SaveableBehaviour> _currentObjects;

    public int TimeTick => _timeTick;
    public bool IsManipulatingTime => !_isPaused;
    
    public GameObject _bulletPrefab;

    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);
        
        _timeline = new Dictionary<int, object>[MAXIMUM_FRAMES];
        _currentObjects = new Dictionary<int, SaveableBehaviour>();
        
        // Пусть будет раз в 0.1 секунды. Это позволит сохранять до 16,6 минут игрового процесса
        _timeInterval = 0.1f;
        _isPaused = false;
        _objectCount = 0;
    }
    
    private void Start()
    {
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        if (_eventManager != null)
        {
            _eventManager.GamePaused.AddListener(OnGamePause);
            _eventManager.GameResumed.AddListener(OnGameResume);
            _eventManager.TimeScrolled.AddListener(OnTimeScrolled);
        }

        _enemyManager = GameManager.Instance.GetManager<EnemyManager>();

        StartCoroutine(CollectStatesRoutine());
    }

    private IEnumerator CollectStatesRoutine()
    {
        do
        {
            Dictionary<int, object> frame = new Dictionary<int, object>();
            
            frame.Clear();
            
            _eventManager.GameTimeIncreased.Invoke();

            foreach (SaveableBehaviour obj in _currentObjects.Values)
            {
                object state = obj.GetState();

                if (state != null && !frame.ContainsKey(obj.GlobalId))
                {
                    frame.Add(obj.GlobalId, state);
                }
            }

            _timeline[_timeTick] = frame;
            
            _timeTick++;
            
            yield return new WaitForSeconds(_timeInterval);
            
        } while (!_isPaused && _timeTick < MAXIMUM_FRAMES);    // Пока пауза происходит через timeScale = 0, то корутина и без этой проверки остановится. Но всё может поменяться
    }

    private void OnGamePause()
    {
        _isPaused = true;
    }
    
    private void OnGameResume()
    {
        _isPaused = false;
    }
    
    public void OnTimeScrolled(int time)
    {
        // Восстанавливаем состояние на данное время
        if (_timeline.Length < time || _timeline[time] == null || time == _timeTick)
        {
            return;
        }

        Dictionary<int, object> selectedFrame = _timeline[time];
        
        // Сначала удалим ненужные объекты
        foreach (int key in _currentObjects.Keys.ToList()) // Маленький лайфхак, чтобы не нарваться на эксепшн о попытке изменения коллекции в foreach
        {
            if (!selectedFrame.ContainsKey(key))
            {
                Destroy(_currentObjects[key].gameObject);
                _currentObjects.Remove(key);
            }
        }
        
        foreach (KeyValuePair<int, object> kvp in selectedFrame)
        {
            if (!_currentObjects.ContainsKey(kvp.Key))
            {
                SaveableBehaviour b = null;
                
                switch (SaveableBehaviour.DefineType(kvp.Value))
                {
                    case ObjectType.Goblin:
                        if (_enemyManager != null)
                        {
                            b = _enemyManager.SpawnEnemy(Vector3.zero);
                        }
                        break;
                    case ObjectType.Cannon: 
                        // Напомню, пушка в динамике не создаётся.
                        break;
                    case ObjectType.Bullet:
                        // На данный момент тут отсутствует что-то типа BulletSpawnera, поэтому приходится создавать вручную
                        // Но в теории можно, например, обращаться к скрипту типа CannonBehaviour и дёргать
                        // у него метод CreateBullet, который вернёт gameObject пули
                        GameObject go = GameObject.Instantiate(_bulletPrefab);
                        b = go.GetComponent<SaveableBehaviour>();
                        break;
                    default: Debug.LogError("Saved data for id " + kvp.Key + "can't be defined!");
                        break;
                }

                if (b != null)
                {
                    b.GlobalId = kvp.Key;
                    _currentObjects.Add(kvp.Key, b);
                }
            }
        }
        
        // Сделаем сначала SetState
        foreach (KeyValuePair<int,SaveableBehaviour> pair in _currentObjects)
        {
            pair.Value.SetState(selectedFrame[pair.Key]);
        }
        
        // Ииии LateSetState
        foreach (SaveableBehaviour behaviour in _currentObjects.Values)
        {
            behaviour.LateSetState();
        }

        _timeTick = time;
    }

    public void RegisterObject(SaveableBehaviour obj)
    {
        obj.Init(_objectCount++);
        
        if (!_currentObjects.ContainsKey(obj.GlobalId))
        {
            _currentObjects.Add(obj.GlobalId, obj);
        }

        Debug.Log(obj.name + " was registered with id " + obj.GlobalId);
    }

    public void UnregisterObject(SaveableBehaviour obj)
    {
        if (_currentObjects.ContainsKey(obj.GlobalId))
        {
            _currentObjects.Remove(obj.GlobalId);
        }
        
        Debug.Log(obj.name + " was unregistered with id " + obj.GlobalId);
    }

    public GameObject GetGameObjectByGlobalId(int targetGlobalId)
    {
        if (_currentObjects.ContainsKey(targetGlobalId))
        {
            return _currentObjects[targetGlobalId].gameObject;
        }

        Debug.LogError("Object with id " + targetGlobalId + "not exist.");
        return null;
    }

    private void OnDestroy()
    {
        if (_eventManager != null)
        {
            _eventManager.GamePaused.RemoveListener(OnGamePause);
            _eventManager.GameResumed.RemoveListener(OnGameResume);
            _eventManager.TimeScrolled.RemoveListener(OnTimeScrolled);
        }
    }
}
