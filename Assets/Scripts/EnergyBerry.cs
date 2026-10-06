using UnityEngine;

namespace PeakClimber
{
    [ExecuteAlways]
    public class EnergyBerry : MonoBehaviour
    {
        public float staminaRestoreAmount = 45f;
        private Vector3 startPos;
        private SpriteRenderer sr;
        private bool collected = false;

        private void Awake()
        {
            EnsureVisual();
        }

        private void OnValidate()
        {
            EnsureVisual();
        }

        private void EnsureVisual()
        {
            startPos = transform.position;
            if (sr == null)
            {
                sr = GetComponent<SpriteRenderer>();
                if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            }
            if (sr.sprite == null)
            {
                sr.sprite = ProceduralSpriteGenerator.CreateEnergyBerrySprite();
            }
            sr.sortingOrder = 6;

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.4f;
            }
        }

        private void Update()
        {
            if (collected || !Application.isPlaying) return;
            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
            transform.localScale = new Vector3(pulse, pulse, 1f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected) return;
            var scout = collision.GetComponent<ScoutClimber>();
            if (scout != null)
            {
                collected = true;
                scout.RestoreStamina(staminaRestoreAmount);
                if (SoundManager.Instance != null) SoundManager.Instance.PlayBerry();
                Destroy(gameObject);
            }
        }
    }
}
