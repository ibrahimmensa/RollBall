using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    float initial_y;

    // Start is called before the first frame update
    void Start()
    {
        initial_y = transform.position.y;
    }

    public Transform target;

    public Vector3 CameraOffset;

    // Update is called once per frame
    void Update()
    {
        if (GameSystem.Sytem.LEVEL.GAME_STATE == GameState.GAME || GameSystem.Sytem.LEVEL.GAME_STATE == GameState.IDLE)
            transform.transform.position = new Vector3(target.position.x + CameraOffset.x, initial_y, target.position.z + CameraOffset.z);
    }
}
