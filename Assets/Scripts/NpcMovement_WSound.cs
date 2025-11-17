using UnityEngine;

namespace AiSims
{
    public class NpcMovement_WSound : NpcMovement
    {
        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip chaseClip;
        private bool isPlayingChaseSound = false;

        void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        // Called whenever NPC starts moving (thanks to base being virtual now)
        public override void StartMovement()
        {
            base.StartMovement();
            StartChasingTarget();   // ensure chase sound starts immediately
        }

        // Called whenever NPC begins chasing (base OR derived calls will use this)
        public override void StartChasingTarget(int speed = 2)
        {
            base.StartChasingTarget(speed);

            if (audioSource != null && chaseClip != null && !isPlayingChaseSound)
            {
                audioSource.clip = chaseClip;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingChaseSound = true;
            }
        }

        private void StopChaseSound()
        {
            if (audioSource != null && isPlayingChaseSound)
            {
                audioSource.Stop();
                isPlayingChaseSound = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            StopChaseSound();
            base.OnTriggerEnter(other);
        }
    }
}
