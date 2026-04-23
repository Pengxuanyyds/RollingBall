# RollingBall

Unity 无尽跑酷游戏 — DASE7141 课程项目

Unity 版本：**2022.3.4f1c1** | 渲染管线：**Built-in**

---

## 游戏玩法

玩家控制一个在无限道路上自动向前滚动的球，通过**左右方向键**（或 A/D）躲避障碍物、拾取道具，尽可能存活更长时间。

### 操作方式

| 按键 | 功能 |
|------|------|
| 左 / 右方向键 | 左右移动 |
| A / D | 左右移动 |
| R | 重新开始 |

### 核心机制

- **自动前进**：球自动向前滚动，速度随距离逐渐加快
- **生命系统**：初始 3 HP（上限 5），撞到障碍物 -1 HP，归零则 Game Over
- **坠落判定**：掉落道路直接清零 HP
- **计分系统**：分数根据移动距离持续累加，最高分本地保存
- **渐进难度**：速度和障碍物密度逐步提升，达到上限后封顶

### 道具说明

| 道具 | 效果 | 出现概率 |
|------|------|----------|
| 红心（红色球体） | 回复 +1 HP（不超过上限） | ~8% |
| 护盾（金色盾牌） | 抵挡一次障碍物碰撞 | ~4% |

---

## 架构设计

### 设计模式

- **观察者/事件模式**：碰撞事件通过静态 C# 事件传递（`Barrier.OnAnyBarrierHit`、`Bonus.OnBonusCollected`、`PowerUp.OnShieldCollected`），Player 订阅事件，脚本之间完全解耦
- **对象池模式**：BarrierManager 使用 `Queue<GameObject>` 池化所有障碍物、红心和护盾，超出视野的对象回收复用，减少 GC 开销
- **程序化生成**：障碍物和道具在玩家前方按可配置的密度和概率动态生成
- **无限地面**：两个地面平面交替移动，实现无尽道路效果

### 事件流程

```
Barrier.OnTriggerEnter → Barrier.OnAnyBarrierHit → Player.HandleObstacleHit()
Bonus.OnTriggerEnter   → Bonus.OnBonusCollected   → Player.HandleBonusCollected()
PowerUp.OnTriggerEnter → PowerUp.OnShieldCollected → Player.HandleShieldCollected()
```

### 难度曲线

| 距离 | 速度 | 生成密度 | 障碍物/秒 |
|------|------|----------|-----------|
| 0    | 20   | 0.10     | ~0.9      |
| 1000 | 23   | 0.16     | ~1.6      |
| 2000 | 29   | 0.22     | ~2.8      |
| 3000+| 35-40| 0.22（封顶）| ~3.4-3.9 |

速度每 350 距离 +3，封顶 40。密度在 2000 距离内从 0.10 线性增长至 0.22。

---

## 项目结构

```
Assets/
├── _Scripts/                # C# 游戏脚本（7 个文件）
│   ├── Player.cs            # 核心控制器：HP、计分、难度递增、事件处理
│   ├── BarrierManager.cs    # 程序化生成与对象池管理
│   ├── Barrier.cs           # 障碍物碰撞 + 粒子破碎效果
│   ├── Bonus.cs             # 红心道具（回复 HP）
│   ├── PowerUp.cs           # 护盾道具（抵挡一次碰撞）
│   ├── CameraFollow.cs      # 摄像机跟随（仅位置，不跟随旋转）
│   └── InfiniteGround.cs    # 无限地面切换
├── Scenes/
│   └── SampleScene.unity    # 主游戏场景
├── Audio/
│   └── deadmau5 - 8bit.mp3  # 背景音乐
├── Meshes/
│   ├── HeartMesh.asset      # 程序化心形网格
│   └── ShieldMesh.asset     # 程序化盾牌网格
├── Barrier.prefab           # 橙色障碍物（缩放方块）
├── Bonus.prefab             # 红色球体（红心/回血）
├── Shield.prefab            # 金色盾牌形状（抵挡一次）
├── BGMusic.prefab           # 持久化背景音乐（DontDestroyOnLoad）
├── matBarrier.mat           # 橙色，Standard Shader
├── matHeart.mat             # 红色，不透明
├── matShield.mat            # 金色，不透明，Metallic 0.6
├── matGround.mat            # 灰色地面
└── Wispy Sky/               # 天空盒资源
```

---

## 场景层级

```
SampleScene
├── Main Camera          (CameraFollow, AudioListener)
├── Directional Light
├── Player               (Player, SphereCollider, TrailRenderer, MeshFilter/Sphere)
├── BarrierManager       (BarrierManager，管理生成与池化)
├── Ground1              (InfiniteGround)
├── Ground2              (InfiniteGround)
├── Finish               （遗留物体，无尽模式中未使用）
├── BGMusic              （运行时生成，DontDestroyOnLoad）
└── Canvas               (Screen Space Overlay, 1920x1080)
    ├── HP Text              "HP: 3"
    ├── Score Text           "Score: 0"
    ├── BestScore Text       "Record: 0"
    ├── SpeedUp Text         "SPEED UP!"（淡入淡出动画）
    ├── ItemLegend Panel      图例：红心=回血，护盾=挡一次
    └── GameOverPanel         "Game Over"（死亡后显示）
```

---

## 技术细节

### 可调参数（Inspector 面板）

| 组件 | 参数 | 默认值 | 说明 |
|------|------|--------|------|
| Player | gameSpeed | 20 | 初始前进速度 |
| Player | maxSpeed | 40 | 速度上限 |
| Player | turnSpeed | 6 | 左右移动速度 |
| Player | maxHP | 5 | 最大生命值 |
| Player | scoreMultiplier | 0.5 | 每速度-秒的分数系数 |
| BarrierManager | startDensity | 0.10 | 初始生成密度 |
| BarrierManager | maxDensity | 0.22 | 最大生成密度 |
| BarrierManager | rowInterval | 10 | 生成行间距 |
| BarrierManager | bonusChance | 0.08 | 红心生成概率 |
| BarrierManager | shieldChance | 0.04 | 护盾生成概率 |

### 视觉效果

- **滚动效果**：基于移动方向计算物理正确的旋转（叉积计算旋转轴）
- **拖尾特效**：球后方白色渐隐拖尾（TrailRenderer，0.3 秒持续）
- **障碍破碎**：碰撞时触发粒子系统
- **加速提示**：难度提升时显示 "SPEED UP!" 淡入淡出文字
- **道具旋转**：道具缓慢旋转（60 度/秒），更容易被发现

### 依赖包

- TextMesh Pro（内置）
- Wispy Sky（天空盒资源）

---

## 快速开始

1. 克隆本仓库
2. 使用 Unity Hub 打开（需要 Unity 2022.3.4f1c1 或兼容的 2022.3 LTS 版本）
3. 打开 `Assets/Scenes/SampleScene.unity`
4. 点击 Play 运行游戏
