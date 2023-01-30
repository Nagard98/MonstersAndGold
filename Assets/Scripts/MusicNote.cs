using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MusicNote : MonoBehaviour
{
    private Vector2 _spawnPosition;
    private Vector2 _removePosition;
    private Vector2 _hitPosition;
    public RectTransform noteEntryPoint;
    public RectTransform noteExitPoint;
    public RectTransform noteHitPoint;

    public Note note;
    public float tval;
    private bool moving;
    public FloatVariable beatsShownInAdvance;
    public FloatVariable songPositionInBeats;

    private RectTransform noteTransform;

    public Texture upArrowTexture, downArrowTexture, leftArrowTexture, rightArrowTexture;
    private RawImage _rawImage;
    
    private bool reachedHitPos;
    private Vector3 interpolationPos;
        
    void Start()
    {
        _rawImage = GetComponent<RawImage>();
        loadNoteTexture((NoteType)note.NoteType);
        noteTransform = (RectTransform) gameObject.transform;
        _spawnPosition = noteEntryPoint.anchoredPosition;
        _removePosition = noteExitPoint.anchoredPosition;
        _hitPosition = noteHitPoint.anchoredPosition;
        noteTransform.anchoredPosition = _spawnPosition;

        reachedHitPos = false;
        moving = false;
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

        if (moving) 
        {
            if (noteTransform.anchoredPosition.x == 0)//_hitPosition)
            {
                reachedHitPos = true;
            }

            if (reachedHitPos)
            {
                tval = (beatsShownInAdvance.Value - (note.NoteBeat + beatsShownInAdvance.Value - songPositionInBeats.Value)) / beatsShownInAdvance.Value;
                interpolationPos = Vector3.Lerp(_hitPosition, _removePosition, tval);
                if (noteTransform.anchoredPosition == _removePosition)
                {
                    moving = false;
                }
            }
            else
            {
                if (noteTransform.anchoredPosition == _hitPosition)
                {
                    reachedHitPos = true;
                }
                tval = (beatsShownInAdvance.Value - (note.NoteBeat - songPositionInBeats.Value)) / beatsShownInAdvance.Value;
                interpolationPos = Vector3.Lerp(_spawnPosition, _hitPosition, tval);
            }

            noteTransform.anchoredPosition = interpolationPos;

        }
    }

    public void StartNote(Note note, float beatDuration=0f)
    {
        //this.note = note;
        //moving = true;
        noteTransform.anchoredPosition = _spawnPosition;
        loadNoteTexture((NoteType)note.noteType);
        //reachedHitPos = false;
        //moving = true; ;
        noteTransform.DOAnchorPos(_removePosition, beatDuration * 2f, true).SetEase(Ease.Linear);
    }
}
