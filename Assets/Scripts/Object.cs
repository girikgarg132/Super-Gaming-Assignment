using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Object : MonoBehaviour
{
    public ObjectSpawner ObjectSpawner;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _shapeSprites;
    [SerializeField] private bool _isEnemy;

    private void OnEnable()
    {
        _spriteRenderer.sprite = _shapeSprites[Random.Range(0, _shapeSprites.Length)];
    }

    private void OnBecameVisible()
    {
        if (!_isEnemy) return;
        PlayerController.VisibleEnemySpriteRenderers.Add(_spriteRenderer);
    }

    private void OnBecameInvisible()
    {
        if (_isEnemy) return;
        ObjectSpawner.ReleaseObstacle(gameObject);
        ObjectSpawner.SpawnObstacle();
    }

    private void OnDisable()
    {
        if (!_isEnemy) return;
        PlayerController.VisibleEnemySpriteRenderers.Remove(_spriteRenderer);
    }
}
