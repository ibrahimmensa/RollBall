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

    bool StartCounting;

    void OnEnable()
    {
        StartCounting = false;

        ProgressBorder.fillAmount = 1;
        CounterText.text = "10";

        Invoke(nameof(StartCountingCall), 0.5f);

        timeElapsed = 10f;
    }

    void StartCountingCall()
    {
        StartCounting = true;
    }

    void Update()
    {
        if (StartCounting)
        {
            timeElapsed -= Time.deltaTime;
            if (timeElapsed >= 0)
            {
                CounterText.text = ((int)timeElapsed).ToString();
            }

            ProgressBorder.fillAmount = timeElapsed / 10f;
        }
        if (timeElapsed <= 5f && timeElapsed > 0)
        {
            RestartLevelCall();
        }
        else if(timeElapsed <= 0)
        {
            RestartLeveCall();
            return;
        }
        
    }

    void RestartLevelCall()
    {

        Restart.SetActive(true);
    }
    void RestartLeveCall()
    {
        StartCounting = false;
        ResumeBtn.interactable = false;
    }
}
