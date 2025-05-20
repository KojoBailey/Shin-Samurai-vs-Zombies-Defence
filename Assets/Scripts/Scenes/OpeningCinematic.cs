using UnityEngine;
using UnityEngine.Video;

public class OpeningCinematic : MonoBehaviour { // Opening Cinematic
    [SerializeField] private VideoPlayer m_videoPlayer;
	[SerializeField] private AudioSource m_audioSource;
    private bool m_videoStarted = false;

    void Start() {
        m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        m_videoPlayer.EnableAudioTrack(0, true);
        m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
        m_videoPlayer.timeUpdateMode = VideoTimeUpdateMode.DSPTime;

        m_videoPlayer.prepareCompleted += OnVideoPrepared;
        m_videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp) {
        vp.Play();  // Starts video + synced audio
        m_videoStarted = true;
    }

    void Update() {
        if (m_videoStarted && (!m_videoPlayer.isPlaying || Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return)))
			SceneLoadManager.LoadScene("TitleScreen", false);
    }
}
