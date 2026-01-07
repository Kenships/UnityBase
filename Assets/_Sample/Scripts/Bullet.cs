using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    [SerializeField] private float bulletSpeed = 100f;
    [SerializeField] private float maxDistanceSquared = 10000f;
    private Vector3 _direction;
    private Vector3 _originalPosition;
    private void Start()
    {
        _direction = transform.up.normalized;
        _originalPosition = transform.position;
    }

    void Update()
    {
        if ((transform.position - _originalPosition).sqrMagnitude > maxDistanceSquared)
        {
            Destroy(gameObject);
        }
        transform.position += _direction * (bulletSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
