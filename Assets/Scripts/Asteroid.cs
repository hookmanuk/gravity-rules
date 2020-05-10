using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class Asteroid : MonoBehaviour
{
    public GameObject Spaceship;
    public bool IsClone;
    public bool IsOutOfRange;

    // Start is called before the first frame update
    void Start()
    {               
    }

    // Update is called once per frame
    void FixedUpdate()
    {        
        if (IsClone && (transform.position - Spaceship.transform.position).magnitude > GameState.Instance.AsteroidRange)
        {
            IsOutOfRange = true;
        }
    }
}
