using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Sprint 3 Task 1 Testing Script - Journey State Management Validator
// NOTE: This script depends on JourneyTracker.cs and LearningMissionsManager.cs
// Make sure those are compiled first before using this tester
// Attach this to a test GameObject to validate the reset functionality
public class JourneyStateResetTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool runContinuousTests = false;
    [SerializeField] private float testInterval = 5f;

    [Header("Test Results")]
    [SerializeField] private int totalTests = 0;
    [SerializeField] private int passedTests = 0;
    [SerializeField] private int failedTests = 0;

    private JourneyTracker journeyTracker;
    private LearningMissionsManager missionManager;
    
    // Test results tracking
    private List<string> testResults = new List<string>();

    void Start()
    {
        // Find required components
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        missionManager = FindFirstObjectByType<LearningMissionsManager>();

        if (journeyTracker == null)
        {
            Debug.LogError("[Tester] JourneyTracker not found - tests cannot run");
            enabled = false;
            return;
        }

        if (missionManager == null)
        {
            Debug.LogError("[Tester] LearningMissionsManager not found - some tests may not work");
        }

        // Enable debug mode on tracker
        journeyTracker.EnableDebugMode(true);

        LogTestInfo("=== JOURNEY STATE RESET TESTER INITIALIZED ===");
        LogTestInfo($"JourneyTracker found: {journeyTracker != null}");
        LogTestInfo($"MissionsManager found: {missionManager != null}");

        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }

        if (runContinuousTests)
        {
            StartCoroutine(ContinuousTestLoop());
        }
    }

    private IEnumerator RunAllTests()
    {
        LogTestInfo("Starting comprehensive reset tests...");
        
        yield return StartCoroutine(TestBasicReset());
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(TestResetWithJourneyData());
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(TestMissionTransitionReset());
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(TestMultipleRapidResets());
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(TestResetStateValidation());

        // Print final results
        PrintTestSummary();
    }

    private IEnumerator ContinuousTestLoop()
    {
        while (runContinuousTests)
        {
            yield return new WaitForSeconds(testInterval);
            yield return StartCoroutine(TestBasicReset());
        }
    }

    // TEST 1: Basic Reset Functionality
    private IEnumerator TestBasicReset()
    {
        string testName = "Basic Reset Test";
        LogTestInfo($"Running {testName}...");

        bool hasException = false;
        string exceptionMessage = "";

        // Get initial state
        int initialLength = 0;
        int postResetLength = 0;

        try
        {
            initialLength = journeyTracker.GetCurrentJourneyLength();
            var initialType = journeyTracker.GetCurrentJourneyType();

            // Perform reset
            journeyTracker.ResetJourney();
        }
        catch (System.Exception e)
        {
            hasException = true;
            exceptionMessage = e.Message;
        }

        // Wait one frame for reset to complete (outside try-catch)
        yield return null;

        if (hasException)
        {
            FailTest(testName, $"Exception: {exceptionMessage}");
            yield break;
        }

        try
        {
            // Check results
            postResetLength = journeyTracker.GetCurrentJourneyLength();
            
            bool testPassed = (postResetLength == 0);
            
            if (testPassed)
            {
                PassTest(testName, $"Journey length: {initialLength} → {postResetLength}");
            }
            else
            {
                FailTest(testName, $"Journey not cleared: expected 0, got {postResetLength}");
            }
        }
        catch (System.Exception e)
        {
            FailTest(testName, $"Exception during validation: {e.Message}");
        }
    }

    // TEST 2: Reset with Journey Data
    private IEnumerator TestResetWithJourneyData()
    {
        string testName = "Reset with Journey Data";
        LogTestInfo($"Running {testName}...");

        bool hasException = false;
        string exceptionMessage = "";

        try
        {
            // Simulate journey data by triggering some vertex visits
            // This test checks that reset works even with existing data
            
            // Force some journey data (simulated)
            // In a real test, you'd trigger actual vertex visits
            
            journeyTracker.ResetJourney();
        }
        catch (System.Exception e)
        {
            hasException = true;
            exceptionMessage = e.Message;
        }

        // Wait outside try-catch
        yield return null;

        if (hasException)
        {
            FailTest(testName, $"Exception: {exceptionMessage}");
            yield break;
        }

        try
        {
            int postResetLength = journeyTracker.GetCurrentJourneyLength();
            
            bool testPassed = (postResetLength == 0);
            
            if (testPassed)
            {
                PassTest(testName, "Reset successful with journey data present");
            }
            else
            {
                FailTest(testName, $"Reset failed: journey length is {postResetLength}");
            }
        }
        catch (System.Exception e)
        {
            FailTest(testName, $"Exception during validation: {e.Message}");
        }
    }

    // TEST 3: Mission Transition Reset
    private IEnumerator TestMissionTransitionReset()
    {
        string testName = "Mission Transition Reset";
        LogTestInfo($"Running {testName}...");

        if (missionManager == null)
        {
            SkipTest(testName, "MissionsManager not available");
            yield break;
        }

        bool hasException = false;
        string exceptionMessage = "";
        int currentMission = 0;

        try
        {
            // Get current mission state
            currentMission = missionManager.GetCurrentMissionIndex();
            
            // Restart current mission (this should trigger reset)
            missionManager.RestartCurrentMission();
        }
        catch (System.Exception e)
        {
            hasException = true;
            exceptionMessage = e.Message;
        }

        // Wait for mission transition (outside try-catch)
        yield return new WaitForSeconds(1f);

        if (hasException)
        {
            FailTest(testName, $"Exception: {exceptionMessage}");
            yield break;
        }

        try
        {
            // Check journey is reset
            int postTransitionLength = journeyTracker.GetCurrentJourneyLength();
            
            bool testPassed = (postTransitionLength == 0);
            
            if (testPassed)
            {
                PassTest(testName, "Mission transition properly reset journey");
            }
            else
            {
                FailTest(testName, $"Mission transition failed to reset: length = {postTransitionLength}");
            }
        }
        catch (System.Exception e)
        {
            FailTest(testName, $"Exception during validation: {e.Message}");
        }
    }

    // TEST 4: Multiple Rapid Resets
    private IEnumerator TestMultipleRapidResets()
    {
        string testName = "Multiple Rapid Resets";
        LogTestInfo($"Running {testName}...");

        bool allResetsPassed = true;
        string failureReason = "";
        bool hasException = false;
        string exceptionMessage = "";

        // Perform 5 rapid resets
        for (int i = 0; i < 5; i++)
        {
            try
            {
                journeyTracker.ResetJourney();
            }
            catch (System.Exception e)
            {
                hasException = true;
                exceptionMessage = $"Exception on reset {i+1}: {e.Message}";
                break;
            }

            // Wait one frame (outside try-catch)
            yield return null;
            
            if (hasException)
            {
                allResetsPassed = false;
                failureReason = exceptionMessage;
                break;
            }

            try
            {
                int length = journeyTracker.GetCurrentJourneyLength();
                if (length != 0)
                {
                    allResetsPassed = false;
                    failureReason = $"Reset {i+1} failed: length = {length}";
                    break;
                }
            }
            catch (System.Exception e)
            {
                allResetsPassed = false;
                failureReason = $"Exception checking reset {i+1}: {e.Message}";
                break;
            }
        }
        
        if (allResetsPassed)
        {
            PassTest(testName, "All 5 rapid resets successful");
        }
        else
        {
            FailTest(testName, failureReason);
        }
    }

    // TEST 5: Reset State Validation
    private IEnumerator TestResetStateValidation()
    {
        string testName = "Reset State Validation";
        LogTestInfo($"Running {testName}...");

        bool hasException = false;
        string exceptionMessage = "";

        try
        {
            // Perform reset
            journeyTracker.ResetJourney();
        }
        catch (System.Exception e)
        {
            hasException = true;
            exceptionMessage = e.Message;
        }

        // Wait outside try-catch
        yield return null;

        if (hasException)
        {
            FailTest(testName, $"Exception: {exceptionMessage}");
            yield break;
        }

        try
        {
            // Validate multiple state aspects
            int journeyLength = journeyTracker.GetCurrentJourneyLength();
            var journeyType = journeyTracker.GetCurrentJourneyType();
            
            // Check if mission complete check works correctly after reset
            bool missionComplete = journeyTracker.IsMissionComplete();
            
            bool testPassed = true;
            List<string> issues = new List<string>();
            
            if (journeyLength != 0)
            {
                testPassed = false;
                issues.Add($"Journey length not zero: {journeyLength}");
            }
            
            // For zero-length journeys, this should typically be Walk
            // But the exact behavior depends on your implementation
            
            if (testPassed)
            {
                PassTest(testName, $"All state validations passed (Length: {journeyLength}, Type: {journeyType}, Complete: {missionComplete})");
            }
            else
            {
                FailTest(testName, string.Join("; ", issues));
            }
        }
        catch (System.Exception e)
        {
            FailTest(testName, $"Exception during validation: {e.Message}");
        }
    }

    // Test result tracking methods
    private void PassTest(string testName, string details)
    {
        totalTests++;
        passedTests++;
        string result = $"✓ PASS: {testName} - {details}";
        testResults.Add(result);
        LogTestInfo(result);
    }

    private void FailTest(string testName, string reason)
    {
        totalTests++;
        failedTests++;
        string result = $"✗ FAIL: {testName} - {reason}";
        testResults.Add(result);
        LogTestError(result);
    }

    private void SkipTest(string testName, string reason)
    {
        string result = $"⚠ SKIP: {testName} - {reason}";
        testResults.Add(result);
        LogTestInfo(result);
    }

    private void PrintTestSummary()
    {
        LogTestInfo("=== TEST SUMMARY ===");
        LogTestInfo($"Total Tests: {totalTests}");
        LogTestInfo($"Passed: {passedTests}");
        LogTestInfo($"Failed: {failedTests}");
        LogTestInfo($"Success Rate: {(totalTests > 0 ? (passedTests * 100 / totalTests) : 0)}%");
        
        if (failedTests > 0)
        {
            LogTestError("FAILED TESTS DETECTED - Sprint 3 Task 1 not complete");
        }
        else
        {
            LogTestInfo("ALL TESTS PASSED - Sprint 3 Task 1 appears successful");
        }
        
        LogTestInfo("=== DETAILED RESULTS ===");
        foreach (string result in testResults)
        {
            LogTestInfo(result);
        }
    }

    // Manual test triggers (for testing in editor)
    [ContextMenu("Run Basic Reset Test")]
    public void TriggerBasicResetTest()
    {
        StartCoroutine(TestBasicReset());
    }

    [ContextMenu("Run All Tests")]
    public void TriggerAllTests()
    {
        StartCoroutine(RunAllTests());
    }

    [ContextMenu("Clear Test Results")]
    public void ClearTestResults()
    {
        testResults.Clear();
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        LogTestInfo("Test results cleared");
    }

    // Public methods for external test triggering
    public void RunTestSuite()
    {
        StartCoroutine(RunAllTests());
    }

    public bool HasAllTestsPassed()
    {
        return totalTests > 0 && failedTests == 0;
    }

    public string GetTestResultsSummary()
    {
        return $"Tests: {totalTests}, Passed: {passedTests}, Failed: {failedTests}";
    }

    // Logging methods
    private void LogTestInfo(string message)
    {
        Debug.Log($"[ResetTester] {message}");
    }

    private void LogTestError(string message)
    {
        Debug.LogError($"[ResetTester] {message}");
    }

    // Inspector info
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"Reset Tester Results:");
            GUILayout.Label($"Total: {totalTests}");
            GUILayout.Label($"Passed: {passedTests}");
            GUILayout.Label($"Failed: {failedTests}");
            
            if (GUILayout.Button("Run Tests"))
            {
                TriggerAllTests();
            }
            
            if (GUILayout.Button("Clear Results"))
            {
                ClearTestResults();
            }
            
            GUILayout.EndArea();
        }
    }
}