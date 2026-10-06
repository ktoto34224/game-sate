using System.Collections;
using UnityEngine;

namespace PeakClimber
{
    public enum SurfaceType
    {
        NormalRock,
        Crumbling,
        Icy,
        Bouncy
    }

    public class ClimbableSurface : MonoBehaviour
    {
        public SurfaceType surfaceType = SurfaceType.NormalRock;
        public float staminaDrainMultiplier = 1.0f;
        public float bounceForce = 16f;

        [Header("Crumble Settings")]
        public float crumbleDelay = 1.2f;
        public float respawnDelay = 3.5f;

        private Collider2D col;
        private SpriteRenderer sr;
        private Vector3 originalPos;
        private bool isCrumbling = false;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            originalPos = transform.position;

            if (surfaceType == SurfaceType.Icy)
            {
                staminaDrainMultiplier = 2.0f;
            }
        }

        public void OnGrabbed(ScoutClimber scout)
        {
            if (surfaceType == SurfaceType.Crumbling && !isCrumbling)
            {
                StartCoroutine(CrumbleRoutine(scout));
            }
        }

        private IEnumerator CrumbleRoutine(ScoutClimber scout)
        {
            isCrumbling = true;
            float elapsed = 0f;

            // Shake warning
            while (elapsed < crumbleDelay)
            {
                float shake = Mathf.Sin(elapsed * 40f) * 0.08f;
                transform.position = originalPos + new Vector3(shake, 0f, 0f);
                if (sr != null)
                {
                    sr.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), elapsed / crumbleDelay);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;

            // Break!
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;

            if (scout != null && scout.CurrentClimbSurface == this)
            {
                scout.ForceReleaseGrip();
            }

            // Respawn after delay
            yield return new WaitForSeconds(respawnDelay);

            if (col != null) col.enabled = true;
            if (sr != null)
            {
                sr.enabled = true;
                sr.color = Color.white;
            }
            isCrumbling = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (surfaceType == SurfaceType.Bouncy)
            {
                var scout = collision.gameObject.GetComponent<ScoutClimber>();
                if (scout != null)
                {
                    scout.Bounce(bounceForce);
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayBounce();
                }
            }
        }
    }
}
