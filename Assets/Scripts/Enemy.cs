using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;

    public GameObject currentAttacking;

    private Rigidbody2D rb;

    public float coolDown;

    private float timer;

    public bool vegetableMode = false;

    public int damage = 1;

    //1 is enemy
    //0 is friend

    public int team = 1;

    public enum AiType
    {
        
        Melee,
        Projectile

    }

    public enum AIJob
    {
        WaitingForJob,
        Stupid,
        Buddy,
        LongHopSquad
    }
    public AIJob thisJob;
    public AiType thisType;
    public int health;

    public float distanceBeforeAttack;

    public float checkRaduis;

    public Vector3 checkPos;

    public bool lastIsGrounded;

    public float jumpVel;

    public float maxSpeed;

    public GameObject projectile;

    private Animator anim;

    private SpriteRenderer render;

    public int dir;

    public float flySpeed;



    

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(checkPos + transform.position, checkRaduis);
    }
    private void Start()
    {
        distanceBeforeAttack += Random.Range(0, 2f);

        render = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        if(team == 1)
        {
            currentAttacking = NearestEnemy();
        }
        if(team == 0)
        { 
            currentAttacking = NearestEnemy();
            Dictionary.enemyManager.AddFocus(gameObject);
        }

        anim = GetComponent<Animator>();
    }

    

    public GameObject NearestEnemy()
    {
        if(team == 0)
        {
            GameObject nearest = null;

            for (int i = 0; i < Dictionary.enemyManager.enemies.Count; i++)
            {

                if (nearest == null || Vector2.Distance(transform.position, nearest.transform.position)
                    >= Vector2.Distance(transform.position, Dictionary.enemyManager.enemies[i].transform.position))
                    nearest = Dictionary.enemyManager.enemies[i].gameObject;
            }
            return nearest;
        }
        else
        {
            GameObject nearest = null;

            for (int i = 0; i < Dictionary.enemyManager.currentFocuses.Count; i++)
            {
                if (nearest == null || Vector2.Distance(transform.position, nearest.transform.position)
                    >= Vector2.Distance(transform.position, Dictionary.enemyManager.currentFocuses[i].transform.position))
                    nearest = Dictionary.enemyManager.currentFocuses[i].gameObject;
            }
            return nearest;
        }
        
        
    }
    public void TakeDamage(int dmg)
    {
        if (vegetableMode || !lastIsGrounded)
            return;

        health -= dmg;



        if (health <= 0)
        {
            

            if(anim != null)
            {
                Destroy(gameObject, 10f);

                anim.SetBool("isGrounded", true);

                vegetableMode = true;

                anim.SetTrigger("Death");

                anim.SetFloat("DeathAnimationChosen", Random.Range(0, 5));

                rb.bodyType = RigidbodyType2D.Static;

                Destroy(GetComponent<BoxCollider2D>());

                if(team == 1)
                {
                    Dictionary.enemyManager.enemies.Remove(this);

                    Dictionary.player.kills++;
                }
                else
                {
                    Dictionary.enemyManager.currentFocuses.Remove(gameObject);
                }
                    
            }
            else
            {
                Destroy(gameObject);
            }
            


            
            
        }
    }

    public void Attack()
    {
        if(thisType == AiType.Projectile)
        {
            //Object we attack is right infront of us

            Quaternion rotateDir = Quaternion.LookRotation(currentAttacking.transform.position - transform.position);

            Vector3 t_dir = currentAttacking.transform.position - transform.position;

            float angle = Mathf.Atan2(t_dir.y, t_dir.x) * Mathf.Rad2Deg + 180;

            Projectile t_proj = Instantiate(projectile, transform.position, Quaternion.AngleAxis(angle, Vector3.forward)).GetComponent<Projectile>();

            t_proj.team = team;

            timer = coolDown;

            anim.SetTrigger("Fire");
        }
        else if(thisType == AiType.Melee)
        {
            
            if(currentAttacking.GetComponent<Enemy>().team != team)
            {
                currentAttacking.GetComponent<Enemy>().TakeDamage(damage);

                anim.SetTrigger("Fire");

                timer = coolDown;
            }

            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= -50)
            TakeDamage(100);

        currentAttacking = NearestEnemy();

        if (currentAttacking == null /*|| team == 0 && currentAttacking.GetComponent<Enemy>() && !Dictionary.enemyManager.enemies.Contains(currentAttacking.GetComponent<Enemy>())*/)
        {
            
            //currentAttacking = NearestEnemy();
            return;
        }



        if (thisJob == AIJob.WaitingForJob || vegetableMode || currentAttacking == null)
            return;

        if (timer >= 0)
            timer -= Time.deltaTime;

        bool isgrounded = Physics2D.OverlapCircle(checkPos + transform.position, checkRaduis, Dictionary.groundMask);

        if (!isgrounded && lastIsGrounded)
        {
            rb.AddForce(transform.up * jumpVel);
        }
        else if(isgrounded && !lastIsGrounded && thisType == AiType.Melee)
        {
            //damage on fall
        }

        anim.SetBool("isGrounded", isgrounded);

        if (!isgrounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, Dictionary.groundMask);
            if (hit.collider != null)
            {

                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.AddForce(new Vector2(dir * flySpeed, 0));
            }
        }

        if (Mathf.Abs(transform.position.x - currentAttacking.transform.position.x) <= distanceBeforeAttack && isgrounded)
        {
            anim.SetBool("isRunning", false);

            if (timer <= 0 && isgrounded)
            {

                Attack();


            }
            
        }
        else
        {

            anim.SetBool("isRunning", true);
            if(isgrounded)
                dir = Mathf.RoundToInt(Mathf.Sign(rb.velocity.x));

            if (Mathf.Abs(rb.velocity.x) <= maxSpeed)
            {
                rb.AddForce(new Vector2(-Mathf.Sign(transform.position.x - currentAttacking.transform.position.x) * speed, 0));
            }

            if(Mathf.Sign(transform.position.x - currentAttacking.transform.position.x) == -1)
            {
                if (rb.velocity != Vector2.zero)
                {
                    render.flipX = false;
                }
                
            }
            else
            {
                if (rb.velocity != Vector2.zero)
                {
                    render.flipX = true;
                }
            }

            
        }
        lastIsGrounded = isgrounded;
    }
}
