using System.Collections.Generic;
using UnityEngine;

public class PlayerSpecialAbilities : MonoBehaviour
{
    public enum SpecialAbilityId
    {
        None,
        ExplosiveShots,
        LightningStrike,
        OrbitingBlade,
        FireTrail,
        BlackHole,
        DroneTurret,
        IceNova,
        PoisonCloud,
        RicochetRounds,
        LaserBeam,
        Shockwave,
        GuardianShield,
        MeteorStrike,
        ShrapnelMines,
        BloodPact,
        TimeFracture,
        CryoBlast,
        SolarVortex,
        SawBarrier,
        ClusterBombs
    }

    [Header("Explosive Shots")]
    [SerializeField] private bool explosiveShotsUnlocked = false;
    [SerializeField] private float explosiveShotRadius = 2.25f;
    [SerializeField] private float explosiveShotDamageMultiplier = 0.4f;
    [SerializeField] private bool explosionsCanCrit = false;
    [SerializeField] private LayerMask explosionHitMask = ~0;

    [Header("Explosion Visual")]
    [SerializeField] private GameObject explosionVisualPrefab;
    [SerializeField] private bool spawnExplosionVisual = true;
    [SerializeField] private float explosionVisualDuration = 0.22f;
    [SerializeField] private Color explosionVisualStartColor = new Color(1f, 0.65f, 0.1f, 0.75f);
    [SerializeField] private Color explosionVisualEndColor = new Color(1f, 0.1f, 0f, 0f);

    [Header("Lightning Strike")]
    [SerializeField] private bool lightningStrikeUnlocked = false;
    [SerializeField] private float lightningStrikeCooldown = 4f;
    [SerializeField] private float lightningStrikeRange = 12f;
    [SerializeField] private float lightningStrikeDamage = 35f;
    [SerializeField] private int lightningStrikesPerActivation = 1;
    [SerializeField] private LayerMask lightningHitMask = ~0;

    [Header("Lightning Visual")]
    [SerializeField] private GameObject lightningStrikeVisualPrefab;
    [SerializeField] private bool spawnLightningVisual = true;
    [SerializeField] private float lightningVisualBoltLength = 3.5f;
    [SerializeField] private float lightningVisualDuration = 0.16f;
    [SerializeField] private float lightningVisualWidth = 0.14f;
    [SerializeField] private int lightningVisualSegments = 6;
    [SerializeField] private float lightningVisualJaggedness = 0.35f;
    [SerializeField] private Color lightningVisualStartColor = new Color(0.4f, 0.9f, 1f, 1f);
    [SerializeField] private Color lightningVisualEndColor = new Color(1f, 1f, 1f, 0f);

    [Header("Orbiting Blade")]
    [SerializeField] private bool orbitingBladeUnlocked = false;
    [SerializeField] private OrbitingBlade orbitingBladePrefab;
    [SerializeField] private Transform orbitingBladeHolder;
    [SerializeField] private int orbitingBladeCount = 0;
    [SerializeField] private int maxOrbitingBlades = 6;
    [SerializeField] private float orbitingBladeDamage = 18f;
    [SerializeField] private float orbitingBladeOrbitRadius = 2.25f;
    [SerializeField] private float orbitingBladeSpeed = 180f;
    [SerializeField] private float orbitingBladeHitRadius = 0.55f;
    [SerializeField] private float orbitingBladeHitCooldown = 0.35f;
    [SerializeField] private LayerMask orbitingBladeHitMask = ~0;

    [Header("Fire Trail")]
    [SerializeField] private bool fireTrailUnlocked = false;
    [SerializeField] private GameObject fireTrailZonePrefab;
    [SerializeField] private Transform fireTrailHolder;
    [SerializeField] private float fireTrailSpawnInterval = 0.28f;
    [SerializeField] private float fireTrailMinMoveDistance = 0.45f;
    [SerializeField] private float fireTrailRadius = 1.15f;
    [SerializeField] private float fireTrailDamagePerTick = 8f;
    [SerializeField] private float fireTrailTickInterval = 0.35f;
    [SerializeField] private float fireTrailLifetime = 3f;
    [SerializeField] private LayerMask fireTrailHitMask = ~0;

    [Header("Fire Trail Visual")]
    [SerializeField] private Color fireTrailStartColor = new Color(1f, 0.38f, 0.05f, 0.85f);
    [SerializeField] private Color fireTrailEndColor = new Color(1f, 0.05f, 0f, 0f);
    [SerializeField] private float fireTrailLineWidth = 0.18f;
    [SerializeField] private int fireTrailCircleSegments = 48;

    [Header("Black Hole")]
    [SerializeField] private bool blackHoleUnlocked = false;
    [SerializeField] private GameObject blackHoleZonePrefab;
    [SerializeField] private float blackHoleCooldown = 7f;
    [SerializeField] private float blackHoleTargetRange = 18f;
    [SerializeField] private float blackHoleClusterSearchRadius = 4f;
    [SerializeField] private float blackHoleRadius = 3f;
    [SerializeField] private float blackHoleLifetime = 3.25f;
    [SerializeField] private float blackHoleDamagePerTick = 5f;
    [SerializeField] private float blackHoleTickInterval = 0.35f;
    [SerializeField] private float blackHolePullStrength = 36f;
    [SerializeField] private LayerMask blackHoleHitMask = ~0;

    [Header("Black Hole Visual")]
    [SerializeField] private Color blackHoleStartColor = new Color(0.45f, 0.15f, 1f, 0.9f);
    [SerializeField] private Color blackHoleEndColor = new Color(0.03f, 0f, 0.08f, 0f);
    [SerializeField] private float blackHoleLineWidth = 0.22f;

    [Header("Drone Turret")]
    [SerializeField] private bool droneTurretUnlocked = false;
    [SerializeField] private DroneTurret droneTurretPrefab;
    [SerializeField] private Transform droneTurretHolder;
    [SerializeField] private int droneTurretCount = 0;
    [SerializeField] private int maxDroneTurrets = 4;
    [SerializeField] private float droneTurretDamage = 12f;
    [SerializeField] private float droneTurretFireCooldown = 0.55f;
    [SerializeField] private float droneTurretRange = 10f;
    [SerializeField] private float droneTurretOrbitRadius = 1.75f;
    [SerializeField] private float droneTurretOrbitSpeed = 120f;
    [SerializeField] private float droneTurretFollowSharpness = 18f;
    [SerializeField] private LayerMask droneTurretHitMask = ~0;

    [Header("Drone Turret Visual")]
    [SerializeField] private Color droneTurretBodyColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    [SerializeField] private Color droneTurretBeamColor = new Color(1f, 0.85f, 0.2f, 0.85f);
    [SerializeField] private float droneTurretBodyRadius = 0.28f;
    [SerializeField] private float droneTurretBodyLineWidth = 0.08f;
    [SerializeField] private float droneTurretBeamWidth = 0.08f;

    [Header("Ice Nova")]
    [SerializeField] private bool iceNovaUnlocked = false;
    [SerializeField] private GameObject iceNovaZonePrefab;
    [SerializeField] private float iceNovaCooldown = 5.5f;
    [SerializeField] private float iceNovaRadius = 4f;
    [SerializeField] private float iceNovaDamage = 18f;
    [SerializeField] private float iceNovaLifetime = 0.65f;
    [SerializeField] private float iceNovaVelocityDamping = 0.25f;
    [SerializeField] private float iceNovaKnockbackForce = 2.5f;
    [SerializeField] private LayerMask iceNovaHitMask = ~0;

    [Header("Ice Nova Visual")]
    [SerializeField] private Color iceNovaStartColor = new Color(0.25f, 0.9f, 1f, 0.95f);
    [SerializeField] private Color iceNovaEndColor = new Color(0.75f, 1f, 1f, 0f);
    [SerializeField] private float iceNovaLineWidth = 0.22f;

    [Header("Poison Cloud")]
    [SerializeField] private bool poisonCloudUnlocked = false;
    [SerializeField] private GameObject poisonCloudZonePrefab;
    [SerializeField] private float poisonCloudCooldown = 4.8f;
    [SerializeField] private float poisonCloudTargetRange = 16f;
    [SerializeField] private float poisonCloudClusterSearchRadius = 3.5f;
    [SerializeField] private float poisonCloudRadius = 2.35f;
    [SerializeField] private float poisonCloudLifetime = 4.25f;
    [SerializeField] private float poisonCloudDamagePerTick = 7f;
    [SerializeField] private float poisonCloudTickInterval = 0.45f;
    [SerializeField] private LayerMask poisonCloudHitMask = ~0;

    [Header("Poison Cloud Visual")]
    [SerializeField] private Color poisonCloudStartColor = new Color(0.25f, 1f, 0.1f, 0.75f);
    [SerializeField] private Color poisonCloudEndColor = new Color(0.05f, 0.4f, 0f, 0f);
    [SerializeField] private float poisonCloudLineWidth = 0.2f;

    [Header("Ricochet Rounds")]
    [SerializeField] private bool ricochetRoundsUnlocked = false;
    [SerializeField] private int ricochetBounceCount = 2;
    [SerializeField] private int maxRicochetBounceCount = 6;
    [SerializeField] private float ricochetRange = 6f;
    [SerializeField] private float ricochetDamageMultiplier = 0.85f;
    [SerializeField] private LayerMask ricochetHitMask = ~0;

    [Header("Laser Beam")]
    [SerializeField] private bool laserBeamUnlocked = false;
    [SerializeField] private RotatingLaserBeam laserBeamPrefab;
    [SerializeField] private Transform laserBeamHolder;
    [SerializeField] private int laserBeamCount = 0;
    [SerializeField] private int maxLaserBeamCount = 4;
    [SerializeField] private float laserBeamDamage = 9f;
    [SerializeField] private float laserBeamLength = 6.5f;
    [SerializeField] private float laserBeamRotationSpeed = 105f;
    [SerializeField] private float laserBeamHitWidth = 0.34f;
    [SerializeField] private float laserBeamHitCooldown = 0.22f;
    [SerializeField] private LayerMask laserBeamHitMask = ~0;

    [Header("Laser Beam Visual")]
    [SerializeField] private Color laserBeamColor = new Color(1f, 0.1f, 0.08f, 0.9f);
    [SerializeField] private Color laserBeamCoreColor = new Color(1f, 0.75f, 0.3f, 0.95f);
    [SerializeField] private float laserBeamLineWidth = 0.18f;
    [SerializeField] private float laserBeamCoreRadius = 0.25f;


    [Header("Shockwave")]
    [SerializeField] private bool shockwaveUnlocked = false;
    [SerializeField] private GameObject shockwaveZonePrefab;
    [SerializeField] private float shockwaveCooldown = 5.25f;
    [SerializeField] private float shockwaveRadius = 4.25f;
    [SerializeField] private float shockwaveDamage = 20f;
    [SerializeField] private float shockwaveLifetime = 0.5f;
    [SerializeField] private float shockwaveKnockbackForce = 4.25f;
    [SerializeField] private float shockwaveVelocityDamping = 0.7f;
    [SerializeField] private LayerMask shockwaveHitMask = ~0;

    [Header("Shockwave Visual")]
    [SerializeField] private Color shockwaveStartColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color shockwaveEndColor = new Color(0.55f, 0.85f, 1f, 0f);
    [SerializeField] private float shockwaveLineWidth = 0.28f;

    [Header("Guardian Shield")]
    [SerializeField] private bool guardianShieldUnlocked = false;
    [SerializeField] private GuardianShield guardianShieldPrefab;
    [SerializeField] private Transform guardianShieldHolder;
    [SerializeField] private int guardianShieldCount = 0;
    [SerializeField] private int maxGuardianShields = 4;
    [SerializeField] private float guardianShieldDamage = 10f;
    [SerializeField] private float guardianShieldOrbitRadius = 1.45f;
    [SerializeField] private float guardianShieldOrbitSpeed = 160f;
    [SerializeField] private float guardianShieldHitRadius = 0.42f;
    [SerializeField] private float guardianShieldEnemyHitCooldown = 0.35f;
    [SerializeField] private LayerMask guardianShieldEnemyHitMask = ~0;
    [SerializeField] private LayerMask guardianShieldProjectileHitMask = ~0;
    [SerializeField] private string guardianShieldProjectileTag = "EnemyProjectile";

    [Header("Guardian Shield Visual")]
    [SerializeField] private Color guardianShieldColor = new Color(0.35f, 0.95f, 1f, 0.9f);
    [SerializeField] private Color guardianShieldBlockFlashColor = Color.white;
    [SerializeField] private float guardianShieldVisualRadius = 0.32f;
    [SerializeField] private float guardianShieldLineWidth = 0.1f;
    [SerializeField] private int guardianShieldCircleSegments = 32;

    [Header("Meteor Strike")]
    [SerializeField] private bool meteorStrikeUnlocked = false;
    [SerializeField] private GameObject meteorStrikeBlastPrefab;
    [SerializeField] private float meteorStrikeCooldown = 5.8f;
    [SerializeField] private float meteorStrikeTargetRange = 18f;
    [SerializeField] private float meteorStrikeClusterSearchRadius = 4f;
    [SerializeField] private float meteorStrikeRadius = 2.15f;
    [SerializeField] private float meteorStrikeDamage = 42f;
    [SerializeField] private float meteorStrikeWarningDuration = 0.85f;
    [SerializeField] private float meteorStrikeVisualDuration = 0.28f;
    [SerializeField] private float meteorStrikeKnockback = 2.4f;
    [SerializeField] private LayerMask meteorStrikeHitMask = ~0;

    [Header("Meteor Strike Visual")]
    [SerializeField] private Color meteorWarningColor = new Color(1f, 0.3f, 0.05f, 0.85f);
    [SerializeField] private Color meteorBlastColor = new Color(1f, 0.78f, 0.18f, 1f);
    [SerializeField] private float meteorStrikeLineWidth = 0.2f;

    [Header("Shrapnel Mines")]
    [SerializeField] private bool shrapnelMinesUnlocked = false;
    [SerializeField] private ShrapnelMine shrapnelMinePrefab;
    [SerializeField] private Transform shrapnelMineHolder;
    [SerializeField] private float shrapnelMineCooldown = 2.1f;
    [SerializeField] private float shrapnelMineMinMoveDistance = 0.75f;
    [SerializeField] private float shrapnelMineTriggerRadius = 1.05f;
    [SerializeField] private float shrapnelMineBlastRadius = 2f;
    [SerializeField] private float shrapnelMineDamage = 24f;
    [SerializeField] private float shrapnelMineArmTime = 0.45f;
    [SerializeField] private float shrapnelMineLifetime = 5f;
    [SerializeField] private float shrapnelMineKnockback = 2.5f;
    [SerializeField] private LayerMask shrapnelMineHitMask = ~0;

    [Header("Shrapnel Mine Visual")]
    [SerializeField] private Color shrapnelMineIdleColor = new Color(1f, 0.8f, 0.15f, 0.9f);
    [SerializeField] private Color shrapnelMineArmedColor = new Color(1f, 0.2f, 0.05f, 0.95f);
    [SerializeField] private Color shrapnelMineBlastColor = new Color(1f, 0.65f, 0.1f, 1f);
    [SerializeField] private float shrapnelMineLineWidth = 0.11f;
    [SerializeField] private int shrapnelMineCircleSegments = 32;

    [Header("Blood Pact")]
    [SerializeField] private bool bloodPactUnlocked = false;
    [SerializeField] private float bloodPactHealChance = 0.35f;
    [SerializeField] private float bloodPactHealPerKill = 2f;
    [SerializeField] private float bloodPactEnemyScanInterval = 0.35f;

    [Header("Time Fracture")]
    [SerializeField] private bool timeFractureUnlocked = false;
    [SerializeField] private GameObject timeFractureZonePrefab;
    [SerializeField] private float timeFractureCooldown = 6.2f;
    [SerializeField] private float timeFractureRadius = 4.5f;
    [SerializeField] private float timeFractureLifetime = 2.4f;
    [SerializeField] private float timeFractureDamagePerTick = 4f;
    [SerializeField] private float timeFractureTickInterval = 0.3f;
    [SerializeField] private float timeFractureVelocityDamping = 0.08f;
    [SerializeField] private LayerMask timeFractureHitMask = ~0;

    [Header("Time Fracture Visual")]
    [SerializeField] private Color timeFractureStartColor = new Color(0.75f, 0.35f, 1f, 0.9f);
    [SerializeField] private Color timeFractureEndColor = new Color(0.1f, 0.02f, 0.25f, 0f);
    [SerializeField] private float timeFractureLineWidth = 0.24f;

    [Header("Ability Slot Limits")]
    [SerializeField] private int maxNormalAbilitySlots = 10;
    [SerializeField] private int maxEvolvedAbilitySlots = 5;
    [SerializeField] private int normalSlotsConsumedPerEvolution = 2;

    [Header("Cryo Blast Evolution")]
    [SerializeField] private bool cryoBlastUnlocked = false;
    [SerializeField] private GameObject cryoBlastZonePrefab;
    [SerializeField] private float cryoBlastCooldown = 4.25f;
    [SerializeField] private float cryoBlastRadius = 5.35f;
    [SerializeField] private float cryoBlastDamage = 48f;
    [SerializeField] private float cryoBlastLifetime = 0.85f;
    [SerializeField] private float cryoBlastVelocityDamping = 0.08f;
    [SerializeField] private float cryoBlastKnockbackForce = 8.5f;
    [SerializeField] private LayerMask cryoBlastHitMask = ~0;

    [Header("Cryo Blast Visual")]
    [SerializeField] private Color cryoBlastStartColor = new Color(0.55f, 1f, 1f, 1f);
    [SerializeField] private Color cryoBlastEndColor = new Color(0.95f, 1f, 1f, 0f);
    [SerializeField] private float cryoBlastLineWidth = 0.34f;

    [Header("Solar Vortex Evolution")]
    [SerializeField] private bool solarVortexUnlocked = false;
    [SerializeField] private GameObject solarVortexZonePrefab;
    [SerializeField] private float solarVortexCooldown = 5.25f;
    [SerializeField] private float solarVortexTargetRange = 20f;
    [SerializeField] private float solarVortexClusterSearchRadius = 4.75f;
    [SerializeField] private float solarVortexRadius = 4.15f;
    [SerializeField] private float solarVortexLifetime = 4.35f;
    [SerializeField] private float solarVortexDamagePerTick = 14f;
    [SerializeField] private float solarVortexTickInterval = 0.25f;
    [SerializeField] private float solarVortexPullStrength = 58f;
    [SerializeField] private LayerMask solarVortexHitMask = ~0;

    [Header("Solar Vortex Visual")]
    [SerializeField] private Color solarVortexStartColor = new Color(1f, 0.35f, 0.04f, 0.95f);
    [SerializeField] private Color solarVortexEndColor = new Color(0.25f, 0.02f, 0f, 0f);
    [SerializeField] private float solarVortexLineWidth = 0.32f;

    [Header("Saw Barrier Evolution")]
    [SerializeField] private bool sawBarrierUnlocked = false;
    [SerializeField] private GuardianShield sawBarrierShieldPrefab;
    [SerializeField] private Transform sawBarrierHolder;
    [SerializeField] private int sawBarrierCount = 4;
    [SerializeField] private int maxSawBarrierCount = 6;
    [SerializeField] private float sawBarrierDamage = 24f;
    [SerializeField] private float sawBarrierOrbitRadius = 1.95f;
    [SerializeField] private float sawBarrierOrbitSpeed = 230f;
    [SerializeField] private float sawBarrierHitRadius = 0.58f;
    [SerializeField] private float sawBarrierEnemyHitCooldown = 0.18f;
    [SerializeField] private LayerMask sawBarrierEnemyHitMask = ~0;
    [SerializeField] private LayerMask sawBarrierProjectileHitMask = ~0;
    [SerializeField] private string sawBarrierProjectileTag = "EnemyProjectile";

    [Header("Saw Barrier Visual")]
    [SerializeField] private Color sawBarrierColor = new Color(0.9f, 0.95f, 1f, 0.95f);
    [SerializeField] private Color sawBarrierBlockFlashColor = Color.white;
    [SerializeField] private float sawBarrierVisualRadius = 0.36f;
    [SerializeField] private float sawBarrierLineWidth = 0.12f;
    [SerializeField] private int sawBarrierCircleSegments = 36;

    [Header("Cluster Bombs Evolution")]
    [SerializeField] private bool clusterBombsUnlocked = false;
    [SerializeField] private GameObject clusterBombSecondaryBlastPrefab;
    [SerializeField] private float clusterBombPrimaryRadius = 2.65f;
    [SerializeField] private float clusterBombPrimaryDamageMultiplier = 0.7f;
    [SerializeField] private int clusterBombSecondaryBlastCount = 3;
    [SerializeField] private float clusterBombSecondarySpreadRadius = 1.85f;
    [SerializeField] private float clusterBombSecondaryRadius = 1.35f;
    [SerializeField] private float clusterBombSecondaryDamageMultiplier = 0.35f;
    [SerializeField] private float clusterBombSecondaryWarningDuration = 0.18f;
    [SerializeField] private float clusterBombSecondaryVisualDuration = 0.2f;
    [SerializeField] private float clusterBombKnockback = 1.6f;
    [SerializeField] private Color clusterBombWarningColor = new Color(1f, 0.42f, 0.04f, 0.75f);
    [SerializeField] private Color clusterBombBlastColor = new Color(1f, 0.78f, 0.18f, 1f);
    [SerializeField] private float clusterBombLineWidth = 0.16f;

    [Header("Debug")]
    [SerializeField] private bool drawExplosionDebugCircle = false;
    [SerializeField] private float debugCircleDuration = 0.2f;

    private float lightningTimer;
    private float fireTrailTimer;
    private float blackHoleTimer;
    private float iceNovaTimer;
    private float poisonCloudTimer;
    private float shockwaveTimer;
    private float cryoBlastTimer;
    private float solarVortexTimer;
    private float meteorStrikeTimer;
    private float shrapnelMineTimer;
    private float bloodPactScanTimer;
    private float timeFractureTimer;

    private Vector3 lastFireTrailSpawnPosition;
    private bool hasSpawnedFirstFireTrailZone;
    private Vector3 lastShrapnelMinePosition;
    private bool hasDroppedFirstShrapnelMine;

    private Health playerHealth;

    private readonly List<Health> lightningTargets = new List<Health>();
    private readonly HashSet<Health> lightningTargetSet = new HashSet<Health>();
    private readonly List<OrbitingBlade> activeOrbitingBlades = new List<OrbitingBlade>();
    private readonly List<DroneTurret> activeDroneTurrets = new List<DroneTurret>();
    private readonly List<RotatingLaserBeam> activeLaserBeams = new List<RotatingLaserBeam>();
    private readonly List<GuardianShield> activeGuardianShields = new List<GuardianShield>();
    private readonly List<GuardianShield> activeSawBarrierShields = new List<GuardianShield>();
    private readonly List<Transform> enemySearchTargets = new List<Transform>();
    private readonly Dictionary<Health, System.Action> bloodPactSubscriptions = new Dictionary<Health, System.Action>();
    private readonly List<Health> bloodPactCleanup = new List<Health>();

    public bool ExplosiveShotsUnlocked => explosiveShotsUnlocked;
    public float ExplosiveShotRadius => explosiveShotRadius;
    public float ExplosiveShotDamageMultiplier => explosiveShotDamageMultiplier;

    public bool LightningStrikeUnlocked => lightningStrikeUnlocked;
    public float LightningStrikeCooldown => lightningStrikeCooldown;
    public float LightningStrikeRange => lightningStrikeRange;
    public float LightningStrikeDamage => lightningStrikeDamage;
    public int LightningStrikesPerActivation => lightningStrikesPerActivation;

    public bool OrbitingBladeUnlocked => orbitingBladeUnlocked;
    public int OrbitingBladeCount => orbitingBladeCount;
    public int MaxOrbitingBlades => maxOrbitingBlades;
    public float OrbitingBladeDamage => orbitingBladeDamage;
    public float OrbitingBladeOrbitRadius => orbitingBladeOrbitRadius;
    public float OrbitingBladeSpeed => orbitingBladeSpeed;
    public float OrbitingBladeHitRadius => orbitingBladeHitRadius;
    public float OrbitingBladeHitCooldown => orbitingBladeHitCooldown;

    public bool FireTrailUnlocked => fireTrailUnlocked;
    public float FireTrailSpawnInterval => fireTrailSpawnInterval;
    public float FireTrailRadius => fireTrailRadius;
    public float FireTrailDamagePerTick => fireTrailDamagePerTick;
    public float FireTrailTickInterval => fireTrailTickInterval;
    public float FireTrailLifetime => fireTrailLifetime;

    public bool BlackHoleUnlocked => blackHoleUnlocked;
    public float BlackHoleCooldown => blackHoleCooldown;
    public float BlackHoleRadius => blackHoleRadius;
    public float BlackHoleDamagePerTick => blackHoleDamagePerTick;
    public float BlackHoleLifetime => blackHoleLifetime;

    public bool DroneTurretUnlocked => droneTurretUnlocked;
    public int DroneTurretCount => droneTurretCount;
    public int MaxDroneTurrets => maxDroneTurrets;
    public float DroneTurretDamage => droneTurretDamage;
    public float DroneTurretFireCooldown => droneTurretFireCooldown;

    public bool IceNovaUnlocked => iceNovaUnlocked;
    public float IceNovaCooldown => iceNovaCooldown;
    public float IceNovaRadius => iceNovaRadius;
    public float IceNovaDamage => iceNovaDamage;

    public bool PoisonCloudUnlocked => poisonCloudUnlocked;
    public float PoisonCloudCooldown => poisonCloudCooldown;
    public float PoisonCloudRadius => poisonCloudRadius;
    public float PoisonCloudDamagePerTick => poisonCloudDamagePerTick;

    public bool RicochetRoundsUnlocked => ricochetRoundsUnlocked;
    public int RicochetBounceCount => ricochetBounceCount;
    public int MaxRicochetBounceCount => maxRicochetBounceCount;
    public float RicochetRange => ricochetRange;
    public float RicochetDamageMultiplier => ricochetDamageMultiplier;

    public bool LaserBeamUnlocked => laserBeamUnlocked;
    public int LaserBeamCount => laserBeamCount;
    public int MaxLaserBeamCount => maxLaserBeamCount;
    public float LaserBeamDamage => laserBeamDamage;
    public float LaserBeamLength => laserBeamLength;
    public float LaserBeamRotationSpeed => laserBeamRotationSpeed;

    public bool ShockwaveUnlocked => shockwaveUnlocked;
    public float ShockwaveCooldown => shockwaveCooldown;
    public float ShockwaveRadius => shockwaveRadius;
    public float ShockwaveDamage => shockwaveDamage;
    public float ShockwaveKnockbackForce => shockwaveKnockbackForce;

    public bool GuardianShieldUnlocked => guardianShieldUnlocked;
    public int GuardianShieldCount => guardianShieldCount;
    public int MaxGuardianShields => maxGuardianShields;
    public float GuardianShieldDamage => guardianShieldDamage;

    public bool MeteorStrikeUnlocked => meteorStrikeUnlocked;
    public float MeteorStrikeCooldown => meteorStrikeCooldown;
    public float MeteorStrikeRadius => meteorStrikeRadius;
    public float MeteorStrikeDamage => meteorStrikeDamage;

    public bool ShrapnelMinesUnlocked => shrapnelMinesUnlocked;
    public float ShrapnelMineCooldown => shrapnelMineCooldown;
    public float ShrapnelMineDamage => shrapnelMineDamage;
    public float ShrapnelMineBlastRadius => shrapnelMineBlastRadius;

    public bool BloodPactUnlocked => bloodPactUnlocked;
    public float BloodPactHealChance => bloodPactHealChance;
    public float BloodPactHealPerKill => bloodPactHealPerKill;

    public bool TimeFractureUnlocked => timeFractureUnlocked;
    public float TimeFractureCooldown => timeFractureCooldown;
    public float TimeFractureRadius => timeFractureRadius;
    public float TimeFractureDamagePerTick => timeFractureDamagePerTick;

    public bool CryoBlastUnlocked => cryoBlastUnlocked;
    public float CryoBlastCooldown => cryoBlastCooldown;
    public float CryoBlastRadius => cryoBlastRadius;
    public float CryoBlastDamage => cryoBlastDamage;
    public float CryoBlastKnockbackForce => cryoBlastKnockbackForce;

    public bool SolarVortexUnlocked => solarVortexUnlocked;
    public float SolarVortexCooldown => solarVortexCooldown;
    public float SolarVortexRadius => solarVortexRadius;
    public float SolarVortexDamagePerTick => solarVortexDamagePerTick;
    public float SolarVortexLifetime => solarVortexLifetime;

    public bool SawBarrierUnlocked => sawBarrierUnlocked;
    public int SawBarrierCount => sawBarrierCount;
    public int MaxSawBarrierCount => maxSawBarrierCount;
    public float SawBarrierDamage => sawBarrierDamage;

    public bool ClusterBombsUnlocked => clusterBombsUnlocked;
    public int ClusterBombSecondaryBlastCount => clusterBombSecondaryBlastCount;
    public float ClusterBombPrimaryDamageMultiplier => clusterBombPrimaryDamageMultiplier;

    public int NormalAbilityCount => CountOwnedNormalAbilities();
    public int EvolvedAbilityCount => CountOwnedEvolvedAbilities();
    public int MaxNormalAbilitySlots => maxNormalAbilitySlots;
    public int CurrentMaxNormalAbilitySlots => Mathf.Max(0, maxNormalAbilitySlots - (EvolvedAbilityCount * normalSlotsConsumedPerEvolution));
    public int MaxEvolvedAbilitySlots => maxEvolvedAbilitySlots;
    public int NormalSlotsConsumedPerEvolution => normalSlotsConsumedPerEvolution;
    public bool HasOpenNormalAbilitySlot => NormalAbilityCount < CurrentMaxNormalAbilitySlots;
    public bool HasOpenEvolvedAbilitySlot => EvolvedAbilityCount < maxEvolvedAbilitySlots;

    private void OnEnable()
    {
        lightningTimer = lightningStrikeCooldown;
        fireTrailTimer = 0f;
        blackHoleTimer = 0.75f;
        iceNovaTimer = 1f;
        poisonCloudTimer = 1.25f;
        shockwaveTimer = 1.1f;
        cryoBlastTimer = 0.95f;
        solarVortexTimer = 1.15f;
        meteorStrikeTimer = 1.35f;
        shrapnelMineTimer = 0.5f;
        bloodPactScanTimer = 0f;
        timeFractureTimer = 1.4f;

        playerHealth = GetComponent<Health>();

        lastFireTrailSpawnPosition = transform.position;
        hasSpawnedFirstFireTrailZone = false;
        lastShrapnelMinePosition = transform.position;
        hasDroppedFirstShrapnelMine = false;

        if (orbitingBladeUnlocked && orbitingBladeCount > 0)
        {
            EnsureOrbitingBladesExist();
            RefreshOrbitingBladeFormation();
        }

        if (droneTurretUnlocked && droneTurretCount > 0)
        {
            EnsureDroneTurretsExist();
            RefreshDroneTurretFormation();
        }

        if (laserBeamUnlocked && laserBeamCount > 0)
        {
            EnsureLaserBeamsExist();
            RefreshLaserBeamFormation();
        }

        if (guardianShieldUnlocked && guardianShieldCount > 0)
        {
            EnsureGuardianShieldsExist();
            RefreshGuardianShieldFormation();
        }

        if (sawBarrierUnlocked && sawBarrierCount > 0)
        {
            EnsureSawBarrierShieldsExist();
            RefreshSawBarrierFormation();
        }
    }

    private void OnDisable()
    {
        ClearBloodPactSubscriptions();
    }

    private void Update()
    {
        HandleLightningStrikeTimer();
        HandleFireTrailTimer();
        HandleBlackHoleTimer();
        HandleIceNovaTimer();
        HandlePoisonCloudTimer();
        HandleShockwaveTimer();
        HandleCryoBlastTimer();
        HandleSolarVortexTimer();
        HandleMeteorStrikeTimer();
        HandleShrapnelMineTimer();
        HandleBloodPactScanner();
        HandleTimeFractureTimer();
    }

    private void HandleLightningStrikeTimer()
    {
        if (!lightningStrikeUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        lightningTimer -= Time.deltaTime;

        if (lightningTimer > 0f)
        {
            return;
        }

        PerformLightningStrike();
        lightningTimer = lightningStrikeCooldown;
    }

    private void HandleFireTrailTimer()
    {
        if (!fireTrailUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        fireTrailTimer -= Time.deltaTime;

        if (fireTrailTimer > 0f)
        {
            return;
        }

        float distanceFromLastZone = Vector3.Distance(transform.position, lastFireTrailSpawnPosition);

        if (!hasSpawnedFirstFireTrailZone || distanceFromLastZone >= fireTrailMinMoveDistance)
        {
            SpawnFireTrailZone(transform.position);
            lastFireTrailSpawnPosition = transform.position;
            hasSpawnedFirstFireTrailZone = true;
            fireTrailTimer = fireTrailSpawnInterval;
        }
    }

    private void HandleBlackHoleTimer()
    {
        if (!blackHoleUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        blackHoleTimer -= Time.deltaTime;

        if (blackHoleTimer > 0f)
        {
            return;
        }

        SpawnBlackHole();
        blackHoleTimer = blackHoleCooldown;
    }

    private void HandleIceNovaTimer()
    {
        if (!iceNovaUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        iceNovaTimer -= Time.deltaTime;

        if (iceNovaTimer > 0f)
        {
            return;
        }

        SpawnIceNova();
        iceNovaTimer = iceNovaCooldown;
    }

    private void HandlePoisonCloudTimer()
    {
        if (!poisonCloudUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        poisonCloudTimer -= Time.deltaTime;

        if (poisonCloudTimer > 0f)
        {
            return;
        }

        SpawnPoisonCloud();
        poisonCloudTimer = poisonCloudCooldown;
    }

    private void HandleShockwaveTimer()
    {
        if (!shockwaveUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        shockwaveTimer -= Time.deltaTime;

        if (shockwaveTimer > 0f)
        {
            return;
        }

        SpawnShockwave();
        shockwaveTimer = shockwaveCooldown;
    }

    private void HandleCryoBlastTimer()
    {
        if (!cryoBlastUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        cryoBlastTimer -= Time.deltaTime;

        if (cryoBlastTimer > 0f)
        {
            return;
        }

        SpawnCryoBlast();
        cryoBlastTimer = cryoBlastCooldown;
    }

    private void HandleSolarVortexTimer()
    {
        if (!solarVortexUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        solarVortexTimer -= Time.deltaTime;

        if (solarVortexTimer > 0f)
        {
            return;
        }

        SpawnSolarVortex();
        solarVortexTimer = solarVortexCooldown;
    }

    private void HandleMeteorStrikeTimer()
    {
        if (!meteorStrikeUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        meteorStrikeTimer -= Time.deltaTime;

        if (meteorStrikeTimer > 0f)
        {
            return;
        }

        SpawnMeteorStrike();
        meteorStrikeTimer = meteorStrikeCooldown;
    }

    private void HandleShrapnelMineTimer()
    {
        if (!shrapnelMinesUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        shrapnelMineTimer -= Time.deltaTime;

        if (shrapnelMineTimer > 0f)
        {
            return;
        }

        float distanceFromLastMine = Vector3.Distance(transform.position, lastShrapnelMinePosition);

        if (!hasDroppedFirstShrapnelMine || distanceFromLastMine >= shrapnelMineMinMoveDistance)
        {
            SpawnShrapnelMine(transform.position);
            lastShrapnelMinePosition = transform.position;
            hasDroppedFirstShrapnelMine = true;
            shrapnelMineTimer = shrapnelMineCooldown;
        }
    }

    private void HandleBloodPactScanner()
    {
        if (!bloodPactUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        bloodPactScanTimer -= Time.deltaTime;

        if (bloodPactScanTimer > 0f)
        {
            return;
        }

        ScanEnemiesForBloodPact();
        bloodPactScanTimer = bloodPactEnemyScanInterval;
    }

    private void HandleTimeFractureTimer()
    {
        if (!timeFractureUnlocked || Time.timeScale <= 0f)
        {
            return;
        }

        timeFractureTimer -= Time.deltaTime;

        if (timeFractureTimer > 0f)
        {
            return;
        }

        SpawnTimeFracture();
        timeFractureTimer = timeFractureCooldown;
    }

    public bool IsNormalAbilityUnlocked(SpecialAbilityId abilityId)
    {
        switch (abilityId)
        {
            case SpecialAbilityId.ExplosiveShots:
                return explosiveShotsUnlocked;
            case SpecialAbilityId.LightningStrike:
                return lightningStrikeUnlocked;
            case SpecialAbilityId.OrbitingBlade:
                return orbitingBladeUnlocked;
            case SpecialAbilityId.FireTrail:
                return fireTrailUnlocked;
            case SpecialAbilityId.BlackHole:
                return blackHoleUnlocked;
            case SpecialAbilityId.DroneTurret:
                return droneTurretUnlocked;
            case SpecialAbilityId.IceNova:
                return iceNovaUnlocked;
            case SpecialAbilityId.PoisonCloud:
                return poisonCloudUnlocked;
            case SpecialAbilityId.RicochetRounds:
                return ricochetRoundsUnlocked;
            case SpecialAbilityId.LaserBeam:
                return laserBeamUnlocked;
            case SpecialAbilityId.Shockwave:
                return shockwaveUnlocked;
            case SpecialAbilityId.GuardianShield:
                return guardianShieldUnlocked;
            case SpecialAbilityId.MeteorStrike:
                return meteorStrikeUnlocked;
            case SpecialAbilityId.ShrapnelMines:
                return shrapnelMinesUnlocked;
            case SpecialAbilityId.BloodPact:
                return bloodPactUnlocked;
            case SpecialAbilityId.TimeFracture:
                return timeFractureUnlocked;
        }

        return false;
    }

    public bool IsEvolvedAbilityUnlocked(SpecialAbilityId abilityId)
    {
        switch (abilityId)
        {
            case SpecialAbilityId.CryoBlast:
                return cryoBlastUnlocked;
            case SpecialAbilityId.SolarVortex:
                return solarVortexUnlocked;
            case SpecialAbilityId.SawBarrier:
                return sawBarrierUnlocked;
            case SpecialAbilityId.ClusterBombs:
                return clusterBombsUnlocked;
        }

        return false;
    }

    public bool IsNormalAbilityLockedByEvolution(SpecialAbilityId abilityId)
    {
        if (cryoBlastUnlocked && (abilityId == SpecialAbilityId.IceNova || abilityId == SpecialAbilityId.Shockwave))
        {
            return true;
        }

        if (solarVortexUnlocked && (abilityId == SpecialAbilityId.FireTrail || abilityId == SpecialAbilityId.BlackHole))
        {
            return true;
        }

        if (sawBarrierUnlocked && (abilityId == SpecialAbilityId.OrbitingBlade || abilityId == SpecialAbilityId.GuardianShield))
        {
            return true;
        }

        if (clusterBombsUnlocked && (abilityId == SpecialAbilityId.ExplosiveShots || abilityId == SpecialAbilityId.ShrapnelMines))
        {
            return true;
        }

        return false;
    }

    public bool CanUnlockNormalAbility(SpecialAbilityId abilityId)
    {
        if (abilityId == SpecialAbilityId.None ||
            abilityId == SpecialAbilityId.CryoBlast ||
            abilityId == SpecialAbilityId.SolarVortex ||
            abilityId == SpecialAbilityId.SawBarrier ||
            abilityId == SpecialAbilityId.ClusterBombs)
        {
            return false;
        }

        if (IsNormalAbilityUnlocked(abilityId))
        {
            return true;
        }

        if (IsNormalAbilityLockedByEvolution(abilityId))
        {
            return false;
        }

        return HasOpenNormalAbilitySlot;
    }

    public bool CanEvolveCryoBlast()
    {
        if (cryoBlastUnlocked)
        {
            return false;
        }

        if (!HasOpenEvolvedAbilitySlot)
        {
            return false;
        }

        return iceNovaUnlocked && shockwaveUnlocked;
    }

    public bool CanEvolveSolarVortex()
    {
        if (solarVortexUnlocked)
        {
            return false;
        }

        if (!HasOpenEvolvedAbilitySlot)
        {
            return false;
        }

        return fireTrailUnlocked && blackHoleUnlocked;
    }

    public bool CanEvolveSawBarrier()
    {
        if (sawBarrierUnlocked)
        {
            return false;
        }

        if (!HasOpenEvolvedAbilitySlot)
        {
            return false;
        }

        return orbitingBladeUnlocked && guardianShieldUnlocked;
    }

    public bool CanEvolveClusterBombs()
    {
        if (clusterBombsUnlocked)
        {
            return false;
        }

        if (!HasOpenEvolvedAbilitySlot)
        {
            return false;
        }

        return explosiveShotsUnlocked && shrapnelMinesUnlocked;
    }

    private int CountOwnedNormalAbilities()
    {
        int count = 0;

        if (explosiveShotsUnlocked) count++;
        if (lightningStrikeUnlocked) count++;
        if (orbitingBladeUnlocked) count++;
        if (fireTrailUnlocked) count++;
        if (blackHoleUnlocked) count++;
        if (droneTurretUnlocked) count++;
        if (iceNovaUnlocked) count++;
        if (poisonCloudUnlocked) count++;
        if (ricochetRoundsUnlocked) count++;
        if (laserBeamUnlocked) count++;
        if (shockwaveUnlocked) count++;
        if (guardianShieldUnlocked) count++;
        if (meteorStrikeUnlocked) count++;
        if (shrapnelMinesUnlocked) count++;
        if (bloodPactUnlocked) count++;
        if (timeFractureUnlocked) count++;

        return count;
    }

    private int CountOwnedEvolvedAbilities()
    {
        int count = 0;

        if (cryoBlastUnlocked) count++;
        if (solarVortexUnlocked) count++;
        if (sawBarrierUnlocked) count++;
        if (clusterBombsUnlocked) count++;

        return count;
    }

    private bool TryUnlockNormalAbility(SpecialAbilityId abilityId, string displayName)
    {
        if (IsNormalAbilityUnlocked(abilityId))
        {
            return true;
        }

        if (IsNormalAbilityLockedByEvolution(abilityId))
        {
            Debug.Log($"Cannot unlock {displayName}. It has already been consumed by an evolved ability.");
            return false;
        }

        if (!HasOpenNormalAbilitySlot)
        {
            Debug.Log($"Cannot unlock {displayName}. Normal ability slots are full ({NormalAbilityCount}/{CurrentMaxNormalAbilitySlots}).");
            return false;
        }

        SetNormalAbilityUnlocked(abilityId, true);
        Debug.Log($"Special Ability Unlocked: {displayName} ({NormalAbilityCount}/{CurrentMaxNormalAbilitySlots} normal slots)");
        return true;
    }

    private void SetNormalAbilityUnlocked(SpecialAbilityId abilityId, bool unlocked)
    {
        switch (abilityId)
        {
            case SpecialAbilityId.ExplosiveShots:
                explosiveShotsUnlocked = unlocked;
                break;
            case SpecialAbilityId.LightningStrike:
                lightningStrikeUnlocked = unlocked;
                break;
            case SpecialAbilityId.OrbitingBlade:
                orbitingBladeUnlocked = unlocked;
                break;
            case SpecialAbilityId.FireTrail:
                fireTrailUnlocked = unlocked;
                break;
            case SpecialAbilityId.BlackHole:
                blackHoleUnlocked = unlocked;
                break;
            case SpecialAbilityId.DroneTurret:
                droneTurretUnlocked = unlocked;
                break;
            case SpecialAbilityId.IceNova:
                iceNovaUnlocked = unlocked;
                break;
            case SpecialAbilityId.PoisonCloud:
                poisonCloudUnlocked = unlocked;
                break;
            case SpecialAbilityId.RicochetRounds:
                ricochetRoundsUnlocked = unlocked;
                break;
            case SpecialAbilityId.LaserBeam:
                laserBeamUnlocked = unlocked;
                break;
            case SpecialAbilityId.Shockwave:
                shockwaveUnlocked = unlocked;
                break;
            case SpecialAbilityId.GuardianShield:
                guardianShieldUnlocked = unlocked;
                break;
            case SpecialAbilityId.MeteorStrike:
                meteorStrikeUnlocked = unlocked;
                break;
            case SpecialAbilityId.ShrapnelMines:
                shrapnelMinesUnlocked = unlocked;
                break;
            case SpecialAbilityId.BloodPact:
                bloodPactUnlocked = unlocked;
                break;
            case SpecialAbilityId.TimeFracture:
                timeFractureUnlocked = unlocked;
                break;
        }
    }

    private void ConsumeNormalAbility(SpecialAbilityId abilityId)
    {
        if (!IsNormalAbilityUnlocked(abilityId))
        {
            return;
        }

        SetNormalAbilityUnlocked(abilityId, false);

        switch (abilityId)
        {
            case SpecialAbilityId.OrbitingBlade:
                DestroyOrbitingBlades();
                orbitingBladeCount = 0;
                break;

            case SpecialAbilityId.DroneTurret:
                DestroyDroneTurrets();
                droneTurretCount = 0;
                break;

            case SpecialAbilityId.LaserBeam:
                DestroyLaserBeams();
                laserBeamCount = 0;
                break;

            case SpecialAbilityId.GuardianShield:
                DestroyGuardianShields();
                guardianShieldCount = 0;
                break;

            case SpecialAbilityId.BloodPact:
                ClearBloodPactSubscriptions();
                break;
        }
    }

    private void DestroyOrbitingBlades()
    {
        for (int i = activeOrbitingBlades.Count - 1; i >= 0; i--)
        {
            if (activeOrbitingBlades[i] != null)
            {
                Destroy(activeOrbitingBlades[i].gameObject);
            }
        }

        activeOrbitingBlades.Clear();
    }

    private void DestroyDroneTurrets()
    {
        for (int i = activeDroneTurrets.Count - 1; i >= 0; i--)
        {
            if (activeDroneTurrets[i] != null)
            {
                Destroy(activeDroneTurrets[i].gameObject);
            }
        }

        activeDroneTurrets.Clear();
    }

    private void DestroyLaserBeams()
    {
        for (int i = activeLaserBeams.Count - 1; i >= 0; i--)
        {
            if (activeLaserBeams[i] != null)
            {
                Destroy(activeLaserBeams[i].gameObject);
            }
        }

        activeLaserBeams.Clear();
    }

    private void DestroyGuardianShields()
    {
        for (int i = activeGuardianShields.Count - 1; i >= 0; i--)
        {
            if (activeGuardianShields[i] != null)
            {
                Destroy(activeGuardianShields[i].gameObject);
            }
        }

        activeGuardianShields.Clear();
    }

    private void DestroySawBarrierShields()
    {
        for (int i = activeSawBarrierShields.Count - 1; i >= 0; i--)
        {
            if (activeSawBarrierShields[i] != null)
            {
                Destroy(activeSawBarrierShields[i].gameObject);
            }
        }

        activeSawBarrierShields.Clear();
    }

    public void UnlockExplosiveShots()
    {
        TryUnlockNormalAbility(SpecialAbilityId.ExplosiveShots, "Explosive Shots");
    }

    public void IncreaseExplosiveShotRadius(float amount)
    {
        explosiveShotRadius = Mathf.Max(0.25f, explosiveShotRadius + amount);
        Debug.Log($"Explosive Shot Radius increased to {explosiveShotRadius:0.00}");
    }

    public void IncreaseExplosiveShotDamageMultiplier(float amount)
    {
        explosiveShotDamageMultiplier = Mathf.Max(0.05f, explosiveShotDamageMultiplier + amount);
        Debug.Log($"Explosive Shot Damage Multiplier increased to {explosiveShotDamageMultiplier:0.00}");
    }

    public void UnlockLightningStrike()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.LightningStrike, "Lightning Strike"))
        {
            return;
        }

        lightningTimer = 0.25f;
    }

    public void IncreaseLightningStrikeDamage(float amount)
    {
        lightningStrikeDamage = Mathf.Max(1f, lightningStrikeDamage + amount);
        Debug.Log($"Lightning Strike Damage increased to {lightningStrikeDamage:0}");
    }

    public void IncreaseLightningStrikeRange(float amount)
    {
        lightningStrikeRange = Mathf.Max(1f, lightningStrikeRange + amount);
        Debug.Log($"Lightning Strike Range increased to {lightningStrikeRange:0.00}");
    }

    public void ReduceLightningStrikeCooldown(float amount)
    {
        lightningStrikeCooldown = Mathf.Max(1f, lightningStrikeCooldown - amount);
        Debug.Log($"Lightning Strike Cooldown reduced to {lightningStrikeCooldown:0.00}");
    }

    public void IncreaseLightningStrikesPerActivation(int amount)
    {
        lightningStrikesPerActivation = Mathf.Max(1, lightningStrikesPerActivation + amount);
        Debug.Log($"Lightning Strikes Per Activation increased to {lightningStrikesPerActivation}");
    }

    public void UnlockOrbitingBlade()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.OrbitingBlade, "Orbiting Blade"))
        {
            return;
        }

        if (orbitingBladeCount <= 0)
        {
            orbitingBladeCount = 1;
        }

        orbitingBladeCount = Mathf.Clamp(orbitingBladeCount, 1, maxOrbitingBlades);

        EnsureOrbitingBladesExist();
        RefreshOrbitingBladeFormation();

    }

    public void AddOrbitingBlade(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!orbitingBladeUnlocked && !TryUnlockNormalAbility(SpecialAbilityId.OrbitingBlade, "Orbiting Blade"))
        {
            return;
        }
        orbitingBladeCount = Mathf.Clamp(orbitingBladeCount + amount, 1, maxOrbitingBlades);

        EnsureOrbitingBladesExist();
        RefreshOrbitingBladeFormation();

        Debug.Log($"Orbiting Blade Count increased to {orbitingBladeCount}");
    }

    public void IncreaseOrbitingBladeDamage(float amount)
    {
        orbitingBladeDamage = Mathf.Max(1f, orbitingBladeDamage + amount);
        RefreshOrbitingBladeFormation();
        Debug.Log($"Orbiting Blade Damage increased to {orbitingBladeDamage:0}");
    }

    public void IncreaseOrbitingBladeOrbitRadius(float amount)
    {
        orbitingBladeOrbitRadius = Mathf.Max(0.5f, orbitingBladeOrbitRadius + amount);
        RefreshOrbitingBladeFormation();
        Debug.Log($"Orbiting Blade Orbit Radius increased to {orbitingBladeOrbitRadius:0.00}");
    }

    public void IncreaseOrbitingBladeSpeed(float amount)
    {
        orbitingBladeSpeed = Mathf.Max(10f, orbitingBladeSpeed + amount);
        RefreshOrbitingBladeFormation();
        Debug.Log($"Orbiting Blade Speed increased to {orbitingBladeSpeed:0}");
    }

    public void IncreaseOrbitingBladeHitRadius(float amount)
    {
        orbitingBladeHitRadius = Mathf.Max(0.1f, orbitingBladeHitRadius + amount);
        RefreshOrbitingBladeFormation();
        Debug.Log($"Orbiting Blade Hit Radius increased to {orbitingBladeHitRadius:0.00}");
    }

    public void ReduceOrbitingBladeHitCooldown(float amount)
    {
        orbitingBladeHitCooldown = Mathf.Max(0.08f, orbitingBladeHitCooldown - amount);
        RefreshOrbitingBladeFormation();
        Debug.Log($"Orbiting Blade Hit Cooldown reduced to {orbitingBladeHitCooldown:0.00}");
    }

    public void UnlockFireTrail()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.FireTrail, "Fire Trail"))
        {
            return;
        }

        fireTrailTimer = 0f;
        lastFireTrailSpawnPosition = transform.position;
        hasSpawnedFirstFireTrailZone = false;
    }

    public void IncreaseFireTrailDamage(float amount)
    {
        fireTrailDamagePerTick = Mathf.Max(1f, fireTrailDamagePerTick + amount);
        Debug.Log($"Fire Trail Damage increased to {fireTrailDamagePerTick:0}");
    }

    public void IncreaseFireTrailRadius(float amount)
    {
        fireTrailRadius = Mathf.Max(0.25f, fireTrailRadius + amount);
        Debug.Log($"Fire Trail Radius increased to {fireTrailRadius:0.00}");
    }

    public void ReduceFireTrailSpawnInterval(float amount)
    {
        fireTrailSpawnInterval = Mathf.Max(0.08f, fireTrailSpawnInterval - amount);
        Debug.Log($"Fire Trail Spawn Interval reduced to {fireTrailSpawnInterval:0.00}");
    }

    public void IncreaseFireTrailLifetime(float amount)
    {
        fireTrailLifetime = Mathf.Max(0.25f, fireTrailLifetime + amount);
        Debug.Log($"Fire Trail Lifetime increased to {fireTrailLifetime:0.00}");
    }

    public void ReduceFireTrailTickInterval(float amount)
    {
        fireTrailTickInterval = Mathf.Max(0.08f, fireTrailTickInterval - amount);
        Debug.Log($"Fire Trail Tick Interval reduced to {fireTrailTickInterval:0.00}");
    }

    public void UnlockBlackHole()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.BlackHole, "Black Hole"))
        {
            return;
        }

        blackHoleTimer = 0.35f;
    }

    public void IncreaseBlackHoleDamage(float amount)
    {
        blackHoleDamagePerTick = Mathf.Max(1f, blackHoleDamagePerTick + amount);
        Debug.Log($"Black Hole Damage increased to {blackHoleDamagePerTick:0}");
    }

    public void IncreaseBlackHoleRadius(float amount)
    {
        blackHoleRadius = Mathf.Max(0.5f, blackHoleRadius + amount);
        Debug.Log($"Black Hole Radius increased to {blackHoleRadius:0.00}");
    }

    public void IncreaseBlackHoleDuration(float amount)
    {
        blackHoleLifetime = Mathf.Max(0.25f, blackHoleLifetime + amount);
        Debug.Log($"Black Hole Duration increased to {blackHoleLifetime:0.00}");
    }

    public void IncreaseBlackHolePullStrength(float amount)
    {
        blackHolePullStrength = Mathf.Max(1f, blackHolePullStrength + amount);
        Debug.Log($"Black Hole Pull Strength increased to {blackHolePullStrength:0}");
    }

    public void ReduceBlackHoleCooldown(float amount)
    {
        blackHoleCooldown = Mathf.Max(1f, blackHoleCooldown - amount);
        Debug.Log($"Black Hole Cooldown reduced to {blackHoleCooldown:0.00}");
    }

    public void UnlockDroneTurret()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.DroneTurret, "Drone Turret"))
        {
            return;
        }

        if (droneTurretCount <= 0)
        {
            droneTurretCount = 1;
        }

        droneTurretCount = Mathf.Clamp(droneTurretCount, 1, maxDroneTurrets);

        EnsureDroneTurretsExist();
        RefreshDroneTurretFormation();

    }

    public void AddDroneTurret(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!droneTurretUnlocked && !TryUnlockNormalAbility(SpecialAbilityId.DroneTurret, "Drone Turret"))
        {
            return;
        }
        droneTurretCount = Mathf.Clamp(droneTurretCount + amount, 1, maxDroneTurrets);

        EnsureDroneTurretsExist();
        RefreshDroneTurretFormation();

        Debug.Log($"Drone Turret Count increased to {droneTurretCount}");
    }

    public void IncreaseDroneTurretDamage(float amount)
    {
        droneTurretDamage = Mathf.Max(1f, droneTurretDamage + amount);
        RefreshDroneTurretFormation();
        Debug.Log($"Drone Turret Damage increased to {droneTurretDamage:0}");
    }

    public void ReduceDroneTurretCooldown(float amount)
    {
        droneTurretFireCooldown = Mathf.Max(0.08f, droneTurretFireCooldown - amount);
        RefreshDroneTurretFormation();
        Debug.Log($"Drone Turret Cooldown reduced to {droneTurretFireCooldown:0.00}");
    }

    public void IncreaseDroneTurretRange(float amount)
    {
        droneTurretRange = Mathf.Max(1f, droneTurretRange + amount);
        RefreshDroneTurretFormation();
        Debug.Log($"Drone Turret Range increased to {droneTurretRange:0.00}");
    }

    public void UnlockIceNova()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.IceNova, "Ice Nova"))
        {
            return;
        }

        iceNovaTimer = 0.35f;
    }

    public void IncreaseIceNovaDamage(float amount)
    {
        iceNovaDamage = Mathf.Max(1f, iceNovaDamage + amount);
        Debug.Log($"Ice Nova Damage increased to {iceNovaDamage:0}");
    }

    public void IncreaseIceNovaRadius(float amount)
    {
        iceNovaRadius = Mathf.Max(0.5f, iceNovaRadius + amount);
        Debug.Log($"Ice Nova Radius increased to {iceNovaRadius:0.00}");
    }

    public void ReduceIceNovaCooldown(float amount)
    {
        iceNovaCooldown = Mathf.Max(1f, iceNovaCooldown - amount);
        Debug.Log($"Ice Nova Cooldown reduced to {iceNovaCooldown:0.00}");
    }

    public void IncreaseIceNovaKnockback(float amount)
    {
        iceNovaKnockbackForce = Mathf.Max(0f, iceNovaKnockbackForce + amount);
        Debug.Log($"Ice Nova Knockback increased to {iceNovaKnockbackForce:0.00}");
    }

    public void UnlockPoisonCloud()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.PoisonCloud, "Poison Cloud"))
        {
            return;
        }

        poisonCloudTimer = 0.35f;
    }

    public void IncreasePoisonCloudDamage(float amount)
    {
        poisonCloudDamagePerTick = Mathf.Max(1f, poisonCloudDamagePerTick + amount);
        Debug.Log($"Poison Cloud Damage increased to {poisonCloudDamagePerTick:0}");
    }

    public void IncreasePoisonCloudRadius(float amount)
    {
        poisonCloudRadius = Mathf.Max(0.5f, poisonCloudRadius + amount);
        Debug.Log($"Poison Cloud Radius increased to {poisonCloudRadius:0.00}");
    }

    public void IncreasePoisonCloudDuration(float amount)
    {
        poisonCloudLifetime = Mathf.Max(0.25f, poisonCloudLifetime + amount);
        Debug.Log($"Poison Cloud Duration increased to {poisonCloudLifetime:0.00}");
    }

    public void ReducePoisonCloudCooldown(float amount)
    {
        poisonCloudCooldown = Mathf.Max(1f, poisonCloudCooldown - amount);
        Debug.Log($"Poison Cloud Cooldown reduced to {poisonCloudCooldown:0.00}");
    }

    public void UnlockRicochetRounds()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.RicochetRounds, "Ricochet Rounds"))
        {
            return;
        }

        ricochetBounceCount = Mathf.Clamp(ricochetBounceCount, 1, maxRicochetBounceCount);
    }

    public void AddRicochetBounce(int amount)
    {
        if (!ricochetRoundsUnlocked && !TryUnlockNormalAbility(SpecialAbilityId.RicochetRounds, "Ricochet Rounds"))
        {
            return;
        }
        ricochetBounceCount = Mathf.Clamp(ricochetBounceCount + amount, 1, maxRicochetBounceCount);
        Debug.Log($"Ricochet Bounce Count increased to {ricochetBounceCount}");
    }

    public void IncreaseRicochetRange(float amount)
    {
        ricochetRange = Mathf.Max(1f, ricochetRange + amount);
        Debug.Log($"Ricochet Range increased to {ricochetRange:0.00}");
    }

    public void IncreaseRicochetDamageMultiplier(float amount)
    {
        ricochetDamageMultiplier = Mathf.Clamp(ricochetDamageMultiplier + amount, 0.1f, 1.25f);
        Debug.Log($"Ricochet Damage Multiplier increased to {ricochetDamageMultiplier:0.00}");
    }

    public void UnlockLaserBeam()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.LaserBeam, "Laser Beam"))
        {
            return;
        }

        if (laserBeamCount <= 0)
        {
            laserBeamCount = 1;
        }

        laserBeamCount = Mathf.Clamp(laserBeamCount, 1, maxLaserBeamCount);

        EnsureLaserBeamsExist();
        RefreshLaserBeamFormation();

    }

    public void AddLaserBeam(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!laserBeamUnlocked && !TryUnlockNormalAbility(SpecialAbilityId.LaserBeam, "Laser Beam"))
        {
            return;
        }
        laserBeamCount = Mathf.Clamp(laserBeamCount + amount, 1, maxLaserBeamCount);

        EnsureLaserBeamsExist();
        RefreshLaserBeamFormation();

        Debug.Log($"Laser Beam Count increased to {laserBeamCount}");
    }

    public void IncreaseLaserBeamDamage(float amount)
    {
        laserBeamDamage = Mathf.Max(1f, laserBeamDamage + amount);
        RefreshLaserBeamFormation();
        Debug.Log($"Laser Beam Damage increased to {laserBeamDamage:0}");
    }

    public void IncreaseLaserBeamLength(float amount)
    {
        laserBeamLength = Mathf.Max(1f, laserBeamLength + amount);
        RefreshLaserBeamFormation();
        Debug.Log($"Laser Beam Length increased to {laserBeamLength:0.00}");
    }

    public void IncreaseLaserBeamRotationSpeed(float amount)
    {
        laserBeamRotationSpeed = Mathf.Max(10f, laserBeamRotationSpeed + amount);
        RefreshLaserBeamFormation();
        Debug.Log($"Laser Beam Rotation Speed increased to {laserBeamRotationSpeed:0}");
    }

    public void IncreaseLaserBeamHitWidth(float amount)
    {
        laserBeamHitWidth = Mathf.Max(0.08f, laserBeamHitWidth + amount);
        RefreshLaserBeamFormation();
        Debug.Log($"Laser Beam Hit Width increased to {laserBeamHitWidth:0.00}");
    }

    public void ReduceLaserBeamHitCooldown(float amount)
    {
        laserBeamHitCooldown = Mathf.Max(0.06f, laserBeamHitCooldown - amount);
        RefreshLaserBeamFormation();
        Debug.Log($"Laser Beam Hit Cooldown reduced to {laserBeamHitCooldown:0.00}");
    }

    public void UnlockShockwave()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.Shockwave, "Shockwave"))
        {
            return;
        }

        shockwaveTimer = 0.35f;
    }

    public void IncreaseShockwaveDamage(float amount)
    {
        shockwaveDamage = Mathf.Max(1f, shockwaveDamage + amount);
        Debug.Log($"Shockwave Damage increased to {shockwaveDamage:0}");
    }

    public void IncreaseShockwaveRadius(float amount)
    {
        shockwaveRadius = Mathf.Max(0.5f, shockwaveRadius + amount);
        Debug.Log($"Shockwave Radius increased to {shockwaveRadius:0.00}");
    }

    public void ReduceShockwaveCooldown(float amount)
    {
        shockwaveCooldown = Mathf.Max(1f, shockwaveCooldown - amount);
        Debug.Log($"Shockwave Cooldown reduced to {shockwaveCooldown:0.00}");
    }

    public void IncreaseShockwaveKnockback(float amount)
    {
        shockwaveKnockbackForce = Mathf.Max(0f, shockwaveKnockbackForce + amount);
        Debug.Log($"Shockwave Knockback increased to {shockwaveKnockbackForce:0.00}");
    }

    public void UnlockGuardianShield()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.GuardianShield, "Guardian Shield"))
        {
            return;
        }

        if (guardianShieldCount <= 0)
        {
            guardianShieldCount = 1;
        }

        guardianShieldCount = Mathf.Clamp(guardianShieldCount, 1, maxGuardianShields);

        EnsureGuardianShieldsExist();
        RefreshGuardianShieldFormation();

    }

    public void AddGuardianShield(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!guardianShieldUnlocked && !TryUnlockNormalAbility(SpecialAbilityId.GuardianShield, "Guardian Shield"))
        {
            return;
        }
        guardianShieldCount = Mathf.Clamp(guardianShieldCount + amount, 1, maxGuardianShields);

        EnsureGuardianShieldsExist();
        RefreshGuardianShieldFormation();

        Debug.Log($"Guardian Shield Count increased to {guardianShieldCount}");
    }

    public void IncreaseGuardianShieldDamage(float amount)
    {
        guardianShieldDamage = Mathf.Max(1f, guardianShieldDamage + amount);
        RefreshGuardianShieldFormation();
        Debug.Log($"Guardian Shield Damage increased to {guardianShieldDamage:0}");
    }

    public void IncreaseGuardianShieldOrbitSpeed(float amount)
    {
        guardianShieldOrbitSpeed = Mathf.Max(10f, guardianShieldOrbitSpeed + amount);
        RefreshGuardianShieldFormation();
        Debug.Log($"Guardian Shield Orbit Speed increased to {guardianShieldOrbitSpeed:0}");
    }

    public void IncreaseGuardianShieldHitRadius(float amount)
    {
        guardianShieldHitRadius = Mathf.Max(0.1f, guardianShieldHitRadius + amount);
        RefreshGuardianShieldFormation();
        Debug.Log($"Guardian Shield Hit Radius increased to {guardianShieldHitRadius:0.00}");
    }

    public void UnlockMeteorStrike()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.MeteorStrike, "Meteor Strike"))
        {
            return;
        }

        meteorStrikeTimer = 0.4f;
    }

    public void IncreaseMeteorStrikeDamage(float amount)
    {
        meteorStrikeDamage = Mathf.Max(1f, meteorStrikeDamage + amount);
        Debug.Log($"Meteor Strike Damage increased to {meteorStrikeDamage:0}");
    }

    public void IncreaseMeteorStrikeRadius(float amount)
    {
        meteorStrikeRadius = Mathf.Max(0.5f, meteorStrikeRadius + amount);
        Debug.Log($"Meteor Strike Radius increased to {meteorStrikeRadius:0.00}");
    }

    public void ReduceMeteorStrikeCooldown(float amount)
    {
        meteorStrikeCooldown = Mathf.Max(1f, meteorStrikeCooldown - amount);
        Debug.Log($"Meteor Strike Cooldown reduced to {meteorStrikeCooldown:0.00}");
    }

    public void UnlockShrapnelMines()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.ShrapnelMines, "Shrapnel Mines"))
        {
            return;
        }

        shrapnelMineTimer = 0.35f;
        lastShrapnelMinePosition = transform.position;
        hasDroppedFirstShrapnelMine = false;
    }

    public void IncreaseShrapnelMineDamage(float amount)
    {
        shrapnelMineDamage = Mathf.Max(1f, shrapnelMineDamage + amount);
        Debug.Log($"Shrapnel Mine Damage increased to {shrapnelMineDamage:0}");
    }

    public void IncreaseShrapnelMineBlastRadius(float amount)
    {
        shrapnelMineBlastRadius = Mathf.Max(0.5f, shrapnelMineBlastRadius + amount);
        Debug.Log($"Shrapnel Mine Blast Radius increased to {shrapnelMineBlastRadius:0.00}");
    }

    public void ReduceShrapnelMineCooldown(float amount)
    {
        shrapnelMineCooldown = Mathf.Max(0.35f, shrapnelMineCooldown - amount);
        Debug.Log($"Shrapnel Mine Cooldown reduced to {shrapnelMineCooldown:0.00}");
    }

    public void UnlockBloodPact()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.BloodPact, "Blood Pact"))
        {
            return;
        }

        bloodPactScanTimer = 0f;

        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }
    }

    public void IncreaseBloodPactHealChance(float amount)
    {
        bloodPactHealChance = Mathf.Clamp01(bloodPactHealChance + amount);
        Debug.Log($"Blood Pact Heal Chance increased to {bloodPactHealChance:0.00}");
    }

    public void IncreaseBloodPactHealPerKill(float amount)
    {
        bloodPactHealPerKill = Mathf.Max(0.25f, bloodPactHealPerKill + amount);
        Debug.Log($"Blood Pact Heal Per Kill increased to {bloodPactHealPerKill:0.00}");
    }

    public void UnlockTimeFracture()
    {
        if (!TryUnlockNormalAbility(SpecialAbilityId.TimeFracture, "Time Fracture"))
        {
            return;
        }

        timeFractureTimer = 0.4f;
    }

    public void IncreaseTimeFractureDamage(float amount)
    {
        timeFractureDamagePerTick = Mathf.Max(0f, timeFractureDamagePerTick + amount);
        Debug.Log($"Time Fracture Damage increased to {timeFractureDamagePerTick:0}");
    }

    public void IncreaseTimeFractureRadius(float amount)
    {
        timeFractureRadius = Mathf.Max(0.5f, timeFractureRadius + amount);
        Debug.Log($"Time Fracture Radius increased to {timeFractureRadius:0.00}");
    }

    public void IncreaseTimeFractureDuration(float amount)
    {
        timeFractureLifetime = Mathf.Max(0.25f, timeFractureLifetime + amount);
        Debug.Log($"Time Fracture Duration increased to {timeFractureLifetime:0.00}");
    }

    public void ReduceTimeFractureCooldown(float amount)
    {
        timeFractureCooldown = Mathf.Max(1f, timeFractureCooldown - amount);
        Debug.Log($"Time Fracture Cooldown reduced to {timeFractureCooldown:0.00}");
    }

    public void UnlockCryoBlast()
    {
        if (!CanEvolveCryoBlast())
        {
            Debug.Log("Cannot evolve Cryo Blast yet. Requires Ice Nova + Shockwave and an open evolved slot.");
            return;
        }

        ConsumeNormalAbility(SpecialAbilityId.IceNova);
        ConsumeNormalAbility(SpecialAbilityId.Shockwave);

        cryoBlastUnlocked = true;
        cryoBlastTimer = 0.25f;

        Debug.Log($"Evolution Unlocked: Cryo Blast ({EvolvedAbilityCount}/{MaxEvolvedAbilitySlots} evolved, {NormalAbilityCount}/{CurrentMaxNormalAbilitySlots} normal)");
    }

    public void IncreaseCryoBlastDamage(float amount)
    {
        cryoBlastDamage = Mathf.Max(1f, cryoBlastDamage + amount);
        Debug.Log($"Cryo Blast Damage increased to {cryoBlastDamage:0}");
    }

    public void IncreaseCryoBlastRadius(float amount)
    {
        cryoBlastRadius = Mathf.Max(0.5f, cryoBlastRadius + amount);
        Debug.Log($"Cryo Blast Radius increased to {cryoBlastRadius:0.00}");
    }

    public void IncreaseCryoBlastKnockback(float amount)
    {
        cryoBlastKnockbackForce = Mathf.Max(0f, cryoBlastKnockbackForce + amount);
        Debug.Log($"Cryo Blast Knockback increased to {cryoBlastKnockbackForce:0.00}");
    }

    public void ReduceCryoBlastCooldown(float amount)
    {
        cryoBlastCooldown = Mathf.Max(1f, cryoBlastCooldown - amount);
        Debug.Log($"Cryo Blast Cooldown reduced to {cryoBlastCooldown:0.00}");
    }

    public void UnlockSolarVortex()
    {
        if (!CanEvolveSolarVortex())
        {
            Debug.Log("Cannot evolve Solar Vortex yet. Requires Fire Trail + Black Hole and an open evolved slot.");
            return;
        }

        ConsumeNormalAbility(SpecialAbilityId.FireTrail);
        ConsumeNormalAbility(SpecialAbilityId.BlackHole);

        solarVortexUnlocked = true;
        solarVortexTimer = 0.25f;

        Debug.Log($"Evolution Unlocked: Solar Vortex ({EvolvedAbilityCount}/{MaxEvolvedAbilitySlots} evolved, {NormalAbilityCount}/{CurrentMaxNormalAbilitySlots} normal)");
    }

    public void IncreaseSolarVortexDamage(float amount)
    {
        solarVortexDamagePerTick = Mathf.Max(1f, solarVortexDamagePerTick + amount);
        Debug.Log($"Solar Vortex Damage increased to {solarVortexDamagePerTick:0}");
    }

    public void IncreaseSolarVortexRadius(float amount)
    {
        solarVortexRadius = Mathf.Max(0.5f, solarVortexRadius + amount);
        Debug.Log($"Solar Vortex Radius increased to {solarVortexRadius:0.00}");
    }

    public void IncreaseSolarVortexPullStrength(float amount)
    {
        solarVortexPullStrength = Mathf.Max(0f, solarVortexPullStrength + amount);
        Debug.Log($"Solar Vortex Pull Strength increased to {solarVortexPullStrength:0.00}");
    }

    public void IncreaseSolarVortexDuration(float amount)
    {
        solarVortexLifetime = Mathf.Max(0.25f, solarVortexLifetime + amount);
        Debug.Log($"Solar Vortex Duration increased to {solarVortexLifetime:0.00}");
    }

    public void ReduceSolarVortexCooldown(float amount)
    {
        solarVortexCooldown = Mathf.Max(1f, solarVortexCooldown - amount);
        Debug.Log($"Solar Vortex Cooldown reduced to {solarVortexCooldown:0.00}");
    }

    public void UnlockSawBarrier()
    {
        if (!CanEvolveSawBarrier())
        {
            Debug.Log("Cannot evolve Saw Barrier yet. Requires Orbiting Blade + Guardian Shield and an open evolved slot.");
            return;
        }

        ConsumeNormalAbility(SpecialAbilityId.OrbitingBlade);
        ConsumeNormalAbility(SpecialAbilityId.GuardianShield);

        sawBarrierUnlocked = true;
        sawBarrierCount = Mathf.Clamp(sawBarrierCount, 1, maxSawBarrierCount);
        EnsureSawBarrierShieldsExist();
        RefreshSawBarrierFormation();

        Debug.Log($"Evolution Unlocked: Saw Barrier ({EvolvedAbilityCount}/{MaxEvolvedAbilitySlots} evolved, {NormalAbilityCount}/{CurrentMaxNormalAbilitySlots} normal)");
    }

    public void AddSawBarrierShield(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        sawBarrierCount = Mathf.Clamp(sawBarrierCount + amount, 1, maxSawBarrierCount);
        EnsureSawBarrierShieldsExist();
        RefreshSawBarrierFormation();
        Debug.Log($"Saw Barrier Count increased to {sawBarrierCount}");
    }

    public void IncreaseSawBarrierDamage(float amount)
    {
        sawBarrierDamage = Mathf.Max(1f, sawBarrierDamage + amount);
        RefreshSawBarrierFormation();
        Debug.Log($"Saw Barrier Damage increased to {sawBarrierDamage:0}");
    }

    public void IncreaseSawBarrierOrbitSpeed(float amount)
    {
        sawBarrierOrbitSpeed = Mathf.Max(10f, sawBarrierOrbitSpeed + amount);
        RefreshSawBarrierFormation();
        Debug.Log($"Saw Barrier Orbit Speed increased to {sawBarrierOrbitSpeed:0}");
    }

    public void IncreaseSawBarrierHitRadius(float amount)
    {
        sawBarrierHitRadius = Mathf.Max(0.1f, sawBarrierHitRadius + amount);
        RefreshSawBarrierFormation();
        Debug.Log($"Saw Barrier Hit Radius increased to {sawBarrierHitRadius:0.00}");
    }

    public void ReduceSawBarrierHitCooldown(float amount)
    {
        sawBarrierEnemyHitCooldown = Mathf.Max(0.06f, sawBarrierEnemyHitCooldown - amount);
        RefreshSawBarrierFormation();
        Debug.Log($"Saw Barrier Hit Cooldown reduced to {sawBarrierEnemyHitCooldown:0.00}");
    }

    public void UnlockClusterBombs()
    {
        if (!CanEvolveClusterBombs())
        {
            Debug.Log("Cannot evolve Cluster Bombs yet. Requires Explosive Shots + Shrapnel Mines and an open evolved slot.");
            return;
        }

        ConsumeNormalAbility(SpecialAbilityId.ExplosiveShots);
        ConsumeNormalAbility(SpecialAbilityId.ShrapnelMines);

        clusterBombsUnlocked = true;

        Debug.Log($"Evolution Unlocked: Cluster Bombs ({EvolvedAbilityCount}/{MaxEvolvedAbilitySlots} evolved, {NormalAbilityCount}/{CurrentMaxNormalAbilitySlots} normal)");
    }

    public void IncreaseClusterBombDamage(float amount)
    {
        clusterBombPrimaryDamageMultiplier = Mathf.Max(0.1f, clusterBombPrimaryDamageMultiplier + amount);
        clusterBombSecondaryDamageMultiplier = Mathf.Max(0.05f, clusterBombSecondaryDamageMultiplier + amount * 0.5f);
        Debug.Log($"Cluster Bomb Damage increased to {clusterBombPrimaryDamageMultiplier:0.00} primary multiplier");
    }

    public void IncreaseClusterBombRadius(float amount)
    {
        clusterBombPrimaryRadius = Mathf.Max(0.5f, clusterBombPrimaryRadius + amount);
        clusterBombSecondaryRadius = Mathf.Max(0.35f, clusterBombSecondaryRadius + amount * 0.5f);
        Debug.Log($"Cluster Bomb Radius increased to {clusterBombPrimaryRadius:0.00}");
    }

    public void AddClusterBombSecondaryBlast(int amount)
    {
        clusterBombSecondaryBlastCount = Mathf.Clamp(clusterBombSecondaryBlastCount + amount, 1, 8);
        Debug.Log($"Cluster Bomb Secondary Blast Count increased to {clusterBombSecondaryBlastCount}");
    }

    public void TriggerExplosiveShot(
        Vector3 explosionPosition,
        float projectileDamage,
        Health primaryTarget,
        bool originalHitWasCritical)
    {
        if (!explosiveShotsUnlocked && !clusterBombsUnlocked)
        {
            return;
        }

        float activeRadius = clusterBombsUnlocked
            ? Mathf.Max(explosiveShotRadius, clusterBombPrimaryRadius)
            : explosiveShotRadius;

        float activeDamageMultiplier = clusterBombsUnlocked
            ? Mathf.Max(explosiveShotDamageMultiplier, clusterBombPrimaryDamageMultiplier)
            : explosiveShotDamageMultiplier;

        float explosionDamage = projectileDamage * activeDamageMultiplier;

        if (explosionDamage <= 0f)
        {
            return;
        }

        SpawnExplosionVisual(explosionPosition, activeRadius);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            explosionPosition,
            activeRadius,
            explosionHitMask
        );

        HashSet<Health> damagedEnemies = new HashSet<Health>();

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null || !IsEnemyCollider(hitCollider))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null || enemyHealth == primaryTarget || damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            damagedEnemies.Add(enemyHealth);

            bool explosionIsCritical = explosionsCanCrit && originalHitWasCritical;
            enemyHealth.TakeDamage(explosionDamage, explosionIsCritical);
        }

        if (clusterBombsUnlocked)
        {
            SpawnClusterBombSecondaryBlasts(explosionPosition, projectileDamage);
        }

        if (drawExplosionDebugCircle)
        {
            DrawDebugCircle(explosionPosition, activeRadius, debugCircleDuration);
        }
    }

    public bool TryFindRicochetTarget(Vector3 fromPosition, HashSet<Health> ignoredEnemies, out Health targetHealth)
    {
        targetHealth = null;

        if (!ricochetRoundsUnlocked)
        {
            return false;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            fromPosition,
            ricochetRange,
            ricochetHitMask
        );

        float bestDistanceSquared = ricochetRange * ricochetRange;

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null || !IsEnemyCollider(hitCollider))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            if (ignoredEnemies != null && ignoredEnemies.Contains(enemyHealth))
            {
                continue;
            }

            float distanceSquared = (enemyHealth.transform.position - fromPosition).sqrMagnitude;

            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                targetHealth = enemyHealth;
            }
        }

        return targetHealth != null;
    }

    private void PerformLightningStrike()
    {
        FindLightningTargets();

        if (lightningTargets.Count == 0)
        {
            return;
        }

        int strikesToPerform = Mathf.Max(1, lightningStrikesPerActivation);

        for (int i = 0; i < strikesToPerform; i++)
        {
            if (lightningTargets.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, lightningTargets.Count);
            Health targetHealth = lightningTargets[randomIndex];

            lightningTargets.RemoveAt(randomIndex);

            if (targetHealth == null)
            {
                continue;
            }

            targetHealth.TakeDamage(lightningStrikeDamage, false);
            SpawnLightningVisual(targetHealth.transform.position);
        }
    }

    private void FindLightningTargets()
    {
        lightningTargets.Clear();
        lightningTargetSet.Clear();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            lightningStrikeRange,
            lightningHitMask
        );

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null || !IsEnemyCollider(hitCollider))
            {
                continue;
            }

            Health enemyHealth = hitCollider.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<Health>();
            }

            if (enemyHealth == null || enemyHealth.IsDead || lightningTargetSet.Contains(enemyHealth))
            {
                continue;
            }

            lightningTargetSet.Add(enemyHealth);
            lightningTargets.Add(enemyHealth);
        }
    }

    private void EnsureOrbitingBladesExist()
    {
        RemoveNullOrbitingBladeReferences();

        while (activeOrbitingBlades.Count < orbitingBladeCount)
        {
            OrbitingBlade newBlade = CreateOrbitingBlade();

            if (newBlade == null)
            {
                Debug.LogWarning("PlayerSpecialAbilities could not create an Orbiting Blade.");
                return;
            }

            activeOrbitingBlades.Add(newBlade);
        }

        while (activeOrbitingBlades.Count > orbitingBladeCount)
        {
            int lastIndex = activeOrbitingBlades.Count - 1;
            OrbitingBlade bladeToRemove = activeOrbitingBlades[lastIndex];

            activeOrbitingBlades.RemoveAt(lastIndex);

            if (bladeToRemove != null)
            {
                Destroy(bladeToRemove.gameObject);
            }
        }
    }

    private OrbitingBlade CreateOrbitingBlade()
    {
        Transform holder = GetOrbitingBladeHolder();
        OrbitingBlade blade = null;

        if (orbitingBladePrefab != null)
        {
            blade = Instantiate(orbitingBladePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject bladeObject = new GameObject("Orbiting Blade");
            bladeObject.transform.position = transform.position;
            blade = bladeObject.AddComponent<OrbitingBlade>();
        }

        if (blade == null)
        {
            return null;
        }

        blade.transform.SetParent(holder);
        blade.gameObject.SetActive(true);

        return blade;
    }

    private Transform GetOrbitingBladeHolder()
    {
        if (orbitingBladeHolder != null)
        {
            return orbitingBladeHolder;
        }

        GameObject holderObject = new GameObject("Orbiting Blade Holder");
        holderObject.transform.SetParent(transform);
        holderObject.transform.localPosition = Vector3.zero;
        holderObject.transform.localRotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        orbitingBladeHolder = holderObject.transform;

        return orbitingBladeHolder;
    }

    private void RefreshOrbitingBladeFormation()
    {
        RemoveNullOrbitingBladeReferences();

        if (!orbitingBladeUnlocked)
        {
            return;
        }

        for (int i = 0; i < activeOrbitingBlades.Count; i++)
        {
            OrbitingBlade blade = activeOrbitingBlades[i];

            if (blade == null)
            {
                continue;
            }

            blade.Initialize(
                transform,
                i,
                activeOrbitingBlades.Count,
                orbitingBladeOrbitRadius,
                orbitingBladeSpeed,
                orbitingBladeDamage,
                orbitingBladeHitRadius,
                orbitingBladeHitCooldown,
                orbitingBladeHitMask
            );
        }
    }

    private void RemoveNullOrbitingBladeReferences()
    {
        for (int i = activeOrbitingBlades.Count - 1; i >= 0; i--)
        {
            if (activeOrbitingBlades[i] == null)
            {
                activeOrbitingBlades.RemoveAt(i);
            }
        }
    }

    private void EnsureDroneTurretsExist()
    {
        RemoveNullDroneTurretReferences();

        while (activeDroneTurrets.Count < droneTurretCount)
        {
            DroneTurret newDrone = CreateDroneTurret();

            if (newDrone == null)
            {
                Debug.LogWarning("PlayerSpecialAbilities could not create a Drone Turret.");
                return;
            }

            activeDroneTurrets.Add(newDrone);
        }

        while (activeDroneTurrets.Count > droneTurretCount)
        {
            int lastIndex = activeDroneTurrets.Count - 1;
            DroneTurret droneToRemove = activeDroneTurrets[lastIndex];

            activeDroneTurrets.RemoveAt(lastIndex);

            if (droneToRemove != null)
            {
                Destroy(droneToRemove.gameObject);
            }
        }
    }

    private DroneTurret CreateDroneTurret()
    {
        Transform holder = GetDroneTurretHolder();
        DroneTurret drone = null;

        if (droneTurretPrefab != null)
        {
            drone = Instantiate(droneTurretPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject droneObject = new GameObject("Drone Turret");
            droneObject.transform.position = transform.position;
            drone = droneObject.AddComponent<DroneTurret>();
        }

        if (drone == null)
        {
            return null;
        }

        drone.transform.SetParent(holder);
        drone.gameObject.SetActive(true);

        return drone;
    }

    private Transform GetDroneTurretHolder()
    {
        if (droneTurretHolder != null)
        {
            return droneTurretHolder;
        }

        GameObject holderObject = new GameObject("Drone Turret Holder");
        holderObject.transform.SetParent(transform);
        holderObject.transform.localPosition = Vector3.zero;
        holderObject.transform.localRotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        droneTurretHolder = holderObject.transform;

        return droneTurretHolder;
    }

    private void RefreshDroneTurretFormation()
    {
        RemoveNullDroneTurretReferences();

        if (!droneTurretUnlocked)
        {
            return;
        }

        for (int i = 0; i < activeDroneTurrets.Count; i++)
        {
            DroneTurret drone = activeDroneTurrets[i];

            if (drone == null)
            {
                continue;
            }

            drone.Initialize(
                transform,
                i,
                activeDroneTurrets.Count,
                droneTurretOrbitRadius,
                droneTurretOrbitSpeed,
                droneTurretFollowSharpness,
                droneTurretDamage,
                droneTurretFireCooldown,
                droneTurretRange,
                droneTurretHitMask,
                droneTurretBodyColor,
                droneTurretBeamColor,
                droneTurretBodyRadius,
                droneTurretBodyLineWidth,
                droneTurretBeamWidth
            );
        }
    }

    private void RemoveNullDroneTurretReferences()
    {
        for (int i = activeDroneTurrets.Count - 1; i >= 0; i--)
        {
            if (activeDroneTurrets[i] == null)
            {
                activeDroneTurrets.RemoveAt(i);
            }
        }
    }

    private void EnsureLaserBeamsExist()
    {
        RemoveNullLaserBeamReferences();

        while (activeLaserBeams.Count < laserBeamCount)
        {
            RotatingLaserBeam newLaser = CreateLaserBeam();

            if (newLaser == null)
            {
                Debug.LogWarning("PlayerSpecialAbilities could not create a Laser Beam.");
                return;
            }

            activeLaserBeams.Add(newLaser);
        }

        while (activeLaserBeams.Count > laserBeamCount)
        {
            int lastIndex = activeLaserBeams.Count - 1;
            RotatingLaserBeam laserToRemove = activeLaserBeams[lastIndex];

            activeLaserBeams.RemoveAt(lastIndex);

            if (laserToRemove != null)
            {
                Destroy(laserToRemove.gameObject);
            }
        }
    }

    private RotatingLaserBeam CreateLaserBeam()
    {
        Transform holder = GetLaserBeamHolder();
        RotatingLaserBeam laser = null;

        if (laserBeamPrefab != null)
        {
            laser = Instantiate(laserBeamPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject laserObject = new GameObject("Rotating Laser Beam");
            laserObject.transform.position = transform.position;
            laser = laserObject.AddComponent<RotatingLaserBeam>();
        }

        if (laser == null)
        {
            return null;
        }

        laser.transform.SetParent(holder);
        laser.gameObject.SetActive(true);

        return laser;
    }

    private Transform GetLaserBeamHolder()
    {
        if (laserBeamHolder != null)
        {
            return laserBeamHolder;
        }

        GameObject holderObject = new GameObject("Laser Beam Holder");
        holderObject.transform.SetParent(transform);
        holderObject.transform.localPosition = Vector3.zero;
        holderObject.transform.localRotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        laserBeamHolder = holderObject.transform;

        return laserBeamHolder;
    }

    private void RefreshLaserBeamFormation()
    {
        RemoveNullLaserBeamReferences();

        if (!laserBeamUnlocked)
        {
            return;
        }

        for (int i = 0; i < activeLaserBeams.Count; i++)
        {
            RotatingLaserBeam laser = activeLaserBeams[i];

            if (laser == null)
            {
                continue;
            }

            laser.Initialize(
                transform,
                i,
                activeLaserBeams.Count,
                laserBeamDamage,
                laserBeamLength,
                laserBeamRotationSpeed,
                laserBeamHitWidth,
                laserBeamHitCooldown,
                laserBeamHitMask,
                laserBeamColor,
                laserBeamCoreColor,
                laserBeamLineWidth,
                laserBeamCoreRadius
            );
        }
    }

    private void RemoveNullLaserBeamReferences()
    {
        for (int i = activeLaserBeams.Count - 1; i >= 0; i--)
        {
            if (activeLaserBeams[i] == null)
            {
                activeLaserBeams.RemoveAt(i);
            }
        }
    }

    private void EnsureGuardianShieldsExist()
    {
        RemoveNullGuardianShieldReferences();

        while (activeGuardianShields.Count < guardianShieldCount)
        {
            GuardianShield newShield = CreateGuardianShield();

            if (newShield == null)
            {
                Debug.LogWarning("PlayerSpecialAbilities could not create a Guardian Shield.");
                return;
            }

            activeGuardianShields.Add(newShield);
        }

        while (activeGuardianShields.Count > guardianShieldCount)
        {
            int lastIndex = activeGuardianShields.Count - 1;
            GuardianShield shieldToRemove = activeGuardianShields[lastIndex];

            activeGuardianShields.RemoveAt(lastIndex);

            if (shieldToRemove != null)
            {
                Destroy(shieldToRemove.gameObject);
            }
        }
    }

    private GuardianShield CreateGuardianShield()
    {
        Transform holder = GetGuardianShieldHolder();
        GuardianShield shield = null;

        if (guardianShieldPrefab != null)
        {
            shield = Instantiate(guardianShieldPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject shieldObject = new GameObject("Guardian Shield");
            shieldObject.transform.position = transform.position;
            shield = shieldObject.AddComponent<GuardianShield>();
        }

        if (shield == null)
        {
            return null;
        }

        shield.transform.SetParent(holder);
        shield.gameObject.SetActive(true);

        return shield;
    }

    private void EnsureSawBarrierShieldsExist()
    {
        RemoveNullSawBarrierShieldReferences();

        if (!sawBarrierUnlocked)
        {
            return;
        }

        sawBarrierCount = Mathf.Clamp(sawBarrierCount, 1, maxSawBarrierCount);

        while (activeSawBarrierShields.Count < sawBarrierCount)
        {
            GuardianShield shield = CreateSawBarrierShield();

            if (shield == null)
            {
                Debug.LogWarning("PlayerSpecialAbilities could not create a Saw Barrier shield.");
                return;
            }

            activeSawBarrierShields.Add(shield);
        }

        while (activeSawBarrierShields.Count > sawBarrierCount)
        {
            int lastIndex = activeSawBarrierShields.Count - 1;
            GuardianShield shieldToRemove = activeSawBarrierShields[lastIndex];
            activeSawBarrierShields.RemoveAt(lastIndex);

            if (shieldToRemove != null)
            {
                Destroy(shieldToRemove.gameObject);
            }
        }
    }

    private GuardianShield CreateSawBarrierShield()
    {
        GuardianShield shield = null;

        if (sawBarrierShieldPrefab != null)
        {
            shield = Instantiate(sawBarrierShieldPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject shieldObject = new GameObject("Saw Barrier Shield");
            shieldObject.transform.position = transform.position;
            shield = shieldObject.AddComponent<GuardianShield>();
        }

        if (shield == null)
        {
            return null;
        }

        shield.transform.SetParent(GetSawBarrierHolder());
        shield.gameObject.SetActive(true);

        return shield;
    }

    private void RefreshSawBarrierFormation()
    {
        RemoveNullSawBarrierShieldReferences();

        if (!sawBarrierUnlocked)
        {
            return;
        }

        for (int i = 0; i < activeSawBarrierShields.Count; i++)
        {
            GuardianShield shield = activeSawBarrierShields[i];

            if (shield == null)
            {
                continue;
            }

            shield.Initialize(
                transform,
                i,
                activeSawBarrierShields.Count,
                sawBarrierOrbitRadius,
                sawBarrierOrbitSpeed,
                sawBarrierDamage,
                sawBarrierHitRadius,
                sawBarrierEnemyHitCooldown,
                sawBarrierEnemyHitMask,
                sawBarrierProjectileHitMask,
                sawBarrierProjectileTag,
                sawBarrierColor,
                sawBarrierBlockFlashColor,
                sawBarrierVisualRadius,
                sawBarrierLineWidth,
                sawBarrierCircleSegments
            );
        }
    }

    private void RemoveNullSawBarrierShieldReferences()
    {
        for (int i = activeSawBarrierShields.Count - 1; i >= 0; i--)
        {
            if (activeSawBarrierShields[i] == null)
            {
                activeSawBarrierShields.RemoveAt(i);
            }
        }
    }

    private Transform GetSawBarrierHolder()
    {
        if (sawBarrierHolder != null)
        {
            return sawBarrierHolder;
        }

        GameObject holderObject = new GameObject("Saw Barrier Holder");
        holderObject.transform.SetParent(transform);
        holderObject.transform.localPosition = Vector3.zero;
        holderObject.transform.localRotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        sawBarrierHolder = holderObject.transform;

        return sawBarrierHolder;
    }

    private Transform GetGuardianShieldHolder()
    {
        if (guardianShieldHolder != null)
        {
            return guardianShieldHolder;
        }

        GameObject holderObject = new GameObject("Guardian Shield Holder");
        holderObject.transform.SetParent(transform);
        holderObject.transform.localPosition = Vector3.zero;
        holderObject.transform.localRotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        guardianShieldHolder = holderObject.transform;

        return guardianShieldHolder;
    }

    private void RefreshGuardianShieldFormation()
    {
        RemoveNullGuardianShieldReferences();

        if (!guardianShieldUnlocked)
        {
            return;
        }

        for (int i = 0; i < activeGuardianShields.Count; i++)
        {
            GuardianShield shield = activeGuardianShields[i];

            if (shield == null)
            {
                continue;
            }

            shield.Initialize(
                transform,
                i,
                activeGuardianShields.Count,
                guardianShieldOrbitRadius,
                guardianShieldOrbitSpeed,
                guardianShieldDamage,
                guardianShieldHitRadius,
                guardianShieldEnemyHitCooldown,
                guardianShieldEnemyHitMask,
                guardianShieldProjectileHitMask,
                guardianShieldProjectileTag,
                guardianShieldColor,
                guardianShieldBlockFlashColor,
                guardianShieldVisualRadius,
                guardianShieldLineWidth,
                guardianShieldCircleSegments
            );
        }
    }

    private void RemoveNullGuardianShieldReferences()
    {
        for (int i = activeGuardianShields.Count - 1; i >= 0; i--)
        {
            if (activeGuardianShields[i] == null)
            {
                activeGuardianShields.RemoveAt(i);
            }
        }
    }

    private void SpawnFireTrailZone(Vector3 position)
    {
        GameObject zoneObject = SpawnAbilityObject(fireTrailZonePrefab, "Fire Trail Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        if (fireTrailZonePrefab == null)
        {
            zoneObject.transform.SetParent(GetFireTrailHolder());
        }

        FireTrailZone fireZone = zoneObject.GetComponent<FireTrailZone>();

        if (fireZone == null)
        {
            fireZone = zoneObject.AddComponent<FireTrailZone>();
        }

        fireZone.Initialize(
            position,
            fireTrailRadius,
            fireTrailDamagePerTick,
            fireTrailTickInterval,
            fireTrailLifetime,
            fireTrailHitMask,
            fireTrailStartColor,
            fireTrailEndColor,
            fireTrailLineWidth,
            fireTrailCircleSegments
        );
    }

    private void SpawnBlackHole()
    {
        Vector3 position;

        if (!TryFindEnemyClusterPosition(blackHoleTargetRange, blackHoleClusterSearchRadius, out position))
        {
            position = transform.position;
        }

        GameObject zoneObject = SpawnAbilityObject(blackHoleZonePrefab, "Black Hole Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.BlackHole,
            position,
            blackHoleRadius,
            blackHoleLifetime,
            blackHoleDamagePerTick,
            blackHoleTickInterval,
            blackHoleHitMask,
            blackHoleStartColor,
            blackHoleEndColor,
            blackHoleLineWidth,
            72,
            blackHolePullStrength
        );
    }

    private void SpawnIceNova()
    {
        Vector3 position = transform.position;
        GameObject zoneObject = SpawnAbilityObject(iceNovaZonePrefab, "Ice Nova Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.IceNova,
            position,
            iceNovaRadius,
            iceNovaLifetime,
            iceNovaDamage,
            0.1f,
            iceNovaHitMask,
            iceNovaStartColor,
            iceNovaEndColor,
            iceNovaLineWidth,
            72,
            0f,
            iceNovaKnockbackForce,
            iceNovaVelocityDamping,
            true,
            true
        );
    }

    private void SpawnPoisonCloud()
    {
        Vector3 position;

        if (!TryFindEnemyClusterPosition(poisonCloudTargetRange, poisonCloudClusterSearchRadius, out position))
        {
            position = transform.position;
        }

        GameObject zoneObject = SpawnAbilityObject(poisonCloudZonePrefab, "Poison Cloud Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.PoisonCloud,
            position,
            poisonCloudRadius,
            poisonCloudLifetime,
            poisonCloudDamagePerTick,
            poisonCloudTickInterval,
            poisonCloudHitMask,
            poisonCloudStartColor,
            poisonCloudEndColor,
            poisonCloudLineWidth,
            64
        );
    }

    private void SpawnShockwave()
    {
        Vector3 position = transform.position;
        GameObject zoneObject = SpawnAbilityObject(shockwaveZonePrefab, "Shockwave Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.IceNova,
            position,
            shockwaveRadius,
            shockwaveLifetime,
            shockwaveDamage,
            0.1f,
            shockwaveHitMask,
            shockwaveStartColor,
            shockwaveEndColor,
            shockwaveLineWidth,
            80,
            0f,
            shockwaveKnockbackForce,
            shockwaveVelocityDamping,
            true,
            true
        );
    }

    private void SpawnCryoBlast()
    {
        Vector3 position = transform.position;
        GameObject zoneObject = SpawnAbilityObject(cryoBlastZonePrefab, "Cryo Blast Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.IceNova,
            position,
            cryoBlastRadius,
            cryoBlastLifetime,
            cryoBlastDamage,
            0.1f,
            cryoBlastHitMask,
            cryoBlastStartColor,
            cryoBlastEndColor,
            cryoBlastLineWidth,
            96,
            0f,
            cryoBlastKnockbackForce,
            cryoBlastVelocityDamping,
            true,
            true
        );
    }

    private void SpawnSolarVortex()
    {
        Vector3 position;

        if (!TryFindEnemyClusterPosition(solarVortexTargetRange, solarVortexClusterSearchRadius, out position))
        {
            position = transform.position;
        }

        GameObject zoneObject = SpawnAbilityObject(solarVortexZonePrefab, "Solar Vortex Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.BlackHole,
            position,
            solarVortexRadius,
            solarVortexLifetime,
            solarVortexDamagePerTick,
            solarVortexTickInterval,
            solarVortexHitMask,
            solarVortexStartColor,
            solarVortexEndColor,
            solarVortexLineWidth,
            88,
            solarVortexPullStrength
        );
    }

    private void SpawnMeteorStrike()
    {
        Vector3 position;

        if (!TryFindEnemyClusterPosition(meteorStrikeTargetRange, meteorStrikeClusterSearchRadius, out position))
        {
            position = transform.position;
        }

        GameObject blastObject = SpawnAbilityObject(meteorStrikeBlastPrefab, "Meteor Strike", position);

        if (blastObject == null)
        {
            return;
        }

        SpecialAbilityDelayedBlast delayedBlast = blastObject.GetComponent<SpecialAbilityDelayedBlast>();

        if (delayedBlast == null)
        {
            delayedBlast = blastObject.AddComponent<SpecialAbilityDelayedBlast>();
        }

        delayedBlast.Initialize(
            position,
            meteorStrikeRadius,
            meteorStrikeWarningDuration,
            meteorStrikeRadius,
            meteorStrikeDamage,
            meteorStrikeVisualDuration,
            meteorStrikeHitMask,
            meteorWarningColor,
            meteorBlastColor,
            meteorStrikeLineWidth,
            72,
            meteorStrikeKnockback
        );
    }

    private void SpawnShrapnelMine(Vector3 position)
    {
        ShrapnelMine mine = null;

        if (shrapnelMinePrefab != null && PoolManager.HasInstance)
        {
            GameObject pooledObject = PoolManager.Instance.Spawn(shrapnelMinePrefab.gameObject, position, Quaternion.identity);

            if (pooledObject != null)
            {
                mine = pooledObject.GetComponent<ShrapnelMine>();
            }
        }

        if (mine == null && shrapnelMinePrefab != null)
        {
            mine = Instantiate(shrapnelMinePrefab, position, Quaternion.identity);
        }

        if (mine == null)
        {
            GameObject mineObject = new GameObject("Shrapnel Mine");
            mineObject.transform.position = position;
            mine = mineObject.AddComponent<ShrapnelMine>();
        }

        if (mine == null)
        {
            return;
        }

        if (shrapnelMinePrefab == null)
        {
            mine.transform.SetParent(GetShrapnelMineHolder());
        }

        mine.Initialize(
            position,
            shrapnelMineTriggerRadius,
            shrapnelMineBlastRadius,
            shrapnelMineDamage,
            shrapnelMineArmTime,
            shrapnelMineLifetime,
            0.25f,
            shrapnelMineHitMask,
            shrapnelMineIdleColor,
            shrapnelMineArmedColor,
            shrapnelMineBlastColor,
            shrapnelMineLineWidth,
            shrapnelMineCircleSegments,
            shrapnelMineKnockback
        );
    }

    private void SpawnTimeFracture()
    {
        Vector3 position = transform.position;
        GameObject zoneObject = SpawnAbilityObject(timeFractureZonePrefab, "Time Fracture Zone", position);

        if (zoneObject == null)
        {
            return;
        }

        SpecialAbilityZone zone = zoneObject.GetComponent<SpecialAbilityZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<SpecialAbilityZone>();
        }

        zone.Initialize(
            SpecialAbilityZone.ZoneType.TimeFracture,
            position,
            timeFractureRadius,
            timeFractureLifetime,
            timeFractureDamagePerTick,
            timeFractureTickInterval,
            timeFractureHitMask,
            timeFractureStartColor,
            timeFractureEndColor,
            timeFractureLineWidth,
            80,
            0f,
            0f,
            timeFractureVelocityDamping,
            false,
            false
        );
    }

    private void SpawnClusterBombSecondaryBlasts(Vector3 centerPosition, float projectileDamage)
    {
        int safeBlastCount = Mathf.Max(1, clusterBombSecondaryBlastCount);

        for (int i = 0; i < safeBlastCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * clusterBombSecondarySpreadRadius;
            Vector3 blastPosition = centerPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            SpawnClusterBombSecondaryBlast(blastPosition, projectileDamage);
        }
    }

    private void SpawnClusterBombSecondaryBlast(Vector3 position, float projectileDamage)
    {
        GameObject blastObject = SpawnAbilityObject(clusterBombSecondaryBlastPrefab, "Cluster Bomb Secondary Blast", position);

        if (blastObject == null)
        {
            return;
        }

        SpecialAbilityDelayedBlast delayedBlast = blastObject.GetComponent<SpecialAbilityDelayedBlast>();

        if (delayedBlast == null)
        {
            delayedBlast = blastObject.AddComponent<SpecialAbilityDelayedBlast>();
        }

        delayedBlast.Initialize(
            position,
            clusterBombSecondaryRadius,
            clusterBombSecondaryWarningDuration,
            clusterBombSecondaryRadius,
            projectileDamage * clusterBombSecondaryDamageMultiplier,
            clusterBombSecondaryVisualDuration,
            explosionHitMask,
            clusterBombWarningColor,
            clusterBombBlastColor,
            clusterBombLineWidth,
            48,
            clusterBombKnockback
        );
    }

    private GameObject SpawnAbilityObject(GameObject prefab, string runtimeName, Vector3 position)
    {
        GameObject spawnedObject = null;

        if (prefab != null && PoolManager.HasInstance)
        {
            spawnedObject = PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);
        }

        if (spawnedObject == null && prefab != null)
        {
            spawnedObject = Instantiate(prefab, position, Quaternion.identity);
        }

        if (spawnedObject == null)
        {
            spawnedObject = new GameObject(runtimeName);
            spawnedObject.transform.position = position;
            spawnedObject.AddComponent<LineRenderer>();
        }

        return spawnedObject;
    }

    private Transform GetShrapnelMineHolder()
    {
        if (shrapnelMineHolder != null)
        {
            return shrapnelMineHolder;
        }

        GameObject holderObject = new GameObject("Shrapnel Mine Holder");
        holderObject.transform.SetParent(null);
        holderObject.transform.position = Vector3.zero;
        holderObject.transform.rotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        shrapnelMineHolder = holderObject.transform;

        return shrapnelMineHolder;
    }

    private void ScanEnemiesForBloodPact()
    {
        CleanupBloodPactSubscriptions();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemyObject = enemies[i];

            if (enemyObject == null)
            {
                continue;
            }

            Health enemyHealth = enemyObject.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.GetComponentInChildren<Health>();
            }

            if (enemyHealth == null || enemyHealth.IsDead || bloodPactSubscriptions.ContainsKey(enemyHealth))
            {
                continue;
            }

            System.Action handler = () => HandleBloodPactEnemyDied(enemyHealth);
            bloodPactSubscriptions.Add(enemyHealth, handler);
            enemyHealth.OnDied += handler;
        }
    }

    private void HandleBloodPactEnemyDied(Health enemyHealth)
    {
        if (enemyHealth != null)
        {
            System.Action handler;

            if (bloodPactSubscriptions.TryGetValue(enemyHealth, out handler))
            {
                enemyHealth.OnDied -= handler;
                bloodPactSubscriptions.Remove(enemyHealth);
            }
        }

        if (!bloodPactUnlocked)
        {
            return;
        }

        if (Random.value > bloodPactHealChance)
        {
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.Heal(bloodPactHealPerKill);
        }
    }

    private void CleanupBloodPactSubscriptions()
    {
        bloodPactCleanup.Clear();

        foreach (KeyValuePair<Health, System.Action> pair in bloodPactSubscriptions)
        {
            if (pair.Key == null || pair.Key.IsDead)
            {
                bloodPactCleanup.Add(pair.Key);
            }
        }

        for (int i = 0; i < bloodPactCleanup.Count; i++)
        {
            Health enemyHealth = bloodPactCleanup[i];

            if (enemyHealth != null)
            {
                System.Action handler;

                if (bloodPactSubscriptions.TryGetValue(enemyHealth, out handler))
                {
                    enemyHealth.OnDied -= handler;
                }
            }

            bloodPactSubscriptions.Remove(enemyHealth);
        }
    }

    private void ClearBloodPactSubscriptions()
    {
        foreach (KeyValuePair<Health, System.Action> pair in bloodPactSubscriptions)
        {
            if (pair.Key != null)
            {
                pair.Key.OnDied -= pair.Value;
            }
        }

        bloodPactSubscriptions.Clear();
        bloodPactCleanup.Clear();
    }

    private Transform GetFireTrailHolder()
    {
        if (fireTrailHolder != null)
        {
            return fireTrailHolder;
        }

        GameObject holderObject = new GameObject("Fire Trail Holder");
        holderObject.transform.SetParent(null);
        holderObject.transform.position = Vector3.zero;
        holderObject.transform.rotation = Quaternion.identity;
        holderObject.transform.localScale = Vector3.one;

        fireTrailHolder = holderObject.transform;

        return fireTrailHolder;
    }

    private bool TryFindEnemyClusterPosition(float searchRange, float clusterRadius, out Vector3 position)
    {
        position = transform.position;
        enemySearchTargets.Clear();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemyObject = enemies[i];

            if (enemyObject == null)
            {
                continue;
            }

            Health enemyHealth = enemyObject.GetComponent<Health>();

            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.GetComponentInChildren<Health>();
            }

            if (enemyHealth != null && enemyHealth.IsDead)
            {
                continue;
            }

            float distanceSquared = (enemyObject.transform.position - transform.position).sqrMagnitude;

            if (distanceSquared <= searchRange * searchRange)
            {
                enemySearchTargets.Add(enemyObject.transform);
            }
        }

        if (enemySearchTargets.Count == 0)
        {
            return false;
        }

        Transform bestTarget = enemySearchTargets[Random.Range(0, enemySearchTargets.Count)];
        int bestScore = -1;
        float clusterRadiusSquared = clusterRadius * clusterRadius;

        for (int i = 0; i < enemySearchTargets.Count; i++)
        {
            Transform candidate = enemySearchTargets[i];

            if (candidate == null)
            {
                continue;
            }

            int score = 0;

            for (int j = 0; j < enemySearchTargets.Count; j++)
            {
                Transform other = enemySearchTargets[j];

                if (other == null)
                {
                    continue;
                }

                if ((other.position - candidate.position).sqrMagnitude <= clusterRadiusSquared)
                {
                    score++;
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        Vector2 offset = Random.insideUnitCircle * 0.65f;
        position = bestTarget.position + new Vector3(offset.x, offset.y, 0f);

        return true;
    }

    private bool IsEnemyCollider(Collider2D hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }

        Transform currentTransform = hitCollider.transform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag("Enemy"))
            {
                return true;
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private void SpawnExplosionVisual(Vector3 explosionPosition, float visualRadius)
    {
        if (!spawnExplosionVisual || explosionVisualPrefab == null)
        {
            return;
        }

        GameObject visualObject = null;

        if (PoolManager.HasInstance)
        {
            visualObject = PoolManager.Instance.Spawn(
                explosionVisualPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            visualObject = Instantiate(
                explosionVisualPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            return;
        }

        ExplosionVisual explosionVisual = visualObject.GetComponent<ExplosionVisual>();

        if (explosionVisual != null)
        {
            explosionVisual.Play(
                visualRadius,
                explosionVisualDuration,
                explosionVisualStartColor,
                explosionVisualEndColor
            );
        }
    }

    private void SpawnLightningVisual(Vector3 strikePosition)
    {
        if (!spawnLightningVisual || lightningStrikeVisualPrefab == null)
        {
            return;
        }

        GameObject visualObject = null;

        if (PoolManager.HasInstance)
        {
            visualObject = PoolManager.Instance.Spawn(
                lightningStrikeVisualPrefab,
                strikePosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            visualObject = Instantiate(
                lightningStrikeVisualPrefab,
                strikePosition,
                Quaternion.identity
            );
        }

        if (visualObject == null)
        {
            return;
        }

        LightningStrikeVisual lightningVisual = visualObject.GetComponent<LightningStrikeVisual>();

        if (lightningVisual != null)
        {
            lightningVisual.Play(
                strikePosition,
                lightningVisualBoltLength,
                lightningVisualDuration,
                lightningVisualStartColor,
                lightningVisualEndColor,
                lightningVisualWidth,
                lightningVisualSegments,
                lightningVisualJaggedness
            );
        }
    }

    private void DrawDebugCircle(Vector3 center, float radius, float duration)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            Debug.DrawLine(previousPoint, nextPoint, Color.yellow, duration);
            previousPoint = nextPoint;
        }
    }
}
