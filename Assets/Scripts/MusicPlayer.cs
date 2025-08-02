using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource introSource, loopSource;

    void Start()
    {
        // Bắt đầu chơi clip intro ngay lập tức
        introSource.Play();

        // Lên lịch để clip loop bắt đầu ngay khi clip intro kết thúc
        loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
    }
}