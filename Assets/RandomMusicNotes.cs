using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMusicNotes : MonoBehaviour
{
    public List<GameObject> notes = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<HostileAI>().projectilePrefab = notes[0];
    }

    // Update is called once per frame
    void Update()
    {
        int index = Random.Range(0, notes.Count);
        this.GetComponent<HostileAI>().projectilePrefab = notes[index];
    }
}
