using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Bundle", menuName = "Audio Bundle")]
public class AudioBundle : ScriptableObject { // Audio Bundle
    public AudioClip[] audioClips;

    public AudioClip GetRandom() {
        if (audioClips.Length > 0)
            return audioClips[Random.Range(0, audioClips.Length)];
        Debug.LogWarning("Attempted to get random audio clip from array without any audio.");
        return null;
    }
}