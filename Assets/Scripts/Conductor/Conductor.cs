using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

enum Accuracy
{
    Perfect,
    Great,
    Good,
    Miss
}

enum NoteType
{
    Up,
    Down,
    Left,
    Right
}

[Serializable]
public struct Note
{
    public float noteBeat;
    public int noteType;

    public Note(float note_beat, int note_type)
    {
        this.noteType = note_type;
        this.noteBeat = note_beat;
    }
    public float NoteBeat { get { return noteBeat; } }
    public int NoteType { get { return noteType; } }
}

//Manages the note spawning and timing, determinig also the speed of the player
[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{

    public ResultsVariable results;

    private float _songPosition;
    private SongInfo[] songsInfo;
    public FloatVariable songPositionInBeats;
    private float _dspSongTime;
    public GameObject note;
    public FloatVariable beatsShownInAdvance;
    
    public RectTransform rectTransform;
    private AudioSource _audioSource;

    private MusicNote[] instancedNotes;
    private int _noteToSpawnIndex;
    private int _noteToHitIndex;
    private bool _attemptedHit;

    private bool _isRunning;
    private GameStateVariable _gameState;

    public WorkoutPhasesVariable workoutPhases;
    private int _currentPhase;

    public FloatVariable playerSpeed, optSpeed;
    public bool isIncreasingSpeed;
    private const float SPEED_STEP = 0.2f;

    public UnityEvent MenuOpen;
    public UnityEvent<float> AttemptedHit;
    public UnityEvent ShowTutorial;

    private void Awake()
    {
        _gameState = Resources.Load<GameStateVariable>("GameState");
        _audioSource = GetComponent<AudioSource>();

        //TO-DO: change so that the player can choose
        loadWorkoutSongs();
        _attemptedHit = false;
        results.Reset();
        playerSpeed.Value = 2f;

        //TO-DO: remove 
        instancedNotes = InstantiateNotes(30);

        isIncreasingSpeed = false;
        _isRunning = false;
    }

    public void Init()
    {
        loadWorkoutSongs();
        results.Reset();
        _attemptedHit = false;
        playerSpeed.Value = 2f;

        //TO-DO: remove note instantiation
        instancedNotes = InstantiateNotes(30);
        
        _isRunning = false;

        if (_gameState.isFirstTutorial)
        {
            ShowTutorial.Invoke();
            _gameState.isFirstTutorial = false;
        }
    }

    public void CleanUp()
    {
        _currentPhase = 0;
        _isRunning = false;

        //TODO: remove
        songPositionInBeats.Value = 0;
        foreach(MusicNote note in instancedNotes)
        {
            note.CleanUp();
        }
    }

    public void StartConductor()
    {
        optSpeed.Value = 3f;
        playerSpeed.Value = 3f;
        _currentPhase = 0;
        _dspSongTime = (float)UnityEngine.AudioSettings.dspTime;
        _audioSource.Play();
        _isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isRunning)
        {
            //TODO: remove
            Note[] _noteBeats = songsInfo[_currentPhase].noteBeats;

            if (_gameState.isPaused == true) return;

            //TODO: remove
            //When its time activates the next note
            if (_noteToSpawnIndex < _noteBeats.Length && _noteBeats[_noteToSpawnIndex].NoteBeat <= songPositionInBeats.Value)
            {
                instancedNotes[_noteToSpawnIndex % 30].StartNote(_noteBeats[_noteToSpawnIndex], songsInfo[_currentPhase].beatDuration + songsInfo[_currentPhase].beatDuration * beatsShownInAdvance.Value);
                _noteToSpawnIndex += 1;
            }

            //TODO: remove
            if (_noteToHitIndex < _noteBeats.Length && _noteBeats[_noteToHitIndex].NoteBeat + beatsShownInAdvance.Value + 0.5f <= songPositionInBeats.Value - 1)
            {
                if (!_attemptedHit)
                {
                    UpdateSpeed(Accuracy.Miss);
                }
                _noteToHitIndex += 1;
                _attemptedHit = false;
            }

            //TODO: remove
            _songPosition = (float)(UnityEngine.AudioSettings.dspTime - _dspSongTime);
            songPositionInBeats.Value = _songPosition / songsInfo[_currentPhase].beatDuration;
        }
    }

    public void StartPhaseSong(float phase)
    {
        //TODO: remove
        isIncreasingSpeed = (PathGenerator.rpng.Next() % 2) == 1 ? true : false;

        _currentPhase = (int)phase;

        //TODO: remove
        _noteToSpawnIndex = 0;
        _noteToHitIndex = 0;
        songPositionInBeats.Value = 0f;

        _audioSource.clip = workoutPhases.Value[(int)phase].phaseSong;

        //TODO: remove
        _dspSongTime = (float)UnityEngine.AudioSettings.dspTime;

        _audioSource.Play();
    }

    private void loadWorkoutSongs()
    {
        songsInfo = new SongInfo[workoutPhases.Value.Length];
        for (int i = 0; i < workoutPhases.Value.Length; i++)
        {
            float beatDur = 60f / workoutPhases.Value[i].bpm;
            int numBeats = Mathf.FloorToInt(workoutPhases.Value[i].totalSongDuration / beatDur);
            songsInfo[i] = new SongInfo(beatDur, 0f, createBeats(numBeats));
        }
        //TODO:resetta a zero quando inizia fase

    }

    private Note[] createBeats(int numBeats)
    {
        Note[] _tmpBeats = new Note[numBeats];

        for (int i = 0; i < numBeats; i++)
        {
            int _tmpNoteType = UnityEngine.Random.Range(0, 4);
            _tmpBeats[i] = new((float)i, _tmpNoteType);
        }

        return _tmpBeats;
    }

    //Creates a pool of notes
    private MusicNote[] InstantiateNotes(int poolDim = 15)
    {
        MusicNote[] musicNotes = new MusicNote[poolDim];
        for (int i = 0; i < poolDim; i++)
        {
            MusicNote musicNote = Instantiate(note, rectTransform).GetComponent<MusicNote>();
            musicNotes[i] = musicNote;
        }
        return musicNotes;
    }


    private void manageInput(NoteType noteType)
    {
        if (_attemptedHit == false)
        {
            _attemptedHit = true;
            if (_noteToHitIndex < songsInfo[_currentPhase].noteBeats.Length)
            {
                Accuracy accuracy;
                measureError(out accuracy, songsInfo[_currentPhase].noteBeats[_noteToHitIndex].NoteType, noteType);
                AttemptedHit.Invoke(((float)accuracy));
            }
        }
    }

    private float measureError(out Accuracy accuracyLevel, int noteTypeHit, NoteType expectedType)
    {
        float distance = Mathf.Abs(songsInfo[_currentPhase].noteBeats[_noteToHitIndex].NoteBeat - songPositionInBeats.Value + 1 + beatsShownInAdvance.Value);
        if(noteTypeHit == ((int)expectedType))
        {
            if (distance < 0.03f)
            {
                accuracyLevel = Accuracy.Perfect;
                results.perfectHits += 1;
            }
            else if (distance < 0.10f)
            {
                accuracyLevel = Accuracy.Great;
                results.greatHits += 1;
            }
            else if (distance < 0.3f)
            {
                accuracyLevel = Accuracy.Good;
                results.goodHits += 1;
            }
            else
            {
                accuracyLevel = Accuracy.Miss;
                results.missHits += 1;
            }
        }
        else
        {
            accuracyLevel = Accuracy.Miss;
            results.missHits += 1;
        }
        UpdateSpeed(accuracyLevel);

        return distance;
    }

    //Called by spawn event; determines if accelerate or decelerate when you miss hits
    public void DecideIncreaseDecreaseSpeed(POIVariable poi, SpawnSettings spawnSettings)
    {
        if (poi.isCollectable) isIncreasingSpeed = false;
        else isIncreasingSpeed = true;
    }

    public void RandomIncreaseDecreaseSpeed()
    {
        isIncreasingSpeed = (PathGenerator.rpng.Next() % 2) == 1 ? true : false;
    }
    

    private void UpdateSpeed(Accuracy accuracy)
    {

        //TODO: remove
        if (isIncreasingSpeed)
        {
            if(Accuracy.Miss == accuracy)
            {
                playerSpeed.Value = Mathf.Clamp(playerSpeed.Value + SPEED_STEP, optSpeed.Value - SPEED_STEP, optSpeed.Value + 2f);
            }
            else
            {
                playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, optSpeed.Value - SPEED_STEP, optSpeed.Value + 2f);
            }
        }
        //TODO: remove
        else
        {
            if (Accuracy.Miss == accuracy)
            {
                playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, Mathf.Clamp(optSpeed.Value - 1.5f, 1, optSpeed.Value), optSpeed.Value + SPEED_STEP * 1.5f);
            }
            else
            {
                playerSpeed.Value = Mathf.Clamp(playerSpeed.Value + SPEED_STEP, Mathf.Clamp(optSpeed.Value - 1.5f, 1, optSpeed.Value), optSpeed.Value + SPEED_STEP * 1.5f);
            }
        }
        
    }

    //TODO: remove
    private void spawnNote(Note noteToSpawn)
    {
        GameObject noteObject = Instantiate(note, rectTransform);
        MusicNote musicNote = noteObject.GetComponent<MusicNote>();
        musicNote.note = noteToSpawn;
    }


    private void OnMenu(InputValue input)
    {
        if (input.isPressed && _gameState.isInGame)
        {
            MenuOpen.Invoke();
        }
    }

    private void OnDown(InputValue input)
    {
        if (input.isPressed)
        {
            manageInput(NoteType.Down);
        }
    }

    private void OnUp(InputValue input)
    {
        if (input.isPressed)
        {
            manageInput(NoteType.Up);
        }
    }

    private void OnLeft(InputValue input)
    {
        if (input.isPressed)
        {
            manageInput(NoteType.Left);
        }
    }

    private void OnRight(InputValue input)
    {
        if (input.isPressed)
        {
            manageInput(NoteType.Right);
        }
    }
}

struct SongInfo
{
    public float beatDuration;
    public float startOffsetBeats;
    public Note[] noteBeats;

    public SongInfo(float beatDuration, float startOffsetBeats, Note[] noteBeats)
    {
        this.beatDuration = beatDuration;
        this.startOffsetBeats = startOffsetBeats;
        this.noteBeats = noteBeats;
    }
}