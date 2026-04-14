using UnityEngine;

public class MachineGunProjectile : MonoBehaviour
{
    //Projectile Stats
    public float speed = 20f;
    public int damage = 10;
    public float lifetime = 2f;
    public string enemyTag = "Zombie";

    void Start()
    {
        Destroy(gameObject, lifetime); //Starts a self destruct countdown the moment the bullet is spawned.
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime); //Keeps the projectile moving in the same direcition.
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag(enemyTag)) //If arrow made contact with zombie
        {
            //ZombieHealth health = hitInfo.GetComponent<ZombieHealth>();
            //if (health != null)
            //{
            //    health.TakeDamage(damage);
            //}

            Destroy(gameObject); //Deletes bullet
        }
    }
}