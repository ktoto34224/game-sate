using UnityEngine;

namespace PeakClimber
{
    public class ParallaxBackground : MonoBehaviour
    {
        public Transform camTransform;
        public float parallaxFactorY = 0.5f;
        public float parallaxFactorX = 0.2f;

        private Vector3 lastCamPos;

        private void Start()
        {
            if (camTransform == null && Camera.main != null)
            {
                camTransform = Camera.main.transform;
            }
            if (camTransform != null)
            {
                lastCamPos = camTransform.position;
            }
        }

        private void LateUpdate()
        {
            if (camTransform == null) return;

            Vector3 delta = camTransform.position - lastCamPos;
            transform.position += new Vector3(delta.x * parallaxFactorX, delta.y * parallaxFactorY, 0f);
            lastCamPos = camTransform.position;
        }
    }
}
