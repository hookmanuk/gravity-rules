using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int Number;
    private Rigidbody _player;
    private GameObject _spaceShip;
    private bool _isActiveCheckpoint = true;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<Rigidbody>();
        _spaceShip = GameObject.FindWithTag("RightController");

        if (Number > 0)
        {
            _isActiveCheckpoint = false; 
        }
        else
        {
            GetComponent<Transform>().localScale = new Vector3(10f, 10f, 10f);
        }
    }

    public void SetActive(bool isActive)
    {
        _isActiveCheckpoint = isActive;
        if (isActive)
        {
            GetComponent<Transform>().localScale = new Vector3(10f, 10f, 10f);
        }
        else
        {
            GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isActiveCheckpoint)
        {
            float dist = Vector3.Distance(_spaceShip.transform.position, transform.position);            

            if (dist < 10f)
            {
                _player.GetComponent<GameState>().IncrementCheckpoint();                
                SetActive(false);
            }
        }
    }

   public void EnableCheckpoint()
    {
        SetActive(true);
    }
}
