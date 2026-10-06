using UnityEngine;

namespace PeakClimber
{
    [ExecuteAlways]
    public class CampfireCheckpoint : MonoBehaviour
    {
        public bool isLit = false;
        public int checkpointIndex = 0;
        public string campName = "Base Camp";

        private SpriteRenderer sr;

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
            if (sr == null)
            {
                sr = GetComponent<SpriteRenderer>();
                if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            }
            UpdateVisual();
        }

        public void ActivateCheckpoint(ScoutClimber scout)
        {
            if (!isLit)
            {
                isLit = true;
                UpdateVisual();
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayCampfire();
                }
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnCheckpointReached(this);
                }
            }
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                sr.sprite = ProceduralSpriteGenerator.CreateCampfireSprite(isLit);
                sr.sortingOrder = 4;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var scout = collision.GetComponent<ScoutClimber>();
            if (scout != null)
            {
                ActivateCheckpoint(scout);
            }
        }
    }
}
