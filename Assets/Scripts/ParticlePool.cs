using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Instance { get; private set; }

    [Header("Prefab & Pool")]
    [SerializeField] private ParticleSystem particlePrefab;   // assign your burst PS prefab
    [SerializeField, Min(0)] private int initialSize = 8;

    private readonly Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < initialSize; i++)
        {
            var ps = CreateInstance();
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    ParticleSystem CreateInstance()
    {
        var ps = Instantiate(particlePrefab, transform);
        ConfigureCommon(ps);

        var ret = ps.GetComponent<_ReturnToBurstPool>();
        if (!ret) ret = ps.gameObject.AddComponent<_ReturnToBurstPool>();
        ret.Init(this);
        return ps;
    }

    void ConfigureCommon(ParticleSystem ps)
    {
        var main = ps.main;
        main.loop = false;                                    // one-shot
        main.stopAction = ParticleSystemStopAction.Callback;  // auto return via callback
        // IMPORTANT: do NOT set simulationSpace here; we set it per-spawn.
    }

    ParticleSystem Get()
    {
        var ps = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        ConfigureCommon(ps);      // keep settings safe if prefab changed
        ps.transform.SetParent(transform, true);
        ps.gameObject.SetActive(true);
        return ps;
    }

    public void Release(ParticleSystem ps)
    {
        if (!ps) return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // reparent back to pool and reset local transform
        ps.transform.SetParent(transform, false);         // false = use local space
        ps.transform.localPosition = Vector3.zero;
        ps.transform.localRotation = Quaternion.identity;
        ps.transform.localScale = Vector3.one;            // <- reset scale!
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }

    // ---- SPAWN APIS ----

    /// Spawns a burst that STAYS in the world at the given position/rotation.
    public ParticleSystem PlayBurstWorld(Vector3 position, Quaternion rotation)
    {
        var ps = Get();
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ps.transform.SetParent(transform, true);
        ps.transform.SetPositionAndRotation(position, rotation);

        ps.Clear(true);
        ps.Play(true);
        return ps;
    }

    public ParticleSystem PlayBurstAttached(Transform anchor, Vector3 localOffset = default)
    {
        if (!anchor) return null;

        var ps = Get();
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode     = ParticleSystemScalingMode.Local; // <- ignore parent scale

        ps.transform.SetParent(anchor, false);          // use local space under anchor
        ps.transform.localPosition = localOffset;
        ps.transform.localRotation = Quaternion.identity;
        ps.transform.localScale    = Vector3.one;       // <- reset scale!

        ps.Clear(true);
        ps.Play(true);
        return ps;
    }

}

/// Auto-returns to pool when the single PS stops.
sealed class _ReturnToBurstPool : MonoBehaviour
{
    ParticlePool pool;
    ParticleSystem ps;

    public void Init(ParticlePool p)
    {
        pool = p;
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    void OnParticleSystemStopped()
    {
        pool.Release(ps);
    }
}
