using UnityEngine;
using System;
using System.Collections.Generic;

public class AnimationHandler {
    private Animation animation;

    private struct AnimationEntry {
        public string id;
        public bool loop;
        public float crossFade;
        public string sequenceId;
    }

    private List<AnimationEntry> queue = new List<AnimationEntry>();
    private string animationInProgress;
    private string sequenceInProgress = "*";
    private int sequenceIndex;

    public event Action onAnimationStart;
    public event Action onAnimationEnd;

    public AnimationHandler(Animation _animation) => animation = _animation;

    public void Update() {
        if (queue.Count == 0) return;
        if (sequenceIndex >= queue.Count) {
            sequenceIndex = 0;
            queue.Clear();
            return;
        }
        if (!queue[sequenceIndex].loop && !animation.IsPlaying(animationInProgress)) {
            onAnimationEnd?.Invoke();
            animationInProgress = "";
            if (++sequenceIndex >= queue.Count) {
                sequenceIndex = 0;
                queue.Clear();
                return;
            }
        }
        Play(sequenceIndex);
    }

    public void Queue(string _id, bool _loop, float _crossFade = 0, string _sequenceId = "") {
        AnimationEntry animationEntry = new AnimationEntry {
            id = _id,
            loop = _loop,
            crossFade = _crossFade,
            sequenceId = _sequenceId,
        };
        queue.Add(animationEntry);
    }
    public void Reset(string _id, bool _loop, float _crossFade = 0, string _sequenceId = "") {
        queue.Clear();
        onAnimationStart = null;
        onAnimationEnd = null;
        sequenceInProgress = _sequenceId;
        Queue(_id, _loop, _crossFade, _sequenceId);
    }

    private void Play(int index) {
        if (index >= queue.Count) {
            Debug.LogError($"Tried to play animation of index {index} that does not exist.");
            return;
        }
        AnimationEntry animationEntry = queue[index];
        if (animationEntry.crossFade == 0)
            animation.Play(animationEntry.id);
        else
            animation.CrossFade(animationEntry.id, animationEntry.crossFade);
        animationInProgress = animationEntry.id;
        onAnimationStart?.Invoke();
        if (animationEntry.sequenceId != sequenceInProgress || animationEntry.sequenceId == "")
            sequenceIndex = 0;
        sequenceInProgress = animationEntry.sequenceId;
    }
}