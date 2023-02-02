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
    public FloatVariable beatsShownInAdvance;
    public FloatVariable songPositionInBeats;

    private RectTransform noteTransform;

    public Texture upArrowTexture, downArrowTexture, leftArrowTexture, rightArrowTexture;
    private RawImage _rawImage;
    
        
    void Start()
    {
        _rawImage = GetComponent<RawImage>();
        loadNoteTexture((NoteType)note.NoteType);
        noteTransform = (RectTransform) gameObject.transform;
        _spawnPosition = noteEntryPoint.anchoredPosition;
        _removePosition = noteExitPoint.anchoredPosition;
        _hitPosition = noteHitPoint.anchoredPosition;
        noteTransform.anchoredPosition = _spawnPosition;
    }

    public void CleanUp()
    {
        noteTransform.DOKill();
        noteTransform.anchoredPosition = noteEntryPoint.anchoredPosition;
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

    //Tweens the note position from one end to the other with correct timing
    public void StartNote(Note note, float beatDuration=0f)
    {
        noteTransform.anchoredPosition = _spawnPosition;
        loadNoteTexture((NoteType)note.noteType);
        noteTransform.DOAnchorPos(_removePosition, beatDuration * 2f, true).SetEase(Ease.Linear);
    }

}
