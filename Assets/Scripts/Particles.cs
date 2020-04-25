using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour
{
    public int maxStars = 1000;
    public int universeSize = 10;

    private ParticleSystem.Particle[] points;

    private ParticleSystem _particleSystem;
    private int _updates;

    private void Create()
    {
        _particleSystem = gameObject.GetComponent<ParticleSystem>();

        //points = new ParticleSystem.Particle[maxStars];

        //for (int i = 0; i < maxStars; i++)
        //{
        //    points[i].position = Random.insideUnitSphere * universeSize;
        //    points[i].startSize = Random.Range(1f, 100f);
        //    points[i].startColor = new Color(255, 255, 255, 0);
        //}

        

        //_particleSystem.SetParticles(points, points.Length);
    }

    void Start()
    {
        Create();
    }

    void FixedUpdate()
    {
        //Wait 50 frames then pause the particle generator, we don't want the particles moving themselves
        if (_updates < 50)
        {
            _updates++;
        }
        else if (_updates == 50)
        {
            _particleSystem.Pause();
        }
    }
}
