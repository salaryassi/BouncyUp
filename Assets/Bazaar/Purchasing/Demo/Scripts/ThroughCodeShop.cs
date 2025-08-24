using UnityEngine;
using UnityEngine.Purchasing;

public class ThroughCodeShop : MonoBehaviour, IStoreListener
{
    private IStoreController m_StoreController;

    [Header("Music Unlock")]
    [SerializeField] AudioClip premiumMusic;  // assign in Inspector
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Register products (IDs must match Bazaar panel)
        builder.AddProduct("premium_music", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("Bazaar IAP initialized");
        m_StoreController = controller;
    }

    // Required (legacy signature)
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning("IAP init failed (legacy): " + error);
    }

    // Required (new signature for Unity IAP 4.6+)
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP init failed: {error}, details: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("Purchase complete: " + args.purchasedProduct.definition.id);

        if (args.purchasedProduct.definition.id == "premium_music")
        {
            Debug.Log("✅ Premium music unlocked!");
            if (gameManager != null && premiumMusic != null)
            {
                gameManager.SwapMusic(premiumMusic);
            }
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, reason: {failureReason}");
    }

    // Call this from your Buy Button
    public void BuyProduct(string productId)
    {
        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(productId);
        }
        else
        {
            Debug.LogWarning("Purchase failed: IAP not initialized yet");
        }
    }
}
