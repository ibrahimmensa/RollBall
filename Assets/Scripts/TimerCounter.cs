using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerCounter : MonoBehaviour
{
    public Text CounterText;

    public Image ProgressBorder;

    //public GameObject TapToRestartGO;

    public float timeElapsed = 10f;

    public GameObject Restart;
    public Button ResumeBtn;

    bool LevelRestartCalled;
    bool LeveRestartCalled;

    bool StartCounting;

    void OnEnable()
    {
        StartCounting = false;
        LevelRestartCalled = false;
        LeveRestartCalled = false;

        ProgressBorder.fillAmount = 1;
        CounterText.text = "10";

        Invoke("StartCountingCall", 0.5f);

        timeElapsed = 10f;
    }

    void StartCountingCall()
    {
        StartCounting = true;
    }

    void Update()
    {
        if (timeElapsed <= 5f)
        {
            RestartLevelCall();
        }
        if(timeElapsed <= 0)
        {
            RestartLeveCall();
            return;
        }
        if (!StartCounting) return;

        timeElapsed -= Time.unscaledDeltaTime;

        CounterText.text = ((int)timeElapsed).ToString();

        ProgressBorder.fillAmount = timeElapsed / 10f;

        //if (timeElapsed <= 3.5f)
        //    TapToRestartGO.SetActive(true);
    }

    void RestartLevelCall()
    {
        if (LevelRestartCalled)
            return;

        LevelRestartCalled = true;

        Restart.SetActive(true);
    }
    void RestartLeveCall()
    {
        if (LeveRestartCalled)
            return;

        LeveRestartCalled = true;
        ResumeBtn.interactable = false;
    }
}
