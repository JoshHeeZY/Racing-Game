using UnityEngine;

namespace WanganMidnight
{
    /// <summary>
    /// Defines the highway route as an ordered array of waypoint Transforms.
    /// Shared by TrafficSpawner, RivalAIController, and RaceManager.
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        private static readonly float[] LaneOffsets = { -4.5f, 0f, 4.5f };

        private float[] _segmentDistances;
        private float _totalLength;

        public Transform[] Waypoints => waypoints;
        public float TotalLength => _totalLength;

        private void Awake()
        {
            BuildSegmentData();
        }

        private void BuildSegmentData()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            _segmentDistances = new float[waypoints.Length];
            _segmentDistances[0] = 0f;
            _totalLength = 0f;

            for (int i = 1; i < waypoints.Length; i++)
            {
                _totalLength += Vector3.Distance(waypoints[i - 1].position, waypoints[i].position);
                _segmentDistances[i] = _totalLength;
            }
        }

        /// <summary>Returns the interpolated world position at a given distance along the path.</summary>
        public Vector3 GetPositionAtDistance(float distance)
        {
            GetSegmentAtDistance(distance, out int segIndex, out float t);
            return Vector3.Lerp(waypoints[segIndex].position, waypoints[segIndex + 1].position, t);
        }

        /// <summary>Returns the interpolated forward rotation at a given distance along the path.</summary>
        public Quaternion GetRotationAtDistance(float distance)
        {
            GetSegmentAtDistance(distance, out int segIndex, out _);
            Vector3 dir = waypoints[segIndex + 1].position - waypoints[segIndex].position;
            return dir != Vector3.zero ? Quaternion.LookRotation(dir) : Quaternion.identity;
        }

        /// <summary>Projects a world position onto the path and returns the closest distance.</summary>
        public float GetClosestDistance(Vector3 worldPos)
        {
            float closestDist = 0f;
            float closestSqr = float.MaxValue;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Vector3 a = waypoints[i].position;
                Vector3 b = waypoints[i + 1].position;
                float t = Mathf.Clamp01(Vector3.Dot(worldPos - a, b - a) / (b - a).sqrMagnitude);
                Vector3 closest = Vector3.Lerp(a, b, t);
                float sqr = (worldPos - closest).sqrMagnitude;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closestDist = _segmentDistances[i] + t * Vector3.Distance(a, b);
                }
            }

            return closestDist;
        }

        /// <summary>Returns the lateral world offset for a given lane at a distance along the path.</summary>
        public float GetLaneOffset(int laneIndex, float distance)
        {
            laneIndex = Mathf.Clamp(laneIndex, 0, LaneOffsets.Length - 1);
            return LaneOffsets[laneIndex];
        }

        private void GetSegmentAtDistance(float distance, out int segIndex, out float t)
        {
            distance = Mathf.Clamp(distance, 0f, _totalLength);
            int lo = 0, hi = _segmentDistances.Length - 2;

            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (_segmentDistances[mid] <= distance) lo = mid;
                else hi = mid - 1;
            }

            segIndex = lo;
            float segLen = _segmentDistances[lo + 1] - _segmentDistances[lo];
            t = segLen > 0f ? (distance - _segmentDistances[lo]) / segLen : 0f;
        }
    }
}
