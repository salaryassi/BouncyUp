using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [SerializeField] float initialUpImpulse = 6.5f;
    [SerializeField] float paddleBounceBoost = 1.15f;
    [SerializeField] float maxSpeed = 14f;

    Rigidbody2D rb;
    GameManager gm;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
    }

    void Start()
    {
        rb.AddForce(Vector2.up * initialUpImpulse, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Paddle"))
        {
            // Add a little extra vertical velocity; slight horizontal randomness
            var v = rb.linearVelocity;
            v.y = Mathf.Abs(v.y) * paddleBounceBoost + 0.5f;
            v.x += Random.Range(-0.8f, 0.8f);
            v = Vector2.ClampMagnitude(v, maxSpeed);
            rb.linearVelocity = v;

            gm.OnSuccessfulJuggle();
            gm.PlaySfx(SfxType.Bounce);
        }
    }
}
