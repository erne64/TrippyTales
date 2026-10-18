using UnityEngine;

// Questo script rappresenta un oggetto distruttibile nel gioco.
// L'oggetto può ricevere danni e viene distrutto quando la sua salute raggiunge zero.
public class ObjectToHit : MonoBehaviour
{
    public float ObjectHealth = 50000f; // Quantità di salute totale dell’oggetto

    // Applica danno all’oggetto
    public void ObjectHItDamage(float damage)
    {
        ObjectHealth -= damage; // Sottrae i punti vita in base al danno ricevuto

        // Se la salute è esaurita, distrugge l’oggetto
        if (ObjectHealth <= 0f)
        {
            Die();
        }
    }

    // Alias con ortografia corretta
    public void ObjectHitDamage(float damage)
    {
        ObjectHItDamage(damage);
    }

    // Distrugge l’oggetto dalla scena
    void Die()
    {
        Destroy(gameObject);
    }
}
