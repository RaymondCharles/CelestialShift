using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mineableBehaviour : MonoBehaviour
{
    public GameObject materialToSpawn;
    public Transform stoneSpawn;
    private int HP = 30;

    public void SpawnMaterials(int swordDamage)
    {
        Debug.Log("Mining stone with " + swordDamage + " sword damage.");
        int numToSpawn = Mathf.Min((HP / 10), (swordDamage / 10));
        for (int i=0; i< numToSpawn; i++)
        {
            stoneSpawn.rotation *= Quaternion.Euler(Random.Range(-180,180), Random.Range(-180, 180), Random.Range(0, 90));
            GameObject p = Instantiate(materialToSpawn, stoneSpawn.position, stoneSpawn.rotation);
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(stoneSpawn.forward, ForceMode.Impulse);
            }
        }
        HP -= swordDamage;
        CheckHP();
    }

    private void CheckHP()
    {
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
