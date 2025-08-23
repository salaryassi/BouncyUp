using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [SerializeField] float tapImpulse = 7f;
    [SerializeField] float tapRadius = 1.2f; // how close finger must be
    [SerializeField] float maxSpeed = 15f;

    Rigidbody2D rb;
    GameManager gm;
    Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
        cam = Camera.main;
    }

    void Start()
    {
        // Small initial toss so it falls naturally
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began)
                {
                    HandleTap(t.position);
                }
            }
        }

#if UNITY_EDITOR
        // mouse for testing in editor
        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }
#endif
    }

    void HandleTap(Vector2 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));

        // Check if tap is close under the ball
        float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.y), rb.position);
        if (dist <= tapRadius && worldPos.y <= rb.position.y)
        {
            Vector2 impulse = (Vector2.up * tapImpulse) + new Vector2(Random.Range(-1f, 1f), 0);
            rb.AddForce(impulse, ForceMode2D.Impulse);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

            gm.OnSuccessfulJuggle();
            gm.PlaySfx(SfxType.Bounce);
        }
    }
}
