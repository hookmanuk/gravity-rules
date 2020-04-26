using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{ 	
	const float G = 667.4f;

	private Rigidbody _player;
	private GameState _gameState;
	private Vector3 _hitForce;
	private bool _isCameraHit;	

	public Rigidbody rb;

	void FixedUpdate()
	{
		if (_gameState.Started)
		{
			Attract();
		}
	}

	void OnEnable()
	{
		//if (Attractors == null)
		//	Attractors = new List<Attractor>();

		//Attractors.Add(this);
		_player = GameObject.FindWithTag("PlayerBody").GetComponent<Rigidbody>();
		_gameState = _player.GetComponent<GameState>();
	}

	void OnDisable()
	{
		
	}

	void Attract()
	{				        
		_player.AddForce(GetForce(_player.transform));
    }

	public Vector3 GetForce(Transform sourceTransform)
	{
		Vector3 force = new Vector3();
		Vector3 direction = rb.position - sourceTransform.position;

		//if not enabled, _player will not be set
		if (_player != null)
		{
			if (!_isCameraHit)
			{
				float forceMagnitude = G * (rb.mass * _player.mass) / direction.sqrMagnitude;

				force = direction.normalized * forceMagnitude;
			}
			else
			{
				force = _hitForce;
			}
		}

		return force;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "[CameraRig]")
		{			
			//player camera hit, set constant force to use from now on
			_hitForce = GetForce(_player.transform);
			_isCameraHit = true;
		}
		else if (other.gameObject.tag == "Spaceship")
		{
			ResetAttractor();
			_gameState.ExplodeSpaceship();			
		}
	}

	public void ResetAttractor()
	{
		_isCameraHit = false;
	}
}