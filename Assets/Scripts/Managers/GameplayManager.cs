using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;

/* Manages the main, non-menu gameplay. */
public class GameplayManager { // Gameplay Manager
    /* Singleton Instance */
    public static GameplayManager instance;

    /* Debug Tools */
    public static string className = typeof(GameplayManager).Name;
    public static bool fastLoad = false;
    public static bool heroDoNotAttack = false;

    /* Sub-managers */
    // !! Later these will need separating from GameplayManager for use in menus.
    public AbilityManager abilityManager = new AbilityManager();
    public EntityManager entityManager = new EntityManager();
    public AllyManager allyManager = new AllyManager();

    /* Assets & Data */
    private List<object> addressableAssets = new List<object>();
    public static GameObject healthBarPrefab;
    public WaveData wave;
    public List<AbilityData> equippedAbilities = new List<AbilityData>();
    public List<AllyData> equippedAllies = new List<AllyData>();
    private struct EnemySpawnData {
        public EnemyData enemy;
        public float spacing;
    }
    private List<EnemySpawnData> enemySpawnQueue = new List<EnemySpawnData>();

    /* Game Objects */
    public Stage stage;
    public Gate gate;
    private GameObject hud;
    private BGM bgm;
    public Hero hero;

    /* Time Trackers */
    public float waveStopwatch = 0; // Time since wave began.
    public List<float> allyCooldowns = new List<float>();
    public List<float> abilityCooldowns = new List<float>();
    private float enemySpawnTimer; // Time between groups of enemy spawns.
    private const float enemySpacingDuration = 0.5f; // Time between enemy spawns of the same group.
    private float enemySpacingTimer;
    private const float slowMoDuration = 0.4f; // Duration of the victory slow-mo.
    private float slowMoTimer;
    private float victoryDuration; // Equal to the victory music duration.
    private float victoryTimer;

    private int enemyCounter;
    private int allyCounter;

    /* Flags */
    public bool paused = false;
    public bool waveStarted = false;
    public bool defeated = false;
    public bool waveComplete = false;
    public bool startSlowMo = false;

    private Transform camera;

    public int smithy = 0;
    private float smithySave = 0;
    public const float smithyRate = 1.5f;

    public int totalEnemies;
    public int enemiesRemaining;
    private int waveEntryIndex = 0; // Which sub-wave of enemies is currently being called.

    public static List<string> entitiesWithTags = new List<string>();

    public event System.Action onWaveComplete;

    public Dictionary<string, GameplayEntity>.ValueCollection GetEntities() {
        return entityManager.entities.Values;
    }

    private async Task LoadWaveData() {
        var waveHandle = Addressables.LoadAssetAsync<WaveData>("Data/Waves/1");
        wave = await waveHandle.Task;
        addressableAssets.Add(wave);
        totalEnemies = 0;
        foreach (WaveData.Entry entry in wave.entries) {
            totalEnemies += entry.enemyQuanitity;
        }
        enemiesRemaining = totalEnemies;
    }
    private async Task LoadHealthBar() {
        var healthBarHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Entity Health Bar");
        healthBarPrefab = await healthBarHandle.Task;
        addressableAssets.Add(healthBarPrefab);
    }
    private async Task LoadAllyData() {
        string[] ids = {"Ashigaru", "Katana", "Kyudo", "Yari"};

        foreach (string id in ids) {
            var allyDataHandle = Addressables.LoadAssetAsync<AllyData>($"Data/Allies/Humans/{id}");
            AllyData allyData = await allyDataHandle.Task;
            addressableAssets.Add(allyData);
            if (allyData == null) {
                Debug.LogError($"Could not find or load Ally of ID \"{$"Humans/{id}"}\".");
                return;
            }
            equippedAllies.Add(allyData);
            allyCooldowns.Add(0); // !! Replace when Allies are loaded properly
            SaveManager.SetLevel(allyData, 1); // !! Remove once save system implemented
        }
    }
    private async Task PreLoadAudioClips() {
        await SFXManager.Load(className, "Wave Victory");
        await SFXManager.Load(className, "Wave Defeat");
        if (!fastLoad) {
            await SFXManager.Load(className, "Combat/Swoosh Small");
            await SFXManager.Load(className, "Combat/Swoosh Medium");
            await SFXManager.Load(className, "Combat/Arrow Fire");
            await SFXManager.Load(className, "Combat/Footstep");
            await SFXManager.Load(className, "Combat/Footstep Large");
        }
    }
    private async Task LoadStage() {
        stage = new Stage(wave.stage);
        await stage.Init();
    }
    private async Task LoadGate() {
        SaveManager.EquipCostume("AlliesGate", 0);
        gate = new Gate(GameplayEntity.Side.Left);
        await gate.Init();
        entityManager.AddEntity("Gate", gate);
    }
    private async Task LoadHUD() {
        var hudHandle = Addressables.InstantiateAsync("Prefabs/Gameplay HUD");
        hud = await hudHandle.Task;
    }
    private async Task LoadHero() {
        SaveManager.EquipCostume("Samurai", 0);
        SaveManager.EquipCostume("Kunoichi", 0);
        SaveManager.EquipCostume("Ronin", 0);
        SaveManager.EquipCostume("Ashigaru", 0);
        SaveManager.EquipCostume("Katana", 0);
        SaveManager.EquipCostume("Kyudo", 0);
        SaveManager.EquipCostume("Yari", 0);
        hero = new Hero(SaveManager.selectedHero);
        hero.SetBounds(stage.leftBound, stage.rightBound);
        hero.allegiance = GameplayEntity.Side.Left;
        await hero.Init(stage.heroSpawn);
        entityManager.AddEntity("Hero", hero);
    }
    private async Task LoadBGM() {
        bgm = new BGM("Zen Garden Day");
        await bgm.Init();
    }
    private void RegisterEvents() {
        hero.onDeath += PlayWaveDefeat;
        onWaveComplete += PlayWaveVictory;
    }

    // Does not start the wave.
    public static async Task Init(Transform _cameraTransform) {
        // Initialise new instance.
        instance = new GameplayManager();
        instance.camera = _cameraTransform;

        await instance.LoadWaveData();
        await instance.LoadHealthBar();
        await instance.LoadAllyData();
        await instance.PreLoadAudioClips();
        await instance.abilityManager.Init();
        await instance.LoadStage();
        await instance.LoadGate();
        await instance.LoadHUD();
        await instance.LoadHero();
        await instance.LoadBGM();
        instance.RegisterEvents();
    }

    public void PlayWaveDefeat() {
        defeated = true;
        bgm.Stop();
        victoryDuration = SFXManager.PlayFromBundle("Wave Defeat");
        victoryTimer = waveStopwatch;
    }
    public void PlayWaveVictory() {
        waveComplete = true;
        slowMoTimer = 0;
        victoryDuration = SFXManager.PlayFromBundle("Wave Victory");
        victoryTimer = waveStopwatch;
        Time.timeScale = 1;
    }

    public void StartWave() {
        bgm.Play();
        waveStarted = true;
    }

    public void SpawnEnemy(EnemyData _data) {
        Enemy enemy = new Enemy(_data, GameplayEntity.Side.Right);
        enemy.SetBounds(stage.leftBound, float.MaxValue);
        entityManager.AddEntity($"Enemy{enemyCounter++}", enemy);
        enemy.Spawn(stage.zombieSpawn);
    }
    public void SpawnAlly(AllyData _data) {
        Ally ally = new Ally(_data, GameplayEntity.Side.Left);
        ally.SetBounds(float.MinValue, stage.rightBound);
        entityManager.AddEntity($"Ally{allyCounter++}", ally);
        ally.Spawn(stage.allySpawn);
    }

    public void Update() {
        if (!waveStarted) return;
        abilityManager.Update();
        entityManager.Update();
        UpdateCooldowns();
        UpdateSmithy();
        HandleWaveEnd();
        waveStopwatch += Time.deltaTime;
    }

    private void UpdateCooldowns() {
        for (int i = 0; i < allyCooldowns.Count; i++)
            allyCooldowns[i] -= Time.deltaTime;
        for (int i = 0; i < abilityCooldowns.Count; i++)
            abilityCooldowns[i] -= Time.deltaTime;
    }

    private void UpdateSmithy() {
        if (waveStopwatch - smithySave > 1 / smithyRate) {
            smithySave = waveStopwatch;
            smithy += 1;
        }
    }

    private void HandleWaveEnd() {
        if (defeated && waveStopwatch - victoryTimer > victoryDuration) {
            Terminate();
            SceneLoadManager.LoadScene("TitleScreen");
        }

        // On wave completion:
        if (enemiesRemaining == 0 && !hero.isDead) {
            if (!startSlowMo) {
                slowMoTimer = waveStopwatch;
                bgm.Stop();
                camera.position = new Vector3(camera.position.x, 1.12f, -3.3f);
                Time.timeScale = 0.2f;
                startSlowMo = true;
            } else {
                if (slowMoTimer == 0) {
                    camera.localPosition += new Vector3(0, 0.01f, -0.1f) * Time.deltaTime;
                } else if (waveStopwatch - slowMoTimer > slowMoDuration) {
                    onWaveComplete?.Invoke();
                }
                if (waveComplete && waveStopwatch - victoryTimer > victoryDuration) {
                    Terminate();
                    SceneLoadManager.LoadScene("TitleScreen");
                }
            }
        } else {
            if (!(waveEntryIndex > wave.entries.Length - 1) && waveStopwatch - enemySpawnTimer > wave.entries[waveEntryIndex].delay) {
                enemySpawnTimer = waveStopwatch;
                for (int i = 0; i < wave.entries[waveEntryIndex].enemyQuanitity; i++) {
                    float spacing = wave.entries[waveEntryIndex].spacing;
                    EnemySpawnData spawnData = new EnemySpawnData {
                        enemy = wave.entries[waveEntryIndex].enemy,
                        spacing = (spacing > enemySpacingDuration) ? spacing : enemySpacingDuration
                    };
                    enemySpawnQueue.Add(spawnData);
                }
                waveEntryIndex++;
            }

            if (enemySpawnQueue.Count > 0 && waveStopwatch - enemySpacingTimer > enemySpawnQueue[0].spacing) {
                enemySpacingTimer = waveStopwatch;
                SpawnEnemy(enemySpawnQueue[0].enemy);
                enemySpawnQueue.RemoveAt(0);
            }
        }
    }

    public void DealDamage(string entityId) {
        GameplayEntity entity = entityManager.entities[entityId];
        foreach (GameplayEntity enemy in entityManager.entities.Values) {
            if (enemy == null || enemy.allegiance == entity.allegiance || enemy.isDead)
                continue;
            if (enemy.isFlying && entity.rangedWeapon == null)
                continue;

            if (entity.IsInMeleeRange(enemy.xPos + 0.2f * enemy.direction))
                entity.MeleeHit(enemy);
        }
    }

    public void FireProjectile(string entityId) {
        entityManager.entities[entityId].FireProjectile(entityManager.closestTargets[entityId]);
    }

    public static void Pause() {
        instance.paused = true;
        Time.timeScale = 0;
        instance.bgm.SetVolume(0.5f);
    }
    public static void Resume() {
        instance.paused = false;
        Time.timeScale = 1;
        instance.bgm.SetVolume(1);
    }

    public static void Terminate() {
        foreach (var handle in instance.addressableAssets)
            Addressables.Release(handle);
        SFXManager.Clear(className);
        Resume();
    }
};