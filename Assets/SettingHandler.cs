using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SoundOff;
    public GameObject VibOff;
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                SoundOff.SetActive(true);
            }
        }
        if (PlayerPrefs.HasKey("Vibrate"))
        {
            if (PlayerPrefs.GetInt("Vibrate") == 0)
            {
                VibOff.SetActive(true);
            }
        }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PrivacyPolicy()
    {
        Application.OpenURL("https://mensaplay.com/wensa/privacy-policy.html");
    }
}
