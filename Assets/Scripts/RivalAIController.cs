using UnityEngine;
using WanganMidnight.Core;

namespace WanganMidnight
{
    /// <summary>
    /// Controls the rival car. Follows WaypointPath with a rubber-band speed system.
    /// RivalData drives all display info for UI and HUD.
    /// </summary>
    public class RivalAIController : MonoBehaviour
    {
        private const float BaseRivalSpeedKph = 280f;
        private const float RubberBandFactor = 0.15f;
        private const float MinSpeed = 220f;
        private const float MaxSpeed = 340f;
        private const float Acceleration = 20f;
        private const int DefaultLane = 1; // Centre lane

        [SerializeField] private WaypointPath waypointPath;
        [SerializeField] private RivalData rivalData;

        private float _currentSpeed;
        private float _currentDistance;

        public float DistanceAlongPath => _currentDistance;
        public float SpeedKph => _currentSpeed;
        public string RivalName => rivalData != null ? rivalData.rivalName : string.Empty;
        public string CarName => rivalData != null ? rivalData.carName : string.Empty;

        private void Update()
        {
            if (waypointPath == null) return;
            if (RaceManager.Instance == null || RaceManager.Instance.CurrentPhase != Core.RacePhase.Racing) return;

            float gap = RaceManager.Instance.PlayerDistanceAlongPath - _currentDistance;
            float targetSpeed = Mathf.Clamp(BaseRivalSpeedKph + gap * RubberBandFactor, MinSpeed, MaxSpeed);
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Acceleration * Time.deltaTime);

            _currentDistance += (_currentSpeed / 3.6f) * Time.deltaTime;

            float laneOffset = waypointPath.GetLaneOffset(DefaultLane, _currentDistance);
            Vector3 pos = waypointPath.GetPositionAtDistance(_currentDistance);
            Quaternion rot = waypointPath.GetRotationAtDistance(_currentDistance);

            transform.position = pos + rot * new Vector3(laneOffset, 0f, 0f);
            transform.rotation = rot;
        }
    }
}
