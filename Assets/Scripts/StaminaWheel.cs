using UnityEngine;

namespace PeakClimber
{
    public class StaminaWheel : MonoBehaviour
    {
        private ScoutClimber scout;
        private SpriteRenderer bgRenderer;
        private SpriteRenderer fillRenderer;
        private Transform fillTransform;

        private Color fullColor = new Color(0.18f, 0.85f, 0.35f, 1f);
        private Color midColor = new Color(0.95f, 0.78f, 0.15f, 1f);
        private Color lowColor = new Color(0.95f, 0.22f, 0.22f, 1f);

        private float currentFill = 1f;

        private void Awake()
        {
            scout = GetComponentInParent<ScoutClimber>();

            // Create background ring
            GameObject bgGo = new GameObject("WheelBG");
            bgGo.transform.SetParent(transform);
            bgGo.transform.localPosition = Vector3.zero;
            bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = ProceduralSpriteGenerator.CreateRingSprite(20, 4, new Color(0f, 0f, 0f, 0.55f));
            bgRenderer.sortingOrder = 10;

            // Create fill indicator
            GameObject fillGo = new GameObject("WheelFill");
            fillGo.transform.SetParent(transform);
            fillGo.transform.localPosition = Vector3.zero;
            fillTransform = fillGo.transform;
            fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = ProceduralSpriteGenerator.CreateCircleSprite(14, fullColor);
            fillRenderer.sortingOrder = 11;

            transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }

        private void Update()
        {
            if (scout == null) return;

            float ratio = Mathf.Clamp01(scout.currentStamina / scout.maxStamina);
            currentFill = Mathf.Lerp(currentFill, ratio, Time.deltaTime * 12f);

            // Hide wheel when full and grounded
            bool shouldShow = (ratio < 0.98f) || (scout.State == ScoutState.Climbing) || (scout.State == ScoutState.AnchoredPiton);
            float targetAlpha = shouldShow ? 1f : 0f;

            Color c = (ratio > 0.5f) ? Color.Lerp(midColor, fullColor, (ratio - 0.5f) * 2f) : Color.Lerp(lowColor, midColor, ratio * 2f);

            if (ratio < 0.25f && shouldShow)
            {
                // Panic flash and pulse
                float pulse = 1f + Mathf.Sin(Time.time * 16f) * 0.15f;
                transform.localScale = new Vector3(0.6f * pulse, 0.6f * pulse, 1f);
                if (Mathf.Sin(Time.time * 16f) > 0f) c = Color.white;
            }
            else
            {
                transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            }

            c.a *= targetAlpha;
            fillRenderer.color = c;

            Color bgC = new Color(0f, 0f, 0f, 0.55f * targetAlpha);
            bgRenderer.color = bgC;

            // Scale fill radius by stamina
            fillTransform.localScale = new Vector3(Mathf.Max(0.05f, currentFill), Mathf.Max(0.05f, currentFill), 1f);
        }
    }
}
