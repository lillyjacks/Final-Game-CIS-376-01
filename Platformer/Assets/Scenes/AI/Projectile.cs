using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float knockbackForce = 6f;
    public float upwardLift = 1f;
    public float lifeTime = 5f;
    public string playerTag = "Player";

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryKnockBackPlayer(collision.collider);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        TryKnockBackPlayer(other);
        Destroy(gameObject);
    }

    void TryKnockBackPlayer(Collider hitCollider)
    {
        Transform hitTransform = hitCollider.transform;
        if (!hitTransform.CompareTag(playerTag) && !hitTransform.root.CompareTag(playerTag))
        {
            return;
        }

        Rigidbody playerRigidbody = hitCollider.GetComponentInParent<Rigidbody>();
        if (playerRigidbody == null)
        {
            return;
        }

        Vector3 pushDirection = (playerRigidbody.position - transform.position).normalized;
        pushDirection.y = Mathf.Max(pushDirection.y, upwardLift);
        pushDirection.Normalize();

        playerRigidbody.AddForce(pushDirection * knockbackForce, ForceMode.Impulse);
    }
}
