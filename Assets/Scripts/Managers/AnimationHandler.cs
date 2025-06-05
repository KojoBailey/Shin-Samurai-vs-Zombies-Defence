using UnityEngine;
using System;
using System.Collections.Generic;

public class AnimationHandler {
    private Animation animation;

    private struct AnimationEntry {
        public string id;
        public bool loop;
        public float crossFade;
    }

    private List<AnimationEntry> queue = new List<AnimationEntry>();
    private AnimationEntry animationInProgress;
    public string currentAnimation => animationInProgress.id;
    private bool playing;

    public event Action onAnimationStart;
    public event Action onAnimationEnd;

    public AnimationHandler(Animation _animation) => animation = _animation;

    public void Update() {
        if (animation.IsPlaying(animationInProgress.id) && playing) {
            return;
        } else if (!animationInProgress.loop && animationInProgress.id != "") {
            onAnimationEnd?.Invoke();
            animationInProgress.id = "";
        }
        if (queue.Count == 0) return;
        AnimationEntry animationEntry = queue[0];
        if (animationEntry.id == animationInProgress.id) return;
        animationInProgress = animationEntry;
        if (animationEntry.crossFade == 0)
            animation.Play(animationEntry.id);
        else
            animation.CrossFade(animationEntry.id, animationEntry.crossFade);
        onAnimationStart?.Invoke();
        queue.RemoveAt(0);
        playing = true;
    }

    private void Queue(string _id, bool _loop, float _crossFade = 0) {
        if (animation[_id] == null) {
            Debug.LogError($"Animation of name \"{_id}\" does not exist.");
            return;
        }
        AnimationEntry animationEntry = new AnimationEntry {
            id = _id,
            loop = _loop,
            crossFade = _crossFade,
        };
        queue.Add(animationEntry);
    }
    public void Play(string _id, bool _loop, float _crossFade = 0) {
        queue.Clear();
        onAnimationStart = null;
        onAnimationEnd = null;
        animationInProgress.id = "";
        playing = false;
        Queue(_id, _loop, _crossFade);
    }
    public void PlaySequence(params (string _id, bool _loop, float _crossFade)[] entries) {
        Play(entries[0]._id, entries[0]._loop, entries[0]._crossFade);
        for (int i = 1; i < entries.Length; i++) {
            Queue(entries[i]._id, entries[i]._loop, entries[i]._crossFade);
        }
    }
}