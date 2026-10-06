using UnityEngine;

namespace PeakClimber
{
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 2f, -10f);
        public float smoothSpeed = 4f;
        public float verticalLead = 3.5f;

        private Camera cam;
        private ScoutClimber scout;
        private float targetOrthoSize = 7.5f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = targetOrthoSize;
            }
        }

        private void Start()
        {
            if (target == null)
            {
                var s = FindAnyObjectByType<ScoutClimber>();
                if (s != null)
                {
                    target = s.transform;
                    scout = s;
                }
            }
            else
            {
                scout = target.GetComponent<ScoutClimber>();
            }

            if (target != null)
            {
                transform.position = new Vector3(target.position.x, target.position.y + offset.y, offset.z);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float yLead = 0f;
            if (scout != null && scout.State == ScoutState.Climbing)
            {
                yLead = verticalLead;
                targetOrthoSize = 6.8f; // Closer focus during climbing
            }
            else if (scout != null && scout.State == ScoutState.ExhaustedFall)
            {
                yLead = -2.5f;
                targetOrthoSize = 8.5f; // Zoom out during falling
            }
            else
            {
                targetOrthoSize = 7.5f;
            }

            Vector3 desiredPosition = new Vector3(
                Mathf.Clamp(target.position.x + offset.x, -14f, 14f),
                target.position.y + offset.y + yLead,
                offset.z
            );

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, Time.deltaTime * 3f);
            }
        }
    }
}
