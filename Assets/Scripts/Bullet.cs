using System;
using System.Threading.Tasks;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _timeToReach;

    public async void Shoot(Vector3 startPosition, Transform target, Sprite targetSprite, Action onComplete)
    {
        _spriteRenderer.sprite = targetSprite;
        for (float time = 0f; time < _timeToReach; time += 0.05f)
        {
            transform.position = Vector3.Lerp(startPosition, target.position, time / _timeToReach);
            await Task.Delay(50);
        }
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
    
}
