using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class GameCharacter : MonoBehaviour
{
    public Rigidbody Rb { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public PlayerInput InputComponent { get; private set; }

    [Header("Falling")]
    public float fallGravityMultiplier = 2.5f;

    private IPlayerController activeController;
    private Animator animator;
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private const float MovingSpeedThreshold = 0.1f;

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Agent = GetComponent<NavMeshAgent>();
        InputComponent = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();

        // Set default physics settings
        Rb.constraints = RigidbodyConstraints.FreezeRotation;
        Rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetController(IPlayerController controller, float speed, bool isHunter)
    {
        if (activeController != null)
        {
            activeController.Deactivate();
        }

        activeController = controller;

        if (activeController != null)
        {
            activeController.Initialize(this, speed, isHunter);
        }
    }

    void Update()
    {
        if (activeController != null)
        {
            activeController.UpdateController();
        }

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = activeController != null ? activeController.CurrentSpeed : 0f;
        animator.SetFloat(SpeedParam, speed < MovingSpeedThreshold ? 0f : speed);
    }

    void FixedUpdate()
    {
        if (activeController != null)
        {
            activeController.FixedUpdateController();
        }

        // Extra downward acceleration while falling, so drops feel snappier than default gravity
        if (!Rb.isKinematic && Rb.linearVelocity.y < 0f)
        {
            Rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    // Forward message from PlayerInput component
    public void OnMove(InputValue value)
    {
        if (activeController != null)
        {
            activeController.OnMove(value.Get<Vector2>());
        }
    }
}
