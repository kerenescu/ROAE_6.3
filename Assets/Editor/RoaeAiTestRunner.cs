using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using ApiTestMode = UnityEditor.TestTools.TestRunner.Api.TestMode;
using ApiTestStatus = UnityEditor.TestTools.TestRunner.Api.TestStatus;
using RuntimeTestMode = UnityEngine.TestTools.TestMode;

public static class RoaeAiTestRunner
{
    private const string MenuRoot = "Tools/ROAE/NPC/Run AI Tests/";
    private const string TestCategory = "ROAE.AI";
    private const string ResultsFolderRelative = "Temp/ROAE_AI_Tests";
    private const string SessionPrefix = "ROAE.AI.Tests.";
    private const string SessionIsRunningKey = SessionPrefix + "IsRunning";
    private const string SessionExitOnFinishKey = SessionPrefix + "ExitOnFinish";
    private const string SessionPendingModesKey = SessionPrefix + "PendingModes";
    private const string SessionActiveModeKey = SessionPrefix + "ActiveMode";
    private const string SessionHasFailuresKey = SessionPrefix + "HasFailures";
    private const string SessionSummaryKey = SessionPrefix + "Summary";

    private static readonly Queue<RuntimeTestMode> PendingModes = new Queue<RuntimeTestMode>();

    private static TestRunnerApi runnerApi;
    private static RoaeAiTestRunnerCallbacks callbacks;
    private static RoaeAiTestRunSession currentSession;
    private static bool exitEditorOnFinish;

    [MenuItem(MenuRoot + "All")]
    public static void RunAllFromMenu()
    {
        StartInteractiveRun(RuntimeTestMode.EditMode, RuntimeTestMode.PlayMode);
    }

    [MenuItem(MenuRoot + "EditMode")]
    public static void RunEditModeFromMenu()
    {
        StartInteractiveRun(RuntimeTestMode.EditMode);
    }

    [MenuItem(MenuRoot + "PlayMode")]
    public static void RunPlayModeFromMenu()
    {
        StartInteractiveRun(RuntimeTestMode.PlayMode);
    }

    [MenuItem(MenuRoot + "Reveal Results Folder")]
    public static void RevealResultsFolder()
    {
        EnsureResultsFolderExists();
        EditorUtility.RevealInFinder(ResultsFolderAbsolute);
    }

    public static bool IsRunInProgress => SessionIsRunning;

    public static string LatestSummary => SessionSummary;

    public static string ResultsFolderPath => ResultsFolderAbsolute;

    public static string BatchLogPath
    {
        get
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, "Temp", "roae-ai-test-runner.log");
        }
    }

    public static string GetResultsFilePath(RuntimeTestMode mode)
    {
        return GetResultsFileAbsolutePath(mode);
    }

    [MenuItem(MenuRoot + "All", true)]
    [MenuItem(MenuRoot + "EditMode", true)]
    [MenuItem(MenuRoot + "PlayMode", true)]
    private static bool ValidateRunMenus()
    {
        return !SessionIsRunning;
    }

    [InitializeOnLoadMethod]
    private static void RestoreRunningSessionAfterDomainReload()
    {
        if (!SessionIsRunning)
            return;

        exitEditorOnFinish = SessionExitOnFinish;
        currentSession = new RoaeAiTestRunSession
        {
            ActiveMode = SessionActiveMode
        };

        RestorePendingModesFromSession();
        EnsureRunnerInfrastructure();
    }

    public static void RunAllBatch()
    {
        StartBatchRun(RuntimeTestMode.EditMode, RuntimeTestMode.PlayMode);
    }

    public static void RunEditModeBatch()
    {
        StartBatchRun(RuntimeTestMode.EditMode);
    }

    public static void RunPlayModeBatch()
    {
        StartBatchRun(RuntimeTestMode.PlayMode);
    }

    private static void StartInteractiveRun(params RuntimeTestMode[] modes)
    {
        StartRun(false, modes);
    }

    private static void StartBatchRun(params RuntimeTestMode[] modes)
    {
        StartRun(true, modes);
    }

    private static void StartRun(bool shouldExitEditorOnFinish, params RuntimeTestMode[] modes)
    {
        if (SessionIsRunning)
        {
            Debug.LogWarning("[ROAE][AI][Tests][FAIL] reason=run_already_in_progress");
            return;
        }

        PendingModes.Clear();
        foreach (RuntimeTestMode mode in modes.Where(IsSupportedMode))
            PendingModes.Enqueue(mode);

        if (PendingModes.Count == 0)
        {
            Debug.LogWarning("[ROAE][AI][Tests][FAIL] reason=no_supported_test_modes_requested");
            return;
        }

        exitEditorOnFinish = shouldExitEditorOnFinish;
        currentSession = new RoaeAiTestRunSession();
        SessionIsRunning = true;
        SessionExitOnFinish = shouldExitEditorOnFinish;
        SessionHasFailures = false;
        SessionSummary = string.Empty;
        PersistPendingModesToSession();

        EnsureRunnerInfrastructure();
        EnsureResultsFolderExists();

        Debug.Log("[ROAE][AI][Tests][START] modes=" + string.Join(",", PendingModes.Select(ModeToString)) + " category=" + TestCategory + " resultsFolder=" + ResultsFolderAbsolute);
        RunNextQueuedMode();
    }

    private static void EnsureRunnerInfrastructure()
    {
        if (runnerApi == null)
        {
            runnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            runnerApi.hideFlags = HideFlags.HideAndDontSave;
        }

        if (callbacks == null)
        {
            callbacks = ScriptableObject.CreateInstance<RoaeAiTestRunnerCallbacks>();
            callbacks.hideFlags = HideFlags.HideAndDontSave;
            TestRunnerApi.RegisterTestCallback(callbacks);
        }
    }

    private static void RunNextQueuedMode()
    {
        if (PendingModes.Count == 0)
            RestorePendingModesFromSession();

        if (PendingModes.Count == 0)
        {
            FinishSession();
            return;
        }

        RuntimeTestMode mode = PendingModes.Dequeue();
        if (currentSession == null)
            currentSession = new RoaeAiTestRunSession();
        currentSession.ActiveMode = mode;
        SessionActiveMode = mode;
        PersistPendingModesToSession();

        Debug.Log("[ROAE][AI][Tests][MODE_START] mode=" + ModeToString(mode) + " category=" + TestCategory);

        Filter filter = new Filter
        {
            testMode = (ApiTestMode)mode,
            categoryNames = new[] { TestCategory }
        };

        ExecutionSettings settings = new ExecutionSettings(filter)
        {
            runSynchronously = mode == RuntimeTestMode.EditMode
        };

        runnerApi.Execute(settings);
    }

    internal static void HandleRunStarted(ITestAdaptor testsToRun)
    {
        if (!SessionIsRunning)
            return;

        Debug.Log("[ROAE][AI][Tests][DISCOVERED] mode=" + ModeToString(SessionActiveMode) + " cases=" + testsToRun.TestCaseCount);
    }

    internal static void HandleRunFinished(ITestResultAdaptor result)
    {
        if (!SessionIsRunning)
            return;

        RuntimeTestMode activeMode = SessionActiveMode;
        if (currentSession == null)
            currentSession = new RoaeAiTestRunSession { ActiveMode = activeMode };

        string absoluteResultPath = GetResultsFileAbsolutePath(activeMode);
        try
        {
            EnsureResultsFolderExists();
            TestRunnerApi.SaveResultToFile(result, absoluteResultPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[ROAE][AI][Tests][RESULT_SAVE_FAIL] mode=" + ModeToString(currentSession.ActiveMode) + " path=" + absoluteResultPath + " reason=" + exception.Message);
        }

        RoaeAiModeResult modeResult = new RoaeAiModeResult
        {
            Mode = activeMode,
            Total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount,
            Passed = result.PassCount,
            Failed = result.FailCount,
            Skipped = result.SkipCount,
            Inconclusive = result.InconclusiveCount,
            Status = result.TestStatus,
            ResultState = result.ResultState,
            DurationSeconds = result.Duration,
            ResultPath = absoluteResultPath,
            Message = result.Message
        };

        currentSession.Results.Add(modeResult);
        AppendSummary(modeResult);
        if (modeResult.Failed > 0 || modeResult.Status == ApiTestStatus.Failed)
            SessionHasFailures = true;

        string logMessage =
            "[ROAE][AI][Tests][" + (modeResult.Failed > 0 ? "FAIL" : "SUCCESS") + "] mode=" + ModeToString(modeResult.Mode) +
            " total=" + modeResult.Total +
            " passed=" + modeResult.Passed +
            " failed=" + modeResult.Failed +
            " skipped=" + modeResult.Skipped +
            " inconclusive=" + modeResult.Inconclusive +
            " durationSec=" + modeResult.DurationSeconds.ToString("0.###") +
            " result=" + modeResult.ResultState +
            " xml=" + modeResult.ResultPath;

        if (modeResult.Failed > 0)
            Debug.LogError(logMessage);
        else if (modeResult.Total == 0)
            Debug.LogWarning(logMessage + " reason=no_tests_matched_filter");
        else
            Debug.Log(logMessage);

        EditorApplication.delayCall += RunNextQueuedMode;
    }

    internal static void HandleTestFinished(ITestResultAdaptor result)
    {
        if (!SessionIsRunning || result.HasChildren || result.TestStatus != ApiTestStatus.Failed)
            return;

        Debug.LogError(
            "[ROAE][AI][Tests][CASE_FAIL] mode=" + ModeToString(SessionActiveMode) +
            " test=" + result.FullName +
            " message=" + result.Message +
            Environment.NewLine +
            result.StackTrace);
    }

    internal static void HandleRunError(string message)
    {
        if (!SessionIsRunning)
            return;

        RuntimeTestMode activeMode = SessionActiveMode;
        Debug.LogError("[ROAE][AI][Tests][ERROR] mode=" + ModeToString(activeMode) + " message=" + message);

        if (currentSession != null)
        {
            currentSession.Results.Add(new RoaeAiModeResult
            {
                Mode = activeMode,
                Total = 0,
                Passed = 0,
                Failed = 1,
                Skipped = 0,
                Inconclusive = 0,
                Status = ApiTestStatus.Failed,
                ResultState = "Failed:Error",
                DurationSeconds = 0d,
                ResultPath = GetResultsFileAbsolutePath(activeMode),
                Message = message
            });
        }

        SessionHasFailures = true;
        AppendSummary("ERROR " + ModeToString(activeMode) + ": " + message);

        PendingModes.Clear();
        PersistPendingModesToSession();
        FinishSession();
    }

    private static void FinishSession()
    {
        if (!SessionIsRunning)
            return;

        bool hasFailures = SessionHasFailures || (currentSession != null && currentSession.Results.Any(result => result.Failed > 0 || result.Status == ApiTestStatus.Failed));
        string summary = string.IsNullOrWhiteSpace(SessionSummary)
            ? "no_results"
            : SessionSummary;

        string logMessage = "[ROAE][AI][Tests][" + (hasFailures ? "FAIL" : "SUCCESS") + "] summary=" + summary;
        if (hasFailures)
            Debug.LogError(logMessage);
        else
            Debug.Log(logMessage);

        CleanupRunnerInfrastructure();

        if (exitEditorOnFinish)
        {
            int exitCode = hasFailures ? 1 : 0;
            EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
        }

        ClearSessionState();
        PendingModes.Clear();
        currentSession = null;
        exitEditorOnFinish = false;
    }

    private static void CleanupRunnerInfrastructure()
    {
        if (callbacks != null)
        {
            TestRunnerApi.UnregisterTestCallback(callbacks);
            UnityEngine.Object.DestroyImmediate(callbacks);
            callbacks = null;
        }

        if (runnerApi != null)
        {
            UnityEngine.Object.DestroyImmediate(runnerApi);
            runnerApi = null;
        }
    }

    private static string GetResultsFileAbsolutePath(RuntimeTestMode mode)
    {
        return Path.Combine(ResultsFolderAbsolute, "roae-ai-" + ModeToString(mode).ToLowerInvariant() + "-results.xml");
    }

    private static void EnsureResultsFolderExists()
    {
        Directory.CreateDirectory(ResultsFolderAbsolute);
    }

    private static bool IsSupportedMode(RuntimeTestMode mode)
    {
        return mode == RuntimeTestMode.EditMode || mode == RuntimeTestMode.PlayMode;
    }

    private static string ModeToString(RuntimeTestMode mode)
    {
        return mode == RuntimeTestMode.EditMode ? "EditMode" : "PlayMode";
    }

    private static string ResultsFolderAbsolute
    {
        get
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, ResultsFolderRelative.Replace('/', Path.DirectorySeparatorChar));
        }
    }

    private static bool SessionIsRunning
    {
        get => SessionState.GetBool(SessionIsRunningKey, false);
        set => SessionState.SetBool(SessionIsRunningKey, value);
    }

    private static bool SessionExitOnFinish
    {
        get => SessionState.GetBool(SessionExitOnFinishKey, false);
        set => SessionState.SetBool(SessionExitOnFinishKey, value);
    }

    private static bool SessionHasFailures
    {
        get => SessionState.GetBool(SessionHasFailuresKey, false);
        set => SessionState.SetBool(SessionHasFailuresKey, value);
    }

    private static string SessionSummary
    {
        get => SessionState.GetString(SessionSummaryKey, string.Empty);
        set => SessionState.SetString(SessionSummaryKey, value ?? string.Empty);
    }

    private static RuntimeTestMode SessionActiveMode
    {
        get
        {
            int storedValue = SessionState.GetInt(SessionActiveModeKey, (int)RuntimeTestMode.EditMode);
            return Enum.IsDefined(typeof(RuntimeTestMode), storedValue)
                ? (RuntimeTestMode)storedValue
                : RuntimeTestMode.EditMode;
        }
        set => SessionState.SetInt(SessionActiveModeKey, (int)value);
    }

    private static void PersistPendingModesToSession()
    {
        SessionState.SetString(
            SessionPendingModesKey,
            string.Join(",", PendingModes.Select(mode => ((int)mode).ToString())));
    }

    private static void RestorePendingModesFromSession()
    {
        PendingModes.Clear();

        string serializedModes = SessionState.GetString(SessionPendingModesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(serializedModes))
            return;

        string[] values = serializedModes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string value in values)
        {
            if (!int.TryParse(value, out int parsedValue))
                continue;

            RuntimeTestMode mode = (RuntimeTestMode)parsedValue;
            if (IsSupportedMode(mode))
                PendingModes.Enqueue(mode);
        }
    }

    private static void AppendSummary(RoaeAiModeResult modeResult)
    {
        AppendSummary(
            ModeToString(modeResult.Mode) +
            ": total=" + modeResult.Total +
            ", passed=" + modeResult.Passed +
            ", failed=" + modeResult.Failed +
            ", skipped=" + modeResult.Skipped +
            ", inconclusive=" + modeResult.Inconclusive);
    }

    private static void AppendSummary(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return;

        SessionSummary = string.IsNullOrWhiteSpace(SessionSummary)
            ? entry
            : SessionSummary + " | " + entry;
    }

    private static void ClearSessionState()
    {
        SessionIsRunning = false;
        SessionExitOnFinish = false;
        SessionHasFailures = false;
        SessionSummary = string.Empty;
        SessionState.SetString(SessionPendingModesKey, string.Empty);
        SessionActiveMode = RuntimeTestMode.EditMode;
    }

    private sealed class RoaeAiTestRunSession
    {
        public RuntimeTestMode ActiveMode;
        public readonly List<RoaeAiModeResult> Results = new List<RoaeAiModeResult>();
    }

    private sealed class RoaeAiModeResult
    {
        public RuntimeTestMode Mode;
        public int Total;
        public int Passed;
        public int Failed;
        public int Skipped;
        public int Inconclusive;
        public ApiTestStatus Status;
        public string ResultState;
        public double DurationSeconds;
        public string ResultPath;
        public string Message;
    }
}

internal sealed class RoaeAiTestRunnerCallbacks : ScriptableObject, IErrorCallbacks
{
    public void RunStarted(ITestAdaptor testsToRun)
    {
        RoaeAiTestRunner.HandleRunStarted(testsToRun);
    }

    public void RunFinished(ITestResultAdaptor result)
    {
        RoaeAiTestRunner.HandleRunFinished(result);
    }

    public void TestStarted(ITestAdaptor test)
    {
    }

    public void TestFinished(ITestResultAdaptor result)
    {
        RoaeAiTestRunner.HandleTestFinished(result);
    }

    public void OnError(string message)
    {
        RoaeAiTestRunner.HandleRunError(message);
    }
}
