using UnityEngine;

namespace PeakClimber
{
    public class Piton : MonoBehaviour
    {
        public bool isPlayerAttached = false;
        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }
            sr.sprite = ProceduralSpriteGenerator.CreatePitonSprite();
            sr.sortingOrder = 5;

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.6f;
            }
        }
    }
}
