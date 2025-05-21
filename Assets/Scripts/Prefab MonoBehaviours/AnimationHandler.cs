using Mono.Cecil;
using UnityEngine;

public class AnimationHandler : MonoBehaviour {
    public Animation anim = new Animation();
    [SerializeField] private AnimationClip[] m_idle;
    [SerializeField] private AnimationClip[] m_attack;
    [SerializeField] private AnimationClip[] m_forward;
    [SerializeField] private AnimationClip[] m_backward;
    [SerializeField] private AnimationClip[] m_knockedBack;
    [SerializeField] private AnimationClip[] m_land;
    [SerializeField] private AnimationClip[] m_cower;
    [SerializeField] private AnimationClip[] m_die;
    [SerializeField] private AnimationClip[] m_victory;

    public AnimationClip idle {
        get => m_idle[Random.Range(0, m_idle.Length)];
    }
    public bool idleIsPlaying {
        get {
            foreach (AnimationClip clip in m_idle) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public AnimationClip attack {
        get => m_attack[Random.Range(0, m_attack.Length)];
    }
    public bool attackIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public AnimationClip forward {
        get => m_forward[Random.Range(0, m_forward.Length)];
    }
    public AnimationClip backward {
        get => m_backward[Random.Range(0, m_backward.Length)];
    }
    public AnimationClip knockedBack {
        get => m_knockedBack[Random.Range(0, m_knockedBack.Length)];
    }
    public bool knockedBackIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public AnimationClip land {
        get => m_land[Random.Range(0, m_land.Length)];
    }
    public bool landIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public AnimationClip cower {
        get => m_cower[Random.Range(0, m_cower.Length)];
    }
    public AnimationClip die {
        get => m_die[Random.Range(0, m_die.Length)];
    }
    public bool dieIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public AnimationClip victory {
        get => m_victory[Random.Range(0, m_victory.Length)];
    }
    public bool victoryIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }

    public void LoadClips() {
        foreach (AnimationClip clip in m_idle)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_attack)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_forward)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_backward)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_knockedBack)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_land)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_cower)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_die)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_victory)
            anim.AddClip(clip, clip.name);
    }
}