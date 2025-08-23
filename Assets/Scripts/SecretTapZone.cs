// using UnityEngine;

// public class CheatManager : MonoBehaviour
// {
//     public static CheatManager Instance { get; private set; }
//     public bool CheatActive { get; private set; }

//     void Awake()
//     {
//         if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//     }

//     public void ToggleCheat() => CheatActive = !CheatActive;
// }
