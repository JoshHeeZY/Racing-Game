using System.Collections;
using UnityEngine;
using WanganMidnight.Core;
using WanganMidnight.UI;

namespace WanganMidnight.Sequencing
{
    /// <summary>
    /// Coroutine-driven orchestrator for the full race start sequence.
    /// Flow: PreRaceScreen → fade → intro cameras → CountdownController → RaceManager.StartRace()
    /// </summary>
    public class RaceStartSequence : MonoBehaviour
    {
        [Header("Subsystems")]
        [SerializeField] private PreRaceScreen preRaceScreen;
        [SerializeField] private CountdownController countdown;
        [SerializeField] private ChaseCameraController chaseCamera;
        [SerializeField] private RaceManager raceManager;

        [Header("Intro Cameras")]
        [SerializeField] private Camera introCameraA;   // Side-angle cinematic cam
        [SerializeField] private Camera introCameraB;   // Front-angle cinematic cam

        [Header("Black Fade Panel")]
        [SerializeField] private CanvasGroup blackPanel;

        [Header("Race Config")]
        [SerializeField] private string routeName = "WANGAN LINE";
        [SerializeField] private float routeDistanceKm = 14.5f;
        [SerializeField] private float introDuration = 4f;

        private const float FadeSpeed = 0.5f;
        private const float FastFadeSpeed = 0.3f;

        private bool _playerIsReady;

        /// <summary>Kicks off the full pre-race flow.</summary>
        public void BeginSequence(RivalData rival)
        {
            StartCoroutine(BeginSequenceCoroutine(rival));
        }

        private IEnumerator BeginSequenceCoroutine(RivalData rival)
        {
            // Step 1: Lock inputs
            raceManager.InitializeRace(rival);

            // Step 2: Show pre-race screen, wait for ready
            _playerIsReady = false;
            preRaceScreen.OnPlayerReady += HandlePlayerReady;
            preRaceScreen.Show(rival, routeName, routeDistanceKm);
            yield return new WaitUntil(() => _playerIsReady);
            preRaceScreen.OnPlayerReady -= HandlePlayerReady;

            // Step 3: Fade to black
            preRaceScreen.Hide();
            yield return StartCoroutine(FadeCoroutine(blackPanel, 0f, 1f, FadeSpeed));

            // Step 4: Switch to introCameraA
            if (chaseCamera != null && chaseCamera.Camera != null)
                chaseCamera.Camera.enabled = false;

            if (introCameraA != null) introCameraA.enabled = true;
            if (introCameraB != null) introCameraB.enabled = false;

            yield return StartCoroutine(FadeCoroutine(blackPanel, 1f, 0f, FadeSpeed));

            // Step 5: Side-angle dolly for first half of introDuration
            yield return new WaitForSeconds(introDuration / 2f);

            // Step 6: Cut to introCameraB (front angle)
            if (introCameraA != null) introCameraA.enabled = false;
            if (introCameraB != null) introCameraB.enabled = true;

            yield return new WaitForSeconds(introDuration / 2f);

            // Step 7: Fade to black, switch back to chase camera, fade in
            yield return StartCoroutine(FadeCoroutine(blackPanel, 0f, 1f, FastFadeSpeed));

            if (introCameraB != null) introCameraB.enabled = false;
            if (chaseCamera != null && chaseCamera.Camera != null)
                chaseCamera.Camera.enabled = true;

            yield return StartCoroutine(FadeCoroutine(blackPanel, 1f, 0f, FastFadeSpeed));

            // Step 8: Countdown
            yield return StartCoroutine(countdown.PlayCountdown());

            // Step 9: Start race and unlock inputs
            raceManager.StartRace();
        }

        private void HandlePlayerReady()
        {
            _playerIsReady = true;
        }

        private IEnumerator FadeCoroutine(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration)
            {
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            group.alpha = to;
        }
    }
}
