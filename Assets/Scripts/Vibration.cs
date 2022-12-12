using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vibration : MonoBehaviour
{
    public Image vibration_waves;

    public static bool isVibratingOn;

    void Start()
    {
        isVibratingOn = (PlayerPrefs.GetInt("vib") - 1) != 0;

        if (PlayerPrefs.GetInt("vib") == 0) isVibratingOn = true;

        vibration_waves.gameObject.SetActive(isVibratingOn);
    }

    public void OnClick()
    {
        if (isVibratingOn)
            PlayerPrefs.SetInt("vib", 1);
        else
            PlayerPrefs.SetInt("vib", 2);


        isVibratingOn = (PlayerPrefs.GetInt("vib") - 1) != 0;

        vibration_waves.gameObject.SetActive(isVibratingOn);
    }
}
