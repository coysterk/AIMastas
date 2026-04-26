using UnityEngine;

public class BallistaProjectile : MonoBehaviour
{
    //Projectile Stats
    public float speed = 15f;
    public int damage = 35;
    public float lifetime = 3f;
    public string enemyTag = "Zombie";

    void Start()
    {
        Destroy(gameObject, lifetime); //Starts a self destruct countdown the moment the arrow is spawned.
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime); //Keeps the projectile moving in the same direcition.
    }
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag(enemyTag)) //If arrow made contact with zombie
        {
            ZombieHealth healthScript = hitInfo.GetComponent<ZombieHealth>();
            if (healthScript != null)
            {
                healthScript.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject); //Delete arrow.
        }
    }
}