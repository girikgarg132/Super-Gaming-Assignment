using System;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private Vector2 _spawnDeltaBetweenEach;
    [SerializeField] private Vector2 _initialSpawnDistanceDelta;
    [SerializeField] private int _spawnProbability;
    [SerializeField] private int _poolSize;

    [Header("Enemy")]
    [SerializeField] private float[] _enemySpawnLocationsX;
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Obstacle")]
    [SerializeField] private float[] _obstacleSpawnLocationsX;
    [SerializeField] private GameObject _obstaclePrefab;

    private IObjectPool<GameObject> _enemyPool;
    private IObjectPool<GameObject> _obstaclePool;
    private Vector3[] _currentEnemySpawnLocation;
    private Vector3[] _currentObstacleSpawnLocation;

    private void Start()
    {
        _enemyPool = CreateObjectPool(_enemyPrefab, _poolSize * _enemySpawnLocationsX.Length);
        _obstaclePool = CreateObjectPool(_obstaclePrefab, _poolSize * _obstacleSpawnLocationsX.Length);

        _currentEnemySpawnLocation = new Vector3[_enemySpawnLocationsX.Length];
        _currentObstacleSpawnLocation = new Vector3[_obstacleSpawnLocationsX.Length];

        for (int i = 0; i < _enemySpawnLocationsX.Length; i++)
        {
            _currentEnemySpawnLocation[i] = new Vector3(_enemySpawnLocationsX[i], _player.transform.position.y, 0f);
        }

        for (int i = 0; i < _obstacleSpawnLocationsX.Length; i++)
        {
            _currentObstacleSpawnLocation[i] = new Vector3(_obstacleSpawnLocationsX[i], _player.transform.position.y, 0f);
        }

        for (int i = 0; i < _poolSize * _enemySpawnLocationsX.Length; i++)
        {
            SpawnEnemy();
        }
        for (int i = 0; i < _poolSize * _obstacleSpawnLocationsX.Length; i++)
        {
            SpawnObstacle();
        }
    }

    private IObjectPool<GameObject> CreateObjectPool(GameObject prefab, int poolSize)
    {
        return new ObjectPool<GameObject>(
            createFunc: () =>
                {
                    var obj = Instantiate(prefab, transform);
                    obj.SetActive(false);
                    return obj;
                },
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            maxSize: poolSize
        );
    }

    private float GetNextYPosition(float currentYPosition)
    {
        do
        {
            if (currentYPosition <= _player.transform.position.y)
            {
                currentYPosition = _player.transform.position.y +
                                   Random.Range(_initialSpawnDistanceDelta.x, _initialSpawnDistanceDelta.y);
            }
            else
            {
                currentYPosition += Random.Range(_spawnDeltaBetweenEach.x, _spawnDeltaBetweenEach.y);
            }
        } while (Random.Range(1, 101) > _spawnProbability);
        return currentYPosition;
    }

    public void SpawnEnemy()
    {
        GameObject enemy = _enemyPool.Get();
        if (enemy != null)
        {
            int randomLane = Random.Range(0, _currentEnemySpawnLocation.Length);
            _currentEnemySpawnLocation[randomLane].y = GetNextYPosition(_currentEnemySpawnLocation[randomLane].y);
            enemy.transform.position = _currentEnemySpawnLocation[randomLane];
            enemy.GetComponent<Object>().ObjectSpawner = this;
        }
    }

    public void SpawnObstacle()
    {
        GameObject obstacle = _obstaclePool.Get();
        if (obstacle != null)
        {
            int randomLane = Random.Range(0, _currentObstacleSpawnLocation.Length);
            _currentObstacleSpawnLocation[randomLane].y = GetNextYPosition(_currentObstacleSpawnLocation[randomLane].y);
            obstacle.transform.position = _currentObstacleSpawnLocation[randomLane];
            obstacle.GetComponent<Object>().ObjectSpawner = this;
        }
    }

    public void ReleaseEnemy(GameObject enemy)
    {
        _enemyPool.Release(enemy);
    }

    public void ReleaseObstacle(GameObject obstacle)
    {
        _obstaclePool.Release(obstacle);
    }

    private void OnDisable()
    {
        _enemyPool.Clear();
        _obstaclePool.Clear();
    }

    private void OnDestroy()
    {
        _enemyPool.Clear();
        _obstaclePool.Clear();
    }
}
