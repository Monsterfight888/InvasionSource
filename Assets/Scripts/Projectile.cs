using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;

    public int team;
    void Start()
    {
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        LayerMask df = LayerMask.GetMask("Player");

        

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, speed);

        if(hit.collider != null && hit.collider.gameObject.GetComponent<Player>())
        {
            hit.collider.gameObject.GetComponent<Player>().TakeDamage(1);
            Destroy(gameObject);
        }
        else if(hit.collider != null && hit.collider.gameObject.GetComponent<Enemy>() 
            && hit.collider.gameObject.GetComponent<Enemy>().team != team)
        {
            hit.collider.gameObject.GetComponent<Enemy>().TakeDamage(1);
            Destroy(gameObject);
        }

        transform.position += -transform.right * speed;
    }
}
