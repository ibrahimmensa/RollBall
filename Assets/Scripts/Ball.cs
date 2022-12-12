using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Ball : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed = 5;
    public float rotationSpeed = 5;

    float distanceTravelled;
    float angle;
    float initSpeed;

    float onGroundY = 0.25f;

    public Vector3 currentPoint;

    Transform Parent;

    public AnimationCurve ballSpeedCurve;
    float _animationTimePosition;

    Rect bounds;

    public bool isMoving;

    public GameObject BallPoofParticle;

    public Vector3 initPoint;


    public bool canMove;

    void Start()
    {
        canMove = true;

        //BallPoofParticle = GameObject.Find("Ball_Fail_Particle");

        bounds = new Rect(0, 0, Screen.width, Screen.height);

        Parent = this.transform.parent;
        initSpeed = speed;
        angle = 1;

        initPoint = pathCreator.path.GetPointAtDistance(0);

        GameSystem.Sytem.LEVEL.OnLevelFinished += LEVEL_OnLevelFinished;
        GameSystem.Sytem.LEVEL.OnLevelFailed += LEVEL_OnLevelFailed;
        GameSystem.Sytem.LEVEL.OnLevelResart += LEVEL_OnLevelResart;
    }

    private void LEVEL_OnLevelResart()
    {
        distanceTravelled = 0f;

        Parent.position = pathCreator.path.GetPointAtDistance(distanceTravelled) + new Vector3(0f, onGroundY, 0f);
        Parent.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);

        isMoving = false;

        this.GetComponent<MeshRenderer>().enabled = true;

        speed = 0f;
        angle = 1;

        _animationTimePosition = 0;

        canMove = true;
    }

    private void LEVEL_OnLevelFailed()
    {
        canMove = false;

        BallPoofParticle.transform.position = this.Parent.transform.position;

        this.GetComponent<MeshRenderer>().enabled = false;

        speed = 0f;

        isMoving = false;

        BallPoofParticle.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        BallPoofParticle.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
    }

    private void LEVEL_OnLevelFinished()
    {
        speed = 0f;

        isMoving = false;

        canMove = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (canMove == false) return;

        if (Input.GetMouseButton(0) && bounds.Contains(Input.mousePosition) && GameSystem.Sytem.LEVEL.GAME_STATE == GameState.GAME)
        {
            isMoving = true;
            GameSystem.Sytem.HoldBtn.enabled = true;
            _animationTimePosition += Time.deltaTime * 1f;
            _animationTimePosition = Mathf.Clamp(_animationTimePosition, 0f, 3f);

            speed = Mathf.Lerp(0f, 3.5f, ballSpeedCurve.Evaluate(_animationTimePosition));

            distanceTravelled += speed * Time.deltaTime;
            Parent.position = pathCreator.path.GetPointAtDistance(distanceTravelled) + new Vector3(0f, onGroundY, 0f);
            Parent.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);

            currentPoint = pathCreator.path.GetPointAtDistance(distanceTravelled);

            angle = Mathf.Lerp(1f, 12f, ballSpeedCurve.Evaluate(_animationTimePosition));
            angle = Mathf.Clamp(angle, 1f, 12f);

            transform.Rotate(Vector3.up, angle);
        }
        else
        {
            if (speed != 0)
            {
                _animationTimePosition = Mathf.Clamp(_animationTimePosition, 0f, 3f);

                _animationTimePosition -= Time.deltaTime * 2.4f;
                speed = Mathf.Lerp(0f, 2.1f, ballSpeedCurve.Evaluate(_animationTimePosition));

                angle = Mathf.Lerp(1f, 12f, ballSpeedCurve.Evaluate(_animationTimePosition));
                angle = Mathf.Clamp(angle, 1f, 12f);

                distanceTravelled += speed * Time.deltaTime;
                Parent.position = pathCreator.path.GetPointAtDistance(distanceTravelled) + new Vector3(0f, onGroundY, 0f);
                Parent.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);

                currentPoint = pathCreator.path.GetPointAtDistance(distanceTravelled);

                transform.Rotate(Vector3.up, angle);
            }

            if (speed == 0) isMoving = false;
        }

        /*DrawDebugTools.Instance.Log("Rotation Angle : " + angle, Color.white);
        DrawDebugTools.Instance.Log("Ball Speed : " + speed, Color.white);
        DrawDebugTools.Instance.Log("distanceTravelled : " + distanceTravelled, Color.red);
        DrawDebugTools.Instance.Log("GetPointAtDistance : " + currentPoint, Color.red);*/
        //DrawDebugTools.Instance.DrawFloatGraph("Ball Speed", speed, 4.5f, false,50);
        //DrawDebugTools.Instance.Log("animationValue : " + _animationTimePosition, Color.red);
        //DrawDebugTools.Instance.Log("progress 0-1 : " + Mathf.Clamp01(distanceTravelled / pathCreator.path.length), Color.red);

    }
}
