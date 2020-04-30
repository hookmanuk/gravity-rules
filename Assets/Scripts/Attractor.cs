using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{ 	
	const float G = 667.4f;

	private BangsPhysics.RigidBody _player;
	private GameState _gameState;
	private bool _isCameraHit;	


	void OnEnable()
	{
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<BangsPhysics.RigidBody>();

        _gameState = _player.GetComponent<GameState>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "[CameraRig]")
		{			
			//player camera hit, set constant force to use from now on
			_isCameraHit = true;
		}
		else if (other.gameObject.tag == "Spaceship")
		{
			ResetAttractor();
			_gameState.ExplodeSpaceship();			
		}	
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.name.StartsWith("FutureMe"))
		{			
			_gameState.SetPathTraceColour(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		
	}

	public void ResetAttractor()
	{
		_isCameraHit = false;
	}
}