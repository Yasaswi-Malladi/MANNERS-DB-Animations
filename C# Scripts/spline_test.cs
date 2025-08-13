using UnityEngine;
using UnityEngine.Splines;

public class RuntimeSplineFollower : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float speed = 0.65f;
    [Range(0f, 1f)] public float startT = 0f;
    public float rotationThreshold = 1f;      // Degrees to consider rotation done
    public float rotationSpeed = 50f;         // Rotation speed (degrees per second)
    public float idleAfterRotationDuration = 0f; // Time to idle after rotation before walking
    public bool enableRotation = true;        // ✅ Toggle for rotation behavior

    private float t;
    private float splineLength;
    private bool initialized = false;
    private bool rotating = true;
    private float idleTimer = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!initialized)
        {
            if (splineContainer != null && splineContainer.Spline != null && splineContainer.Spline.Count >= 2)
            {
                splineLength = splineContainer.CalculateLength();
                t = startT;
                Vector3 startPos = splineContainer.EvaluatePosition(t);
                Vector3 startTangent = ((Vector3)splineContainer.EvaluateTangent(t)).normalized;

                transform.position = startPos;

                // Play idle animation at start
                if (animator) animator.SetBool("isWalking", false);

                rotating = enableRotation; // ✅ Only rotate if enabled
                initialized = true;
            }
            return;
        }

        if (enableRotation && rotating)
        {
            // Rotate toward spline tangent smoothly
            Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(t)).normalized;
            Vector3 lookDir = new Vector3(tangent.x, 0f, tangent.z).normalized;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            float angle = Quaternion.Angle(transform.rotation, targetRot);
            if (angle <= rotationThreshold)
            {
                rotating = false;
                idleTimer = idleAfterRotationDuration;
                if (animator) animator.SetBool("isWalking", false);
            }
            return;
        }
        else
        {
            rotating = false; // skip rotating phase if not enabled
        }

        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
            return;
        }

        if (animator && !animator.GetBool("isWalking"))
            animator.SetBool("isWalking", true);

        if (t >= 1f || splineLength <= 0f)
        {
            if (animator) animator.SetBool("isWalking", false);
            return;
        }

        float deltaDist = speed * Time.deltaTime;
        float deltaT = deltaDist / splineLength;
        t = Mathf.Clamp01(t + deltaT);

        Vector3 pos = splineContainer.EvaluatePosition(t);
        Vector3 tangentMove = ((Vector3)splineContainer.EvaluateTangent(t)).normalized;

        transform.position = pos;

        if (enableRotation)
        {
            // Rotate smoothly toward movement direction while walking
            Vector3 lookDirMove = new Vector3(tangentMove.x, 0, tangentMove.z).normalized;
            if (lookDirMove.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotMove = Quaternion.LookRotation(lookDirMove);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotMove, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
