using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mineableBehaviour : MonoBehaviour
{
    public GameObject materialToSpawn;
    public Transform spawn;
    private int HP = 30;
    private bool canBeHit = false;

    public void SpawnMaterials(int swordDamage, Transform pickaxeEnd)
    {
        Debug.Log("Mining stone with " + swordDamage + " sword damage.");
        int numToSpawn = Mathf.Min((HP / 10), (swordDamage / 10));
        for (int i=0; i< numToSpawn; i++)
        {
            spawn.rotation *= Quaternion.Euler(Random.Range(-180,180), Random.Range(-180, 180), Random.Range(0, 90));
            Vector3 oldPosition = spawn.position;
            spawn.position = pickaxeEnd.position;
            GameObject p = Instantiate(materialToSpawn, spawn.position, spawn.rotation);
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(spawn.forward, ForceMode.Impulse);
            }
            spawn.position = oldPosition;
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
