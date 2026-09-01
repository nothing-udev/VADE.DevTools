using System;
using System.Collections;
using UnityEngine;
using VADE.DevTools.Utilities;

namespace VADE.DevTools.Audio
{
    public class PooledAudioSource
    {
        public GameObject GameObject { get; }
        public AudioSource Source { get; }

        private readonly CoroutineRunner coroutineRunner;
        private Action<PooledAudioSource> onRelease;

        public PooledAudioSource(Transform parent)
        {
            GameObject = new GameObject("PooledAudioSource");
            GameObject.transform.SetParent(parent);

            Source = GameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;

            coroutineRunner = GameObject.AddComponent<CoroutineRunner>();
            GameObject.SetActive(false);
        }

        public void Play(AudioData data, Vector3 position, Action<PooledAudioSource> releaseCallback)
        {
            GameObject.SetActive(true);
            GameObject.transform.position = position;

            var currentClip = data.clips[UnityEngine.Random.Range(0, data.clips.Length)];
            Source.clip = currentClip;
            Source.volume = data.volume;
            Source.loop = data.loop;
            Source.Play();

            onRelease = releaseCallback;

            if (!data.loop)
                coroutineRunner.Run(ReleaseAfter(currentClip.length));
            else if (data.loopDuration > 0f)
                coroutineRunner.Run(ReleaseAfter(data.loopDuration));
        }

        private IEnumerator ReleaseAfter(float time)
        {
            yield return new WaitForSeconds(time);
            Stop();
        }

        public void Stop()
        {
            Source.Stop();
            GameObject.SetActive(false);
            onRelease?.Invoke(this);
        }
    }
}
