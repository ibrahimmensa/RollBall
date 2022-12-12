using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RewardedAdType
{
    FREECOINS,
    REVIVE,
    UNLOCKBALL,
    DOUBLEREWARD,
    RESUME
}

public class AdsInitializer : Singleton<AdsInitializer>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    public string _gameId;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    [SerializeField] string _androidAdUnitIdInterstitial = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitIdInterstitial = "Interstitial_iOS";
    public RewardedAdType currentAdType;
    string _adUnitId = null; // This will remain null for unsupported platforms
    string _adUnitIdInterstitial = null;
    public StoreManager storeManager;
    public override void  Awake()
    {
        if(AdsInitializer.Instance  != this)
        {
            Destroy(gameObject);
        }
        Debug.Log("Initializing");
        DontDestroyOnLoad(this);
        InitializeAds();
    }

    public void InitializeAds()
    {

        
        _gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOSGameId
            : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode, this);
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
        Debug.Log("Android initializing");
#endif

        _adUnitIdInterstitial = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitIdInterstitial
            : _androidAdUnitIdInterstitial;
    }


    #region RewardedAd
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadAd();
        LoadAdInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);

        if (adUnitId.Equals(_adUnitId))
        {
            Debug.Log("Rewarded Ad loaded");
            // Configure the button to call the ShowAd() method when clicked:
            //_showAdButton.onClick.AddListener(ShowAd);
            //// Enable the button for users to click:
            //_showAdButton.interactable = true;
        }
        if (adUnitId.Equals(_adUnitIdInterstitial))
        {
            Debug.Log("Interstitial Loaded");
        }
    }

    // Implement a method to execute when the user clicks the button:
    public void ShowAd(RewardedAdType type)
    {
        Debug.Log("Showing Ad");
        currentAdType = type;
        // Disable the button:
        //_showAdButton.interactable = false;
        // Then show the ad:
        Advertisement.Show(_adUnitId, this);
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward.

            // Load another ad:
            Advertisement.Load(_adUnitId, this);
            switch (currentAdType)
            {
                case RewardedAdType.FREECOINS:
                    PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 15);
                    break;
                case RewardedAdType.DOUBLEREWARD:
                    GameSystem.Sytem.totl = GameSystem.Sytem.totl * 2;
                    if (PlayerPrefs.GetInt("Level") == 0 && PlayerPrefs.GetInt("Coins") == 25)
                    {
                        Debug.Log("this is first level");
                        PlayerPrefs.SetInt("Coins", GameSystem.Sytem.totl);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + GameSystem.Sytem.totl);
                    }
                    GameSystem.Sytem.LEVEL.dubbleScore.text = GameSystem.Sytem.totl.ToString();
                    GameSystem.Sytem.LEVEL.P_Levelcomplete.transform.Find("Coins").transform.GetChild(0).GetComponent<Text>().text = PlayerPrefs.GetInt("Coins").ToString();
                    GameSystem.Sytem.LEVEL.DubbleReward.interactable = false;
                    break;
                case RewardedAdType.RESUME:
                    GameSystem.Sytem.resume();
                    break;
                case RewardedAdType.UNLOCKBALL:
                    storeManager.unlockBallWithAds();
                    break;
            }
        }
        else
        {
            LoadAdInterstitial();
        }
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
        LoadAdInterstitial();
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        LoadAd();
        LoadAdInterstitial();
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    #endregion


    #region InterstitialAd
    public void LoadAdInterstitial()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitIdInterstitial);
        Advertisement.Load(_adUnitIdInterstitial, this);
    }

    public void ShowAdInterstitial()
    {
        // Note that if the ad content wasn't previously loaded, this method will fail
        Debug.Log("Showing Ad: " + _adUnitIdInterstitial);
        Advertisement.Show(_adUnitIdInterstitial, this);
    }

    // Implement Load Listener and Show Listener interface methods: 

    #endregion
}