using UnityEngine;

public class GrassAmbientController : MonoBehaviour
{
    [Header("草地环境音效")]
    public AudioClip[] grassAmbientSounds;
    public float minDelay = 10f;
    public float maxDelay = 30f;
    public float minVolume = 0.1f;
    public float maxVolume = 0.3f;

    private AudioSource audioSource;
    private float nextPlayTime;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0; // 2D音效
        CalculateNextPlayTime();
    }

    void Update()
    {
        if (Time.time >= nextPlayTime)
        {
            PlayRandomGrassSound();
            CalculateNextPlayTime();
        }
    }

    private void PlayRandomGrassSound()
    {
        if (grassAmbientSounds.Length == 0) return;

        AudioClip clip = grassAmbientSounds[Random.Range(0, grassAmbientSounds.Length)];
        float volume = Random.Range(minVolume, maxVolume);
        audioSource.PlayOneShot(clip, volume);
    }

    private void CalculateNextPlayTime()
    {
        nextPlayTime = Time.time + Random.Range(minDelay, maxDelay);
    }
}