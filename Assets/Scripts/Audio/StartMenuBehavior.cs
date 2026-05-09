using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuBehavior : MonoBehaviour
{
    [SerializeField] AudioSource audioSource1;
    [SerializeField] AudioSource audioSource2;
    [SerializeField] string musicInfo;

    [Range(0,1)]
    [SerializeField] float panStereoDistance = 1.0f;
    [Range(0, 10)]
    [SerializeField] float panStereoSpeed = 1.0f;
    [Range(0, 10)]
    [SerializeField] float fadeOutTime = 1.0f;
    string as1IsPlayeing { 
        get {
            if (audioSource1.isPlaying)
                return "Playing";
            else
                return "Stop";
        } 
    }
    string as2IsPlayeing
    {
        get
        {
            if (audioSource2.isPlaying)
                return "Playing";
            else
                return "Stop";
        }
    }
    float cycleTimer;
    void Start()
    {
        audioSource1.playOnAwake = false;
        audioSource2.playOnAwake = false;
        audioSource1.loop = audioSource2.loop = false;
        audioSource1.panStereo = audioSource2.panStereo = Mathf.Cos((float)AudioSettings.dspTime * panStereoSpeed) * panStereoDistance;
        audioSource1.Play();
        cycleTimer = 0;
        StartCoroutine(StartMenuCoroutine());
    }
    IEnumerator StartMenuCoroutine()
    {
        while (true)
        {
            if(Input.anyKeyDown)
            {
                cycleTimer = 0;
                break;
            }
            audioSource1.panStereo = audioSource2.panStereo = Mathf.Cos((float)AudioSettings.dspTime * panStereoSpeed) * panStereoDistance;
            if (cycleTimer >= 40f)
            {
                if (audioSource1.isPlaying)
                {
                    audioSource2.Play();
                }
                else
                {
                    audioSource1.Play();
                }
                cycleTimer = 0;
            }
            musicInfo = $"timer: {cycleTimer}\n" +
                $"audioSource1:{as1IsPlayeing}\n" +
                $"audioSource2:{as2IsPlayeing}";
            cycleTimer += Time.deltaTime;
            yield return null;
        }
        AudioSource audioSourcePlaying = null;
        if(audioSource1.isPlaying)
            audioSourcePlaying = audioSource1;
        else
            audioSourcePlaying = audioSource2;
        float startVolume = audioSourcePlaying.volume;
        if (audioSourcePlaying != null)
            for (float i = 0; i < 1; i += Time.deltaTime / fadeOutTime)
            {
                audioSourcePlaying.volume = (1 - i) * startVolume;
                yield return null;
            }
        audioSourcePlaying.Stop();
        SceneManager.LoadSceneAsync("Battle Scene");
    }
}
