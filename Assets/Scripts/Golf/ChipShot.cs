using UnityEngine;

public class ChipShot : IGolfShotType
{
    public string ShotName => "Chip";
    public string AnimationTrigger => "GolfChip";

    public void ExecuteShot(Vector3 aimDirection, float power)
    {
        // chip shot logic
        Debug.Log("execute chip shot");
    }

    public void SetupCamera()
    {
        //implement later
        Debug.Log("Setting up camera");
    }
}
