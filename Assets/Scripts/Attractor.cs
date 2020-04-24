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
		_player.AddForce(GetForce(_player.transform));
    }

	public Vector3 GetForce(Transform sourceTransform)
	{
		Vector3 force = new Vector3();
		Vector3 direction = rb.position - sourceTransform.position;

		//if not enabled, _player will not be set
		if (_player != null)
		{
			float forceMagnitude = G * (rb.mass * _player.mass) / direction.sqrMagnitude;

			force = direction.normalized * forceMagnitude;
		}

		return force;
	}

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log(collision.collider.gameObject.name);

		//_player.velocity = Vector3.zero;
		_player.isKinematic = true;
		Debug.Log("hit");
	}
}