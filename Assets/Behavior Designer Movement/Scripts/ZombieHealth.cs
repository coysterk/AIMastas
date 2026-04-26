using System.Collections;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    //Health Stats.
    public int maxHealth = 300;
    private int currentHealth;

    //Visual Effects.
    public GameObject bloodSprayPrefab;
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.15f;
    public AudioClip hurtSound;
    public AudioClip[] deathSounds; //Array of sounds.
    private AudioSource audioSource;

    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>(); //Automaticly grabs the SpriteRenderer if you forgot to drag it in.
        originalColor = spriteRenderer.color; //Save the default color so we can return to it after flashing.

        audioSource = GetComponent<AudioSource>(); //Grabs the speaker component.
    }

    public void TakeDamage(int amount, Vector3 damageSourcePosition)
    {
        currentHealth -= amount;
        StartCoroutine(FlashRed()); //Trigger the red flash.

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound); //Plays sound of Zombie getting hurt.
        }

        if (bloodSprayPrefab != null) //Trigger the directional blood spray.
        {
            //Calculate the angle from the bullet/explosion to the zombie.
            Vector3 direction = transform.position - damageSourcePosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion bloodRotation = Quaternion.Euler(0, 0, angle - 90f); //Direction of the spray

            Instantiate(bloodSprayPrefab, transform.position, bloodRotation);
        }

        if (currentHealth <= 0)
        {
            int randomIndex = Random.Range(0, deathSounds.Length);
            AudioClip chosenSound = deathSounds[randomIndex]; //Picks death sound from array.

            if (chosenSound != null)
            {
                AudioSource.PlayClipAtPoint(chosenSound, transform.position); //Plays death sound.
            }

            Destroy(gameObject); //Deletes the zombie when it dies.
        }
    }
    IEnumerator FlashRed() //A Coroutine that runs alongside the main game loop to handle the timer.
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}