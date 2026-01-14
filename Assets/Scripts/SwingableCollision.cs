using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingableCollision : MonoBehaviour
{
    public bool midSwing = false;
    public int swordDamage = 0;
    public bool hasHit = false;
    public Transform edge;
    public ParticleSystem sparks;

    void OnTriggerStay(Collider other)
    {
        /*if (other.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;
            if (!canAttack && !enemiesHit.Contains(enemy))
            {
                enemiesHit.Add(enemy);
                enemy.GetComponent<EnemyHealth>().TakeDamage(swordDamage);
            }
        }*/
        Debug.Log(midSwing + "MID SWING");
        if (other.CompareTag("Mineable"))
        {
            GameObject envObject = other.gameObject;
            if (midSwing && !hasHit)
            {
                sparks.Emit(5);
                hasHit = true;
                Debug.Log("GONNA SPAWN");
                envObject.GetComponent<mineableBehaviour>().SpawnMaterials((int)swordDamage, edge);
            }
        }
    }
    public void ResetHit()
    {
        hasHit = false;
    }

}
