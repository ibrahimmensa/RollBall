using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    private string removeAds = "com.Mensaplay.SuperColorFootball.RemoveAds";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPurchaseComplete(Product product)
    {
        if(product.definition.id == removeAds)
        {
            PlayerPrefs.SetInt("ShowAds", 0);
            AdsInitializer.Instance.ShowAds = false;
            Debug.Log("Remove ads done");
        }
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(product.definition.id + " Failed purchase because " + failureReason);
    }
}
