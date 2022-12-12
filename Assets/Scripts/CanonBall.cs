using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBall : MonoBehaviour
{
    public float speed;

    Transform ball;

    public AnimationCurve curve;

    public float yValue;

    float timeElapsed;

    public float duration = 10;

    Vector3 initpos;

    Transform ballChild;

    public float rotationSpeed = 1;

    void Start()
    {
        ball = this.transform;
        ballChild = transform.GetChild(0);
        initpos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ball.position += -ball.right * (speed * Time.deltaTime);

        timeElapsed += Time.deltaTime;
        rotationSpeed += Time.deltaTime * 0.5f;
        rotationSpeed = Mathf.Clamp(rotationSpeed, 0, 4.3f);

        yValue = Mathf.Lerp(0.025f, -0.17f, curve.Evaluate(timeElapsed / duration));

        ball.position = new Vector3(ball.position.x, yValue, ball.position.z);

        ballChild.Rotate(Vector3.forward, rotationSpeed);
        if(timeElapsed >= duration)
            ballChild.Rotate(Vector3.forward, rotationSpeed);
    }

    void OnEnable()
    {
        Invoke("Disable", GameSystem.Sytem.timeBeforeBallDisable);
    }

    void Disable()
    {
        initpos = transform.position;
        timeElapsed = 0f;

        this.gameObject.SetActive(false);
    }
}
