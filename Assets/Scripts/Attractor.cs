using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{ 	
	const float G = 667.4f;

	private Rigidbody _player;
	private GameState _gameState;

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
        Vector3 direction = rb.position - _player.position;

        float forceMagnitude = G * (rb.mass * _player.mass) / direction.sqrMagnitude;

		Vector3 force = direction.normalized * forceMagnitude;

		_player.AddForce(force);
    }

	private void OnCollisionEnter(Collision collision)
	{
		//_player.velocity = Vector3.zero;
		_player.isKinematic = true;
		Debug.Log("hit");
	}
}