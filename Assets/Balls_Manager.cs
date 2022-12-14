using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls_Manager : MonoBehaviour
{
    public CHaracterInfo BallData;
    public GameObject Bal;
    public GameObject[] Balls;
    public MeshFilter[] Balls_MeshFilters;
    public Material [] LineColor;
    public GameObject Line;
    // Start is called before the first frame update
    private void OnEnable()
    {
        for (int i = 0; i < Balls.Length; i++)
        {
            if (BallData.CD[i].isSelected)
            {
                Bal.GetComponent<MeshFilter>().mesh = Balls_MeshFilters[i].mesh;
                Bal.GetComponent<MeshRenderer>().material = Balls[i].GetComponent<MeshRenderer>().material;
                Bal.GetComponent<MeshRenderer>().enabled = true;
                Line.GetComponent<MeshRenderer>().material = LineColor[i];
                transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = LineColor[i];
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
