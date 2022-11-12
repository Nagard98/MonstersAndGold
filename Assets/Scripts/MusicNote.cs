using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicNote : MonoBehaviour
{
    private Vector3 _spawnPosition;
    private Vector3 _hitPosition;
    private Vector3 _removePosition;
    public Conductor conductor;
    public Note note;
    public float tval;
    public float screenWidth;

    public Texture upArrowTexture, downArrowTexture, leftArrowTexture, rightArrowTexture;
    private RawImage _rawImage;
    
    private bool reachedHitPos;
    private Vector3 interpolationPos;
        
    
    // Start is called before the first frame update
    void Start()
    {
        conductor = transform.parent.GetComponent<Conductor>();
        _rawImage = GetComponent<RawImage>();
        loadNoteTexture((NoteType)note.NoteType);
        _spawnPosition = transform.position;
        _hitPosition = new Vector3((screenWidth/2), transform.position.y, 0);
        _removePosition = new Vector3(-100, transform.position.y, 0);

        reachedHitPos = false;
    }

    private void loadNoteTexture(NoteType noteType)
    {
        switch (noteType)
        {
            case NoteType.Up:
                _rawImage.texture = upArrowTexture;
                break;
            case NoteType.Down:
                _rawImage.texture = downArrowTexture;
                break;
            case NoteType.Left:
                _rawImage.texture = leftArrowTexture;
                break;
            case NoteType.Right:
                _rawImage.texture = rightArrowTexture;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.position == _hitPosition)
        {
            reachedHitPos = true;
        }

        if (reachedHitPos)
        {
            tval = (conductor.beatsShownInAdvance - (note.NoteBeat + conductor.beatsShownInAdvance - conductor.songPositionInBeats)) / conductor.beatsShownInAdvance;
            interpolationPos = Vector3.Lerp(_hitPosition, _removePosition, tval);
            if(transform.position == _removePosition)
            {
                Destroy(transform.gameObject);
            }
        }
        else
        {
            if (transform.position == _hitPosition)
            {
                reachedHitPos = true;
            }
            tval = (conductor.beatsShownInAdvance - (note.NoteBeat - conductor.songPositionInBeats)) / conductor.beatsShownInAdvance;
            interpolationPos =  Vector3.Lerp(_spawnPosition, _hitPosition, tval);
        }

        transform.position = interpolationPos;

    }
}
