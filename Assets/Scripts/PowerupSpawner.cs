using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] Powerup[] powerupPrefabs;
    [SerializeField] float spawnEveryMin = 6f;
    [SerializeField] float spawnEveryMax = 12f;
    [SerializeField] Vector2 xRange = new(-3.2f, 3.2f);
    [SerializeField] Vector2 yRange = new(-1f, 2.8f);

    void Start() => Invoke(nameof(Spawn), Random.Range(spawnEveryMin, spawnEveryMax));

    void Spawn()
    {
        if (powerupPrefabs.Length == 0) return;
        var p = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];
        var pos = new Vector3(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y), 0);
        Instantiate(p, pos, Quaternion.identity, transform);
        Invoke(nameof(Spawn), Random.Range(spawnEveryMin, spawnEveryMax));
    }
}
