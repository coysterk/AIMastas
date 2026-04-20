using UnityEngine;

public class MachineGunTower : MonoBehaviour
{
    //Tower Stats
    public float targetRange = 6f;
    public float fireRate = 5f;
    public float fireCooldown = 0f;

    //Dual Barrel Setup
    public Transform firePointLeft;
    public Transform firePointRight;
    private bool useLeftBarrel = true; //Boolean to flip between left and right barrel.

    //Setup Fields
    public string enemyTag = "Zombie";
    public GameObject projectilePrefab;
    public ParticleSystem flashLeft; //Muzzle flash for left barrel.
    public ParticleSystem flashRight; //Muzzle flash for right barrel.

    private Transform target;

    void Update()
    {
        UpdateTarget();

        if (target != null) //If there is a zombie.
        {
            //Math to calculate the angle between the tower and the target
            Vector3 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); //Points in the direction of zombie.

            if (fireCooldown <= 0f)
            {
                Transform activeBarre = useLeftBarrel ? firePointLeft : firePointRight; //Pick the barrel based on the toggle.
                Instantiate(projectilePrefab, activeBarre.position, activeBarre.rotation); //Spawn the bullet at the chosen barrel.
                if (useLeftBarrel && flashLeft != null) //Plays left or right muzzle flash.
                {
                    flashLeft.Play();
                }
                else if (!useLeftBarrel && flashRight != null)
                {
                    flashRight.Play();
                }
                useLeftBarrel = !useLeftBarrel; //Flip the toggle for the next shot.
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


    void OnDrawGizmosSelected() //Draws the radius of detection.
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}