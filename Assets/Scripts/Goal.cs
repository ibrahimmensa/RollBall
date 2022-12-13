using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
    public UnityEvent onGoal;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Level Finished");
        if(PlayerPrefs.GetInt("Vibrate") == 1)
            Handheld.Vibrate();

        onGoal.Invoke();

        GameSystem.Sytem.LEVEL.LevelFinished();
    }
}
