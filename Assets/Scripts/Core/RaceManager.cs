using System;
using UnityEngine;
using WanganMidnight;

namespace WanganMidnight.Core
{
    public enum RacePhase
    {
        PreRace,
        Countdown,
        Racing,
        Finished
    }

    /// <summary>
    /// Central race state machine. Manages race phases, tracks progress, and detects win/loss.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [SerializeField] private CarController carController;
        [SerializeField] private PlayerInputHandler playerInputHandler;
        [SerializeField] private RivalAIController rivalAIController;
        [SerializeField] private WaypointPath waypointPath;

        public RacePhase CurrentPhase { get; private set; } = RacePhase.PreRace;
        public float PlayerDistanceAlongPath { get; private set; }
        public float RivalDistanceAlongPath { get; private set; }
        public float RaceGoalDistance => waypointPath != null ? waypointPath.TotalLength : 0f;
        public float DistanceToGoal => Mathf.Max(0f, RaceGoalDistance - PlayerDistanceAlongPath);

        public RivalData CurrentRival { get; private set; }

        public event Action<bool> OnRaceFinished;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>Stores rival data, sets phase to PreRace, and positions cars at the start.</summary>
        public void InitializeRace(RivalData rival)
        {
            CurrentRival = rival;
            CurrentPhase = RacePhase.PreRace;
            PlayerDistanceAlongPath = 0f;
            RivalDistanceAlongPath = 0f;
            LockInputs(true);
        }

        /// <summary>Transitions from PreRace → Countdown → Racing. Called by RaceStartSequence after the countdown.</summary>
        public void StartRace()
        {
            CurrentPhase = RacePhase.Countdown;
            // One-frame buffer then move to Racing.
            CurrentPhase = RacePhase.Racing;
            LockInputs(false);
        }

        /// <summary>Toggles the PlayerInputHandler component to lock or unlock driving inputs.</summary>
        public void LockInputs(bool locked)
        {
            if (playerInputHandler != null)
                playerInputHandler.enabled = !locked;

            if (locked && carController != null)
                carController.SetInputs(0f, 0f, false);
        }

        private void Update()
        {
            if (CurrentPhase != RacePhase.Racing) return;

            if (carController != null)
                PlayerDistanceAlongPath = waypointPath.GetClosestDistance(carController.transform.position);

            if (rivalAIController != null)
                RivalDistanceAlongPath = rivalAIController.DistanceAlongPath;

            CheckFinish();
        }

        private void CheckFinish()
        {
            bool playerFinished = PlayerDistanceAlongPath >= RaceGoalDistance;
            bool rivalFinished = RivalDistanceAlongPath >= RaceGoalDistance;

            if (playerFinished || rivalFinished)
            {
                CurrentPhase = RacePhase.Finished;
                LockInputs(true);
                OnRaceFinished?.Invoke(playerFinished);
            }
        }
    }
}
