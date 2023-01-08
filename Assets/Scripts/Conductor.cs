using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum NoteType
{
    None,
    Up,
    Down,
    Left,
    Right
}

public struct Note
{
    public Note(float note_beat, int note_type)
    {
        NoteBeat = note_beat;
        NoteType = note_type;
    }

    public float NoteBeat { get; }
    public int NoteType { get; }
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

    private Note[] _noteBeats;
    private int _noteSpawnIndex;
    private int _noteToHitIndex;
    private bool _attemptedHit;
    public float startOffsetBeats;

    public FloatVariable playerSpeed;
    private const float SPEED_STEP = 0.2f;

    // Start is called before the first frame update
    void Start()
    {

        audioSource = GetComponent<AudioSource>();

        loadSong();

        score = 0f;
        multiplier = 1f;
        _attemptedHit = false;
        _numConsecutiveHits = 0;

        dspSongTime = (float)AudioSettings.dspTime;
        audioSource.Play();
        beatsShownInAdvance.Value = 3;
    }


    // Update is called once per frame
    void Update()
    {

        manageInput();

        if (_noteSpawnIndex < _noteBeats.Length && _noteBeats[_noteSpawnIndex].NoteBeat < songPositionInBeats.Value + beatsShownInAdvance.Value)
        {
            if (!_attemptedHit)
            {
                //Debug.Log("Not Attempted");
                _numConsecutiveHits = 0;
                playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, 1f, 4f);
                updateMultiplier();
            }

            spawnNote(_noteBeats[_noteSpawnIndex]);
            _attemptedHit = false;
            _noteSpawnIndex += 1;
        }


        if (_noteToHitIndex < _noteBeats.Length && _noteBeats[_noteToHitIndex].NoteBeat + 0.2f < songPositionInBeats.Value)
        {
            _noteToHitIndex += 1;
        }

        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats.Value = songPosition / beatDuration;

    }

    private void loadSong()
    {
        audioSource = GetComponent<AudioSource>();
        beatDuration = 60f / bpm;
        _noteSpawnIndex = 0;
        _noteToHitIndex = 0;
        startOffsetBeats = 5f;

        _noteBeats = createBeats(60);
    }

    private Note[] createBeats(int numBeats)
    {
        Note[] _tmpBeats = new Note[numBeats];

        for (int i = 0; i < numBeats; i++)
        {
            int _tmpNoteType = UnityEngine.Random.Range(1, 5);
            _tmpBeats[i] = new((float)i, _tmpNoteType);
        }

        return _tmpBeats;
    }


    private void manageInput()
    {
        NoteType _tmpInputNoteType = NoteType.None;

        if (Input.GetButtonDown("Up"))
        {
            _tmpInputNoteType = NoteType.Up;
        }
        if (Input.GetButtonDown("Left"))
        {
            _tmpInputNoteType = NoteType.Left;
        }
        if (Input.GetButtonDown("Right"))
        {
            _tmpInputNoteType = NoteType.Right;
        }
        if (Input.GetButtonDown("Down"))
        {
            _tmpInputNoteType = NoteType.Down;
        }

        if (_tmpInputNoteType != NoteType.None && _attemptedHit == false)
        {
#if DEBUG
            Debug.Log(_tmpInputNoteType.ToString());
#endif
            _attemptedHit = true;
            if (_noteToHitIndex < _noteBeats.Length)
            { 
                updateScore(_noteBeats[_noteToHitIndex].NoteType, _tmpInputNoteType);
            }
        }
    }

    private float measureError()
    {
        float distance = Mathf.Abs(_noteBeats[_noteToHitIndex].NoteBeat - songPositionInBeats.Value);
        return distance;
    }

    private void updateScore(int noteTypeHit, NoteType expectedType)
    {
        if (noteTypeHit == ((int)expectedType))
        {
#if DEBUG
            Debug.Log("Correct");
#endif
            playerSpeed.Value = Mathf.Clamp(playerSpeed.Value + SPEED_STEP, 1f, 4f);
            score += (BASE_HIT_POINTS * multiplier);

            _numConsecutiveHits += 1;
        }
        else
        {
#if DEBUG
            Debug.Log("Wrong");
#endif
            _numConsecutiveHits = 0;
            playerSpeed.Value = Mathf.Clamp(playerSpeed.Value - SPEED_STEP, 1f, 4f);
            score -= BASE_HIT_POINTS;
        }

        updateMultiplier();

#if DEBUG
        Debug.Log(measureError());
#endif
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
        GameObject noteObject = Instantiate(note, new Vector3(rectTransform.rect.width + 100, 100, 0), rectTransform.rotation);
        MusicNote musicNote = noteObject.GetComponent<MusicNote>();
        musicNote.transform.SetParent(rectTransform);
        musicNote.note = noteToSpawn;
        musicNote.screenWidth = rectTransform.rect.width;
    }
}
