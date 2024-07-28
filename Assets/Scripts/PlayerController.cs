using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerController : MonoBehaviour
{
    public static List<SpriteRenderer> VisibleEnemySpriteRenderers;
    
    public float CurrentSpeed { get; private set; }
    
    [SerializeField] private Transform[] _objectsToMoveForward;
    [SerializeField] private ObjectSpawner _objectSpawner;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _yThresholdForSpeedIncrease;
    [SerializeField] private float _speedIncrease;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _timeLagBeforeShooting;
    [SerializeField] private float _timeBetweenEachShot;
    [SerializeField] private int _bulletPoolSize;
    
    [Header("Body")]
    [SerializeField] private Sprite[] _upperBodySprites;
    [SerializeField] private SpriteRenderer _upperBodySpriteRenderer;
    [SerializeField] private Sprite[] _lowerBodySprites;
    [SerializeField] private SpriteRenderer _lowerBodySpriteRenderer;

    [Space(15)] [SerializeField] private UI _UI;

    [Space(15)] [SerializeField] private Color[] _colorToSwitch;

    private Camera _mainCamera;
    private IObjectPool<Bullet> _bulletPool;
    private CancellationTokenSource _cancellationTokenSource;
    private int _upperBodySpriteIndex;
    private int _lowerBodySpriteIndex;
    private int _backgroundColorIndex;

    private void Awake()
    {
        VisibleEnemySpriteRenderers = new List<SpriteRenderer>();
    }

    private void Start()
    {
        CurrentSpeed = _moveSpeed;
        _bulletPool = CreateObjectPool(_bulletPrefab, _bulletPoolSize);
        _mainCamera = Camera.main;
        _mainCamera.backgroundColor = _colorToSwitch[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnClickUpperBodyChange();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnClickLowerBodyChange();
        }
        
        int noOfThresholdReached = (int)(transform.position.y / _yThresholdForSpeedIncrease);
        int noOfTimesSpeedAlreadyIncreased = (int)((CurrentSpeed - _moveSpeed) / _speedIncrease);
        if (noOfThresholdReached != noOfTimesSpeedAlreadyIncreased && CurrentSpeed < _maxSpeed)
        {
            CurrentSpeed += _speedIncrease;
            _backgroundColorIndex++;
            if (_backgroundColorIndex == _colorToSwitch.Length)
            {
                _backgroundColorIndex = 0;
            }
            _mainCamera.backgroundColor = _colorToSwitch[_backgroundColorIndex];
        }
        foreach (Transform objectToMoveForward in _objectsToMoveForward)
        {
            objectToMoveForward.position += Vector3.up * (CurrentSpeed * Time.deltaTime);
        }
    }
    
    private IObjectPool<Bullet> CreateObjectPool(Bullet prefab, int poolSize)
    {
        return new ObjectPool<Bullet>(
            createFunc: () =>
                {
                    Bullet obj = Instantiate(prefab);
                    obj.gameObject.SetActive(false);;
                    return obj;
                },
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Destroy(obj.gameObject),
            maxSize: poolSize
        );
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        GameOver();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.ENEMY))
        {
            GameOver();
            return;
        }
        if (!other.CompareTag(Tags.OBSTACLE)) return;
        if (other.GetComponent<SpriteRenderer>().sprite != _lowerBodySpriteRenderer.sprite)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        int currentScore = (int)transform.position.y;
        if (currentScore > Prefs.HighScore)
        {
            Prefs.HighScore = currentScore;
        }
        _UI.GameOver(currentScore);
    }

    public void OnClickUpperBodyChange()
    {
        CancelInvoke();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _upperBodySpriteIndex = (_upperBodySpriteIndex + 1) % _upperBodySprites.Length;
        _upperBodySpriteRenderer.sprite = _upperBodySprites[_upperBodySpriteIndex];
        Invoke(nameof(GetAndShootAllVisibleTargets), _timeLagBeforeShooting);
    }

    async Task GetAndShootAllVisibleTargets()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        List<SpriteRenderer> targets = new List<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in VisibleEnemySpriteRenderers)
        {
            if (spriteRenderer.sprite == _upperBodySpriteRenderer.sprite)
            {
                targets.Add(spriteRenderer);
            }
        }
        await Task.Run(async () =>
        {
            foreach (SpriteRenderer spriteRenderer in targets)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Bullet bullet = _bulletPool.Get();
                    bullet.Shoot(transform.position, spriteRenderer.transform, spriteRenderer.sprite, () =>
                    {
                        _bulletPool?.Release(bullet);

                        if (spriteRenderer != null && spriteRenderer.gameObject.activeInHierarchy)
                        {
                            spriteRenderer.gameObject.SetActive(false);
                        }

                        VisibleEnemySpriteRenderers?.Remove(spriteRenderer);

                        if (_objectSpawner != null)
                        {
                            _objectSpawner.ReleaseEnemy(spriteRenderer.gameObject);
                            _objectSpawner.SpawnEnemy();
                        }
                    });
                });
                await Task.Delay((int)(_timeBetweenEachShot * 1000));
            }
        }, _cancellationTokenSource.Token);
    }

    public void OnClickLowerBodyChange()
    {
        _lowerBodySpriteIndex = (_lowerBodySpriteIndex + 1) % _lowerBodySprites.Length;
        _lowerBodySpriteRenderer.sprite = _lowerBodySprites[_lowerBodySpriteIndex];
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource?.Dispose();
        _bulletPool.Clear();
    }
}
