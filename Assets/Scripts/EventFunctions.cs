using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventFunctions : MonoBehaviour
{
    public void RestartLevel()
    {
        GameSystem.Sytem.LEVEL.LevelResart();
    }
}
