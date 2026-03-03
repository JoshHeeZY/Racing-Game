using UnityEngine;
using WanganMidnight;
using WanganMidnight.Sequencing;

public class SequenceTest : MonoBehaviour
{
    [SerializeField] private RaceStartSequence raceStartSequence;
    [SerializeField] private RivalData rivalData;

    private void Start()
    {
        raceStartSequence.BeginSequence(rivalData);
    }
}
