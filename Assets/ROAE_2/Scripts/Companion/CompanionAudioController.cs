using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CompanionStateAudioClip
{
    public CompanionEmotionalState state = CompanionEmotionalState.Healthy;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 0.35f;
}

public class CompanionAudioController : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("One-shots")]
    [SerializeField] private AudioClip summonClip;
    [SerializeField] private AudioClip despawnClip;
    [SerializeField] private AudioClip pulseClip;

    [Header("Emotional Beds")]
    [SerializeField] private List<CompanionStateAudioClip> stateLoops = new List<CompanionStateAudioClip>();

    public void PlaySummon()
    {
        PlayOneShot(summonClip, 0.75f);
    }

    public void PlayDespawn()
    {
        PlayOneShot(despawnClip, 0.55f);
    }

    public void PlayPulse(float intensity)
    {
        PlayOneShot(pulseClip, Mathf.Lerp(0.15f, 0.45f, Mathf.Clamp01(intensity)));
    }

    public void SetEmotion(CompanionEmotionalState state)
    {
        if (loopSource == null)
            return;

        CompanionStateAudioClip match = null;
        for (int i = 0; i < stateLoops.Count; i++)
        {
            if (stateLoops[i] != null && stateLoops[i].state == state)
            {
                match = stateLoops[i];
                break;
            }
        }

        if (match == null || match.clip == null)
        {
            loopSource.Stop();
            loopSource.clip = null;
            return;
        }

        if (loopSource.clip == match.clip && loopSource.isPlaying)
        {
            loopSource.volume = match.volume;
            return;
        }

        loopSource.clip = match.clip;
        loopSource.loop = true;
        loopSource.volume = match.volume;
        loopSource.Play();
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || oneShotSource == null)
            return;

        oneShotSource.PlayOneShot(clip, volume);
    }
}
