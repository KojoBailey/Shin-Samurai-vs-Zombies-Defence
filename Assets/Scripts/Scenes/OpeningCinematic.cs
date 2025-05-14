using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OpeningCinematic : MonoBehaviour {
    [SerializeField] private VideoPlayer videoPlayer;
	[SerializeField] private AudioSource audioSource;
    private bool videoStarted = false;

    void Start() {
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.DSPTime;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp) {
        vp.Play();  // Starts video + synced audio
        videoStarted = true;
    }

    void Update() {
        if (videoStarted && (!videoPlayer.isPlaying || Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return)))
			SceneLoadManager.LoadScene("TitleScreen", false);
    }
}
