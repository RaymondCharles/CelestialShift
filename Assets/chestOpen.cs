using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chestOpen : MonoBehaviour
{
    public List<GameObject> objectives;
    public bool objectiveComplete = false;
    public Animator doorAnimator;

    public List<Item> items;
    public Transform spawn;

    public int lowerBoundItemQuantity = 0;
    public int upperBoundItemQuantity = 10;


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggering");
        if (objectives.Count == 0) objectiveComplete = true;
        else
        {
            for (int i=0; i< objectives.Count; i++)
            {
                if (objectives[i] != null)
                {
                    objectiveComplete = true;
                }
            }
        }
        if (other.gameObject.tag == "Player" && objectiveComplete)
        {
            Debug.Log("isopening");
            doorAnimator.SetBool("Open", true);
            for (int i=0; i<items.Count; i++)
            {
                spawn.rotation *= Quaternion.Euler(Random.Range(-180,180), Random.Range(-180, 180), Random.Range(0, 90));
                GameObject p = Instantiate(items[i].worldPrefab, spawn.position, spawn.rotation);
                p.GetComponent<ItemInstance>().quantity = Random.Range(lowerBoundItemQuantity, upperBoundItemQuantity);
                Rigidbody rb = p.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(spawn.forward, ForceMode.Impulse);
                }
            }
        }
    }
}
