using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomTerrainGenerator : MonoBehaviour
{
    public GameObject repeatable;

    
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(repeatable, this.transform);
    }

}
