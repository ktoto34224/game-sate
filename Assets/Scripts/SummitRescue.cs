using System.Collections;
using UnityEngine;

namespace PeakClimber
{
    public class SummitRescue : MonoBehaviour
    {
        public Transform rotorTransform;
        public Transform helicopterBody;
        public float rotorSpeed = 1200f;

        private bool rescueTriggered = false;
        private Vector3 heliBasePos;

        private void Awake()
        {
            if (helicopterBody != null)
            {
                heliBasePos = helicopterBody.position;
            }
        }

        private void Update()
        {
            // Spin helicopter rotor
            if (rotorTransform != null)
            {
                rotorTransform.Rotate(0f, 0f, rotorSpeed * Time.deltaTime);
            }

            // Hover bobbing
            if (helicopterBody != null)
            {
                float bob = Mathf.Sin(Time.time * 2f) * 0.25f;
                helicopterBody.position = heliBasePos + new Vector3(0f, bob, 0f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (rescueTriggered) return;
            var scout = collision.GetComponent<ScoutClimber>();
            if (scout != null)
            {
                rescueTriggered = true;
                StartCoroutine(RescueSequence(scout));
            }
        }

        private IEnumerator RescueSequence(ScoutClimber scout)
        {
            scout.TriggerVictory();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayVictory();
            }

            // Flare launch effect:
            GameObject flare = new GameObject("RescueFlare");
            flare.transform.position = scout.transform.position;
            var flareSr = flare.AddComponent<SpriteRenderer>();
            flareSr.sprite = ProceduralSpriteGenerator.CreateCircleSprite(8, Color.red);
            flareSr.sortingOrder = 20;

            float t = 0f;
            Vector3 startPos = scout.transform.position;
            Vector3 endPos = startPos + new Vector3(2f, 15f, 0f);

            while (t < 1.5f)
            {
                t += Time.deltaTime;
                flare.transform.position = Vector3.Lerp(startPos, endPos, t / 1.5f);
                yield return null;
            }

            Destroy(flare);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameWon();
            }
        }
    }
}
