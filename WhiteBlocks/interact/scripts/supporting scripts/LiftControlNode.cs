using UnityEngine;

public class LiftControlNode : MonoBehaviour
{
    private CatacombElevator _elevator;

    public bool useLiftDefault = true; // If true, the lift will use its default behavior. If false, it will use the nextLiftIndex to determine which lift to control.
    public int nextLiftIndex = 0;

    private void Start()
    {
        _elevator = GetComponentInParent<CatacombElevator>();
        if (_elevator == null)
        {
            Debug.LogError("LiftControlNode: No CatacombElevator found in parent hierarchy.");
        }
    }

    public void TriggerLift()
    {
        if (_elevator == null) return;

        if (useLiftDefault)
        {
            nextLiftIndex = _elevator.currentLiftIndex;
        }
        _elevator.CommandFromOtherScript(nextLiftIndex);
    }
}