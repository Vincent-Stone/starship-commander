using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuBehavior : MonoBehaviour
{
    [SerializeField] AudioSource audioSource1;
    [SerializeField] AudioSource audioSource2;
    [SerializeField] string musicInfo;
    public Image fadeInImage;

    [Range(0,1)]
    [SerializeField] float panStereoDistance = 1.0f;
    [Range(0, 10)]
    [SerializeField] float panStereoSpeed = 1.0f;
    [Range(0, 10)]
    [SerializeField] float fadeOutTime = 1.0f;
    [SerializeField] float fadeInTime = 1.0f;
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
        for (float t = 1; t > 0; t -= Time.deltaTime / fadeInTime)
        {
            Color color = fadeInImage.color;
            color.a = t;
            fadeInImage.color = color;
            yield return null;
        }
        fadeInImage.gameObject.SetActive(false);
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
        fadeInImage.color = new Color(fadeInImage.color.r, fadeInImage.color.g, fadeInImage.color.b, 0);
        fadeInImage.gameObject.SetActive(true);
        if (audioSourcePlaying != null)
            for (float i = 0; i < 1; i += Time.deltaTime / fadeOutTime)
            {
                audioSourcePlaying.volume = (1 - i) * startVolume;
                Color color = fadeInImage.color;
                color.a = i;
                fadeInImage.color = color;
                yield return null;
            }
        fadeInImage.color = new Color(fadeInImage.color.r, fadeInImage.color.g, fadeInImage.color.b, 1);
        audioSourcePlaying.Stop();
        SceneManager.LoadSceneAsync("Battle Scene");
    }
}
