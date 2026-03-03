using UnityEngine;

namespace WanganMidnight
{
    [CreateAssetMenu(menuName = "Wangan/Rival Data")]
    public class RivalData : ScriptableObject
    {
        public string rivalName;       // e.g. "Tatsuya Shima"
        public string carName;         // e.g. "Porsche 911 Turbo"
        public int horsePower;         // e.g. 450
        public Sprite portrait;        // Character portrait sprite
        public Sprite carSilhouette;   // Car silhouette for briefing screen
        public Color accentColor = Color.white;
    }
}
