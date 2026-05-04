using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using RuntimeTestMode = UnityEngine.TestTools.TestMode;

public sealed class RoaeAiDevDashboardWindow : EditorWindow
{
    private const string MenuPath = "Tools/ROAE/NPC/AI Dev Dashboard";
    private const float SectionSpacing = 10f;
    private const string PrettyReportFileName = "roae-ai-pretty-report.html";

    private Vector2 scrollPosition;
    private RoaeAiTestReport editModeReport;
    private RoaeAiTestReport playModeReport;

    [MenuItem(MenuPath)]
    public static void Open()
    {
        RoaeAiDevDashboardWindow window = GetWindow<RoaeAiDevDashboardWindow>("ROAE AI Dashboard");
        window.minSize = new Vector2(500f, 420f);
        window.RefreshReports();
        window.Show();
    }

    private void OnEnable()
    {
        RefreshReports();
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawOverviewSection();
        EditorGUILayout.Space(SectionSpacing);
        DrawResetSection();
        EditorGUILayout.Space(SectionSpacing);
        DrawTestRunnerSection();
        EditorGUILayout.Space(SectionSpacing);
        DrawResultsSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawOverviewSection()
    {
        EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Un singur loc pentru reset de dev, rulare teste AI si verificarea ultimelor rezultate. Gandit pentru demo, tuning si sanity checks rapide.",
            MessageType.Info);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Runner", RoaeAiTestRunner.IsRunInProgress ? "Running" : "Idle");
            EditorGUILayout.LabelField("Results Folder", RoaeAiTestRunner.ResultsFolderPath);
            EditorGUILayout.LabelField("Batch Log", RoaeAiTestRunner.BatchLogPath);
            EditorGUILayout.LabelField("Pretty Report", PrettyReportPath);

            if (!string.IsNullOrWhiteSpace(RoaeAiTestRunner.LatestSummary))
                EditorGUILayout.LabelField("Current Summary", RoaeAiTestRunner.LatestSummary, EditorStyles.wordWrappedLabel);
        }
    }

    private void DrawResetSection()
    {
        EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Scene Dev Tools", BuildSceneToolsSummary(), EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Reset Dev State"))
            {
                ResetDevState();
                RefreshReports();
            }

            if (GUILayout.Button("Ping Scene Dev Tools"))
                PingSceneDevTools();
        }
    }

    private void DrawTestRunnerSection()
    {
        EditorGUILayout.LabelField("AI Tests", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Category", "ROAE.AI");

            using (new EditorGUI.DisabledScope(RoaeAiTestRunner.IsRunInProgress))
            {
                if (GUILayout.Button("Run AI Tests - All"))
                    RoaeAiTestRunner.RunAllFromMenu();

                if (GUILayout.Button("Run AI Tests - EditMode"))
                    RoaeAiTestRunner.RunEditModeFromMenu();

                if (GUILayout.Button("Run AI Tests - PlayMode"))
                    RoaeAiTestRunner.RunPlayModeFromMenu();
            }

            if (GUILayout.Button("Refresh Results"))
                RefreshReports();

            if (GUILayout.Button("Open Pretty Report"))
                OpenPrettyReport();

            if (GUILayout.Button("Reveal Results Folder"))
                RoaeAiTestRunner.RevealResultsFolder();

            if (GUILayout.Button("Reveal Batch Log"))
                RevealFile(RoaeAiTestRunner.BatchLogPath);
        }
    }

    private void DrawResultsSection()
    {
        EditorGUILayout.LabelField("Latest Results", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            DrawReportCard("EditMode", editModeReport);
            EditorGUILayout.Space(6f);
            DrawReportCard("PlayMode", playModeReport);
        }
    }

    private void DrawReportCard(string label, RoaeAiTestReport report)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (report == null)
            {
                EditorGUILayout.LabelField("Status", "No result file yet");
                return;
            }

            Color originalColor = GUI.color;
            GUI.color = report.Failed > 0 ? new Color(1f, 0.75f, 0.75f) : new Color(0.75f, 1f, 0.8f);
            EditorGUILayout.LabelField("Outcome", report.Failed > 0 ? "FAIL" : "PASS", EditorStyles.boldLabel);
            GUI.color = originalColor;

            EditorGUILayout.LabelField("Generated", report.GeneratedAtText);
            EditorGUILayout.LabelField("Total", report.Total.ToString());
            EditorGUILayout.LabelField("Passed", report.Passed.ToString());
            EditorGUILayout.LabelField("Failed", report.Failed.ToString());
            EditorGUILayout.LabelField("Skipped", report.Skipped.ToString());
            EditorGUILayout.LabelField("Inconclusive", report.Inconclusive.ToString());

            if (!string.IsNullOrWhiteSpace(report.Path))
                EditorGUILayout.LabelField("XML", report.Path, EditorStyles.wordWrappedLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(report.Path) || !File.Exists(report.Path)))
                {
                    if (GUILayout.Button("Reveal XML"))
                        RevealFile(report.Path);
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(report.Path)))
                {
                    if (GUILayout.Button("Copy XML Path"))
                        EditorGUIUtility.systemCopyBuffer = report.Path ?? string.Empty;
                }
            }
        }
    }

    private void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
            RefreshReports();
    }

    private void RefreshReports()
    {
        editModeReport = ReadReport(RoaeAiTestRunner.GetResultsFilePath(RuntimeTestMode.EditMode));
        playModeReport = ReadReport(RoaeAiTestRunner.GetResultsFilePath(RuntimeTestMode.PlayMode));
        WritePrettyReport();
        Repaint();
    }

    private static string PrettyReportPath =>
        Path.Combine(RoaeAiTestRunner.ResultsFolderPath, PrettyReportFileName);

    private void OpenPrettyReport()
    {
        string path = WritePrettyReport();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Debug.LogWarning("[ROAE][AI][Dashboard][FAIL] action=open_pretty_report reason=report_not_generated");
            return;
        }

        Application.OpenURL("file:///" + path.Replace("\\", "/"));
    }

    private string WritePrettyReport()
    {
        try
        {
            Directory.CreateDirectory(RoaeAiTestRunner.ResultsFolderPath);
            File.WriteAllText(PrettyReportPath, BuildPrettyReportHtml(), Encoding.UTF8);
            return PrettyReportPath;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[ROAE][AI][Dashboard][FAIL] action=write_pretty_report reason=" + exception.Message);
            return string.Empty;
        }
    }

    private string BuildPrettyReportHtml()
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        html.AppendLine("<title>ROAE AI Test Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; background: #111827; color: #e5e7eb; }");
        html.AppendLine("h1, h2, h3 { margin: 0 0 12px; }");
        html.AppendLine(".muted { color: #9ca3af; }");
        html.AppendLine(".grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 16px; margin: 20px 0; }");
        html.AppendLine(".card { background: #1f2937; border: 1px solid #374151; border-radius: 8px; padding: 16px; }");
        html.AppendLine(".badge { display: inline-block; padding: 4px 10px; border-radius: 999px; font-weight: 600; font-size: 12px; }");
        html.AppendLine(".pass { background: #064e3b; color: #a7f3d0; }");
        html.AppendLine(".fail { background: #7f1d1d; color: #fecaca; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 12px; }");
        html.AppendLine("th, td { text-align: left; padding: 10px 8px; border-bottom: 1px solid #374151; vertical-align: top; }");
        html.AppendLine("th { color: #93c5fd; font-size: 12px; text-transform: uppercase; letter-spacing: 0.04em; }");
        html.AppendLine("code { font-family: Consolas, monospace; color: #fde68a; }");
        html.AppendLine("a { color: #93c5fd; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<h1>ROAE AI Test Report</h1>");
        html.AppendLine("<p class=\"muted\">Generated " + HtmlEncode(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) + "</p>");
        html.AppendLine("<p class=\"muted\">Summary: " + HtmlEncode(string.IsNullOrWhiteSpace(RoaeAiTestRunner.LatestSummary) ? "No active summary yet" : RoaeAiTestRunner.LatestSummary) + "</p>");
        html.AppendLine("<div class=\"grid\">");
        html.AppendLine(BuildPrettyReportCard("EditMode", editModeReport));
        html.AppendLine(BuildPrettyReportCard("PlayMode", playModeReport));
        html.AppendLine("</div>");
        html.AppendLine(BuildPrettyReportCasesSection("EditMode", editModeReport));
        html.AppendLine(BuildPrettyReportCasesSection("PlayMode", playModeReport));
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string BuildPrettyReportCard(string label, RoaeAiTestReport report)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine("<section class=\"card\">");
        html.AppendLine("<h2>" + HtmlEncode(label) + "</h2>");

        if (report == null)
        {
            html.AppendLine("<p class=\"muted\">No XML result found yet for this mode.</p>");
            html.AppendLine("</section>");
            return html.ToString();
        }

        string badgeClass = report.Failed > 0 ? "fail" : "pass";
        string outcome = report.Failed > 0 ? "FAIL" : "PASS";
        html.AppendLine("<p><span class=\"badge " + badgeClass + "\">" + outcome + "</span></p>");
        html.AppendLine("<p>Generated: <strong>" + HtmlEncode(report.GeneratedAtText) + "</strong></p>");
        html.AppendLine("<p>Total: <strong>" + report.Total + "</strong> | Passed: <strong>" + report.Passed + "</strong> | Failed: <strong>" + report.Failed + "</strong></p>");
        html.AppendLine("<p>Skipped: <strong>" + report.Skipped + "</strong> | Inconclusive: <strong>" + report.Inconclusive + "</strong></p>");
        html.AppendLine("<p class=\"muted\">XML: <code>" + HtmlEncode(report.Path) + "</code></p>");
        html.AppendLine("</section>");
        return html.ToString();
    }

    private static string BuildPrettyReportCasesSection(string label, RoaeAiTestReport report)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine("<section class=\"card\">");
        html.AppendLine("<h2>" + HtmlEncode(label) + " Test Cases</h2>");

        if (report == null)
        {
            html.AppendLine("<p class=\"muted\">No result file available.</p>");
            html.AppendLine("</section>");
            return html.ToString();
        }

        if (report.TestCases.Count == 0)
        {
            html.AppendLine("<p class=\"muted\">No test-case nodes found in the XML.</p>");
            html.AppendLine("</section>");
            return html.ToString();
        }

        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Name</th><th>Description</th><th>Result</th><th>Duration</th><th>Full Name</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (RoaeAiTestCaseReport testCase in report.TestCases)
        {
            string badgeClass = string.Equals(testCase.Result, "Passed", StringComparison.OrdinalIgnoreCase) ? "pass" : "fail";
            html.AppendLine("<tr>");
            html.AppendLine("<td>" + HtmlEncode(testCase.Name) + "</td>");
            html.AppendLine("<td>" + HtmlEncode(string.IsNullOrWhiteSpace(testCase.Description) ? "-" : testCase.Description) + "</td>");
            html.AppendLine("<td><span class=\"badge " + badgeClass + "\">" + HtmlEncode(testCase.Result) + "</span></td>");
            html.AppendLine("<td>" + HtmlEncode(testCase.DurationText) + "</td>");
            html.AppendLine("<td><code>" + HtmlEncode(testCase.FullName) + "</code></td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("</section>");
        return html.ToString();
    }

    private static void ResetDevState()
    {
        List<NpcAIDevTools> sceneTools = GetSceneDevTools();
        if (sceneTools.Count > 0)
        {
            foreach (NpcAIDevTools tool in sceneTools)
                tool.ResetRuntimeStateAndPlannerCache();

            Debug.Log("[ROAE][AI][Dashboard][SUCCESS] action=reset_dev_state mode=scene_tools count=" + sceneTools.Count);
            return;
        }

        NpcAIDevTools.ResetRuntimeState(
            creativity: 40,
            empathy: 0,
            corruption: 0,
            npcIds: new[] { "barista", "anticar", "madame_lichenia" });

        PlayerPrefs.Save();
        Debug.Log("[ROAE][AI][Dashboard][SUCCESS] action=reset_dev_state mode=static_fallback");
    }

    private static void PingSceneDevTools()
    {
        List<NpcAIDevTools> sceneTools = GetSceneDevTools();
        if (sceneTools.Count == 0)
        {
            Debug.LogWarning("[ROAE][AI][Dashboard][FAIL] action=ping_scene_dev_tools reason=no_loaded_scene_tools");
            return;
        }

        foreach (NpcAIDevTools tool in sceneTools)
            EditorGUIUtility.PingObject(tool);
    }

    private static string BuildSceneToolsSummary()
    {
        List<NpcAIDevTools> sceneTools = GetSceneDevTools();
        if (sceneTools.Count == 0)
            return "Nu am gasit NpcAIDevTools in scenele incarcate. Resetul va folosi fallback-ul static.";

        return "Gasite " + sceneTools.Count + " tool(s): " + string.Join(", ", sceneTools.Select(tool => tool.gameObject.name));
    }

    private static List<NpcAIDevTools> GetSceneDevTools()
    {
        return Resources.FindObjectsOfTypeAll<NpcAIDevTools>()
            .Where(tool =>
                tool != null &&
                !EditorUtility.IsPersistent(tool) &&
                tool.gameObject.scene.IsValid())
            .OrderBy(tool => tool.gameObject.scene.name)
            .ThenBy(tool => tool.gameObject.name)
            .ToList();
    }

    private static void RevealFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Debug.LogWarning("[ROAE][AI][Dashboard][FAIL] action=reveal_file reason=missing_file path=" + path);
            return;
        }

        EditorUtility.RevealInFinder(path);
    }

    private static RoaeAiTestReport ReadReport(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            XDocument document = XDocument.Load(path);
            XElement root = document.Root;
            if (root == null)
                return null;

            return new RoaeAiTestReport
            {
                Path = path,
                Total = ReadIntAttribute(root, "total"),
                Passed = ReadIntAttribute(root, "passed"),
                Failed = ReadIntAttribute(root, "failed"),
                Skipped = ReadIntAttribute(root, "skipped"),
                Inconclusive = ReadIntAttribute(root, "inconclusive"),
                GeneratedAtText = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm:ss"),
                TestCases = ReadTestCases(root)
            };
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[ROAE][AI][Dashboard][FAIL] action=read_report path=" + path + " reason=" + exception.Message);
            return null;
        }
    }

    private static int ReadIntAttribute(XElement element, string attributeName)
    {
        XAttribute attribute = element.Attribute(attributeName);
        return attribute != null && int.TryParse(attribute.Value, out int parsedValue)
            ? parsedValue
            : 0;
    }

    private static List<RoaeAiTestCaseReport> ReadTestCases(XElement root)
    {
        return root.Descendants("test-case")
            .Select(testCase => new RoaeAiTestCaseReport
            {
                Name = ReadStringAttribute(testCase, "name"),
                FullName = ReadStringAttribute(testCase, "fullname"),
                Result = ReadStringAttribute(testCase, "result"),
                DurationText = ReadStringAttribute(testCase, "duration"),
                Description = ReadPropertyValue(testCase, "Description")
            })
            .OrderBy(testCase => testCase.FullName)
            .ToList();
    }

    private static string ReadStringAttribute(XElement element, string attributeName)
    {
        return element.Attribute(attributeName)?.Value ?? string.Empty;
    }

    private static string ReadPropertyValue(XElement testCase, string propertyName)
    {
        XElement properties = testCase.Element("properties");
        if (properties == null)
            return string.Empty;

        foreach (XElement property in properties.Elements("property"))
        {
            XAttribute nameAttribute = property.Attribute("name");
            if (nameAttribute == null || !string.Equals(nameAttribute.Value, propertyName, StringComparison.Ordinal))
                continue;

            XAttribute valueAttribute = property.Attribute("value");
            return valueAttribute?.Value ?? string.Empty;
        }

        return string.Empty;
    }

    private static string HtmlEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private sealed class RoaeAiTestReport
    {
        public string Path;
        public int Total;
        public int Passed;
        public int Failed;
        public int Skipped;
        public int Inconclusive;
        public string GeneratedAtText;
        public List<RoaeAiTestCaseReport> TestCases = new List<RoaeAiTestCaseReport>();
    }

    private sealed class RoaeAiTestCaseReport
    {
        public string Name;
        public string Description;
        public string FullName;
        public string Result;
        public string DurationText;
    }
}
