using UnityEngine;

public class Powerup : MonoBehaviour
{
    public PowerupType type;
    public float duration = 6f;
    [SerializeField] float floatSpeed = 0.5f;

    void Update()
    {
        transform.position += Vector3.up * Mathf.Sin(Time.time * 2f) * floatSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Ball"))
        {
            FindObjectOfType<GameManager>().ApplyPowerup(type, duration);
            Destroy(gameObject);
        }
    }
}
