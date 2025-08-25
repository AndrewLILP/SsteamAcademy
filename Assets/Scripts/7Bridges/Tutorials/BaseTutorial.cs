// BaseTutorial.cs  
// Abstract base class for all tutorial implementations using Template Method pattern

using UnityEngine;

/// <summary>
/// Abstract base class implementing common tutorial behavior
/// Uses Template Method pattern - defines algorithm structure, subclasses implement specifics
/// </summary>
public abstract class BaseTutorial : MonoBehaviour, ITutorial
{
    [Header("Tutorial Configuration")]
    [SerializeField] protected TutorialConfig config;
    
    protected JourneyTracker journeyTracker;
    protected bool isComplete = false;
    protected int currentStep = 0;

    public bool IsComplete => isComplete;
    public string TutorialName => config?.tutorialName ?? "Unknown";
    public JourneyType TargetType => config?.journeyType ?? JourneyType.Walk;

    /// <summary>
    /// Initialize the tutorial with required dependencies
    /// </summary>
    public virtual void Initialize(JourneyTracker tracker)
    {
        journeyTracker = tracker;
        
        // Get config based on the specific tutorial type (determined by derived class)
        config = TutorialConfig.GetConfig(GetTutorialType());
        
        // Configure journey tracker for this tutorial
        journeyTracker.SetMissionType(config.journeyType);
        journeyTracker.EnableDebugMode(false); // Cleaner for tutorials
        
        LogTutorialStart();
    }

    /// <summary>
    /// Main progress checking logic - called each frame
    /// </summary>
    public virtual void CheckProgress()
    {
        if (isComplete || journeyTracker == null || config == null) return;

        if (IsTutorialComplete())
        {
            CompleteTutorial();
        }
        else
        {
            UpdateProgressFeedback();
        }
    }

    /// <summary>
    /// Template method - defines completion checking algorithm
    /// Subclasses implement ValidateSpecificRequirements for custom logic
    /// </summary>
    protected virtual bool IsTutorialComplete()
    {
        if (journeyTracker == null || config == null) return false;
        
        int steps = journeyTracker.GetCurrentJourneyLength();
        
        if (steps < config.requiredSteps) return false;

        // Hook method for subclass-specific validation
        return ValidateSpecificRequirements();
    }

    /// <summary>
    /// Update UI with current progress feedback
    /// </summary>
    protected virtual void UpdateProgressFeedback()
    {
        int steps = journeyTracker.GetCurrentJourneyLength();
        int messageIndex = Mathf.Min(steps, config.progressMessages.Length - 1);
        
        var ui = GetComponent<ITutorialUI>();
        ui?.UpdateProgressMessage(config.progressMessages[messageIndex]);
        ui?.UpdateStatusMessage($"Creating {config.journeyType}: {steps}/{config.requiredSteps} steps");
    }

    /// <summary>
    /// Handle tutorial completion
    /// </summary>
    protected virtual void CompleteTutorial()
    {
        isComplete = true;
        OnTutorialComplete();
        
        var ui = GetComponent<ITutorialUI>();
        ui?.ShowCompletion(config.completionMessage);
        ui?.ShowExitOption();
        
        LogTutorialComplete();
    }

    /// <summary>
    /// Hook method for subclasses to implement custom completion behavior
    /// </summary>
    public virtual void OnTutorialComplete()
    {
        // Override in derived classes for specific completion behavior
    }

    /// <summary>
    /// Reset tutorial to initial state
    /// </summary>
    public virtual void Reset()
    {
        isComplete = false;
        currentStep = 0;
        journeyTracker?.ResetJourney();
        LogTutorialReset();
    }

    #region Abstract Methods - Must be implemented by derived classes
    
    /// <summary>
    /// Hook method for subclasses to implement journey-type specific validation
    /// </summary>
    protected abstract bool ValidateSpecificRequirements();
    
    /// <summary>
    /// Abstract method for derived classes to specify their tutorial type
    /// </summary>
    protected abstract JourneyType GetTutorialType();
    
    #endregion

    #region Logging Methods
    
    protected virtual void LogTutorialStart()
    {
        Debug.Log($"[{config.tutorialName}] Tutorial started - {config.instructionMessage}");
    }

    protected virtual void LogTutorialComplete()
    {
        Debug.Log($"[{config.tutorialName}] Tutorial completed successfully!");
    }

    protected virtual void LogTutorialReset()
    {
        Debug.Log($"[{config.tutorialName}] Tutorial reset - try again!");
    }
    
    #endregion
}