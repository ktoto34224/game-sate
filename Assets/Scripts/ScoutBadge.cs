using UnityEngine;

namespace PeakClimber
{
    [ExecuteAlways]
    public class ScoutBadge : MonoBehaviour
    {
        public string badgeName = "Cliff Scaler";
        public int badgeId = 1;

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
                sr.sprite = ProceduralSpriteGenerator.CreateBadgeSprite();
            }
            sr.sortingOrder = 6;

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }
        }

        private void Update()
        {
            if (collected || !Application.isPlaying) return;
            // Gentle hovering animation
            float hover = Mathf.Sin(Time.time * 3f + badgeId) * 0.12f;
            transform.position = startPos + new Vector3(0f, hover, 0f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected) return;
            var scout = collision.GetComponent<ScoutClimber>();
            if (scout != null)
            {
                collected = true;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayBadge();
                if (GameManager.Instance != null) GameManager.Instance.CollectBadge(badgeName);
                Destroy(gameObject);
            }
        }
    }
}
