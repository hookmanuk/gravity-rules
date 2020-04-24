using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public GameObject ScoreValue;
    public GameObject SpeedValue;

    SimpleHelvetica _scoreValueText;
    SimpleHelvetica _speedValueText;

    // Start is called before the first frame update
    void Start()
    {
        _scoreValueText = ScoreValue.GetComponent<SimpleHelvetica>();
        _speedValueText = SpeedValue.GetComponent<SimpleHelvetica>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpeed(float speed)
    {
        _speedValueText.Text = String.Format("{0:N2}", Math.Round(speed,2)) + " lm/s";
        _speedValueText.GenerateText();
    }

    public void SetScore(int score)
    {
        _scoreValueText.Text = score.ToString();
        _scoreValueText.GenerateText();
    }
}
