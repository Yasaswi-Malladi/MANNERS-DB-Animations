using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
#endif

public class AgentLogger : MonoBehaviour
{
    public GameObject Robot;
    public GameObject[] HumanAgents;
    public GameObject[] SittingHumans;
    public GameObject[] KidAgents;
    public GameObject[] GroupedHumans;
    public GameObject[] AnimalAgents;

    public float GroupRadius = 1.0f;
    public Vector3 GroupCenter;
    public bool IsPlayingMusic = false;

    public float logInterval = 0.1f;

    private string logPath;
    private StreamWriter writer;
    private float timer = 0f;
    private float recordingStartTime = -1f;
    private bool isLogging = false;
    private bool headersWritten = false;
    private string screenshotFolder;
    private Vector3[] initialGroupPositions;
    private string groupChanged = "No";

#if UNITY_EDITOR
    private RecorderController controller;
#endif


    void Start()
    {
#if UNITY_EDITOR
        string logFolder = Path.Combine(Application.dataPath, "Logs");
        if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
        screenshotFolder = Path.Combine(Application.dataPath, "Screenshots");
        if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

        logPath = Path.Combine(logFolder, "AgentLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
        writer = new StreamWriter(logPath);
        writer.WriteLine("sep=;");

        var windows = Resources.FindObjectsOfTypeAll<RecorderWindow>();
        if (windows.Length > 0)
        {
            var rw = windows[0];
            var controllerField = typeof(RecorderWindow).GetField("m_RecorderController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (controllerField != null)
            {
                controller = controllerField.GetValue(rw) as RecorderController;
            }
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        bool isRecording = controller != null && controller.IsRecording();

        if (isRecording && !isLogging)
        {
            isLogging = true;
            recordingStartTime = Time.time;
            Debug.Log("Logging started.");

            if (GroupedHumans.Length > 0)
            {
                initialGroupPositions = GroupedHumans.Select(g => g.transform.position).ToArray();
                GroupCenter = initialGroupPositions.Aggregate(Vector3.zero, (acc, pos) => acc + pos) / initialGroupPositions.Length;
                GroupRadius = GroupedHumans.Max(g => Vector3.Distance(GroupCenter, g.transform.position));
            }
        }
        else if (!isRecording && isLogging)
        {
            isLogging = false;
            Debug.Log("Logging stopped.");
            writer?.Flush();
        }

        if (isLogging)
        {
            timer += Time.deltaTime;
            if (timer >= logInterval)
            {
                timer = 0f;
                float timeSinceRecording = Time.time - recordingStartTime;

                int totalHumans = HumanAgents.Count(h => h != null) + SittingHumans.Count(h => h != null) + GroupedHumans.Count(h => h != null);
                int sittingCount = SittingHumans.Count(h => h != null);
                int kidCount = KidAgents.Count(k => k != null);
                int animalCount = AnimalAgents.Count(a => a != null);
                int groupCount = GroupedHumans.Count(g => g != null);
                int totalAgents = totalHumans + kidCount + animalCount + 1;

                float distanceToGroup = Vector3.Distance(Robot.transform.position, GroupCenter);
                int robotWithinGroup = distanceToGroup <= GroupRadius ? 1 : 0;

                int robotFacingGroup = robotWithinGroup == 1 ? 1 : (IsFacing(Robot.transform, GroupCenter) ? 1 : 0);

                // Reset groupChanged at start of each interval
                groupChanged = "No";

                if (GroupedHumans.Length > 0 && initialGroupPositions != null && GroupedHumans.Length == initialGroupPositions.Length)
                {
                    for (int i = 0; i < GroupedHumans.Length; i++)
                    {
                        if (Vector3.Distance(GroupedHumans[i].transform.position, initialGroupPositions[i]) > 0.1f)
                        {
                            groupChanged = "Yes";
                            break;
                        }
                    }
                }

                List<GameObject> nonKidHumans = HumanAgents.Concat(SittingHumans).Concat(GroupedHumans).Where(h => h != null).ToList();
                List<(float dist, float angle, GameObject go)> closestHumans = nonKidHumans
                    .Select(h => (dist: Vector3.Distance(Robot.transform.position, h.transform.position), angle: AngleBetween(Robot.transform, h.transform), go: h))
                    .OrderBy(t => t.dist)
                    .Take(3)
                    .ToList();

                GameObject closestKid = KidAgents.Where(k => k != null).OrderBy(k => Vector3.Distance(Robot.transform.position, k.transform.position)).FirstOrDefault();
                float kidDist = closestKid != null ? Vector3.Distance(Robot.transform.position, closestKid.transform.position) : -1f;
                float kidAngle = closestKid != null ? AngleBetween(Robot.transform, closestKid.transform) : -1f;

                GameObject closestAnimal = AnimalAgents.Where(a => a != null).OrderBy(a => Vector3.Distance(Robot.transform.position, a.transform.position)).FirstOrDefault();
                float animalDist = closestAnimal != null ? Vector3.Distance(Robot.transform.position, closestAnimal.transform.position) : -1f;

                float[] reverseAngles = closestHumans.Select(t => AngleBetween(t.go.transform, Robot.transform)).ToArray();
                int[] robotFacing = closestHumans.Select(t => IsFacing(Robot.transform, t.go.transform) ? 1 : 0).ToArray();
                int[] humansFacingRobot = closestHumans.Select(t => IsFacing(t.go.transform, Robot.transform) ? 1 : 0).ToArray();

                List<string> columns = new List<string>
                {
                    timeSinceRecording.ToString("F2"),
                    totalHumans.ToString(),
                    groupCount.ToString(),
                    GroupRadius.ToString("F2"),
                    distanceToGroup.ToString("F2"),
                    robotWithinGroup.ToString(),
                    robotFacingGroup.ToString(),
                    "0", // RobotWorkRadius

                    // Distances to closest 3 humans
                    closestHumans.Count > 0 ? closestHumans[0].dist.ToString("F2") : "",
                    closestHumans.Count > 1 ? closestHumans[1].dist.ToString("F2") : "",
                    closestHumans.Count > 2 ? closestHumans[2].dist.ToString("F2") : "",

                    // Angles to closest 3 humans
                    closestHumans.Count > 0 ? closestHumans[0].angle.ToString("F2") : "",
                    closestHumans.Count > 1 ? closestHumans[1].angle.ToString("F2") : "",
                    closestHumans.Count > 2 ? closestHumans[2].angle.ToString("F2") : "",

                    // Reverse angle
                    reverseAngles.Length > 0 ? reverseAngles[0].ToString("F2") : "",

                    // Robot facing humans
                    robotFacing.Length > 0 ? robotFacing[0].ToString() : "",
                    robotFacing.Length > 1 ? robotFacing[1].ToString() : "",
                    robotFacing.Length > 2 ? robotFacing[2].ToString() : "",

                    // Humans facing robot
                    humansFacingRobot.Length > 0 ? humansFacingRobot[0].ToString() : "",
                    humansFacingRobot.Length > 1 ? humansFacingRobot[1].ToString() : "",
                    humansFacingRobot.Length > 2 ? humansFacingRobot[2].ToString() : "",

                    kidCount.ToString(),
                    kidDist >= 0f ? kidDist.ToString("F2") : "",
                    animalCount.ToString(),
                    animalDist >= 0f ? animalDist.ToString("F2") : "",
                    sittingCount.ToString(),
                    IsPlayingMusic ? "1" : "0",
                    totalAgents.ToString(),
                    groupChanged,
                    "" // spacer
                };

                void AppendMeta(GameObject obj)
                {
                    if (obj != null)
                    {
                        string pos = $"({obj.transform.position.x:F2},{obj.transform.position.y:F2},{obj.transform.position.z:F2})";

                        float yRot = obj.transform.eulerAngles.y;

                        // Adjust robot rotation to match facing logic (robot front is right in Unity)
                        if (obj == Robot)
                        {
                            yRot = yRot - 90f;
                            if (yRot < 0f) yRot += 360f;
                        }

                        string rot = $"({obj.transform.eulerAngles.x:F2},{yRot:F2},{obj.transform.eulerAngles.z:F2})";
                        columns.Add(obj.name + pos + rot);
                    }
                }


                AppendMeta(Robot);
                foreach (var h in HumanAgents) AppendMeta(h);
                foreach (var h in SittingHumans) AppendMeta(h);
                foreach (var h in GroupedHumans) AppendMeta(h);
                foreach (var k in KidAgents) AppendMeta(k);
                foreach (var a in AnimalAgents) AppendMeta(a);

                if (!headersWritten)
                {
                    WriteHeaders();
                    headersWritten = true;
                }

                writer.WriteLine(string.Join(";", columns));
                TakeScreenshot(timeSinceRecording);
            }
        }
#endif
    }

    private void WriteHeaders()
    {
        List<string> headers = new List<string>
        {
            "Time", "NumberOfHumans", "NumberOfHumansInGroup", "GroupRadius",
            "DistanceToGroup", "RobotWithinGroup", "RobotFacingGroup", "RobotWorkRadius",
            "DistToHuman1", "DistToHuman2", "DistToHuman3",
            "DirToHuman1", "DirToHuman2", "DirToHuman3",
            "Human1ToRobotDir",
            "RobotFacingHuman1", "RobotFacingHuman2", "RobotFacingHuman3",
            "Human1FacingRobot", "Human2FacingRobot", "Human3FacingRobot",
            "NumberOfChildren", "DistanceToClosestChild",
            "NumberOfAnimals", "DistanceToAnimal",
            "NumberOnSofa", "PlayingMusic",
            "TotalNumberOfAgents",
            "GroupChanged",
            ""
        };

        void AddMetaHeader(string label) => headers.Add(label + "_Meta");

        AddMetaHeader("Robot");
        for (int i = 0; i < HumanAgents.Length; i++) AddMetaHeader("Human" + (i + 1));
        for (int i = 0; i < SittingHumans.Length; i++) AddMetaHeader("SittingHuman" + (i + 1));
        for (int i = 0; i < GroupedHumans.Length; i++) AddMetaHeader("GroupedHuman" + (i + 1));
        for (int i = 0; i < KidAgents.Length; i++) AddMetaHeader("Kid" + (i + 1));
        for (int i = 0; i < AnimalAgents.Length; i++) AddMetaHeader("Animal" + (i + 1));

        writer.WriteLine(string.Join(";", headers));
    }

    private void TakeScreenshot(float timestamp)
    {
#if UNITY_EDITOR
        string filename = Path.Combine(screenshotFolder, $"screenshot_{timestamp:F2}.png");
        ScreenCapture.CaptureScreenshot(filename);
#endif
    }

private float AngleBetween(Transform from, Vector3 targetPos)
{
    Vector3 directionToTarget = (targetPos - from.position).normalized;

    // Flatten both vectors to the XZ plane
    Vector3 flatForward = new Vector3(from.forward.x, 0, from.forward.z).normalized;
    Vector3 flatTargetDir = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

    // Compute signed angle around Y axis (up)
    float angle = Vector3.SignedAngle(flatForward, flatTargetDir, Vector3.up);
        angle = -angle;
    // Robot-specific correction
    if (from.gameObject == Robot)
    {
        angle -= 90f;  // Clockwise correction to match robot's logical "front"
    }

    // Normalize to [0, 360)
    if (angle < 0) angle += 360f;

    return angle;
}

private float AngleBetween(Transform from, Transform to)
{
    return AngleBetween(from, to.position);
}

private bool IsFacing(Transform from, Vector3 targetPos, float tolerance = 30f)
{
    float angle = AngleBetween(from, targetPos);
    return angle <= tolerance || angle >= 360f - tolerance;
}

private bool IsFacing(Transform from, Transform to, float tolerance = 30f)
{
    return IsFacing(from, to.position, tolerance);
}


    void OnApplicationQuit()
    {
        writer?.Flush();
        writer?.Close();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        writer?.Flush();
        writer?.Close();
#endif
    }
}
