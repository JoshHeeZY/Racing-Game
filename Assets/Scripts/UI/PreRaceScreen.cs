using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanganMidnight;

namespace WanganMidnight.UI
{
    /// <summary>
    /// Full-screen pre-race briefing panel. Displays rival info and route details,
    /// then fires OnPlayerReady when the player confirms or the auto-start timer expires.
    /// </summary>
    public class PreRaceScreen : MonoBehaviour
    {
        [Header("Route Info")]
        [SerializeField] private TMP_Text routeNameText;
        [SerializeField] private TMP_Text routeDistanceText;

        [Header("Rival Info")]
        [SerializeField] private TMP_Text rivalNameText;
        [SerializeField] private TMP_Text rivalCarText;
        [SerializeField] private TMP_Text rivalHpText;
        [SerializeField] private Image rivalPortraitImage;
        [SerializeField] private Image rivalCarSilhouetteImage;

        [Header("Controls")]
        [SerializeField] private Button readyButton;
        [SerializeField] private TMP_Text autoStartTimerText;

        [Header("Panel")]
        [SerializeField] private CanvasGroup panelGroup;

        private const float AutoStartDuration = 5f;
        private const float HideFadeDuration = 0.4f;

        private Coroutine _autoStartCoroutine;

        public event Action OnPlayerReady;

        private void Awake()
        {
            readyButton.onClick.AddListener(HandleReady);
        }

        /// <summary>Populates all UI fields and starts the auto-start timer.</summary>
        public void Show(RivalData rival, string routeName, float routeDistanceKm)
        {
            routeNameText.text = routeName;
            routeDistanceText.text = $"{routeDistanceKm:0.0} km";

            rivalNameText.text = rival.rivalName;
            rivalCarText.text = rival.carName;
            rivalHpText.text = $"{rival.horsePower} HP";

            if (rival.portrait != null)
                rivalPortraitImage.sprite = rival.portrait;

            if (rival.carSilhouette != null)
                rivalCarSilhouetteImage.sprite = rival.carSilhouette;

            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
            gameObject.SetActive(true);

            _autoStartCoroutine = StartCoroutine(AutoStartCoroutine());
        }

        /// <summary>Fades the panel out and hides the GameObject.</summary>
        public void Hide()
        {
            StartCoroutine(HideCoroutine());
        }

        private void HandleReady()
        {
            if (_autoStartCoroutine != null)
                StopCoroutine(_autoStartCoroutine);

            FireReady();
        }

        private IEnumerator AutoStartCoroutine()
        {
            float remaining = AutoStartDuration;
            while (remaining > 0f)
            {
                autoStartTimerText.text = $"AUTO {Mathf.CeilToInt(remaining)}…";
                remaining -= Time.deltaTime;
                yield return null;
            }

            autoStartTimerText.text = "AUTO 0…";
            FireReady();
        }

        private void FireReady()
        {
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
            OnPlayerReady?.Invoke();
        }

        private IEnumerator HideCoroutine()
        {
            float elapsed = 0f;
            float startAlpha = panelGroup.alpha;
            while (elapsed < HideFadeDuration)
            {
                panelGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / HideFadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            panelGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
