using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using SpeechLib;
using System.Threading;

public class Assistant : MonoBehaviour
{

    private Animator m_animationController;
    SpVoice voice = new SpVoice();
    public AudioClip clip;


    // Start is called before the first frame update
    void Start()
    {
        m_animationController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator DelayedSpeech(string text, float delay)
    {
        yield return new WaitForSeconds(delay);
        voice.Speak("<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='it-IT'>"+ text + "</speak>", SpeechVoiceSpeakFlags.SVSFlagsAsync|SpeechVoiceSpeakFlags.SVSFIsXML);
        StartCoroutine(WaitFinishSpeak());
    }

    public IEnumerator WaitFinishSpeak()
    {
        while (true)
        {
            //voice.WaitUntilDone(Timeout.Infinite);
            if (voice.Status.RunningState == SpeechRunState.SRSEDone)
            {
                Debug.Log("Finished speaking");
                hide();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    public void appearAndInstruct(POIVariable poi, SpawnSettings spawnSettings)
    {
        m_animationController.SetTrigger("Appear");
        string msg = poi.alertMessage + (int)spawnSettings.distance + " metri di distanza. Mantieni questo ritmo per raggiungerlo in " + spawnSettings.TTL + " secondi.";
        StartCoroutine(DelayedSpeech(msg, 3.0f));
    }

    public void appearAndInstructTest()
    {
        m_animationController.SetTrigger("Appear");
        StartCoroutine(DelayedSpeech("Testing the speaking api for text to speech", 3.0f));
    }

    public void hide()
    {
        m_animationController.SetTrigger("Hide");
        //voice.Speak("Testing the speaking api for text to speech", SpeechVoiceSpeakFlags.SVSFlagsAsync);
        //SpeechStreamFileMode SpFileMode = SpeechStreamFileMode.SSFMCreateForWrite;
        //SpFileStream SpFileStream = new SpFileStream();
        //SpFileStream.Open(@"C:\Users\Dragan Vuletic\Documents\GitHub\MonstersAndGold\Assets\Resources\test.wav", SpFileMode, false);
        //voice.AudioOutputStream = SpFileStream;
        //voice.Speak("Testing the speaking api for text to speech", SpeechVoiceSpeakFlags.SVSFlagsAsync);
        //voice.WaitUntilDone(Timeout.Infinite);//Using System.Threading;
        //SpFileStream.Close();

    }
}