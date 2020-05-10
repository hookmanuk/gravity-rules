using AzureTableStorage;
using BangsPhysics;
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
    private BangsPhysics.RigidBody _player;    
    private float _scorePrevious = 10000;

    public SteamVR_Action_Boolean Restart;
    public SteamVR_Action_Boolean TracePath;
    public SteamVR_Action_Single Rewind;    
    public GameObject HUD;
    public SimpleHelvetica HighScoresText;
    public SimpleHelvetica YourScoreText;
    public SimpleHelvetica InstructionsText;
    public GameObject Spaceship;
    public GameObject[] Asteroids;
    public List<GameObject> InGameAsteroids;
    public int AsteroidRange;

    public float Score = 10000;

    private Stack<FrameInfo> _frames;
    private float _secondsRewound;
    private int _rewindMultiplier = 2;
    private HUD _hud;
    private float _startScore;
    private string _playerName = "";
    private Attractor[] Attractors;
    private LineRenderer LineRenderer;
    public bool IsTracingPath = true;
    public bool IsTracePathPlanetHit;

    public float WorldRate = 60f;
    public float ScoreMultiplier = 1f;
    public float ScoreThrustMultiplier = 5f;
    public float ScoreRewindMultiplier = 0.8f;
    public scoremanager ScoreManager;
    public List<highscore> Highscores;    
    public bool IsFinished;
    public bool IsWaitingForName;    
    private int ScoreEntering;
    private Audio _audio;
    public int TotalSimulatePoints = 40;
    public static GameState Instance;


    private bool _started;

    public bool Started
    {
        get { return _started; }
        set { 
            _started = value;
            BangsPhysics.PhysicsManager.Instance.Started = value;
        }
    }

    private bool _isRewinding;
    public bool IsRewinding
    {
        get { return _isRewinding; }
        set
        {
            _isRewinding = value;
            BangsPhysics.PhysicsManager.Instance.IsRewinding = value;
        }
    }


    private void Awake()
    {
        gameObject.AddComponent<BangsPhysics.PhysicsManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _startScore = Score;
        _player = GameObject.FindWithTag("PlayerBody").GetComponent<BangsPhysics.RigidBody>();
        _audio = GetComponentInChildren<Audio>();
        _audio.Forwards(true);

        Transform startTransform = GetComponent<Transform>();        
        Restart.onStateDown += Restart_onStateDown;
        TracePath.onStateDown += TracePath_onStateDown;
        //TracePath.onStateUp += TracePath_onStateUp;

        _frames = new Stack<FrameInfo>();

        _hud = HUD.GetComponent<HUD>();

        ScoreManager = new scoremanager();        
        UpdateHighscores();        

        Attractors = FindObjectsOfType<Attractor>();

        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        LineRenderer.widthMultiplier = 0.004f;
        LineRenderer.positionCount = TotalSimulatePoints;

        GenerateAsteroids();
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
        IsTracingPath = !IsTracingPath;
    }

    //private void TracePath_onStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    IsTracingPath = false;
    //}

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
                IsTracePathPlanetHit = false;
                if (!LineRenderer.enabled)
                {
                    LineRenderer.enabled = true;
                }

                PathResult pathResult = BangsPhysics.PhysicsManager.Instance.ForwardSimulate(_player, TotalSimulatePoints, 60, Spaceship.transform.position - _player.position);

                SetPathTraceColour(pathResult.State);

                int index = 0;
                foreach(var position in pathResult.FuturePoints)
                {
                    LineRenderer.SetPosition(index++, position);
                }

/*                LineRenderer.SetPosition(0, Spaceship.transform.position);

                //loop through future mes and apply forces to them from both the current velocity and attractor planets
                for (int i = 0; i < 80; i++)
                {
                    if (IsTracePathPlanetHit)
                    {
                        break;
                    }
                    Transform previousTransform;

                    previousTransform = (i == 0 ? Spaceship.transform : FutureMes[i - 1].transform);

                    //not sure about this 0.5 multiplier, the larger the number the further out the trace path will go
                    FutureMes[i].transform.position = previousTransform.position + (_player.velocity * 0.5f); 

                    foreach (Attractor attractor in Attractors)
                    {
                        Console.WriteLine(i.ToString() + ": " + attractor.GetForce(FutureMes[i].transform).magnitude);
                        //not sure about this 2 multiplier, seems like far away from planets too little force is applied, close to planets too much force applied!
                        FutureMes[i].transform.position += (attractor.GetForce(FutureMes[i].transform) * 2f); 
                    }
                                                            
                    LineRenderer.SetPosition(i + 1, FutureMes[i].transform.position);                         
                    
                    if (i == 79 && !IsTracePathPlanetHit)
                    {
                        //only set the colour back to white if this frame path hasn't hit an attractor, doesn't seem to work properly though
                        //maybe the attractor collider trigger happens too infrequently for this to work?
                        SetPathTraceColour(false);
                    }
                }*/
            }
            else
            {
                if (LineRenderer.enabled)
                {
                    LineRenderer.enabled = false;
                }
            }            
        }

        RepositionAsteroids();
    }

    public void SetPathTraceColour(FutureState state)
    {
        switch (state)
        {
            case FutureState.OK:
                LineRenderer.startColor = new Color(100, 100, 100);
                LineRenderer.endColor = new Color(100, 100, 100);
                break;
            case FutureState.PlanetHit:
                LineRenderer.startColor = new Color(200, 0, 0);
                LineRenderer.endColor = new Color(200, 0, 0);
                break;
            case FutureState.CheckpointHit:
                LineRenderer.startColor = new Color(0, 200, 0);
                LineRenderer.endColor = new Color(0, 200, 0);
                break;
            default:
                break;
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
                RewindSomeFrames();
            }
        }
    }

    public void RewindSomeFrames(int frames = 1)
    {
        FrameInfo frame = null;
        for (int i = 0; i < _rewindMultiplier * frames; i++)
        {
            if (_frames.Count > 0)
            {
                frame = _frames.Pop();
            }
        }

        if (frame != null)
        {
            GetComponent<Transform>().SetPositionAndRotation(frame.PlayerPosition, frame.PlayerRotation);
            _player.GetComponent<RigidBody>().position = frame.PlayerPosition;
            _player.GetComponent<RigidBody>().velocity = frame.PlayerVelocity;
            if (frame.CurrentCheckpoint != _currentCheckpoint)
            {
                _currentCheckpoint = frame.CurrentCheckpoint;
                EnableCheckpoint(_currentCheckpoint);
            }

            Score = Score + (frame.ScoreDiff * ScoreRewindMultiplier);
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
        IsFinished = true;        

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
        ToggleShip(true);
        
        foreach (var item in Attractors)
        {
            item.ResetAttractor();
        }

        _player.Reset();
        _player.isKinematic = false;
        Score = _startScore;
        _hud.SetScore(Convert.ToInt32(Score));
        IsFinished = false;
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

    public void ExplodeSpaceship()
    {
        GameObject explosion = GameObject.FindGameObjectWithTag("SpaceshipExplosion");
        RewindSomeFrames(2);                
        _player.isKinematic = true;
        IsFinished = true;
        Started = false;
        explosion.GetComponent<ParticleSystem>().Play();
        explosion.GetComponentInChildren<AudioSource>().Play();
        ToggleShip(false);
    }

    private void ToggleShip(bool enable)
    {
        GameObject.FindGameObjectWithTag("Spaceship").GetComponent<MeshRenderer>().enabled = enable;
        foreach (var item in GameObject.FindGameObjectWithTag("SpaceshipHUD").GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = enable;
        }        
    }

    private void RepositionAsteroids()
    {        
        foreach (GameObject asteroidObject in InGameAsteroids)
        {
            if (asteroidObject.GetComponent<Asteroid>().IsOutOfRange)
            {
                asteroidObject.transform.position = (Spaceship.transform.position + UnityEngine.Random.insideUnitSphere * AsteroidRange) + Spaceship.transform.forward * 80;
                asteroidObject.GetComponent<Asteroid>().IsOutOfRange = false;
            }
        }
    }

    private void GenerateAsteroids()
    {
        int intTotalAsteroids;        

        intTotalAsteroids = 1000;        
                      
        System.Random r = new System.Random();        
        for (int i = 0; i < intTotalAsteroids; i++)
        {
            //render asteroids nearby spaceship
            GameObject asteroidObject = Instantiate(Asteroids[r.Next(0, 7)],
                    (Spaceship.transform.position + UnityEngine.Random.insideUnitSphere * AsteroidRange) + Spaceship.transform.forward * 80,
                    //new Vector3(
                    //    r.Next((int)Spaceship.transform.position.x, (int)Spaceship.transform.position.x + (int)Math.Max(10, (Spaceship.transform.forward.x * 100))),
                    //    r.Next((int)Spaceship.transform.position.y, (int)Spaceship.transform.position.y + (int)Math.Max(10, (Spaceship.transform.forward.y * 100))),
                    //    r.Next((int)Spaceship.transform.position.z, (int)Spaceship.transform.position.z + (int)Math.Max(10, (Spaceship.transform.forward.z * 100)))
                    //),
                    new Quaternion(r.Next(0, 360), r.Next(0, 360), r.Next(0, 360), r.Next(0, 360))
                );

            asteroidObject.GetComponent<Asteroid>().IsClone = true;

            InGameAsteroids.Add(asteroidObject);            
        }
    }
}
