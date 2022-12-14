using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public CHaracterInfo BallData;
    int Ball_index;
    public Image Ball_Image;
    public Button Ball_Select;
    public Button Ball_videoAdsUnlock;
    public Button Ball_CoinsUnlock;
    public Text Ball_Inuse;
    public Text CoinsToNeed;
    public Text AdsToWatch;
    public GameObject ComingSoon;
    public Text CoinTxt;
    int lastSelectedballIndex;
    private void OnEnable()
    {
        mapdata();
        if (AdsInitializer.Instance)
            AdsInitializer.Instance.storeManager = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CoinTxt.text = PlayerPrefs.GetInt("Coins").ToString();
    }
    public void nextBall()
    {
        Ball_index++;
        Ball_index = Ball_index % BallData.CD.Length;
        Ball_Image.sprite = BallData.CD[Ball_index].CHaracterImg;
        if(BallData.CD[Ball_index].IsLocked)
        {
            if (BallData.CD[Ball_index].ComingSoon)
            {
                ComingSoon.SetActive(true);
                Ball_Inuse.gameObject.SetActive(false);
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.gameObject.SetActive(false);
            }
            else
            {
                ComingSoon.SetActive(false);
                Ball_Inuse.gameObject.SetActive(false);
                Ball_CoinsUnlock.gameObject.SetActive(true);
                Ball_videoAdsUnlock.gameObject.SetActive(true);
                Ball_Select.gameObject.SetActive(false);
                CoinsToNeed.text = BallData.CD[Ball_index].NeedCoins_ToUnlock.ToString();
                AdsToWatch.text = BallData.CD[Ball_index].AdsWatched.ToString()+"/"+BallData.CD[Ball_index].AdsToWatch_ToUnlock.ToString();
            }
        }
        else
        {
            ComingSoon.SetActive(false);
            Ball_CoinsUnlock.gameObject.SetActive(false);
            Ball_videoAdsUnlock.gameObject.SetActive(false);
            Ball_Select.gameObject.SetActive(true);
            if (BallData.CD[Ball_index].isSelected)
            {
                Ball_Select.interactable = false;
                Ball_Inuse.gameObject.SetActive(true);
            }
            else
            {
                Ball_Select.interactable = true;
                Ball_Inuse.gameObject.SetActive(false);
            }
        }
    }
    public void previousBall()
    {
        Ball_index--;
        if (Ball_index < 0)
        {
            Ball_index = BallData.CD.Length-1;
        }
        Ball_Image.sprite = BallData.CD[Ball_index].CHaracterImg;
        if (BallData.CD[Ball_index].IsLocked)
        {
            if(BallData.CD[Ball_index].ComingSoon)
            {
                ComingSoon.SetActive(true);
                Ball_Inuse.gameObject.SetActive(false);
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.gameObject.SetActive(false);
            }
            else
            {
                ComingSoon.SetActive(false);
                Ball_Inuse.gameObject.SetActive(false);
                Ball_CoinsUnlock.gameObject.SetActive(true);
                Ball_videoAdsUnlock.gameObject.SetActive(true);
                Ball_Select.gameObject.SetActive(false);
                CoinsToNeed.text = BallData.CD[Ball_index].NeedCoins_ToUnlock.ToString();
                AdsToWatch.text = BallData.CD[Ball_index].AdsWatched.ToString() + "/" + BallData.CD[Ball_index].AdsToWatch_ToUnlock.ToString();
            }
        }
        else
        {
            ComingSoon.SetActive(false);
            Ball_CoinsUnlock.gameObject.SetActive(false);
            Ball_videoAdsUnlock.gameObject.SetActive(false);
            Ball_Select.gameObject.SetActive(true);
            if (BallData.CD[Ball_index].isSelected)
            {
                Ball_Select.interactable = false;
                Ball_Inuse.gameObject.SetActive(true);
            }
            else
            {
                Ball_Select.interactable = true;
                Ball_Inuse.gameObject.SetActive(false);
            }
        }
    }
    void mapdata()
    {
        for(int i=0;i<BallData.CD.Length;i++)
        {
            if(BallData.CD[i].isSelected)
            {
                Ball_Image.sprite = BallData.CD[i].CHaracterImg;
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.interactable = false;
                Ball_Inuse.gameObject.SetActive(true);
                Ball_index = i;
                lastSelectedballIndex = i;
            }
        }
    }
    public void unlockBall(int Ball_Index ,bool ByAds) //call By Adsmanager
    {
        //adsUnlock
        if (ByAds)
        {
            BallData.CD[Ball_Index].AdsWatched++;
            if (BallData.CD[Ball_Index].AdsWatched == BallData.CD[Ball_Index].AdsToWatch_ToUnlock)
            {
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.gameObject.SetActive(true);
                Ball_Select.interactable = true;
                BallData.CD[Ball_Index].IsLocked = false;
            }
            else
            {
                AdsToWatch.text = BallData.CD[Ball_Index].AdsWatched.ToString() + "/" + BallData.CD[Ball_Index].AdsToWatch_ToUnlock.ToString();
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("Coins") > BallData.CD[Ball_index].NeedCoins_ToUnlock)
            {
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - BallData.CD[Ball_index].NeedCoins_ToUnlock);
                CoinTxt.text = PlayerPrefs.GetInt("Coins").ToString();
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.gameObject.SetActive(true);
                Ball_Inuse.gameObject.SetActive(true);
                BallData.CD[lastSelectedballIndex].isSelected = false;
                lastSelectedballIndex = Ball_index;
                BallData.CD[Ball_index].isSelected = true;
                BallData.CD[Ball_index].IsLocked = false;
            }
        }
    }
    public void UnlockBallwithCoins()
    {
        unlockBall(Ball_index, false);
    }
    public void unlockBallWithAds()
    {
        unlockBall(Ball_index, true);
    }
    public void selectBll()
    {
        BallData.CD[lastSelectedballIndex].isSelected = false;
        lastSelectedballIndex = Ball_index;
        BallData.CD[Ball_index].isSelected = true;
        Ball_Inuse.gameObject.SetActive(true);
        Ball_Select.interactable = false;
    }
    public void Ads_ToUnlockBall()
    {

        if (AdsInitializer.Instance)
        {
            if (BallData.CD[Ball_index].AdsWatched == BallData.CD[Ball_index].AdsToWatch_ToUnlock)
            {
                Ball_CoinsUnlock.gameObject.SetActive(false);
                Ball_videoAdsUnlock.gameObject.SetActive(false);
                Ball_Select.gameObject.SetActive(true);
                Ball_Select.interactable = true;
                BallData.CD[Ball_index].IsLocked = false;
            }
            else
            {
                AdsInitializer.Instance.ShowAd(RewardedAdType.UNLOCKBALL);
            }
        }
    }
}
