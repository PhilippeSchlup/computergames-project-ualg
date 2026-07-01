using UnityEngine;

public class HumanPlayerController : IPlayerController
{
    private GameCharacter character;
    private float moveSpeed;
    private float turnSpeed = 150f;
    private Vector2 rawInput = Vector2.zero;
    private float moveInput;
    private float turnInput;

    public float CurrentSpeed => Mathf.Abs(moveInput) * moveSpeed;

    public void Initialize(GameCharacter character, float speed, bool isHunter)
    {
        this.character = character;
        this.moveSpeed = speed;

        // Disable NavMeshAgent so it doesn't fight manual Rigidbody control
        if (character.Agent != null)
        {
            character.Agent.enabled = false;
        }

        // Enable Rigidbody physics control
        if (character.Rb != null)
        {
            character.Rb.isKinematic = false;
        }

        // Ensure PlayerInput is enabled to receive inputs
        if (character.InputComponent != null)
        {
            character.InputComponent.enabled = true;
        }
    }

    public void UpdateController()
    {
        Vector2 processedInput = rawInput;
        if (processedInput.magnitude <= 0.15f)
        {
            processedInput = Vector2.zero;
        }

        moveInput = processedInput.y;
        turnInput = processedInput.x;
    }

    public void FixedUpdateController()
    {
        if (character == null) return;

        // Apply forward/backward movement
        Vector3 movement = character.transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        character.Rb.MovePosition(character.Rb.position + movement);

        // Apply rotation
        Quaternion turnRotation = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
        character.Rb.MoveRotation(character.Rb.rotation * turnRotation);
    }

    public void OnMove(Vector2 input)
    {
        rawInput = input;
    }

    public void Deactivate()
    {
        rawInput = Vector2.zero;
        moveInput = 0f;
        turnInput = 0f;

        // Disable PlayerInput so it doesn't listen when character is not controlled by human
        if (character != null && character.InputComponent != null)
        {
            character.InputComponent.enabled = false;
        }
    }
}
