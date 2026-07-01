using UnityEngine;

public class HunterCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Log all physical collisions for easy debugging in the console
        Debug.Log($"[HunterCollision] Physical collision detected on {gameObject.name} with {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        HandleContact(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Log all trigger overlaps in case the colliders are configured as triggers
        Debug.Log($"[HunterCollision] Trigger overlap detected on {gameObject.name} with {other.gameObject.name} (Tag: {other.gameObject.tag})");
        HandleContact(other.gameObject);
    }

    private void HandleContact(GameObject otherGo)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        // Check if this script is on the Hunter and touched the Runner,
        // OR if this script is on the Runner and touched the Hunter,
        // OR if we collided with the Runner/Hunter by tag.
        bool isHunterHittingRunner = (transform == gameManager.hunterTransform) && 
                                     (otherGo.CompareTag("Runner") || otherGo.transform == gameManager.runnerTransform);
                                     
        bool isRunnerHittingHunter = (transform == gameManager.runnerTransform) && 
                                     (otherGo.CompareTag("Hunter") || otherGo.transform == gameManager.hunterTransform);

        // General fallback: check tags directly if transforms aren't assigned
        bool tagMatch = (gameObject.CompareTag("Hunter") && otherGo.CompareTag("Runner")) || 
                        (gameObject.CompareTag("Runner") && otherGo.CompareTag("Hunter"));

        if (isHunterHittingRunner || isRunnerHittingHunter || tagMatch)
        {
            Debug.Log($"[HunterCollision] VALID CATCH REGISTERED: {gameObject.name} contacted {otherGo.name}!");
            gameManager.OnHunterCaughtRunner();
        }
    }
}
