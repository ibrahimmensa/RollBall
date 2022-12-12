using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    static ObjectPooler _pooler;
    public static ObjectPooler Pooler
    {
        get
        {
            if (_pooler == null)
            {
                _pooler = GameObject.FindObjectOfType<ObjectPooler>();

                if (_pooler == null)
                {
                    GameObject container = new GameObject("ObjectPooler");
                    _pooler = container.AddComponent<ObjectPooler>();
                }
            }

            return _pooler;
        }
    }

    public List<GameObject> Balls;

    public GameObject currentBall
    {
        get
        {
            return Balls.Find(d => d.activeSelf == false);
        }
    }
}
