using UnityEngine;
using UnityEngine.UI;
using System;

public enum SfxType { Bounce, Powerup, LoseLife, Click }
public enum PowerupType { DoubleScore, SlowTime, Shield }

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Text scoreText;
    [SerializeField] Text livesText;
    [SerializeField] GameObject gameOverPanel;

    [Header("Gameplay")]
    [SerializeField] int startingLives = 3;
    [SerializeField] Rigidbody2D ballRb;
    [SerializeField] SpriteRenderer ballSprite;
    [SerializeField] Sprite footballSprite;
    [SerializeField] Sprite basketballSprite;
    [SerializeField] Sprite tennisSprite;

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
        UpdateHUD();
        ballSprite.sprite = footballSprite; // start with football
        musicSource.clip = musicStage1;
        musicSource.loop = true;
        musicSource.Play();
        UpdateGravity();
    }

    public void OnSuccessfulJuggle()
    {
        Score += doubleScore ? 2 : 1;
        UpdateHUD();
        UpdateGravity();
        HandleMilestones();
    }

    public void OnBallDropped()
    {
        if (shield)
        {
            shield = false;
            PlaySfx(SfxType.LoseLife); // light feedback but no life loss
            return;
        }

        if (CheatManager.Instance.CheatActive)
        {
            // No life loss during cheat mode (hidden continue)
            return;
        }

        Lives--;
        PlaySfx(SfxType.LoseLife);
        UpdateHUD();

        if (Lives <= 0)
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }
        else
        {
            // Soft reset: nudge ball up
            ballRb.linearVelocity = Vector2.zero;
            ballRb.AddForce(Vector2.up * 6.0f, ForceMode2D.Impulse);
        }
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
            ballSprite.sprite = basketballSprite; // stage 2
            SwapMusic(musicStage2);
        }
        else if (Score == 40)
        {
            ballSprite.sprite = tennisSprite; // stage 3
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

    System.Collections.IEnumerator CoTimedFlag(float duration, Action<bool> setFlag)
    {
        setFlag(true);
        yield return new WaitForSeconds(duration);
        setFlag(false);
    }

    System.Collections.IEnumerator CoSlowTime(float duration)
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
}
