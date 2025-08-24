using UnityEngine;
using AdiveryUnity;

public class AdiveryAdsManager : MonoBehaviour
{
    public static AdiveryAdsManager Instance;

    [Header("Adivery Settings")]
    public string appId = "YOUR_APP_ID";
    public string interstitialPlacement = "INTERSTITIAL_PLACEMENT";
    public string rewardedPlacement = "REWARDED_PLACEMENT";

    private AdiveryListener listener;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize SDK
        Adivery.Configure(appId);

        // Setup Listener
        listener = new AdiveryListener();
        listener.OnError += OnError;
        listener.OnInterstitialAdLoaded += OnInterstitialLoaded;
        listener.OnRewardedAdLoaded += OnRewardedLoaded;
        listener.OnRewardedAdClosed += OnRewardedClosed;
        Adivery.AddListener(listener);

        // Prepare ads
        PrepareInterstitial();
        PrepareRewarded();
    }

    #region Interstitial Ads
    public void PrepareInterstitial()
    {
        Adivery.PrepareInterstitialAd(interstitialPlacement);
    }

    public void ShowInterstitial()
    {
        if (Adivery.IsLoaded(interstitialPlacement))
            Adivery.Show(interstitialPlacement);
        else
            Debug.Log("Interstitial not loaded yet!");
    }

    private void OnInterstitialLoaded(object caller, string placementId)
    {
        Debug.Log("Interstitial loaded: " + placementId);
    }
    #endregion

    #region Rewarded Ads
    public void PrepareRewarded()
    {
        Adivery.PrepareRewardedAd(rewardedPlacement);
    }

    public void ShowRewarded(System.Action rewardCallback)
    {
        if (Adivery.IsLoaded(rewardedPlacement))
        {
            rewardedAction = rewardCallback;
            Adivery.Show(rewardedPlacement);
        }
        else
            Debug.Log("Rewarded ad not loaded yet!");
    }

    private System.Action rewardedAction;

    private void OnRewardedLoaded(object caller, string placementId)
    {
        Debug.Log("Rewarded loaded: " + placementId);
    }

    private void OnRewardedClosed(object caller, AdiveryReward reward)
    {
        if (reward.IsRewarded && rewardedAction != null)
        {
            rewardedAction.Invoke();
        }

        PrepareRewarded(); // Auto-load next rewarded ad
    }
    #endregion

    private void OnError(object caller, AdiveryError error)
    {
        Debug.Log($"Adivery error - Placement: {error.PlacementId}, Reason: {error.Reason}");
    }
}
