using AzureTableStorage;
using HighScores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

public class GameState : MonoBehaviour
{
    private int _currentCheckpoint = 0;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _player;    
    private float _scorePrevious = 10000;

    public SteamVR_Action_Boolean Restart;
    public SteamVR_Action_Boolean TracePath;
    public SteamVR_Action_Single Rewind;
    public GameObject HUD;
    public SimpleHelvetica HighScoresText;
    public SimpleHelvetica YourScoreText;
    public SimpleHelvetica InstructionsText;
    public GameObject FutureMe;
    public GameObject Spaceship;
    
    public float Score = 10000;

    private Stack<FrameInfo> _frames;
    private float _secondsRewound;
    private int _rewindMultiplier = 2;
    private HUD _hud;
    private float _startScore;
    private string _playerName = "";
    private List<GameObject> FutureMes;
    private Attractor[] Attractors;
    private LineRenderer LineRenderer;
    public bool IsTracingPath;

    public float WorldRate = 60f;
    public float ScoreMultiplier = 1f;
    public float ScoreThrustMultiplier = 5f;
    public float ScoreRewindMultiplier = 0.8f;
    public scoremanager ScoreManager;
    public List<highscore> Highscores;
    public bool Started;
    public bool IsWaitingForName;
    public bool IsRewinding;
    private int ScoreEntering;
    private Audio _audio;

    // Start is called before the first frame update
    void Start()
    {
        _startScore = Score;
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<Rigidbody>();
        _audio = GetComponentInChildren<Audio>();
        _audio.Forwards(true);

        Transform startTransform = GetComponent<Transform>();
        _startPosition = new Vector3(startTransform.position.x, startTransform.position.y, startTransform.position.z);
        _startRotation = new Quaternion(startTransform.rotation.x, startTransform.rotation.y, startTransform.rotation.z, startTransform.rotation.w);
        Restart.onStateDown += Restart_onStateDown;
        TracePath.onStateDown += TracePath_onStateDown;
        TracePath.onStateUp += TracePath_onStateUp;

        _frames = new Stack<FrameInfo>();

        _hud = HUD.GetComponent<HUD>();

        ScoreManager = new scoremanager();        
        UpdateHighscores();

        FutureMes = new List<GameObject>();

        for (int i = 0; i < 40; i++)
        {
            FutureMes.Add(Instantiate(FutureMe));
        }
        FutureMe.SetActive(false);

        Attractors = FindObjectsOfType<Attractor>();

        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        LineRenderer.widthMultiplier = 0.01f;
        LineRenderer.positionCount = 41;
    }

    private async void UpdateHighscores()
    {
        StringBuilder sb = new StringBuilder();

        Highscores = await ScoreManager.GetAllScores();

        Highscores = Highscores.OrderByDescending(hs => hs.Score).Take(10).ToList(); //change to pull only top 10 by partition of highscore

        sb.AppendLine("High Scores:");
        int i = 1;
        foreach (highscore highscore in Highscores)
        {
            sb.Append(i.ToString());
            sb.Append(". ");
            sb.Append(highscore.Score.ToString());
            sb.Append(" - ");            
            sb.AppendLine(highscore.UserName);

            i++;
        }

        HighScoresText.Text = sb.ToString().Replace("\r","");
        HighScoresText.GenerateText();
    }

    private void Restart_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (IsWaitingForName)
        {
            EnterName();
        }
        else
        {
            ReInitGame();
        }        
    }

    private void TracePath_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        IsTracingPath = true;
    }

    private void TracePath_onStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        IsTracingPath = false;
    }

    private void EnterName()
    {
        IsWaitingForName = false;
        UpdateScoreText(false);
        _playerName = "";

        SteamVR.instance.overlay.ShowKeyboard(0, 0, "Player Name", 50, "", true, 0);

        SteamVR_Events.System(Valve.VR.EVREventType.VREvent_KeyboardCharInput).RemoveAllListeners();
        SteamVR_Events.System(Valve.VR.EVREventType.VREvent_KeyboardCharInput).AddListener((Valve.VR.VREvent_t arg) =>
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            SteamVR.instance.overlay.GetKeyboardText(stringBuilder, 256);
            _playerName += stringBuilder.ToString();
        });

        SteamVR_Events.System(Valve.VR.EVREventType.VREvent_KeyboardClosed).RemoveAllListeners();
        SteamVR_Events.System(Valve.VR.EVREventType.VREvent_KeyboardClosed).AddListener(async (Valve.VR.VREvent_t args) =>
        {              
            var result = await ScoreManager.InsertScore(new highscore
            {
                PartitionKey = "topten",                
                Score = ScoreEntering,
                Timestamp = DateTimeOffset.UtcNow,
                UserName = _playerName
            });            

            UpdateHighscores();
        });        
    }

    private void FixedUpdate()
    {
        if (Started)
        {
            if (Rewind.axis == 0)
            {
                if (IsRewinding)
                {
                    //start playing music forwards
                    _audio.Forwards(false);
                }
                IsRewinding = false;
                _secondsRewound = 0;
                _rewindMultiplier = 1;

                Score = Score - (ScoreMultiplier * (IsTracingPath ? 2f : 1f) / WorldRate);
                _hud.SetScore(Convert.ToInt32(Score));
            }
            else
            {
                if (!IsRewinding)
                {
                    //start playing music backwards
                    _audio.Reverse();
                }
                IsRewinding = true;
                _secondsRewound = _secondsRewound + (1f / WorldRate);
                _rewindMultiplier = 1 + (int)Math.Floor(_secondsRewound / 5f);

                _hud.SetScore(Convert.ToInt32(Score));
            }

            _hud.SetSpeed(_player.velocity.magnitude / 100f);

            if (IsTracingPath)
            {
                if (!LineRenderer.enabled)
                {
                    LineRenderer.enabled = true;
                }
                
                LineRenderer.SetPosition(0, Spaceship.transform.position);

                //loop through future mes and apply forces to them from both the current velocity and attractor planets
                for (int i = 0; i < 40; i++)
                {
                    Transform previousTransform;

                    previousTransform = (i == 0 ? Spaceship.transform : FutureMes[i - 1].transform);

                    FutureMes[i].transform.position = previousTransform.position + (_player.velocity * 0.1f); //not sure about this 0.1 multiplier

                    foreach (Attractor attractor in Attractors)
                    {
                        Console.WriteLine(i.ToString() + ": " + attractor.GetForce(previousTransform).magnitude);
                        FutureMes[i].transform.position += (attractor.GetForce(previousTransform) * 0.1f); //not sure about this 0.1 multiplier
                    }

                    LineRenderer.SetPosition(i + 1, FutureMes[i].transform.position);
                }
            }
            else
            {
                if (LineRenderer.enabled)
                {
                    LineRenderer.enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Started)
        {
            //not rewinding, so add to the stack
            if (Rewind.axis == 0)
            {
                _frames.Push(new FrameInfo
                {
                    PlayerPosition = _player.position,
                    PlayerRotation = _player.rotation,
                    PlayerVelocity = _player.velocity,
                    CurrentCheckpoint = _currentCheckpoint,
                    ScoreDiff = _scorePrevious - Score
                });

                _scorePrevious = Score;
            }
            else
            {
                //Rewind the gamestate!
                FrameInfo frame = null;
                for (int i = 0; i < _rewindMultiplier; i++)
                {
                    if (_frames.Count > 0)
                    {
                        frame = _frames.Pop();
                    }
                }

                if (frame != null)
                {
                    GetComponent<Transform>().SetPositionAndRotation(frame.PlayerPosition, frame.PlayerRotation);
                    _player.velocity = frame.PlayerVelocity;
                    if (frame.CurrentCheckpoint != _currentCheckpoint)
                    {
                        _currentCheckpoint = frame.CurrentCheckpoint;
                        EnableCheckpoint(_currentCheckpoint);
                    }

                    Score = Score + (frame.ScoreDiff * ScoreRewindMultiplier);
                }
            }
        }
    }

    public void IncrementCheckpoint()
    {
        _currentCheckpoint++;
        EnableCheckpoint(_currentCheckpoint);        
    }

    public void EnableCheckpoint(int intCheckPoint)
    {        
        //find checkpoint and enable halo
        GameObject nextCheckpoint = GameObject.Find($"Checkpoint ({intCheckPoint})");

        if (nextCheckpoint != null)
        {
            nextCheckpoint.GetComponent<Checkpoint>().EnableCheckpoint();
        }
        else
        {
            //all checkpoints done, stop game and enter score                                    
            FinishedCourse();            
        }
    }    

    private void FinishedCourse()
    {
        GameObject engine = GameObject.FindWithTag("Engine");
        Thruster thruster = engine.GetComponent<Thruster>();

        thruster.StopThrusters();

        ScoreEntering = Convert.ToInt32(Score);

        UpdateScoreText(true);

        IsWaitingForName = true;        

        ReInitGame(); //reset player to show scores on screen
    }

    public void UpdateScoreText(bool includePrompt)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Your Score:");
        sb.AppendLine(ScoreEntering.ToString());

        if (includePrompt)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Press A to enter your name");
        }

        YourScoreText.Text = sb.ToString().Replace("\r", "");
        YourScoreText.GenerateText();
    }

    public void ReInitGame()
    {
        GetComponent<Transform>().SetPositionAndRotation(_startPosition, _startRotation);
        _player.velocity = new Vector3(0, 0, 0);
        _player.isKinematic = false;
        Score = _startScore;
        _hud.SetScore(Convert.ToInt32(Score));
        Started = false;
        ShowText();
        _frames.Clear();

        _currentCheckpoint = 0;
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();

        foreach (var checkpoint in checkpoints)
        {            
            checkpoint.SetActive(checkpoint.Number == 0);            
        }
    }

    public void ThrustActiveUpdateScore()
    {
        Score = Score - (ScoreThrustMultiplier / WorldRate);
    }

    public async void FadeOutText()
    {
        await Task.Run(() =>
        {
            Thread.Sleep(2000);            
        });

        HighScoresText.gameObject.SetActive(false);
        YourScoreText.gameObject.SetActive(false);
        InstructionsText.gameObject.SetActive(false);


        //Material textMaterial = HighScoresText.GetComponent<Renderer>().material;
        //textMaterial.color = new Color(255, 255, 255, 0);
        //HighScoresText.GenerateText();
    }

    public void ShowText()
    {
        HighScoresText.gameObject.SetActive(true);
        YourScoreText.gameObject.SetActive(true);
        InstructionsText.gameObject.SetActive(true);
        //Material textMaterial = HighScoresText.GetComponent<Renderer>().material;
        //textMaterial.color = new Color(255, 255, 255, 255);
    }
}
