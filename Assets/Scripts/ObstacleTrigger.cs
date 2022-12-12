using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameSystem.Sytem.LEVEL.GAME_STATE == GameState.LOSE)
            return;

        if (Vibration.isVibratingOn)
            Handheld.Vibrate();

        GameSystem.Sytem.LEVEL.LevelFailed();
        Destroy(this.gameObject.transform.parent.gameObject);
    }
}
