using UnityEngine;

public class Player : MonoBehaviour, ICrosser
{
    public string CrosserId => "Player";
    public Vector3 Position => transform.position;
}
