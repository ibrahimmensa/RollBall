using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    private IEnumerator coroutine;

    float spawnRate;

    GameObject currentBall;

    public Transform ballSpawnPosition;

    // Start is called before the first frame update
    void Start()
    {
        spawnRate = GameSystem.Sytem.spawnRate;

        coroutine = SpawnBall(spawnRate);
        StartCoroutine(coroutine);
    }

    private IEnumerator SpawnBall(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            currentBall = ObjectPooler.Pooler.currentBall;

            currentBall.transform.position = ballSpawnPosition.position;
            currentBall.transform.rotation = ballSpawnPosition.rotation;

            currentBall.SetActive(true);
        }
    }
}
