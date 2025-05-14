using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("Animation Event/Audio/Entity")]
public class AnimEventAudioEntity : MonoBehaviour {
    [SerializeField] private EntityAudioData audioData;

    public void Spawn() {
        audioData.Spawn();
    }
    public async Task Footstep() {
        await audioData.Footstep();
    }
    public void Attack() {
        audioData.Attack();
    }
    public void Die() {
        audioData.Die();
    }
};