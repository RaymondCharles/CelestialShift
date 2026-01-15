using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorOpen : MonoBehaviour
{
    public List<GameObject> objectives;
    public bool objectiveComplete = false;
    public Animator doorAnimator;


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
        }
    }
}
