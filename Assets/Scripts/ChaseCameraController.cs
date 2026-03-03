using UnityEngine;

namespace WanganMidnight
{
    /// <summary>Smooth third-person chase camera that follows the player car.</summary>
    public class ChaseCameraController : MonoBehaviour
    {
        private static readonly Vector3 Offset = new Vector3(0f, 1.8f, -5.5f);
        private static readonly Vector3 LookAheadOffset = new Vector3(0f, 0.5f, 10f);
        private const float BaseFov = 65f;
        private const float MaxFov = 85f;
        private const float MaxSpeedKph = 350f;
        private const float SmoothTime = 0.12f;

        [SerializeField] private CarController target;

        private Camera _camera;
        private Vector3 _velocity;

        /// <summary>The Camera component on this GameObject.</summary>
        public Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>Assigns a new follow target.</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget.GetComponent<CarController>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Transform t = target.transform;
            Vector3 desiredPosition = t.position + t.TransformDirection(Offset);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, SmoothTime);

            Vector3 lookTarget = t.position + t.TransformDirection(LookAheadOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * 10f);

            float speedRatio = Mathf.Clamp01(target.SpeedKph / MaxSpeedKph);
            _camera.fieldOfView = Mathf.Lerp(BaseFov, MaxFov, speedRatio);
        }
    }
}
