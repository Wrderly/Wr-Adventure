using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject enemyBody;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = enemyBody.transform.position;
    }
}
