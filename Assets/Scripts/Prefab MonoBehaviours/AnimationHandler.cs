using Mono.Cecil;
using UnityEngine;

public class AnimationHandler : MonoBehaviour {
    public Animation anim = new Animation();
    [SerializeField] private AnimationClip[] m_idle;
    [SerializeField] private AnimationClip[] m_idleRanged;
    [SerializeField] private AnimationClip[] m_attack;
    [SerializeField] private AnimationClip[] m_attackRanged;
    [SerializeField] private AnimationClip[] m_forward;
    [SerializeField] private AnimationClip[] m_backward;
    [SerializeField] private AnimationClip[] m_backpedal;
    [SerializeField] private AnimationClip[] m_backpedalTurn;
    [SerializeField] private AnimationClip[] m_castForward;
    [SerializeField] private AnimationClip[] m_castMid;
    [SerializeField] private GenericDictionary<string, AnimationClip> m_abilities;
    [SerializeField] private AnimationClip[] m_knockedBack;
    [SerializeField] private AnimationClip[] m_land;
    [SerializeField] private AnimationClip[] m_cower;
    [SerializeField] private AnimationClip[] m_stunned;
    [SerializeField] private AnimationClip[] m_die;
    [SerializeField] private AnimationClip[] m_revive;
    [SerializeField] private AnimationClip[] m_victory;

    public string idle {
        get => m_idle[Random.Range(0, m_idle.Length)].name;
    }
    public bool idleIsPlaying {
        get {
            foreach (AnimationClip clip in m_idle) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string idleRanged {
        get => m_idleRanged[Random.Range(0, m_idleRanged.Length)].name;
    }
    public bool idleRangedIsPlaying {
        get {
            foreach (AnimationClip clip in m_idleRanged) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string attack {
        get => m_attack[Random.Range(0, m_attack.Length)].name;
    }
    public bool attackIsPlaying {
        get {
            foreach (AnimationClip clip in m_attack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string attackRanged {
        get => m_attackRanged[Random.Range(0, m_attackRanged.Length)].name;
    }
    public bool attackRangedIsPlaying {
        get {
            foreach (AnimationClip clip in m_attackRanged) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string castForward {
        get => m_castForward[Random.Range(0, m_castForward.Length)].name;
    }
    public bool castForwardIsPlaying {
        get {
            foreach (AnimationClip clip in m_castForward) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string castMid {
        get => m_castMid[Random.Range(0, m_castMid.Length)].name;
    }
    public bool castMidIsPlaying {
        get {
            foreach (AnimationClip clip in m_castMid) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string forward {
        get => m_forward[Random.Range(0, m_forward.Length)].name;
    }
    public bool forwardIsPlaying {
        get {
            foreach (AnimationClip clip in m_forward) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string backward {
        get => m_backward[Random.Range(0, m_backward.Length)].name;
    }
    public bool backwardIsPlaying {
        get {
            foreach (AnimationClip clip in m_backward) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string backpedal {
        get => m_backpedal[Random.Range(0, m_backpedal.Length)].name;
    }
    public bool backpedalIsPlaying {
        get {
            foreach (AnimationClip clip in m_backpedal) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string backpedalTurn {
        get => m_backpedalTurn[Random.Range(0, m_backpedalTurn.Length)].name;
    }
    public bool backpedalTurnIsPlaying {
        get {
            foreach (AnimationClip clip in m_backpedalTurn) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string knockedBack {
        get => m_knockedBack[Random.Range(0, m_knockedBack.Length)].name;
    }
    public bool knockedBackIsPlaying {
        get {
            foreach (AnimationClip clip in m_knockedBack) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string land {
        get => m_land[Random.Range(0, m_land.Length)].name;
    }
    public bool landIsPlaying {
        get {
            foreach (AnimationClip clip in m_land) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string cower {
        get => m_cower[Random.Range(0, m_cower.Length)].name;
    }
    public bool cowerIsPlaying {
        get {
            foreach (AnimationClip clip in m_cower) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string die {
        get => m_die[Random.Range(0, m_die.Length)].name;
    }
    public bool dieIsPlaying {
        get {
            foreach (AnimationClip clip in m_die) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }
    public string victory {
        get => m_victory[Random.Range(0, m_victory.Length)].name;
    }
    public bool victoryIsPlaying {
        get {
            foreach (AnimationClip clip in m_victory) {
                if (anim.IsPlaying(clip.name)) return true;
            }
            return false;
        }
    }

    public string GetAbility(string abilityId) {
        return m_abilities[abilityId].name;
    }
    public bool IsAbilityPlaying(string abilityId) {
        return anim.IsPlaying(GetAbility(abilityId));
    }

    public void LoadClips() {
        foreach (AnimationClip clip in m_idle)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_idleRanged)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_attack)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_attackRanged)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_castForward)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_castMid)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_abilities.Values)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_forward)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_backward)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_backpedal)
            anim.AddClip(clip, clip.name);
        foreach (AnimationClip clip in m_backpedalTurn)
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