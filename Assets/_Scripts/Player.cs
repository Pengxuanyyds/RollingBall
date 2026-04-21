using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float turnSpeed = 6f; // 左右移动速度
    private float horizontalInput; // 用于暂存键盘输入
    public static float gameSpeed = 50f; // 初速度设为 50

    [Header("游戏数据")]
    public static int currentHP = 3;      // 静态变量，供 Barrier 脚本读取
    public float scoreMultiplier = 0.5f;   // 分数倍率，可以根据喜好调整
    private float currentScore = 0f;
    private float bestScore = 0f;
    private float distanceTravelled = 0f;
    private float nextMilestone = 1000f; // 下一次加速的门槛

    [Header("UI引用")]
    public TextMeshProUGUI speedUpText;
    public TextMeshProUGUI hpText;       // 拖入 HPText
    public TextMeshProUGUI scoreText;      // 拖入 ScoreText
    public TextMeshProUGUI bestScoreText;  // 拖入 BestScoreText
    public GameObject gameOverPanel;     // 拖入 GameOverPanel

    [Header("BGM")]
    public GameObject prefabMusic;
    
    private bool isGameOver = false;     // 防止重复触发游戏结束

    

    void Start()
    {
        // 初始化状态
        currentHP = 3;
        isGameOver = false;
        gameSpeed = 50f;
        Time.timeScale = 1; // 确保时间流逝正常

        // 读取最高分存档
        bestScore = PlayerPrefs.GetFloat("HighScore", 0f);

        // 更新 UI 显示
        UpdateHPUI();
        UpdateBestScoreUI();

        // 音乐初始化逻辑保留
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
        // 原有的 R 键重启逻辑保留 (或者可以通过 UI 按钮)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
            return;
        }

        if (isGameOver) return; // 游戏结束时停止所有逻辑

        // --- 实时计分逻辑 ---
        // 分数随路程（速度*时间）增加
        currentScore += gameSpeed * Time.deltaTime * scoreMultiplier;

        // 如果破纪录，同步更新
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            UpdateBestScoreUI();
        }
        UpdateScoreUI();

        // 1. 输入检测
        horizontalInput = Input.GetAxis("Horizontal");

        // 距离追踪与难度提升
        distanceTravelled = transform.position.z-2; // 直接使用 Z 坐标作为距离

        // 3. 加速逻辑判定
        if (distanceTravelled >= nextMilestone)
        {
            IncreaseDifficulty();
            nextMilestone += 500f; // 设置下一个 500m 目标
        }


        // 修改掉落判定高度，比如低于地块顶面 5m 判定为掉落
        if (transform.position.y < -5f && !isGameOver)
        {
            TriggerGameOver(true); // true 代表是由于掉落导致的
        }
    }

    // 所有的物理位移统一放在这里
    void FixedUpdate()
    {
        if (isGameOver) return; // 游戏结束时停止移动

        // --- 核心修改：小球向前运动 ---
        Vector3 forwardMove = Vector3.forward * gameSpeed * Time.fixedDeltaTime;
        Vector3 sideMove = Vector3.right * horizontalInput * turnSpeed * Time.fixedDeltaTime;

        // 使用 MovePosition 或 Translate 都可以，Translate 比较直观
        transform.Translate(forwardMove + sideMove, Space.World);

        // B. 掉落逻辑逻辑：如果你跑出了地面边缘（假设路宽 8，左右各 4）
        if (transform.position.x < -4f || transform.position.x > 4f)
        {
            // 给球一个向下的速度
            transform.Translate(0, -20f * Time.fixedDeltaTime, 0);
        }
    }

    // 更新 HP 显示的方法
    void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHP;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
        }
    }

    void UpdateBestScoreUI()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = "Record: "+ Mathf.FloorToInt(bestScore).ToString();
        }
    }

    // 触发游戏结束的统一入口
    public void TriggerGameOver(bool isFall)
    {
        if (isGameOver) return; // 防止重复触发
        isGameOver = true;

        if (isFall)
        {
            currentHP = 0; // 掉落则直接清零
            UpdateHPUI();
        }

        Debug.Log("Game Over!");

        // 游戏结束时保存最高分到本地硬盘
        PlayerPrefs.SetFloat("HighScore", bestScore);
        PlayerPrefs.Save();

        // 停止游戏时间（画面定格）
        Time.timeScale = 0;

        // 显示 Game Over 界面
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // 重启游戏的函数
    public void RestartGame()
    {
        // 确保重启前时间恢复正常，否则新场景也是静止的
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnEnable()
    {
        // 订阅障碍物被撞事件
        Barrier.OnAnyBarrierHit += HandleObstacleHit;
        Bonus.OnBonusCollected += HandleBonusCollected; // 监听加血包
    }

    private void OnDisable()
    {
        // 销毁时记得取消订阅，严谨！
        Barrier.OnAnyBarrierHit -= HandleObstacleHit;
        Bonus.OnBonusCollected -= HandleBonusCollected;
    }

    // 事件触发后执行的逻辑
    void HandleObstacleHit()
    {
        currentHP--;
        UpdateHPUI();

        if (currentHP <= 0)
        {
            TriggerGameOver(false);
        }
    }

    void HandleBonusCollected()
    {
        currentHP++; // 吃到心，血量加1
        UpdateHPUI();
    }

    void IncreaseDifficulty()
    {
        gameSpeed += 5f; // 速度增加 5
        Debug.Log("速度提升！当前速度: " + gameSpeed);

        // 显示字幕提示
        if (speedUpText != null)
        {
            StopAllCoroutines(); // 如果之前的提示还没消失，先停止它
            StartCoroutine(ShowSpeedUpMessage());
        }
    }

    // 协程：显示文字 1 秒后自动消失
    IEnumerator ShowSpeedUpMessage()
    {
        // 1. 设置文字内容并激活
        speedUpText.text = "SPEED UP! ";
        speedUpText.gameObject.SetActive(true);

        // 获取文字的初始颜色
        Color originalColor = speedUpText.color;
        originalColor.a = 1f; // 确保是不透明的
        speedUpText.color = originalColor;

        // 2. 保持完全显示一段时间（比如 1 秒）
        yield return new WaitForSeconds(1f);

        // 3. 平滑淡出（用 0.5 秒时间变透明）
        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            speedUpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null; // 等待下一帧
        }

        // 4. 彻底隐藏
        speedUpText.gameObject.SetActive(false);
    }
}
