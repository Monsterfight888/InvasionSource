using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed;

    public float airVelocity;

    public float counterMovement;

    public float maxSpeed;

    public float xInp;

    public float directionFacing;

    public bool isJumpingInp;

    public Transform groundCheck;

    private Camera cam;

    public float checkRaduis;

    public LayerMask ground;

    public float jumpForce;

    private Animator anim;

    public SpriteRenderer render;

    public SpriteRenderer handRender;

    public Sprite middle;

    public Sprite up;

    public Sprite down;

    public float fireRate;

    private float fireCooldown;

    public bool isFiring;

    private Vector2 initArmPos;

    public float lastDir;

    public LayerMask fireAbleMask;

    public SpriteRenderer testRender;

    public float range;

    public float grappleForce;

    // Vector2 debugStart;

    //private Vector2 debugEnd;

    public int health;

    public GameObject boltPref;

    public GameObject titan;

    public int kills;
    private void Awake()
    {
        Dictionary.player = this;

        Dictionary.groundMask = ground;
    }
    void Start()
    {
        fireCooldown = fireRate;

        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody2D>();

        cam = Camera.main;

        anim = GetComponent<Animator>();

        initArmPos = handRender.transform.localPosition;

        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, checkRaduis);

        //Gizmos.DrawLine(debugStart, debugEnd);
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRaduis, ground);

        anim.SetBool("isGrounded", isGrounded);

        float t_maxSpeed = maxSpeed;

        if (rb.velocity.magnitude <= t_maxSpeed)
        {
            rb.AddForce(Vector2.right * (xInp * speed));
        }

        if (!isGrounded)
        {
            //add velocity to facing direction air (regardless of max speed)
            rb.AddForce(Vector2.right * (directionFacing * airVelocity));
        }

        bool isNotInputingRunning = xInp <= 0.5f && xInp >= -0.5f;

        anim.SetBool("isRunning", !isNotInputingRunning);

        if (isNotInputingRunning && Mathf.Abs(rb.velocity.x) >= 1 && isGrounded)
        {
            rb.AddForce(Vector2.right * (-Mathf.Sign(rb.velocity.x) * counterMovement));
        }

        if(isJumpingInp && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce);
        }
        
        
        /*if (xInp <= 0.1f && rb.velocity.x < 0.05f || xInp >= -0.1f && rb.velocity.x < 0.05f)
            rb.AddForce(Vector2.right * (-direction * counterMovement));*/
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;

        if(health <= 0)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        anim.SetTrigger("Hurt");
    }

    void Update()
    {
        //testRender.gameObject.GetComponent<Material>().mainTextureScale = new Vector2(testRender.transform.localScale.x, testRender.transform.localScale.y);

        if (fireCooldown >= 0)
            fireCooldown -= Time.deltaTime;
        handRender.transform.localPosition = Vector2.Lerp(handRender.transform.localPosition, initArmPos, 0.1f);
        //look into whether getting raw feels good.
        xInp = Input.GetAxisRaw("Horizontal");
        //float yInp = Input.GetAxis("Vertical");


        
        isJumpingInp = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.V) && kills >= 5)
        {
            Vector3 t_dir = handRender.transform.right;

            if (directionFacing == -1)
            {
                t_dir = -handRender.transform.right;
            }

            RaycastHit2D hit = Physics2D.Raycast(handRender.transform.position, t_dir, Mathf.Infinity, ground);

            Vector2 spawnPos = hit.point;

            spawnPos.y += 20;

            Instantiate(titan, spawnPos, Quaternion.identity);

            //Debug.Log(spawnPos);

            kills -= 5;
        }

        isFiring = Input.GetMouseButton(0);


        /*(if (xInp >= 0.1f || xInp <= -0.1f)
            direction = xInp;*/

        Vector3 worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition) - cam.transform.position;


        directionFacing = Mathf.Sign(worldMousePos.x);

        if (directionFacing == 1 && directionFacing != lastDir)
        {
            render.flipX = false;
            
            handRender.transform.localPosition = new Vector2(0.03f, 0);

            initArmPos = handRender.transform.localPosition;

            handRender.flipX = false;
        }
        if(directionFacing == -1 && directionFacing != lastDir)
        {
            render.flipX = true;

            handRender.transform.localPosition = new Vector2(-0.03f, 0);

            initArmPos = handRender.transform.localPosition;

            handRender.flipX = true;
        }

        Vector3 cameraPoint = (worldMousePos + transform.position);

        cameraPoint.z = -10;

        cam.transform.position = cameraPoint;

        //2d lookat

        Vector3 dir = (worldMousePos + cam.transform.position) - handRender.transform.position;

        float angle = 0;
        if (directionFacing == 1)
        {
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
        }

        

        

        //Debug.Log(angle);

        handRender.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //270 350
        if(angle >= 15 && angle <= 350)
        {

            handRender.sprite = up;
        }
        else if(angle <= -15 && angle >= -90)
        {
            handRender.sprite = down;
        }
        else if (angle >= -15 && angle <= 15)
        {
            handRender.sprite = middle;
        }
        if(directionFacing == -1 && angle >= 15 && angle <= 90)
        {
            handRender.sprite = down;
        }
        //handRender.transform.eulerAngles = new Vector3(0, 0, handRender.transform.eulerAngles.z);

        if(fireCooldown <= 0 && isFiring)
        {
            Fire();
        }
        lastDir = directionFacing;
    }

    public void Fire()
    {
        GameObject thisObj = Instantiate(boltPref);

        SpriteRenderer thisRend = thisObj.GetComponent<SpriteRenderer>();

        fireCooldown = fireRate;

        Vector3 myPos = handRender.transform.right * (directionFacing * 0.03f);

        handRender.transform.localPosition -= myPos;

        Vector3 t_dir = handRender.transform.right;

        if (directionFacing == -1)
        {
            t_dir = -handRender.transform.right;
        }

        RaycastHit2D hit = Physics2D.Raycast(handRender.transform.position, t_dir, range, fireAbleMask);

        
        

        //RaycastHit2D debugHit = Physics2D.Raycast(handRender.transform.position, handRender.transform.right, Mathf.Infinity);

        if (hit.collider != null && hit.collider.GetComponent<Enemy>())
        {
            //debugStart = transform.position;

            //debugEnd = hit.point;
            //Debug.Log(hit.collider);
            Enemy t_enemy = hit.collider.gameObject.GetComponent<Enemy>();

            t_enemy.TakeDamage(1);
        }

        if (hit.collider != null && hit.collider.gameObject.layer == 8)
        {
            rb.AddForce(t_dir * grappleForce);
        }
            

       Vector3 to = new Vector3(handRender.transform.position.x, handRender.transform.position.y, 0.1f);
        Vector3 from = new Vector3(hit.point.x, hit.point.y, 0.1f);
        // calculate vector from point A to point B

        Vector3 lineVector = to - from;



        thisRend.transform.position = from + (lineVector / 2f);

        // set rotation of line instance so that it's Y axis is from point A to point B
        thisRend.transform.right = lineVector.normalized;

        // set length of line instance
        thisRend.size = new Vector2(lineVector.magnitude / 10, 0.12f);
        //testRender.transform.localScale = new Vector3(lineVector.magnitude, 10, testRender.transform.localScale.z);

        if (hit.collider == null)
        {
            //to = new Vector3(handRender.transform.position.x, handRender.transform.position.y, 0.1f);
            from = handRender.transform.position + (t_dir * range);

            from.z = 0.1f;
            // calculate vector from point A to point B

            lineVector = to - from;



            thisRend.transform.position = from + (lineVector / 2f);

            // set rotation of line instance so that it's Y axis is from point A to point B
            thisRend.transform.right = lineVector.normalized;

            // set length of line instance
            thisRend.size = new Vector2(lineVector.magnitude / 10, 0.12f);
        }

        Destroy(thisObj, 0.25f);

        //testRender.

    }
}
