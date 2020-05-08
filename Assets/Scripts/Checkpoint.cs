using BangsPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int Number;
    private Rigidbody _player;
    private GameObject _spaceShip;
    public bool IsActiveCheckpoint = true;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<Rigidbody>();
        _spaceShip = GameObject.FindWithTag("RightController");

        if (Number > 0)
        {
            SetActive(false);            
        }
        else
        {
            SetActive(true);            
        }
    }

    public void SetActive(bool isActive)
    {
        IsActiveCheckpoint = isActive;
        if (isActive)
        {
            PhysicsManager.Instance.ActiveCheckpoint = this;
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
        if (IsActiveCheckpoint)
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
