using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject pref;

    public float time;

    public List<Enemy> enemies = new List<Enemy>();

    public List <GameObject> currentFocuses = new List<GameObject>();

    Transform longHopPos1;
    Transform longHopPos2;
    Transform longHopPos3;
    Transform longHopPos4;
    //public bool shouldStorm;
    void Awake()
    {
        currentFocuses.Add(Dictionary.player.gameObject);

        Dictionary.enemyManager = this;

        StartCoroutine(spawn());
    }

    public void AddFocus(GameObject newEnemy)
    {
        currentFocuses.Add(newEnemy);

        //make sure no nulls got into the list

        for (int i = 0; i < currentFocuses.Count; i++)
        {
            if (currentFocuses[i] == null)
                currentFocuses.RemoveAt(i);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            
            enemies[i].currentAttacking = newEnemy;
        }
    }

    IEnumerator spawn()
    {
        yield return new WaitForSeconds(time);

        enemies.Add(Instantiate(pref, transform.position, Quaternion.identity).GetComponent<Enemy>());

        

        

        StartCoroutine(spawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
