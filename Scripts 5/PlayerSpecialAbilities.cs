using System.Collections.Generic;
using UnityEngine;

public class PlayerSpecialAbilities : MonoBehaviour
{
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

    [Header("Debug")]
    [SerializeField] private bool drawExplosionDebugCircle = false;
    [SerializeField] private float debugCircleDuration = 0.2f;

    private float lightningTimer;
    private float fireTrailTimer;
    private float blackHoleTimer;
    private float iceNovaTimer;
    private float poisonCloudTimer;

    private Vector3 lastFireTrailSpawnPosition;
    private bool hasSpawnedFirstFireTrailZone;

    private readonly List<Health> lightningTargets = new List<Health>();
    private readonly HashSet<Health> lightningTargetSet = new HashSet<Health>();
    private readonly List<OrbitingBlade> activeOrbitingBlades = new List<OrbitingBlade>();
    private readonly List<DroneTurret> activeDroneTurrets = new List<DroneTurret>();
    private readonly List<RotatingLaserBeam> activeLaserBeams = new List<RotatingLaserBeam>();
    private readonly List<Transform> enemySearchTargets = new List<Transform>();

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

    private void OnEnable()
    {
        lightningTimer = lightningStrikeCooldown;
        fireTrailTimer = 0f;
        blackHoleTimer = 0.75f;
        iceNovaTimer = 1f;
        poisonCloudTimer = 1.25f;

        lastFireTrailSpawnPosition = transform.position;
        hasSpawnedFirstFireTrailZone = false;

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
    }

    private void Update()
    {
        HandleLightningStrikeTimer();
        HandleFireTrailTimer();
        HandleBlackHoleTimer();
        HandleIceNovaTimer();
        HandlePoisonCloudTimer();
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

    public void UnlockExplosiveShots()
    {
        explosiveShotsUnlocked = true;
        Debug.Log("Special Ability Unlocked: Explosive Shots");
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
        lightningStrikeUnlocked = true;
        lightningTimer = 0.25f;
        Debug.Log("Special Ability Unlocked: Lightning Strike");
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
        orbitingBladeUnlocked = true;

        if (orbitingBladeCount <= 0)
        {
            orbitingBladeCount = 1;
        }

        orbitingBladeCount = Mathf.Clamp(orbitingBladeCount, 1, maxOrbitingBlades);

        EnsureOrbitingBladesExist();
        RefreshOrbitingBladeFormation();

        Debug.Log("Special Ability Unlocked: Orbiting Blade");
    }

    public void AddOrbitingBlade(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        orbitingBladeUnlocked = true;
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
        fireTrailUnlocked = true;
        fireTrailTimer = 0f;
        lastFireTrailSpawnPosition = transform.position;
        hasSpawnedFirstFireTrailZone = false;
        Debug.Log("Special Ability Unlocked: Fire Trail");
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
        blackHoleUnlocked = true;
        blackHoleTimer = 0.35f;
        Debug.Log("Special Ability Unlocked: Black Hole");
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
        droneTurretUnlocked = true;

        if (droneTurretCount <= 0)
        {
            droneTurretCount = 1;
        }

        droneTurretCount = Mathf.Clamp(droneTurretCount, 1, maxDroneTurrets);

        EnsureDroneTurretsExist();
        RefreshDroneTurretFormation();

        Debug.Log("Special Ability Unlocked: Drone Turret");
    }

    public void AddDroneTurret(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        droneTurretUnlocked = true;
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
        iceNovaUnlocked = true;
        iceNovaTimer = 0.35f;
        Debug.Log("Special Ability Unlocked: Ice Nova");
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
        poisonCloudUnlocked = true;
        poisonCloudTimer = 0.35f;
        Debug.Log("Special Ability Unlocked: Poison Cloud");
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
        ricochetRoundsUnlocked = true;
        ricochetBounceCount = Mathf.Clamp(ricochetBounceCount, 1, maxRicochetBounceCount);
        Debug.Log("Special Ability Unlocked: Ricochet Rounds");
    }

    public void AddRicochetBounce(int amount)
    {
        ricochetRoundsUnlocked = true;
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
        laserBeamUnlocked = true;

        if (laserBeamCount <= 0)
        {
            laserBeamCount = 1;
        }

        laserBeamCount = Mathf.Clamp(laserBeamCount, 1, maxLaserBeamCount);

        EnsureLaserBeamsExist();
        RefreshLaserBeamFormation();

        Debug.Log("Special Ability Unlocked: Laser Beam");
    }

    public void AddLaserBeam(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        laserBeamUnlocked = true;
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

    public void TriggerExplosiveShot(
        Vector3 explosionPosition,
        float projectileDamage,
        Health primaryTarget,
        bool originalHitWasCritical)
    {
        if (!explosiveShotsUnlocked)
        {
            return;
        }

        float explosionDamage = projectileDamage * explosiveShotDamageMultiplier;

        if (explosionDamage <= 0f)
        {
            return;
        }

        SpawnExplosionVisual(explosionPosition);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            explosionPosition,
            explosiveShotRadius,
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

        if (drawExplosionDebugCircle)
        {
            DrawDebugCircle(explosionPosition, explosiveShotRadius, debugCircleDuration);
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

    private void SpawnExplosionVisual(Vector3 explosionPosition)
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
                explosiveShotRadius,
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
