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
    private string animationInProgress;

    public event Action onAnimationStart;
    public event Action onAnimationEnd;

    public AnimationHandler(Animation _animation) => animation = _animation;

    public void Update() {
        if (queue.Count == 0) return;
        if (!queue[0].loop && !animation.IsPlaying(animationInProgress)) {
            onAnimationEnd?.Invoke();
            animationInProgress = "";
            queue.RemoveAt(0);
        }
        AnimationEntry animationEntry = queue[0];
        if (animationEntry.id == animationInProgress) return;
        animationInProgress = animationEntry.id;
        if (animationEntry.crossFade == 0)
            animation.Play(animationEntry.id);
        else
            animation.CrossFade(animationEntry.id, animationEntry.crossFade);
        onAnimationStart?.Invoke();
    }

    private void Queue(string _id, bool _loop, float _crossFade = 0) {
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
        Queue(_id, _loop, _crossFade);
    }
    public void PlaySequence(params (string _id, bool _loop, float _crossFade)[] entries) {
        Play(entries[0]._id, entries[0]._loop, entries[0]._crossFade);
        for (int i = 1; i < entries.Length; i++) {
            Queue(entries[i]._id, entries[i]._loop, entries[i]._crossFade);
        }
    }
}