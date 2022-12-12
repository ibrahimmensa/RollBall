using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinsAdd : MonoBehaviour
{
    private IEnumerator coroutine;

    public Text currentCoins;
    public Text coinsToAdd;

    public float step;

    public float speed;

    int _currentcoins;
    int _coinsToAdd;

    float timeElapsed;

    public void StartAdd()
    {
        coroutine = WaitAndPrint(step);
        StartCoroutine(coroutine);

        _currentcoins = int.Parse(currentCoins.text);
        _coinsToAdd = int.Parse(coinsToAdd.text);

        GameSystem.Sytem.PLAYER.coins += _coinsToAdd;
    }

    int coins;

    private IEnumerator WaitAndPrint(float step)
    {
        while (true)
        {
            timeElapsed += speed * Time.deltaTime;
            yield return new WaitForSeconds(step);

            if (Vibration.isVibratingOn)
                Handheld.Vibrate();

            coins = (int)(_currentcoins + Mathf.Lerp(0, _coinsToAdd, timeElapsed));
            currentCoins.text = coins.ToString();

            if(timeElapsed >= 1f)
            {
                StopCoroutine(coroutine);
                Invoke("NextLevel", 1.1f);
            }
        }
    }

    void NextLevel()
    {
        GameSystem.Sytem.LEVEL.NextLevel();
    }
}
