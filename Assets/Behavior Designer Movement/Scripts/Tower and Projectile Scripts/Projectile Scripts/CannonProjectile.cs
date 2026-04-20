using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    //Projectile Stats"
    public float speed = 10f;
    public float explosionRadius = 2f;
    public int explosionDamage = 50;
    public float lifetime = 3f;
    public string enemyTag = "Zombie";
    public GameObject explosionEffect; // Explosion effect.

    void Start()
    {
        Destroy(gameObject, lifetime); //Starts a self destruct countdown the moment the cannon ball is spawned.
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime); //Keeps the projectile moving in the same direcition.
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag(enemyTag))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius); //All zombie hitboxes in explosion range.

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(enemyTag)) //If arrow made contact with zombie
                {
                    //ZombieHealth healthScript = collider.GetComponent<ZombieHealth>();
                    //if (healthScript != null)
                    //{
                    //   healthScript.TakeDamage(explosionDamage);
                    //}
                }
            }
            if (explosionEffect != null) //Spawn the explosion effect exactly where the bullet currently is.
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject); //Deletes cannon ball.
        }
    }

    void OnDrawGizmosSelected() //Draws the radius of the explosion.
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}