using UnityEngine;

public class PuttShot : IGolfShotType
{
    public string ShotName => "Putt";
    public string AnimationTrigger => "GolfPutt";

    public void ExecuteShot(Vector3 aimDirection, float power)
    {
        // chip shot logic
        Debug.Log("execute putt shot");
    }

    public void SetupCamera()
    {
        //implement later
        Debug.Log("Setting up camera");
    }
}
