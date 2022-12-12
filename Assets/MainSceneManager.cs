using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{
    public int TotalCoins;
    public Text CoinsTxt;
    // Start is called before the first frame update


    //sound and music
    public GameObject Sound;


    void Start()
    {
        //if (GoogleAds.Instance)
        //{
        //    GoogleAds.Instance.RequestBanner();
        //}
        if (PlayerPrefs.HasKey("Coins"))
        {
            TotalCoins = PlayerPrefs.GetInt("Coins");
            CoinsTxt.text = TotalCoins.ToString();
        }
        else
        {
            PlayerPrefs.SetInt("Coins", 0);
            TotalCoins = PlayerPrefs.GetInt("Coins");
            CoinsTxt.text = TotalCoins.ToString();
        }
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                Sound.SetActive(true);
            }
            else
            {
                Sound.SetActive(false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Sound", 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CoinsTxt.text = PlayerPrefs.GetInt("Coins").ToString();
    }
    public void Privacy()
    {
        Application.OpenURL("https://mensaplay.com/wensa/privacy-policy.html");
    }
    public void VibrationOn()
    {
        PlayerPrefs.SetInt("Vibrate", 1);
    }
    public void VibrationOFF()
    {
        PlayerPrefs.SetInt("Vibrate", 0);
    }
    public void SoundOn()
    {
        PlayerPrefs.SetInt("Sound", 1);
    }
    public void SoundOFF()
    {
        PlayerPrefs.SetInt("Sound", 0);
    }
    public void MusicOn()
    {
        PlayerPrefs.SetInt("Music", 1);
    }
    public void MusicOFF()
    {
        PlayerPrefs.SetInt("Music", 0);
    }
    public void Ads_freeCoins()
    {
        if (AdsInitializer.Instance)
            AdsInitializer.Instance.ShowAd(RewardedAdType.FREECOINS);
    }
    
    public void Ads_Freecoins()
    {
        if (AdsInitializer.Instance)
            AdsInitializer.Instance.ShowAd(RewardedAdType.FREECOINS);
    }
}
