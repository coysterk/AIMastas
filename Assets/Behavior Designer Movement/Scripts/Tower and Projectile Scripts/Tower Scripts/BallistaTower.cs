using UnityEngine;

public class BallistaTower : MonoBehaviour
{
    //Tower Stats
    public float targetRange = 5f;
    public float fireRate = 0.75f;
    public float fireCooldown = 0f;

    //Setup Fields
    public string enemyTag = "Zombie"; //Target entites with tag "Zombie".
    public GameObject projectilePrefab; //To select the projectile to use.
    public Transform firePoint; //Select the point from which to fire from.
    public AudioClip shootSound;
    private AudioSource audioSource;

    private Transform target;

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); //Grab the speaker attached to the prefab.
    }

    void Update()
    {
        UpdateTarget();

        if (target != null) //If there is a zombie.
        {
            //Math to calculate the angle between the tower and the target
            Vector3 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); //Points in the direction of zombie.

            if (fireCooldown <= 0f) //If cooldown is 0.
            {
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation); //Spawn the projectile matching the tower's current rotation

                if (shootSound != null)
                {
                    audioSource.PlayOneShot(shootSound); //Play the sound.
                }

                fireCooldown = 1f / fireRate;
            }
        }

        fireCooldown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, targetRange); //All zombie hitboxes in range.

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D collider in colliders) //Check each hitbox.
        {
            if (collider.CompareTag(enemyTag))
            {
                float distanceToEnemy = Vector2.Distance(transform.position, collider.transform.position); //Checks the distance.
                if (distanceToEnemy < shortestDistance) //Compares distance and selects the shortest.
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = collider.transform;
                }
            }
        }

        target = nearestEnemy;
    }


    void OnDrawGizmosSelected() //Draws the radius of detection
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}