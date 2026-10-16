using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    [Header("音频设置")] public AudioClip footstepClip; // 脚步声音频文件
    public float volume = 0.5f; // 音量大小
    public float moveThreshold = 0.1f; // 移动阈值

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float moveTimer = 0f;
    private float stepInterval = 0.5f; // 每隔多久可播放一次脚步声

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 播放速度
        audioSource.pitch = 1.5F;

        lastPosition = transform.position;
    }

    void Update()
    {
        bool isMoving = IsCharacterMoving();

        if (isMoving && !audioSource.isPlaying)
        {
            PlayFootstepSound();
        }
        else if (!isMoving && audioSource.isPlaying)
        {
            StopFootstepSound();
        }

        lastPosition = transform.position;
    }

    bool IsCharacterMoving()
    {
        // 直接比较当前位置与上一帧位置的距离
        float distance = (transform.position - lastPosition).magnitude;
        return distance > moveThreshold;
    }

    void PlayFootstepSound()
    {
        if (footstepClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = footstepClip;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    void StopFootstepSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}