using UnityEngine;
using UnityEngine.AI;

public class MachinePlayerController : IPlayerController
{
    private GameCharacter character;
    private float agentSpeed;
    private float agentAcceleration = 20f;
    private float agentAngularSpeed = 360f;
    private bool isHunter;

    // Hiding Settings
    private float hideRadius = 30f;
    private float hideOffset = 2.5f;
    private float pathUpdateInterval = 0.1f;
    private LayerMask obstacleLayerMask = ~0; // Target all layers by default

    private float updateTimer = 0f;

    public float CurrentSpeed => (character != null && character.Agent != null && character.Agent.enabled)
        ? character.Agent.velocity.magnitude
        : 0f;

    public void Initialize(GameCharacter character, float speed, bool isHunter)
    {
        this.character = character;
        this.agentSpeed = speed;
        this.isHunter = isHunter;

        // Set Rigidbody to Kinematic so physics doesn't conflict with NavMesh steering
        if (character.Rb != null)
        {
            character.Rb.isKinematic = true;
        }

        // Enable NavMeshAgent and configure parameters
        if (character.Agent != null)
        {
            character.Agent.enabled = true;
            character.Agent.speed = agentSpeed;
            character.Agent.acceleration = agentAcceleration;
            character.Agent.angularSpeed = agentAngularSpeed;

            // Force immediate destination update
            updateTimer = pathUpdateInterval;
        }
    }

    public void UpdateController()
    {
        if (character == null) return;

        // Sync speed directly in update to prevent NavMeshAgent initialization from resetting it to inspector defaults
        if (character.Agent != null && character.Agent.enabled && character.Agent.speed != agentSpeed)
        {
            character.Agent.speed = agentSpeed;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer >= pathUpdateInterval)
        {
            updateTimer = 0f;
            UpdateAIDestination();
        }
    }

    public void FixedUpdateController()
    {
        // NavMeshAgent handles its own physics movement
    }

    public void OnMove(Vector2 input)
    {
        // AI does not use manual controller inputs
    }

    public void Deactivate()
    {
        if (character == null) return;

        // Disable NavMeshAgent
        if (character.Agent != null)
        {
            character.Agent.enabled = false;
        }

        // Re-enable Rigidbody physics for human player control
        if (character.Rb != null)
        {
            character.Rb.isKinematic = false;
        }
    }

    private void UpdateAIDestination()
    {
        if (character == null || character.Agent == null || !character.Agent.enabled || !character.Agent.isOnNavMesh) return;

        if (isHunter)
        {
            Transform runner = GameManager.Instance.runnerTransform;
            if (runner != null)
            {
                character.Agent.SetDestination(runner.position);
            }
        }
        else
        {
            Transform hunter = GameManager.Instance.hunterTransform;
            if (hunter != null)
            {
                Vector3 targetDest = FindBestHidingSpot(hunter);
                character.Agent.SetDestination(targetDest);
            }
        }
    }

    private Vector3 FindBestHidingSpot(Transform hunter)
    {
        // Find potential obstacles in the area
        Collider[] colliders = Physics.OverlapSphere(character.transform.position, hideRadius, obstacleLayerMask);

        Vector3 bestSpot = character.transform.position;
        float bestScore = float.MinValue;
        bool foundHidingSpot = false;

        foreach (var col in colliders)
        {
            // Skip player and opponent capsules
            if (col.transform == character.transform || col.transform == hunter)
                continue;

            // Skip triggers
            if (col.isTrigger)
                continue;

            // Skip ground/plane
            if (col.bounds.size.y < 0.2f && col.bounds.size.x > 15f)
                continue;

            Vector3 obstaclePos = col.bounds.center;
            Vector3 dirFromHunter = (obstaclePos - hunter.position).normalized;

            // Hiding point is on the opposite side of the obstacle center relative to the hunter
            Vector3 candidatePos = obstaclePos + dirFromHunter * (col.bounds.extents.magnitude + hideOffset);

            // Sample on NavMesh to ensure it is traversable
            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Vector3 spot = hit.position;

                // Linecast to verify if Hunter's line of sight to the hiding spot is blocked
                Vector3 hunterEye = hunter.position + Vector3.up * 1.5f;
                Vector3 spotEye = spot + Vector3.up * 1.5f;

                bool hitSomething = Physics.Linecast(hunterEye, spotEye, out RaycastHit lineHit, obstacleLayerMask);

                // Spot is hidden if linecast hit an obstacle that is not the runner/hunter
                bool isHidden = hitSomething && lineHit.transform != character.transform && lineHit.transform != hunter;

                // Score this spot
                float distFromHunter = Vector3.Distance(spot, hunter.position);
                float distFromRunner = Vector3.Distance(spot, character.transform.position);

                float score = 0f;
                if (isHidden)
                {
                    score += 1000f; // Strongly prioritize spots that block vision
                }

                score += distFromHunter * 1.5f; // Favor being far from the hunter
                score -= distFromRunner * 0.5f; // Favor closer hiding spots to minimize travel risk

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSpot = spot;
                    foundHidingSpot = true;
                }
            }
        }

        if (foundHidingSpot)
        {
            return bestSpot;
        }

        // Fallback: If no obstacles, flee directly away from the hunter
        Vector3 fleeDir = (character.transform.position - hunter.position).normalized;
        Vector3 fleePos = character.transform.position + fleeDir * 10f;
        if (NavMesh.SamplePosition(fleePos, out NavMeshHit fallbackHit, 10f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return character.transform.position;
    }
}
