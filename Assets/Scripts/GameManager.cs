using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public enum SfxType { Bounce, Powerup, LoseLife, Click }
public enum PowerupType { DoubleScore, SlowTime, Shield }

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Text scoreText;
    [SerializeField] Text livesText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button adReviveButton;
    [SerializeField] Button iapButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;

    [Header("Gameplay")]
    [SerializeField] int startingLives = 30
    ;
    [SerializeField] Rigidbody2D ballRb;
    [SerializeField] SpriteRenderer ballSprite;

    [SerializeField] Image CourtSprite;
    [SerializeField] Sprite footballSprite;
    [SerializeField] Sprite basketballSprite;
    [SerializeField] Sprite tennisSprite;
     [SerializeField] Sprite footballCourtSprite;
    [SerializeField] Sprite basketballCourtSprite;
    [SerializeField] Sprite tennisCourtSprite;
    [SerializeField] GameObject dropEffectPrefab;

    [Header("Difficulty")]
    [SerializeField] float gravityBase = 1.5f;
    [SerializeField] float gravityPer10Score = 0.15f;

    [Header("Audio")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip sfxBounce;
    [SerializeField] AudioClip sfxPower;
    [SerializeField] AudioClip sfxLoseLife;
    [SerializeField] AudioClip sfxClick;
    [SerializeField] AudioClip musicStage1;
    [SerializeField] AudioClip musicStage2;
    [SerializeField] AudioClip musicStage3;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    bool doubleScore;
    bool shield;

    void Start()
    {
        Lives = startingLives;
        Score = 0;
        UpdateHUD();

        ballSprite.sprite = footballSprite; // start with football
        CourtSprite.sprite = footballCourtSprite;
        if (musicStage1 != null)
        {
            musicSource.clip = musicStage1;
            musicSource.loop = true;
            musicSource.Play();
        }

        UpdateGravity();

        // Setup GameOver menu buttons
        if (adReviveButton) adReviveButton.onClick.AddListener(ReviveWithAd);
        if (iapButton) iapButton.onClick.AddListener(BuyIAP);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (menuButton) menuButton.onClick.AddListener(GoToMenu);

        gameOverPanel.SetActive(false);
    }

    // Called by BallController when player bounces correctly
    public void OnSuccessfulJuggle()
    {
        Score += doubleScore ? 2 : 1;
        UpdateHUD();
        UpdateGravity();
        HandleMilestones();
    }

    // Called when ball hits the floor
    public void OnBallDropped()
    {
        if (shield)
        {
            shield = false;
            PlaySfx(SfxType.LoseLife);
            return;
        }

        if (CheatManager.Instance != null && CheatManager.Instance.CheatActive)
        {
            return;
        }

        Lives--;
        PlaySfx(SfxType.LoseLife);
        UpdateHUD();

        //  CameraShake.Instance?.DoShake();
        SpawnDropEffect(ballRb.position);

        if (Lives <= 0)
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }
        else
        {
            ballRb.gameObject.SetActive(false);
            StartCoroutine(CoRespawnBall());
        }
    }

    IEnumerator CoRespawnBall()
    {
        yield return new WaitForSeconds(1f);
        float x = UnityEngine.Random.Range(-3f, 3f);
        ballRb.transform.position = new Vector2(x, 4.5f);

        ballRb.linearVelocity = Vector2.zero;
        ballRb.AddForce(Vector2.up * 6f, ForceMode2D.Impulse);

        ballRb.gameObject.SetActive(true);
    }

    void SpawnDropEffect(Vector2 pos)
    {
        if (dropEffectPrefab != null)
            Instantiate(dropEffectPrefab, pos, Quaternion.identity);
    }

    void UpdateHUD()
    {
        scoreText.text = Score.ToString();
        livesText.text = $"x{Lives}";
    }

    void UpdateGravity()
    {
        float g = gravityBase + (Score / 10) * gravityPer10Score;
        Physics2D.gravity = new Vector2(0, -9.81f * g);
    }

    void HandleMilestones()
    {
        if (Score == 20)
        {
            ballSprite.sprite = basketballSprite;
            CourtSprite.sprite = basketballCourtSprite;
            SwapMusic(musicStage2);
        }
        else if (Score == 40)
        {
            ballSprite.sprite = tennisSprite;
            CourtSprite.sprite = tennisCourtSprite; 
            SwapMusic(musicStage3);
        }
    }

    void SwapMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    // Powerups
    public void ApplyPowerup(PowerupType type, float duration)
    {
        switch (type)
        {
            case PowerupType.DoubleScore:
                StartCoroutine(CoTimedFlag(duration, v => doubleScore = v));
                break;
            case PowerupType.SlowTime:
                StartCoroutine(CoSlowTime(duration));
                break;
            case PowerupType.Shield:
                StartCoroutine(CoTimedFlag(duration, v => shield = v));
                break;
        }
        PlaySfx(SfxType.Powerup);
    }

    IEnumerator CoTimedFlag(float duration, Action<bool> setFlag)
    {
        setFlag(true);
        yield return new WaitForSeconds(duration);
        setFlag(false);
    }

    IEnumerator CoSlowTime(float duration)
    {
        Time.timeScale = 0.6f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void PlaySfx(SfxType t)
    {
        var clip = t switch
        {
            SfxType.Bounce => sfxBounce,
            SfxType.Powerup => sfxPower,
            SfxType.LoseLife => sfxLoseLife,
            _ => sfxClick
        };
        if (clip != null) { sfxSource.PlayOneShot(clip); }
    }

    // ========== GAME OVER MENU BUTTONS ==========

    void ReviveWithAd()
    {
        Debug.Log("TODO: Show rewarded ad here...");

        // If ad watched successfully:
        Time.timeScale = 1f;
        Lives = 1;
        UpdateHUD();
        gameOverPanel.SetActive(false);

        // Respawn ball
        ballRb.gameObject.SetActive(false);
        StartCoroutine(CoRespawnBall());
    }

    void BuyIAP()
    {
        Debug.Log("TODO: Trigger Unity IAP purchase here...");
        // Example: Remove ads, give special ball, etc.
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Make sure you have a MainMenu scene
    }
}
