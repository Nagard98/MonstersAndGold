using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

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
    public float multiplier;
    public const float BASE_HIT_POINTS = 100f;
    private int _numConsecutiveHits;

    public float bpm;
    public float songPosition;
    public float beatDuration;
    public FloatVariable songPositionInBeats;
    public float dspSongTime;
    public GameObject note;
    public FloatVariable beatsShownInAdvance;
    
    public RectTransform rectTransform;
    private AudioSource audioSource;

    [SerializeField]
    public Note[] _noteBeats;
    private MusicNote[] instancedNotes;
    private int _noteToSpawnIndex;
    private int _noteToHitIndex;
    private bool _attemptedHit;
    public float startOffsetBeats;

    public FloatVariable playerSpeed;
    private const float SPEED_STEP = 0.2f;

    public UnityEvent MenuOpen;
    public UnityEvent<float> AttemptedHit;

    // Start is called before the first frame update
    void Start()
    {

        audioSource = GetComponent<AudioSource>();

        loadSong();

        score = 0f;
        multiplier = 1f;
        _attemptedHit = false;
        _numConsecutiveHits = 0;
        playerSpeed.Value = 2f;
        instancedNotes = InstantiateNotes(30);

        dspSongTime = (float)UnityEngine.AudioSettings.dspTime;
        audioSource.Play();
        beatsShownInAdvance.Value = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (_noteToSpawnIndex < _noteBeats.Length && _noteBeats[_noteToSpawnIndex].NoteBeat <= songPositionInBeats.Value)
        {
            instancedNotes[_noteToSpawnIndex % 30].StartNote(_noteBeats[_noteToSpawnIndex], beatDuration + beatDuration*beatsShownInAdvance.Value);
            _noteToSpawnIndex += 1;
        }

        /*if (_noteToHitIndex < _noteBeats.Length && _noteBeats[_noteToHitIndex].NoteBeat < songPositionInBeats.Value + beatsShownInAdvance.Value)
        {


            //spawnNote(_noteBeats[_noteSpawnIndex]);
            //instancedNotes[_noteSpawnIndex % 15].StartNote(_noteBeats[_noteSpawnIndex]);
            //_attemptedHit = false;
            //_noteSpawnIndex += 1;
            
        }*/

        if (_noteToHitIndex < _noteBeats.Length && _noteBeats[_noteToHitIndex].NoteBeat+ beatsShownInAdvance.Value + 0.5f <= songPositionInBeats.Value - 1)
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
        songPositionInBeats.Value = songPosition / beatDuration;

    }

    private void loadSong()
    {
        audioSource = GetComponent<AudioSource>();
        beatDuration = 60f / bpm;
        _noteToSpawnIndex = 0;
        _noteToHitIndex = 0;
        startOffsetBeats = 5f;
        songPositionInBeats.Value = 0f;

        _noteBeats = createBeats(60);
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
            if (_noteToHitIndex < _noteBeats.Length)
            {
                AttemptedHit.Invoke(measureError());
                updateScore(_noteBeats[_noteToHitIndex].NoteType, noteType);
            }
        }
    }

    private float measureError()
    {
        float distance = Mathf.Abs(_noteBeats[_noteToHitIndex].NoteBeat - songPositionInBeats.Value + 1 + beatsShownInAdvance.Value);
        return distance;
    }

    private void updateScore(int noteTypeHit, NoteType expectedType)
    {
        if (noteTypeHit == ((int)expectedType))
        {
            playerSpeed.Value = Mathf.Clamp(playerSpeed.Value + SPEED_STEP, 1f, 4f);
            score += (BASE_HIT_POINTS * multiplier);

            _numConsecutiveHits += 1;
        }
        else
        {
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
        //musicNote.screenWidth = rectTransform.rect.width;
    }

    private void OnMenu(InputValue input)
    {
        if (input.isPressed)
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
