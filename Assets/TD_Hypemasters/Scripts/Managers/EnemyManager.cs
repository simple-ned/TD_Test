using System;
using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnTransform;
    [SerializeField] private Transform _destination;
    [SerializeField] private float _waitBeforeWave;
    [SerializeField] private float _timeInterval;
    [SerializeField] private int _maximumCount;
    [SerializeField] private bool _infinitySpawn;

    private int _spawnCount;

    private void Awake()
    {
        GameManager.Instance.RegisterManager(this);
    }

    void Start()
    {
        _spawnCount = 1;
        StartCoroutine(SpawnRoutine());

        EventManager e = GameManager.Instance.GetManager<EventManager>();
        
        if (e != null)
        {
            e.MouseClickOnRoad.AddListener(OnMouseClickOnRoad);
        }
    }


    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(_waitBeforeWave);
        
        while (_infinitySpawn || _spawnCount <= _maximumCount)
        {
            SpawnEnemy(_spawnTransform.position);
            
            yield return new WaitForSeconds(_timeInterval);
        }
    }

    public void OnMouseClickOnRoad(Vector3 position)
    {
        SpawnEnemy(position);
    }

    public GoblinBehaviour SpawnEnemy(Vector3 position)
    {
        GameObject go = Instantiate(_prefab, position, Quaternion.identity);
        GoblinBehaviour goblin = go.GetComponent<GoblinBehaviour>();
        goblin.Initialize(_enemyData.ObjectName, _enemyData.Speed, _enemyData.Health, _destination);
        _spawnCount++;

        return goblin;
    }
}
