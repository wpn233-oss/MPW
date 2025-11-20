using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(TerrainTextureDetector))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("音效设置")]
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float minSpeedForFootsteps = 0.1f;

    [Header("音量调整")]
    [Range(0f, 1f)] public float grassVolume = 0.8f;
    [Range(0f, 1f)] public float otherVolume = 0.6f;

    private PlayerController playerController;
    private TerrainTextureDetector textureDetector;
    private float stepTimer;
    private bool wasSprinting;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        textureDetector = GetComponent<TerrainTextureDetector>();

        if (textureDetector == null)
        {
            textureDetector = gameObject.AddComponent<TerrainTextureDetector>();
        }
    }

    void Update()
    {
        // 使用修复后的CanMove属性
        if (playerController == null || !playerController.CanMove) return;

        HandleFootstepSounds();
        HandleSprintSound();
    }

    private void HandleFootstepSounds()
    {
        if (playerController.CurrentHorizontalSpeed > minSpeedForFootsteps)
        {
            float interval = playerController.IsSprinting ? sprintStepInterval : walkStepInterval;

            stepTimer += Time.deltaTime;
            if (stepTimer >= interval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void HandleSprintSound()
    {
        if (playerController.IsSprinting && !wasSprinting)
        {
            SoundManager.Instance.PlaySprintSound();
        }
        wasSprinting = playerController.IsSprinting;
    }

    private void PlayFootstep()
    {
        if (textureDetector == null) return;

        string textureName = textureDetector.CurrentTextureName;
        float volume = textureName.Contains("Grass") ? grassVolume : otherVolume;

        // 使用带音量参数的PlayFootstep方法
        SoundManager.Instance.PlayFootstep(textureName, volume);
    }
}