using System.Collections;
using TMPro;
using UnityEngine;

namespace WanganMidnight.UI
{
    /// <summary>
    /// Displays an animated 3-2-1-GO! countdown over the HUD.
    /// Yield-return PlayCountdown() from RaceStartSequence to await completion.
    /// </summary>
    public class CountdownController : MonoBehaviour
    {
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private CanvasGroup countdownGroup;
        [SerializeField] private float displayDuration = 0.85f;
        [SerializeField] private float scalePunchAmount = 1.4f;

        private const float GoDisplayDuration = 0.6f;
        private const float FadeOutDuration = 0.3f;

        private static readonly int[] CountdownValues = { 3, 2, 1 };

        /// <summary>Plays the full 3-2-1-GO! countdown animation.</summary>
        public IEnumerator PlayCountdown()
        {
            countdownGroup.alpha = 1f;
            gameObject.SetActive(true);

            foreach (int count in CountdownValues)
            {
                countdownText.text = count.ToString();
                countdownText.transform.localScale = new Vector3(scalePunchAmount, scalePunchAmount, 1f);

                float elapsed = 0f;
                while (elapsed < displayDuration)
                {
                    float t = elapsed / displayDuration;
                    float scale = Mathf.Lerp(scalePunchAmount, 1f, t);
                    countdownText.transform.localScale = new Vector3(scale, scale, 1f);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                countdownText.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(displayDuration - (displayDuration % Time.deltaTime));
            }

            countdownText.text = "GO!";
            countdownText.transform.localScale = new Vector3(scalePunchAmount, scalePunchAmount, 1f);

            float goElapsed = 0f;
            while (goElapsed < GoDisplayDuration)
            {
                float t = goElapsed / GoDisplayDuration;
                float scale = Mathf.Lerp(scalePunchAmount, 1f, t);
                countdownText.transform.localScale = new Vector3(scale, scale, 1f);
                goElapsed += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(FadeOutCoroutine());

            gameObject.SetActive(false);
        }

        private IEnumerator FadeOutCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < FadeOutDuration)
            {
                countdownGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / FadeOutDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            countdownGroup.alpha = 0f;
        }
    }
}
