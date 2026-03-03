using UnityEngine;

namespace WanganMidnight
{
    /// <summary>Rigidbody-based arcade car physics for the player car.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        private const float MaxSpeedKph = 350f;
        private const float AccelerationForce = 12000f;
        private const float BrakeForce = 20000f;
        private const float GripStrength = 8f;
        private const float DownforceMultiplier = 2.5f;
        private const float SteerSensitivity = 60f;

        [SerializeField] private float[] gearSpeedThresholds = { 40f, 80f, 130f, 180f, 230f, 290f, 350f };
        [SerializeField] private Transform[] groundCheckTransforms;

        private Rigidbody _rb;
        private float _throttle;
        private float _steer;
        private bool _brake;

        public float SpeedKph => _rb != null ? _rb.linearVelocity.magnitude * 3.6f : 0f;
        public int CurrentGear { get; private set; } = 1;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.mass = 1400f;
            _rb.linearDamping = 0.05f;
            _rb.angularDamping = 5f;
            _rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>Sets the current driving inputs. Called by PlayerInputHandler or locked to zero during sequence.</summary>
        public void SetInputs(float throttle, float steer, bool brake)
        {
            _throttle = throttle;
            _steer = steer;
            _brake = brake;
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyDrive();
            ApplyGrip();
            ApplyBraking();
            ApplyDownforce();
            ClampSpeed();
            UpdateGear();
        }

        private void CheckGround()
        {
            IsGrounded = false;
            if (groundCheckTransforms == null) return;
            foreach (Transform t in groundCheckTransforms)
            {
                if (Physics.Raycast(t.position, Vector3.down, 0.3f))
                {
                    IsGrounded = true;
                    break;
                }
            }
        }

        private void ApplyDrive()
        {
            if (_throttle > 0f && IsGrounded)
                _rb.AddForce(transform.forward * (_throttle * AccelerationForce), ForceMode.Force);

            float speedRatio = Mathf.Clamp01(SpeedKph / MaxSpeedKph);
            float steerAngle = _steer * SteerSensitivity * (1f - speedRatio * 0.7f) * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up, steerAngle);
        }

        private void ApplyGrip()
        {
            Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
            localVel.x = Mathf.Lerp(localVel.x, 0f, GripStrength * Time.fixedDeltaTime);
            _rb.linearVelocity = transform.TransformDirection(localVel);
        }

        private void ApplyBraking()
        {
            if (_brake)
                _rb.AddForce(-_rb.linearVelocity.normalized * BrakeForce * Time.fixedDeltaTime, ForceMode.Force);
        }

        private void ApplyDownforce()
        {
            float speedRatio = Mathf.Clamp01(SpeedKph / MaxSpeedKph);
            _rb.AddForce(Vector3.down * (DownforceMultiplier * speedRatio * _rb.mass), ForceMode.Force);
        }

        private void ClampSpeed()
        {
            if (SpeedKph > MaxSpeedKph)
                _rb.linearVelocity = _rb.linearVelocity.normalized * (MaxSpeedKph / 3.6f);
        }

        private void UpdateGear()
        {
            for (int i = 0; i < gearSpeedThresholds.Length; i++)
            {
                if (SpeedKph < gearSpeedThresholds[i])
                {
                    CurrentGear = i + 1;
                    return;
                }
            }
            CurrentGear = gearSpeedThresholds.Length;
        }
    }
}
