using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class Thruster : MonoBehaviour
{
    // a reference to the action
    public SteamVR_Action_Single Thrust;

    private Rigidbody _player;
    private GameObject _controller;
    private float f_Multiplier = 0.3f;
    private ParticleSystem _particleSystem;
    private GameState _gameState;

    public AudioSource ThrustSound;

    // Start is called before the first frame update
    void Start()
    {
        _gameState = GameObject.FindWithTag("PlayerBody").GetComponent<GameState>();
    }

    public void UpdateHandler(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta)
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_gameState.IsWaitingForName)
        {
            if (Thrust.axis != 0)
            {
                if (!_gameState.Started)
                {
                    _gameState.Started = true;
                    
                    _gameState.FadeOutText();
                }
                //get controller
                if (Thrust.activeDevice == SteamVR_Input_Sources.RightHand)
                {
                    _controller = GameObject.FindWithTag("RightController");
                }
                else if (Thrust.activeDevice == SteamVR_Input_Sources.LeftHand)
                {
                    _controller = GameObject.FindWithTag("LeftController");
                }

                if (_controller != null)
                {
                    _player.AddForce(_controller.transform.forward * f_Multiplier * Thrust.axis, ForceMode.Force);

                    _gameState.ThrustActiveUpdateScore();

                    if (!ThrustSound.isPlaying)
                    {
                        ThrustSound.Play();
                        _particleSystem.Play();
                    }
                }
            }
            else
            {
                StopThrusters();
            }
        }
    }

    void OnEnable()
    {        
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<Rigidbody>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void StopThrusters()
    {
        if (ThrustSound.isPlaying)
        {
            ThrustSound.Stop();
            _particleSystem.Stop();
        }
    }
}
