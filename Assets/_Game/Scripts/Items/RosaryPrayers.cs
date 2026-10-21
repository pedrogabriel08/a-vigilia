using UnityEngine;

namespace Vigilia
{
    public sealed class RosaryPrayers : MonoBehaviour
    {
        [SerializeField] private BabyController baby;
        [SerializeField, Min(.1f)] private float protectionDuration = 15f;
        public void PrayAveMaria() { if (baby != null) baby.Protect(protectionDuration); }
    }
}
