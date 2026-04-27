using UnityEngine;

public class AIShooter : MonoBehaviour
{
    [SerializeField] AudioSource Funny;

    [Header("Target")]
    public Transform player;
    public Transform firePoint;
    public float viewDistance = 30f;
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightMask = Physics.DefaultRaycastLayers;
    public float turnSpeed = 5f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float maxFireRate = 5f;

    [Header("Difficulty")]
    [Range(0f, 1f)]
    public float difficultyPercent = 0.5f;
    public bool useSavedDifficulty = true;

    private float fireTimer;

    void Start()
    {
        float startingDifficulty = useSavedDifficulty
            ? DifficultyManager.GetSavedDifficultyPercent(difficultyPercent)
            : difficultyPercent;

        SetDifficulty(startingDifficulty);
    }

    void Update()
    {
        if (player == null || firePoint == null || projectilePrefab == null)
        {
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.sqrMagnitude > viewDistance * viewDistance)
        {
            return;
        }

        WatchPlayer(toPlayer);

        if (difficultyPercent <= 0f || !CanSeePlayer())
        {
            return;
        }

        fireTimer += Time.deltaTime;
        float activeFireRate = maxFireRate * difficultyPercent;
        float fireInterval = 1f / activeFireRate;

        if (fireTimer >= fireInterval)
        {
            Shoot();
            Funny.Play();
            fireTimer = 0f;
        }
    }

    void WatchPlayer(Vector3 toPlayer)
    {
        Vector3 flatDirection = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flatDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    bool CanSeePlayer()
    {
        if (!requireLineOfSight)
        {
            return true;
        }

        Vector3 origin = firePoint.position;
        Vector3 target = player.position + Vector3.up;
        Vector3 direction = target - origin;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, viewDistance, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector3 aimPoint = player.position + Vector3.up;
        Vector3 direction = (aimPoint - firePoint.position).normalized;

        if (projectile.TryGetComponent(out Rigidbody projectileRigidbody))
        {
            projectileRigidbody.linearVelocity = direction * projectileSpeed;
        }

        projectile.transform.forward = direction;
    }

    public void SetDifficulty(float percent)
    {
        difficultyPercent = Mathf.Clamp01(percent);
        fireTimer = 0f;
    }
}
