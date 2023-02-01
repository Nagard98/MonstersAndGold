using System;
using System.Collections;
using System.Collections.Generic;
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

[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{

    public const int FIRST_MULTIPLIER_THRESHOLD = 5;
    public const int SECOND_MULTIPLIER_THRESHOLD = 15;
    public const int THIRD_MULTIPLIER_THRESHOLD = 30;
    public float score;
    public ResultsVariable results;
    public float multiplier;
    public const float BASE_HIT_POINTS = 100f;
    private int _numConsecutiveHits;

    //public float bpm;
    public float songPosition;
    private SongInfo[] songsInfo;
    //public float beatDuration;
    public FloatVariable songPositionInBeats;
    public float dspSongTime;
    public GameObject note;
    public FloatVariable beatsShownInAdvance;
    
    public RectTransform rectTransform;
    public AudioSource audioSource;

    [SerializeField]
    //public Note[] _noteBeats;
    private MusicNote[] instancedNotes;
    private int _noteToSpawnIndex;
    private int _noteToHitIndex;
    private bool _attemptedHit;
    public float startOffsetBeats;

    private bool isRunning;
    private GameStateVariable gameState;

    public WorkoutPhasesVariable workoutPhases;
    private int _currentPhase;

    public FloatVariable playerSpeed;
    private const float SPEED_STEP = 0.2f;

    public UnityEvent MenuOpen;
    public UnityEvent<float> AttemptedHit;
    public UnityEvent ShowTutorial;

    private void OnEnable()
    {
        gameState = Resources.Load<GameStateVariable>("GameState");
        audioSource = GetComponent<AudioSource>();
        loadWorkoutSongs();
        score = 0f;
        multiplier = 1f;
        _attemptedHit = false;
        _numConsecutiveHits = 0;
        playerSpeed.Value = 2f;
        instancedNotes = InstantiateNotes(30);

        isRunning = false;
    }

    public void Init()
    {
        loadWorkoutSongs();
        score = 0f;
        multiplier = 1f;
        _attemptedHit = false;
        _numConsecutiveHits = 0;
        playerSpeed.Value = 2f;
        instancedNotes = InstantiateNotes(30);

        isRunning = false;

        if (gameState.isFirstTutorial)
        {
            ShowTutorial.Invoke();
            gameState.isFirstTutorial = false;
        }
    }

    public void StartConductor()
    {
        _currentPhase = 0;
        dspSongTime = (float)UnityEngine.AudioSettings.dspTime;
        audioSource.Play();
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            Note[] _noteBeats = songsInfo[_currentPhase].noteBeats;
            if (gameState.isPaused == true) return;

            if (_noteToSpawnIndex < _noteBeats.Length && _noteBeats[_noteToSpawnIndex].NoteBeat <= songPositionInBeats.Value)
            {
                instancedNotes[_noteToSpawnIndex % 30].StartNote(_noteBeats[_noteToSpawnIndex], songsInfo[_currentPhase].beatDuration + songsInfo[_currentPhase].beatDuration * beatsShownInAdvance.Value);
                _noteToSpawnIndex += 1;
            }

            if (_noteToHitIndex < _noteBeats.Length && _noteBeats[_noteToHitIndex].NoteBeat + beatsShownInAdvance.Value + 0.5f <= songPositionInBeats.Value - 1)
            {
                if (!_attemptedHit)
                {
                    _numConsecutiveHits = 0;
                    playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, 1f, 4f);
                    updateMultiplier();
                }
                _noteToHitIndex += 1;
                _attemptedHit = false;

            }

            songPosition = (float)(UnityEngine.AudioSettings.dspTime - dspSongTime);
            songPositionInBeats.Value = songPosition / songsInfo[_currentPhase].beatDuration;
        }
    }

    public void StartPhaseSong(float phase)
    {
        _currentPhase = (int)phase;
        _noteToSpawnIndex = 0;
        _noteToHitIndex = 0;
        songPositionInBeats.Value = 0f;
        audioSource.clip = workoutPhases.Value[(int)phase].phaseSong;
        dspSongTime = (float)UnityEngine.AudioSettings.dspTime;
        audioSource.Play();
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
        //audioSource.clip = workoutPhases.Value[]
        /*beatDuration = 60f / bpm;
        startOffsetBeats = 5f;
        _noteBeats = createBeats(60);
        */
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
                measureError(out accuracy);
                AttemptedHit.Invoke(((float)accuracy));
                updateScore(songsInfo[_currentPhase].noteBeats[_noteToHitIndex].NoteType, noteType, accuracy);
            }
        }
    }

    private float measureError(out Accuracy accuracyLevel)
    {
        float distance = Mathf.Abs(songsInfo[_currentPhase].noteBeats[_noteToHitIndex].NoteBeat - songPositionInBeats.Value + 1 + beatsShownInAdvance.Value);
        if (distance < 0.05f)
        {
            accuracyLevel = Accuracy.Perfect;
        }
        else if (distance < 0.15f)
        {
            accuracyLevel = Accuracy.Great;
        }
        else if (distance < 0.3f)
        {
            accuracyLevel = Accuracy.Good;
        }
        else
        {
            accuracyLevel = Accuracy.Miss;
        }
        return distance;
    }

    private void updateScore(int noteTypeHit, NoteType expectedType, Accuracy accuracy)
    {
        if (noteTypeHit == ((int)expectedType))
        {
            playerSpeed.Value = Mathf.Clamp(playerSpeed.Value + SPEED_STEP, 1f, 4f);
            score += (BASE_HIT_POINTS * multiplier);

            _numConsecutiveHits += 1;
            switch (accuracy)
            {
                case Accuracy.Perfect:
                    results.perfectHits += 1;
                    break;
                case Accuracy.Great:
                    results.greatHits += 1;
                    break;
                case Accuracy.Good:
                    results.goodHits += 1;
                    break;
                case Accuracy.Miss:
                    results.missHits += 1;
                    break;
            }
        }
        else
        {
            results.missHits += 1;
            _numConsecutiveHits = 0;
            playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, 1f, 4f);
            score -= BASE_HIT_POINTS;
        }

        updateMultiplier();


    }

    private void updateMultiplier()
    {
        if (_numConsecutiveHits <= FIRST_MULTIPLIER_THRESHOLD)
        {
            multiplier = 1f;
        }
        else if (_numConsecutiveHits <= SECOND_MULTIPLIER_THRESHOLD)
        {
            multiplier = 1.5f;
        }
        else if (_numConsecutiveHits <= THIRD_MULTIPLIER_THRESHOLD)
        {
            multiplier = 2f;
        }
        else
        {
            multiplier = 3f;
        }
    }

    private void spawnNote(Note noteToSpawn)
    {
        GameObject noteObject = Instantiate(note, rectTransform);
        MusicNote musicNote = noteObject.GetComponent<MusicNote>();
        musicNote.note = noteToSpawn;
    }

    private void OnMenu(InputValue input)
    {
        if (input.isPressed && gameState.isInGame)
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