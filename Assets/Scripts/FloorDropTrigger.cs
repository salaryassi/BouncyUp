using UnityEngine;

public class FloorDropTrigger : MonoBehaviour
{
    GameManager gm;

    void Awake() => gm = FindObjectOfType<GameManager>();

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("FloorDropTrigger activated");    
        if (other.attachedRigidbody && other.attachedRigidbody.CompareTag("Ball"))
        {
            gm.OnBallDropped();
        }
    }
}
