using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class CanvasListener : MonoBehaviour
{
    public UnityEvent OnEnableEvent;

    void OnEnable()
    {
        if (this.gameObject.tag == "CoinText")
        {
            Invoke("setcoins", 0.1f);

            return;
        }

        if(this.gameObject.name == "level_text")
        {
            //Debug.Log(Regex.Match(SceneManager.GetActiveScene().name, @"\d+").Value + " - " + SceneManager.GetActiveScene().name);
            this.GetComponent<Text>().text = "Level " + Regex.Match(SceneManager.GetActiveScene().name, @"\d+").Value + " Complete !";

            return;
        }

        if(OnEnableEvent != null)
            OnEnableEvent.Invoke();
    }

    void setcoins()
    {
        gameObject.GetComponent<Text>().text = GameSystem.Sytem.PLAYER.coins.ToString();
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
}
