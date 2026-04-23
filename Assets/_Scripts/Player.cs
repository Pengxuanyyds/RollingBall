using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float turnSpeed = 6f;
    private float horizontalInput;
    public static float gameSpeed = 20f;
    public float maxSpeed = 40f;

    [Header("Game State")]
    public static int currentHP = 3;
    public int maxHP = 5;
    public float scoreMultiplier = 0.5f;
    private float currentScore = 0f;
    private float bestScore = 0f;
    private float distanceTravelled = 0f;
    private float nextMilestone = 1000f;
    public bool hasShield = false;

    [Header("UI")]
    public TextMeshProUGUI speedUpText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject gameOverPanel;

    [Header("BGM")]
    public GameObject prefabMusic;

    private bool isGameOver = false;

    // Static reset protection for Domain Reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        gameSpeed = 20f;
        currentHP = 3;
    }

    void Start()
    {
        currentHP = 3;
        isGameOver = false;
        gameSpeed = 20f;
        hasShield = false;
        Time.timeScale = 1;

        bestScore = PlayerPrefs.GetFloat("HighScore", 0f);
        UpdateHPUI();
        UpdateBestScoreUI();

        var music = GameObject.Find("BGMusic");
        if (music == null && prefabMusic != null)
        {
            var m = Instantiate(prefabMusic, null);
            m.name = "BGMusic";
            DontDestroyOnLoad(m);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
            return;
        }

        if (isGameOver) return;

        currentScore += gameSpeed * Time.deltaTime * scoreMultiplier;

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            UpdateBestScoreUI();
        }
        UpdateScoreUI();

        horizontalInput = Input.GetAxis("Horizontal");
        distanceTravelled = transform.position.z - 2;

        if (distanceTravelled >= nextMilestone)
        {
            IncreaseDifficulty();
            nextMilestone += 350f;
        }

        if (transform.position.y < -5f && !isGameOver)
        {
            TriggerGameOver(true);
        }
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        Vector3 forwardMove = Vector3.forward * gameSpeed * Time.fixedDeltaTime;
        Vector3 sideMove = Vector3.right * horizontalInput * turnSpeed * Time.fixedDeltaTime;
        Vector3 totalMove = forwardMove + sideMove;
        transform.Translate(totalMove, Space.World);

        // Rolling rotation
        float ballRadius = 0.5f;
        Vector3 rollAxis = Vector3.Cross(Vector3.up, totalMove.normalized);
        if (rollAxis.sqrMagnitude > 0.001f)
        {
            float rollAngle = totalMove.magnitude / ballRadius * Mathf.Rad2Deg;
            transform.Rotate(rollAxis, rollAngle, Space.World);
        }

        if (transform.position.x < -4f || transform.position.x > 4f)
        {
            transform.Translate(0, -20f * Time.fixedDeltaTime, 0);
        }
    }

    void UpdateHPUI()
    {
        if (hpText != null) hpText.text = "HP: " + currentHP;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + Mathf.FloorToInt(currentScore);
    }

    void UpdateBestScoreUI()
    {
        if (bestScoreText != null) bestScoreText.text = "Record: " + Mathf.FloorToInt(bestScore);
    }

    public void TriggerGameOver(bool isFall)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (isFall)
        {
            currentHP = 0;
            UpdateHPUI();
        }

        Debug.Log("Game Over!");
        PlayerPrefs.SetFloat("HighScore", bestScore);
        PlayerPrefs.Save();
        Time.timeScale = 0;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnEnable()
    {
        Barrier.OnAnyBarrierHit += HandleObstacleHit;
        Bonus.OnBonusCollected += HandleBonusCollected;
        PowerUp.OnShieldCollected += HandleShieldCollected;
    }

    private void OnDisable()
    {
        Barrier.OnAnyBarrierHit -= HandleObstacleHit;
        Bonus.OnBonusCollected -= HandleBonusCollected;
        PowerUp.OnShieldCollected -= HandleShieldCollected;
    }

    void HandleObstacleHit()
    {
        if (hasShield)
        {
            hasShield = false;
            Debug.Log("Shield absorbed a hit!");
            return;
        }

        currentHP--;
        UpdateHPUI();

        if (currentHP <= 0) TriggerGameOver(false);
    }

    void HandleBonusCollected()
    {
        currentHP = Mathf.Min(currentHP + 1, maxHP);
        UpdateHPUI();
    }

    void HandleShieldCollected()
    {
        hasShield = true;
        Debug.Log("Shield activated!");
    }

    void IncreaseDifficulty()
    {
        gameSpeed = Mathf.Min(gameSpeed + 3f, maxSpeed);
        Debug.Log("Speed up! Current speed: " + gameSpeed);

        if (speedUpText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowSpeedUpMessage());
        }
    }

    IEnumerator ShowSpeedUpMessage()
    {
        speedUpText.text = "SPEED UP!";
        speedUpText.gameObject.SetActive(true);

        Color c = speedUpText.color;
        c.a = 1f;
        speedUpText.color = c;

        yield return new WaitForSeconds(1f);

        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            speedUpText.color = c;
            yield return null;
        }

        speedUpText.gameObject.SetActive(false);
    }
}
