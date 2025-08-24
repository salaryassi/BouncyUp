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
    [SerializeField] Text highScoreText; // NEW: Text to show the high score on the game over panel.
    [SerializeField] GameObject newHighScoreCelebration; // NEW: An object (like text or an image) to show when a new record is set.

    [Header("Gameplay")]
    [SerializeField] int startingLives = 3;
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

    [Header("Effects & Rewards")] // NEW: Section for new gameplay elements
    [SerializeField] GameObject comboEffectPrefab; // NEW: Particle effect for a successful combo.
    [SerializeField] int comboThreshold = 5; // NEW: How many juggles are needed for a combo reward.

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
    [SerializeField] AudioClip premiumMusic;
    [SerializeField] AdiveryAdsManager AdiveryAdsManager;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    bool doubleScore;
    bool shield;

    // NEW: Variables for combo and high score tracking
    private int comboCount;
    private int highScore;

    void Start()
    {
        Lives = startingLives;
        Score = 0;
        comboCount = 0; // NEW: Initialize combo count.

        // NEW: Load the high score from device storage. Defaults to 0 if not found.
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        UpdateHUD();

        ballSprite.sprite = footballSprite;
        CourtSprite.sprite = footballCourtSprite;
        if (musicStage1 != null)
        {
            musicSource.clip = musicStage1;
            musicSource.loop = true;
            musicSource.Play();
        }

        UpdateGravity();

        if (adReviveButton) adReviveButton.onClick.AddListener(ReviveWithAd);
        if (iapButton) iapButton.onClick.AddListener(BuyIAP);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (menuButton) menuButton.onClick.AddListener(GoToMenu);

        gameOverPanel.SetActive(false);
        if (newHighScoreCelebration != null) newHighScoreCelebration.SetActive(false); // NEW: Hide the celebration message at start.
    }

    // Called by BallController when player bounces correctly
    public void OnSuccessfulJuggle()
    {
        Score += doubleScore ? 2 : 1;
        comboCount++; // NEW: Increase combo counter.

        // NEW: Check if the player has reached the combo threshold.
        if (comboCount >= comboThreshold)
        {
            comboCount = 0; // Reset for the next combo.
            Lives++; // Award an extra life!
            
            // Play the particle effect at the ball's position.
            if (comboEffectPrefab != null)
            {
                Instantiate(comboEffectPrefab, ballRb.transform.position, Quaternion.identity);
            }
        }

        UpdateHUD();
        UpdateGravity();
        HandleMilestones();
    }

    // Called when ball hits the floor
    public void OnBallDropped()
    {
        comboCount = 0; // NEW: Reset the combo count when the ball is dropped.

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

        SpawnDropEffect(ballRb.position);

        if (Lives <= 0)
        {
            Time.timeScale = 0f;
            CheckForHighScore(); // NEW: Check and update the high score before showing the panel.
            gameOverPanel.SetActive(true);
        }
        else
        {
            ballRb.gameObject.SetActive(false);
            StartCoroutine(CoRespawnBall());
        }
    }

    // NEW: This method checks the score and updates the high score if needed.
    void CheckForHighScore()
    {
        if (Score > highScore)
        {
            highScore = Score;
            PlayerPrefs.SetInt("HighScore", highScore); // Save the new high score.
            PlayerPrefs.Save(); // Ensure it's written to disk.
            
            // Update the UI text to show it's a new record.
            highScoreText.text = $"New Record: {highScore}!";
            
            // Show the celebration object!
            if (newHighScoreCelebration != null) newHighScoreCelebration.SetActive(true);
        }
        else
        {
            // Just display the existing record.
            highScoreText.text = $"Record: {highScore}";
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
        }
        else if (Score == 40)
        {
            ballSprite.sprite = tennisSprite;
            CourtSprite.sprite = tennisCourtSprite;
        }
    }

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
        AdiveryAdsManager.Instance.ShowRewarded(() =>
        {
            Time.timeScale = 1f;
            Lives = 1;
            UpdateHUD();
            gameOverPanel.SetActive(false);
            ballRb.gameObject.SetActive(false);
            StartCoroutine(CoRespawnBall());
        });
    }

    void BuyIAP()
    {
        Debug.Log("TODO: Trigger Unity IAP purchase here...");
        SwapMusic(premiumMusic);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void SwapMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("🎵 Music swapped to premium track!");
    }
}