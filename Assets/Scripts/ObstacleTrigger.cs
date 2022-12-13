using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameSystem.Sytem.LEVEL.GAME_STATE == GameState.LOSE)
            return;

        if (PlayerPrefs.GetInt("Vibrate")==1)
            Handheld.Vibrate();

        GameSystem.Sytem.CollisionSound.Play();
        GameSystem.Sytem.LEVEL.LevelFailed();
        Destroy(this.gameObject.transform.parent.gameObject);
    }
}
