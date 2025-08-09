using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class GolfManager : MonoBehaviour
{
    [Header("Golf Settings")]
    [SerializeField] private Transform golfStartPosition;
    [SerializeField] private int shotsPerSession = 10;
    [SerializeField] private LayerMask golfAreaLayer = 1;
    [SerializeField] private float golfAreaRadius = 2f;

    // Current session data
    private GolfState currentState = GolfState.Disabled;
    private IGolfShotType currentShotType;
    private int shotsRemaining;
    private List<IGolfObserver> observers = new List<IGolfObserver>();

    // Shot types
    private Dictionary<string, IGolfShotType> shotTypes;

    // References
    private ThirdPersonController playerController;
    private GolfAnimationController animationController;
    private StarterAssetsInputs input;

    // Properties
    public GolfState CurrentState => currentState;
    public IGolfShotType CurrentShotType => currentShotType;
    public int ShotsRemaining => shotsRemaining;
    public bool IsInGolfMode => currentState != GolfState.Disabled;

    private void Awake()
    {
        InitializeShotTypes();
        playerController = FindFirstObjectByType<ThirdPersonController>();
        animationController = GetComponent<GolfAnimationController>();
        input = FindFirstObjectByType<StarterAssetsInputs>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Check if player wants to enter golf mode
        if (currentState == GolfState.Disabled && input.enterGolfMode)
        {
            TryEnterGolfMode();
            input.enterGolfMode = false;
        }

        // Handle golf mode inputs
        if (IsInGolfMode)
        {
            // Exit golf mode
            if (input.exitGolfMode)
            {
                ExitGolfMode();
                input.exitGolfMode = false;
            }

            // Shot type selection
            if (currentState == GolfState.Disabled || currentState == GolfState.ReadyToShoot)
            {
                if (input.golfDrive)
                {
                    StartGolfSession("Drive");
                    input.golfDrive = false;
                }
                else if (input.golfChip)
                {
                    StartGolfSession("Chip");
                    input.golfChip = false;
                }
                else if (input.golfPutt)
                {
                    StartGolfSession("Putt");
                    input.golfPutt = false;
                }
            }

            // Execute shot
            if (currentState == GolfState.ReadyToShoot && input.executeShot)
            {
                ExecuteShot();
                input.executeShot = false;
            }
        }
    }

    private void InitializeShotTypes()
    {
        shotTypes = new Dictionary<string, IGolfShotType>
        {
            ["Drive"] = new DriveShot(),
            ["Chip"] = new ChipShot(),
            ["Putt"] = new PuttShot()
        };
    }

    private void TryEnterGolfMode()
    {
        // Check if we're near a golf area
        Collider[] golfAreas = Physics.OverlapSphere(transform.position, golfAreaRadius, golfAreaLayer);

        if (golfAreas.Length > 0)
        {
            ChangeState(GolfState.SelectingShot);
            Debug.Log("Entered golf area. Press 1 for Drive, 2 for Chip, 3 for Putt");
        }
    }

    public void StartGolfSession(string shotTypeName)
    {
        if (shotTypes.TryGetValue(shotTypeName, out IGolfShotType shotType))
        {
            currentShotType = shotType;
            shotsRemaining = shotsPerSession;

            // Disable player movement and position at golf start
            if (playerController != null)
            {
                playerController.SetGolfMode(true, golfStartPosition.position);
            }

            // Setup animation controller
            if (animationController != null)
            {
                animationController.SetShotType(currentShotType);
            }

            ChangeState(GolfState.ReadyToShoot);

            NotifyObservers(obs => obs.OnShotTypeChanged(currentShotType));
            NotifyObservers(obs => obs.OnShotsRemainingChanged(shotsRemaining));

            Debug.Log($"Started {shotTypeName} session with {shotsRemaining} shots");
        }
    }

    public void ExecuteShot()
    {
        if (currentState != GolfState.ReadyToShoot || shotsRemaining <= 0) return;

        ChangeState(GolfState.AnimatingShot);

        if (animationController != null)
        {
            animationController.TriggerShotAnimation();
        }

        shotsRemaining--;
        NotifyObservers(obs => obs.OnShotsRemainingChanged(shotsRemaining));

        Debug.Log($"Shot executed! {shotsRemaining} shots remaining");
    }

    public void OnShotAnimationComplete()
    {
        if (shotsRemaining > 0)
        {
            ChangeState(GolfState.ReadyToShoot);
        }
        else
        {
            Debug.Log("Session complete!");
            ChangeState(GolfState.SelectingShot);
        }
    }

    public void ExitGolfMode()
    {
        if (playerController != null)
        {
            playerController.SetGolfMode(false);
        }

        if (animationController != null)
        {
            animationController.ExitGolfAnimations();
        }

        currentShotType = null;
        shotsRemaining = 0;

        ChangeState(GolfState.Disabled);
        Debug.Log("Exited golf mode");
    }

    private void ChangeState(GolfState newState)
    {
        currentState = newState;
        NotifyObservers(obs => obs.OnGolfStateChanged(newState));
    }

    // Observer pattern methods
    public void AddObserver(IGolfObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IGolfObserver observer)
    {
        observers.Remove(observer);
    }

    private void NotifyObservers(System.Action<IGolfObserver> action)
    {
        for (int i = observers.Count - 1; i >= 0; i--)
        {
            if (observers[i] != null)
            {
                action(observers[i]);
            }
            else
            {
                observers.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw golf area radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, golfAreaRadius);

        // Draw golf start position
        if (golfStartPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(golfStartPosition.position, Vector3.one);
        }
    }
}