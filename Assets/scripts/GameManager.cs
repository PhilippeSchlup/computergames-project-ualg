using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ArenaSetup
{
    public GameObject arenaObject;
    public Transform hunterSpawnPoint;
    public Transform runnerSpawnPoint;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const float RoundLength = 30f;

    [Header("Testing Mode")]
    public bool enableTestMode = false;
    public int forceArenaIndex = 0; 

    [Header("Menu Panels")]
    public GameObject startScreenPanel;
    public GameObject instructionsScreenPanel; 
    public GameObject endScreenPanel;
    public GameObject hudPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("End Screen Elements")]
    public TextMeshProUGUI victorText;
    public TextMeshProUGUI statsText; 

    [Header("Arena Progression")]
    public ArenaSetup[] arenas; 

    [Header("Characters & Camera")]
    public CameraFollow cameraController;
    public Transform hunterTransform;
    public Transform runnerTransform;

    [Header("Environment")]
    public SkyboxDayNightController skyboxController;
    public GameObject titleScreenCamera;

    [Header("Game Characters (NEW)")]
    public GameCharacter hunterCharacter;
    public GameCharacter runnerCharacter;

    [Header("Speed Settings (NEW)")]
    public float hunterSpeed = 12f;
    public float runnerSpeed = 9f;
    public float catchDistance = 1.6f;

    private bool isGameActive = false;
    private int currentRound = 1;
    private int humanWins = 0;
    private int machineWins = 0;
    private bool isHumanHunter = true;

    private float totalTimePlayed = 0f;
    private int roundsCompleted = 0;
    private float timer = RoundLength;
    private int lastTransitionFrame = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Auto-assign the GameCharacter references from the provided Transforms
        if (hunterTransform != null) hunterCharacter = hunterTransform.GetComponent<GameCharacter>();
        if (runnerTransform != null) runnerCharacter = runnerTransform.GetComponent<GameCharacter>();

        ShowStartScreen();
    }

    public void ShowStartScreen()
    {
        startScreenPanel.SetActive(true);
        if (instructionsScreenPanel != null) instructionsScreenPanel.SetActive(false);
        hudPanel.SetActive(false);
        endScreenPanel.SetActive(false);

        if (titleScreenCamera != null) titleScreenCamera.SetActive(true);
        if (cameraController != null) cameraController.gameObject.SetActive(false);

        // Reactivate all arenas so the title screen (and its orbiting camera) can show any of them
        if (arenas != null)
        {
            foreach (var arena in arenas)
            {
                if (arena != null && arena.arenaObject != null)
                {
                    arena.arenaObject.SetActive(true);
                }
            }
        }
    }

    public void ShowInstructionsScreen()
    {
        startScreenPanel.SetActive(false);
        if (instructionsScreenPanel != null) instructionsScreenPanel.SetActive(true);
    }

    public void StartGame()
    {
        startScreenPanel.SetActive(false);
        if (instructionsScreenPanel != null) instructionsScreenPanel.SetActive(false); 
        hudPanel.SetActive(true);

        if (titleScreenCamera != null) titleScreenCamera.SetActive(false);
        if (cameraController != null) cameraController.gameObject.SetActive(true);

        currentRound = 1;
        humanWins = 0;
        machineWins = 0;
        totalTimePlayed = 0f;
        roundsCompleted = 0;
        isHumanHunter = true; 
        
        UpdateScoreBoard();
        StartRound();
    }

    private void StartRound()
    {
        isGameActive = true;
        timer = RoundLength;
        UpdateTimerText();

        if (skyboxController != null) skyboxController.PlayEveningToNight(RoundLength);

        if (enableTestMode) roundText.text = "TESTING ARENA " + forceArenaIndex;
        else roundText.text = "Round " + currentRound + " / 5";

        ConfigureActiveArena();
        ResetCharactersToCurrentArena();

        // INJECT COLLEAGUE'S AI CONTROLLER LOGIC
        if (isHumanHunter)
        {
            if (hunterCharacter != null) hunterCharacter.SetController(new HumanPlayerController(), hunterSpeed, true);
            if (runnerCharacter != null) runnerCharacter.SetController(new MachinePlayerController(), runnerSpeed, false);
            cameraController.target = hunterTransform;
        }
        else
        {
            if (runnerCharacter != null) runnerCharacter.SetController(new HumanPlayerController(), runnerSpeed, false);
            if (hunterCharacter != null) hunterCharacter.SetController(new MachinePlayerController(), hunterSpeed, true);
            cameraController.target = runnerTransform;
        }

        if (cameraController != null) cameraController.SnapToTarget();
    }

    private void ConfigureActiveArena()
    {
        int activeIndex = 0;
        if (enableTestMode) activeIndex = forceArenaIndex;
        else
        {
            if (currentRound == 3 || currentRound == 4) activeIndex = 1;
            if (currentRound == 5) activeIndex = 2;
        }

        for (int i = 0; i < arenas.Length; i++)
        {
            if (arenas[i] != null && arenas[i].arenaObject != null)
            {
                arenas[i].arenaObject.SetActive(i == activeIndex);
            }
        }
    }

    public void EndRound(bool didHumanWinRound)
    {
        if (Time.frameCount == lastTransitionFrame) return;
        lastTransitionFrame = Time.frameCount;

        isGameActive = false;
        
        float timeSpentThisRound = RoundLength - timer;
        totalTimePlayed += timeSpentThisRound;
        roundsCompleted++;

        if (didHumanWinRound) humanWins++;
        else machineWins++;

        UpdateScoreBoard();

        if (enableTestMode)
        {
            StartRound();
            return;
        }

        if (humanWins >= 3 || machineWins >= 3 || currentRound >= 5) EndGame();
        else
        {
            currentRound++;
            isHumanHunter = !isHumanHunter; 
            StartRound();
        }
    }

    private void EndGame()
    {
        hudPanel.SetActive(false);
        endScreenPanel.SetActive(true);

        if (victorText != null)
        {
            if (humanWins > machineWins) victorText.text = "Human Wins the Game!";
            else if (machineWins > humanWins) victorText.text = "Machine Wins the Game!";
            else victorText.text = "It's a Tie!";
        }

        if (statsText != null)
        {
            float avgTime = roundsCompleted > 0 ? totalTimePlayed / roundsCompleted : 0f;
            statsText.text = $"Rounds Played: {roundsCompleted}\nAverage Round Time: {avgTime:F1}s";
        }
    }

    private void ResetCharactersToCurrentArena()
    {
        // 1. Deactivate active controllers to clean up NavMesh data (Colleague's logic)
        if (hunterCharacter != null) hunterCharacter.SetController(null, 0f, false);
        if (runnerCharacter != null) runnerCharacter.SetController(null, 0f, false);

        int activeIndex = 0;
        if (enableTestMode) activeIndex = forceArenaIndex;
        else
        {
            if (currentRound == 3 || currentRound == 4) activeIndex = 1;
            if (currentRound == 5) activeIndex = 2;
        }

        ArenaSetup currentSetup = arenas[activeIndex];

        // Safely warp both characters
        TeleportSafely(hunterTransform, currentSetup.hunterSpawnPoint);
        TeleportSafely(runnerTransform, currentSetup.runnerSpawnPoint);
    }

    public void OnHunterCaughtRunner()
    {
        if (!isGameActive) return;
        EndRound(isHumanHunter);
    }

    void Update()
    {
        if (isGameActive)
        {
            timer -= Time.deltaTime;

            // Colleague's proximity catch fallback
            if (hunterTransform != null && runnerTransform != null)
            {
                if (Vector3.Distance(hunterTransform.position, runnerTransform.position) <= catchDistance)
                {
                    OnHunterCaughtRunner();
                    return;
                }
            }

            if (timer <= 0f)
            {
                timer = 0f;
                UpdateTimerText();
                EndRound(!isHumanHunter);
            }
            else
            {
                UpdateTimerText();
            }
        }
    }

    private void UpdateScoreBoard()
    {
        if (scoreText != null) scoreText.text = "Human: " + humanWins + " | Machine: " + machineWins;
    }

    private void UpdateTimerText()
    {
        if (timerText != null) timerText.text = "Time: " + timer.ToString("F1") + "s";
    }

    private void TeleportSafely(Transform charTransform, Transform spawnTransform)
    {
        if (charTransform == null || spawnTransform == null) return;

        // 1. Force NavMeshAgent to warp
        if (charTransform.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            agent.enabled = false; 
            agent.Warp(spawnTransform.position);
        }

        // 2. Kill all Rigidbody momentum
        if (charTransform.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.position = spawnTransform.position;
            rb.rotation = spawnTransform.rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. Set standard transform
        charTransform.position = spawnTransform.position;
        charTransform.rotation = spawnTransform.rotation;
    }
}