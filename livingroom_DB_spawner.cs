using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class livingroom_DB_spawner : MonoBehaviour
{
    public string [] All_Positions;
    public string [] PosRotTemp;
    public String PositionTemp, RotationTemp;
    public GameObject[] Agents;
    public GameObject[] SpawnAgents;
    public Vector3 Position;
    public Quaternion Rotation;
    public int Work_Radius;
    public GameObject Person1;
    public GameObject Person2;
    public GameObject Person3;
    public GameObject Person4;
    public GameObject Person5;
    public GameObject Person6;
    public GameObject Sitting1;
    public GameObject Sitting2;
    public GameObject Child1;
    public GameObject Child2;
    public GameObject Animal;
    public GameObject Robot;
    public GameObject Circle;
    public GameObject Arrow;
    public GameObject Music; 
    public GameObject Person1Temp;
    public GameObject Person2Temp;
    public GameObject Person3Temp;
    public GameObject Person4Temp;
    public GameObject Person5Temp;
    public GameObject Person6Temp;
    public GameObject Sitting1Temp;
    public GameObject Sitting2Temp;
    public GameObject Child1Temp;
    public GameObject Child2Temp;
    public GameObject AnimalTemp;
    public GameObject RobotTemp;
    public GameObject ArrowTemp;
    public GameObject MusicTemp;
    public GameObject [] RobotArray;
    public GameObject Pepper;
    public GameObject Roomba;
    public GameObject PR2;
    public GameObject Nao;
    public Text myText;
    public GameObject canvas; // Reference to the canvas
    public GameObject imagePrefab; // The prefab of the UI image element you want to add

    public Vector3 screenPosition; // The position on the screen where you want to add the UI element
    public GameObject MusicRV;


  



 // Read and parse data from a CSV file
    public string[] CSV_Reader(int csv_line)
    {
        // Use stream reader to read CSV file
        //string File = @"C:\INTERNSHIP_WITH_DR_CHURAMANI\GitHub\data_csv_files\ExtraScenes(cleaned).csv";
        StreamReader csvReader = new StreamReader(@"C:\INTERNSHIP_WITH_DR_CHURAMANI\GitHub\data_csv_files\ExtraScenes_ALL.csv");

        // Read lines until the specified line number
        for (int i = 0; i < csv_line; i++)
        {
            csvReader.ReadLine();
        }
        string Current_Line = csvReader.ReadLine();
        List<string> csvValues = new List<string>();

        // Parse CSV values and handle quoted values
        for (int i = 0; i < Current_Line.Length; i++)
        {
            if (Current_Line[i] == '"')
            {
                string TempValue = "";
                i++;
                while (Current_Line[i] != '"')
                {
                    TempValue += Current_Line[i];
                    i++;
                }
                csvValues.Add(TempValue);
                i++;
            }
            else
            {
                string TempValue = "";
                while (Current_Line[i] != ',')
                {
                    TempValue += Current_Line[i];
                    i++;
                }
                csvValues.Add(TempValue);
            }
        }

        return csvValues.ToArray();
    }

    // Convert a string to Quaternion representation
    public static Quaternion StringToQuaternion(string sQuaternion)
    {
        // Remove parentheses if present
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
        {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }

        // Split the string into components and create a Quaternion
        string[] sArray = sQuaternion.Split(',');
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));

        return result;
    }

    // Convert a string to Vector3 representation
    public static Vector3 StringToVector3(string sVector)
    {
        // Remove parentheses if present
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // Split the string into components and create a Vector3
        string[] sArray = sVector.Split(',');
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
    // Draw a white circle around a specified game object
    private void DrawCircleAroundGameObject(GameObject targetObject, float radius)
    {
        // Get or add a LineRenderer component to the target game object
        LineRenderer lineRenderer = targetObject.GetComponent<LineRenderer>();
        int segments = 50;
        if (lineRenderer == null)
        {
            lineRenderer = targetObject.AddComponent<LineRenderer>();
        }

        // Set LineRenderer properties for the circle
        lineRenderer.positionCount = segments + 1;  // Number of line segments in the circle
        lineRenderer.startWidth = 0.1f;             // Starting width of the line
        lineRenderer.endWidth = 0.1f;               // Ending width of the line
        lineRenderer.startColor = Color.white;      // Starting color of the line
        lineRenderer.endColor = Color.white;        // Ending color of the line
        lineRenderer.useWorldSpace = true;          // Use world space for positions

        // Calculate angle increment based on the number of segments
        float angleIncrement = 360f / segments;
        Material whiteMaterial = new Material(Shader.Find("Unlit/Color"));
        whiteMaterial.color = Color.white;
        lineRenderer.material = whiteMaterial;

        // Loop to create the line segments for the circle
        for (int i = 0; i <= segments; i++)
        {
            // Calculate angle in degrees
            float angle = i * angleIncrement;

            // Calculate x and y positions for the current angle
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            // Calculate position of the current point on the circle
            Vector3 position = new Vector3(targetObject.transform.position.x + x, targetObject.transform.position.y + (float)0.1 , targetObject.transform.position.z + z);

            // Set the position of the LineRenderer point
            lineRenderer.SetPosition(i, position);
        }
    }
    private void ArrowFromRobot(GameObject Robot)
    {
        Vector3 tempArrowRot = Robot.gameObject.transform.forward;
        Vector3 pos = Robot.gameObject.transform.position - (0.5f * tempArrowRot);
        Quaternion arrowRot = Robot.gameObject.transform.rotation;
        Vector3 temp_rot = Robot.gameObject.transform.rotation.eulerAngles;
        temp_rot[0] = 90;
        temp_rot[2] = 90;
        pos[1] = -0.8f;
        arrowRot = Quaternion.Euler(temp_rot);
        Arrow.gameObject.transform.localScale = new Vector3(0.8f, 0.2f, 0.5f);
        ArrowTemp = Instantiate(Arrow, pos, arrowRot);
    }
    private void MusicPLaying()
    {
        
        if (All_Positions[5] == "1")
        {    
            //myText = myText.GetComponent<Text>();
            //myText.text = "Music is playing";
            Vector3 soundPos = new Vector3(-6.756f, 2.511f, -3.901f);
            Vector3 soundRot1 = new Vector3(0, 133, 0);
            Quaternion soundRot = Quaternion.Euler(soundRot1);
            Music.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            MusicTemp = Instantiate(Music, soundPos, soundRot);
        }
    }
    private void DestroyMusic()
    {
        if ( MusicTemp != null)
        {
            Destroy(MusicTemp);
        }
    }
    private void DestroyArrow()
    {
            if (ArrowTemp != null)
            {
                Destroy(ArrowTemp);
            }
    }

    private void DestroyCircle(GameObject targetObject)
    {
        // Check if the target object has a LineRenderer component
        LineRenderer lineRenderer = targetObject.GetComponent<LineRenderer>();
        
        // If the LineRenderer exists, destroy it
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
    public static void MoveCameraToGameObject(Camera camera, GameObject targetObject)
    {
        if (camera != null && targetObject != null)
        {
            // Set the camera's position to match the target object's position
            camera.transform.position = targetObject.transform.position;
         
            
            // Align the camera's rotation with the target object's rotation
            camera.transform.rotation = targetObject.transform.rotation;

    
        }
    }

    public static void TransformOffset(GameObject Robot, int i)
    {
        //robot order pepper,nao, roomba, p2r,cobot, everday robot meta
        
        Vector3[] Transformations = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, (float)-0.139, 0.0f),
            new Vector3(0.0f, (float)0, 0.0f),
            new Vector3(0.0f, 0.0f, 0.0f)
        };
        Robot.gameObject.transform.position += Transformations[i];
                // Define the rotation quaternions for each index
        Quaternion[] Rotations = new Quaternion[]
        {
            Quaternion.Euler(0.0f, 90f, 0.0f),                        // No rotation
            Quaternion.Euler(0.0f, 0f, 0.0f),        
            Quaternion.Euler(0.0f, 0f, 0.0f),         
            Quaternion.Euler(0.0f, 0f, 0.0f)                        // No rotation
        };
        
        // Apply rotation to the GameObject
        Robot.transform.rotation *= Rotations[i] ;
    }
        public void TransformCameraOffset(Camera camera, int i)
    {
        
        // Define the camera offset vectors
        Vector3[] CameraOffsets = new Vector3[]
        {
            new Vector3(0f, 1.075f, 0f),     // Offset for vector 1
            new Vector3(0f, 0.974f, 0f),     // Offset for vector 2
            new Vector3(0f, 0.099f, 0f),  // Offset for vector 3
            new Vector3(0.223f, 1.2f, 0f)     // Offset for vector 4
        };
        float[] scaleFactors = new float[]
        {
            0.1f,
            0.1f,
            0.1f,
            0.2f
        };
        Vector3[] rotationAngles = new Vector3[]
        {
        new Vector3(0f, 0f, 0f),  // Rotation for position 1
        new Vector3(0f, 0f, 0f),  // Rotation for position 2
        new Vector3(0f, 0f, 0f),  // Rotation for position 3
        new Vector3(0f, 0f, 0f)  // Rotation for position 4
        };

        // Check if the index is valid
        if (i >= 0 && i < CameraOffsets.Length)
        {
            camera.transform.position += CameraOffsets[i] + scaleFactors[i] * camera.transform.forward;
                    // Apply rotation using Quaternion.Euler
            camera.transform.localRotation = Quaternion.Euler(rotationAngles[i]);
        }
        else
        {
            Debug.LogError("Invalid index for camera offset.");
        }
    }
    public void RotateObject(GameObject robot, int i)
    {
        // Define the rotation quaternions for each index
        Quaternion[] Rotations = new Quaternion[]
        {
            Quaternion.Euler(0.0f, 180f, 0.0f),                        // No rotation
            Quaternion.Euler(0.0f, 180f, 0.0f),        
            Quaternion.Euler(0.0f, 180f, 0.0f),         
            Quaternion.Euler(0.0f, 180f, 0.0f)                        // No rotation
        };
        
        // Apply rotation to the GameObject
        robot.transform.rotation *= Rotations[i] ;
    }
    

    public void RVmusic()
    {
         if (All_Positions[5] == "1")
         {
        // Create a new instance of the UI image element prefab
        MusicRV = Instantiate(imagePrefab, canvas.transform);

        // Access the RectTransform component of the new UI image element
        RectTransform rectTransform = MusicRV.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Set the anchored position to the top-left corner
            rectTransform.anchorMin = new Vector2(0, 1); // Anchor to the top-left
            rectTransform.anchorMax = new Vector2(0, 1); // Anchor to the top-left
            rectTransform.pivot = new Vector2(0, 1); // Set the pivot to the top-left corner
            rectTransform.anchoredPosition = Vector2.zero; // Set the position to (0, 0)
        }
         }
    }
    public void DestroyRVmusic()
    {
        if (MusicRV != null)
        {
            Destroy(MusicRV); // Destroy the UI image element GameObject
        }
        else
        {
            Debug.LogWarning("UI element is null or already destroyed.");
        }
    }

    public void AssignAgentScript(GameObject Agent)
    {
        if (Agent = Person1)
        {
            Agent.AddComponent<for_h2_rotating_script>();
            
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
        // Initialize the coroutine for spawning and processing agents
        StartCoroutine(ABC());
    }

    // Coroutine for spawning agents and capturing screenshots
    IEnumerator ABC()
    {   
        yield return new WaitForSeconds(2);
        //defining Camera
        Camera camera= Camera.main;
        Vector3 cameraPosition = camera.transform.position;
        Quaternion cameraRotation = camera.transform.rotation;
        // Define arrays for original agents and temporary spawned agents
        RobotArray = new GameObject[] {Pepper, Nao, Roomba, PR2};//
        Agents = new GameObject[] { Person1, Person2, Person3, Person4, Person5, Person6, Sitting1, Sitting2, Child1, Child2, Animal};
        SpawnAgents = new GameObject[] { Person1Temp, Person2Temp, Person3Temp, Person4Temp, Person5Temp, Person6Temp, Sitting1Temp, Sitting2Temp, Child1Temp, Child2Temp, AnimalTemp};
        int[] Lengths = {250, 1039-250, 2000-1039};
        // Loop through CSV data and spawn agents
        //5001 scenes per robot
        //##i<2## to test animating scenes
        //for i = 1 to 5002 to match spreadsheet
        for (int i = 449; i < 450; i++)
        {
            // Read position and rotation data from CSV
            All_Positions = CSV_Reader(i);

            Debug.Log(All_Positions[0]);
            // Loop through agent types and spawn them if valid data exists
            for (int j = 6; j < 17; j++)
            {
                if (All_Positions[j] == "_")
                {
                    // Skip empty positions
                }
                else
                {
                    // Parse position and rotation information
                    PosRotTemp = Regex.Split(All_Positions[j], @"\)\ \(");
                    PositionTemp = PosRotTemp[0] + ")";
                    RotationTemp = "(" + PosRotTemp[1];
                    Position = StringToVector3(PositionTemp);
                    Rotation = StringToQuaternion(RotationTemp);
                    //Debug.Log(j-6);
                    AssignAgentScript(Agents[j-6]);
                    // Spawn the agent at the specified position and rotation
                    SpawnAgents[j - 6] = Instantiate(Agents[j - 6], Position, Rotation);
                }
            }
            PosRotTemp = Regex.Split(All_Positions[17], @"\)\ \(");
            PositionTemp = PosRotTemp[0] + ")";
            RotationTemp = "(" + PosRotTemp[1];
            Position = StringToVector3(PositionTemp);
            Rotation = StringToQuaternion(RotationTemp);
            for( int k = 0; k < RobotArray.Length; k++)
            {
                Robot = Instantiate(RobotArray[k], Position, Rotation);
            
                if (float.Parse(All_Positions[4]) != 0)
                {
                    DrawCircleAroundGameObject(Robot, float.Parse(All_Positions[4]));
                }
                if (float.Parse(All_Positions[3]) == 1)
                {
                    RotateObject(Robot, k);
                    ArrowFromRobot(Robot);
                    RotateObject(Robot, k);
                }
                TransformOffset(Robot, k);
                MusicPLaying();
                String [] Folders = new String[] {"Pepper", "Nao", "Roomba", "PR2"};
                //Debug.Log(Folders[i])
                ScreenCapture.CaptureScreenshot(@"C:\INTERNSHIP_WITH_DR_CHURAMANI\GitHub\SCREENSHOTS\"+ Folders[k] + "_TV" + @"\" + All_Positions[1] + "_" + Folders[k] + "_TV" +".png");
                //yield return new WaitForSeconds(2f);
                // Call PAUSE() to wait for spacebar
                yield return StartCoroutine(PAUSE());

                Debug.Log("Function resumed after spacebar press!");
                yield return null;
                MoveCameraToGameObject(camera, Robot);
                TransformCameraOffset(camera, k);
                DestroyArrow();
                DestroyCircle(Robot);
                DestroyMusic();
                RVmusic();
                // Call PAUSE() to wait for spacebar
                yield return StartCoroutine(PAUSE());

                Debug.Log("Function resumed after spacebar press!");
                Destroy(Robot);
                ScreenCapture.CaptureScreenshot(@"C:\INTERNSHIP_WITH_DR_CHURAMANI\GitHub\SCREENSHOTS\"+ Folders[k] + "_RV" + @"\" + All_Positions[1] + "_" + Folders[k] + "_RV" +".png");
                yield return null;
                DestroyRVmusic();
                //yield return new WaitForSeconds(0.01f);
                // Rest camera position
                camera.transform.position = cameraPosition;
                // Reset Camera Rotation
                camera.transform.rotation = cameraRotation;


            }
            // Destroy spawned agents for the next iteration
            foreach (GameObject Agent in SpawnAgents)
            {
                Destroy(Agent);
            }
            }

        }
        
    IEnumerator PAUSE()
    {
        Debug.Log("Paused! Press Space to continue...");
        
        // Wait until the player presses Spacebar
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        Debug.Log("Spacebar pressed. Resuming...");
    }
    // Update is called once per frame
    void Update()
    {
   
    }
}