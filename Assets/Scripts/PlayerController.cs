using System.Collections;
using UnityEngine;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 10f;

    [SerializeField] private Transform weaponParent;
    [SerializeField] private float stepRate;
    [SerializeField] private AudioSource footstepSource;

    public float dashDistance = 10f;
    private float dashDuration = 0.1f;
    public float dashCooldown = 5f;
    private bool isDashing;
    private bool canDash = true;

    private Vector2 movement;
    private Vector2 lastMove;
    private Vector3 mousePos;
    private float stepCoolDown;

    private Rigidbody2D rb;
    private Animator animator;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private PlayerHUD hud;
    private Camera cam;
    private WeaponManager weaponManager;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hud = GetComponent<PlayerHUD>();
        weaponManager = GetComponent<WeaponManager>();
        lastMove = new Vector2(1f, 0f);
        cam = Camera.main;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Pause.paused || player.IsDead) return;

        //input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        if (movement != Vector2.zero)
            lastMove = movement;

        //dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(Dash(lastMove));   
            
        //animations
        if(movement != Vector2.zero)
            animator.SetBool("Move", true);     
        else
            animator.SetBool("Move", false);

        //player flip
        if (movement.x < 0)
        {
            spriteRenderer.flipX = true;
            ServerFlipPlayerSprite(true);
        }    
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false;
            ServerFlipPlayerSprite(false);
        }

        float angle = 0;
        if (!player.autoAim)
        {
            //weapon look at mouse position
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDirection = (mousePos - weaponParent.position).normalized;
            angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponParent.eulerAngles = new Vector3(0, 0, angle);
        }
        else if (weaponManager.ClosestEnemy)
        {
            Vector2 aimDirection = (weaponManager.ClosestEnemy.transform.position - weaponParent.position).normalized;
            angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponParent.eulerAngles = new Vector3(0, 0, angle);
        }

        //weapon flip
        if (angle >= -90 && angle <= 90)
            weaponParent.rotation *= Quaternion.Euler(0, 0, 0);
        else
            weaponParent.rotation *= Quaternion.Euler(180, 0, 0);

        //footsteps
        stepCoolDown -= Time.deltaTime;

        if (movement != Vector2.zero && stepCoolDown < 0f)
        {
            PlayFootStepAudio();
            stepCoolDown = stepRate;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!isDashing)
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement);
    }

    private IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        SoundManager.Instance.Play("Dash");
        rb.AddForce(direction * dashDistance, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashDuration);
        hud.StartCoroutine(hud.StaminaRestore(dashCooldown));
        isDashing = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    [ObserversRpc]
    private void RpcFlipPlayerSprite(bool status)
    {
        spriteRenderer.flipX = status;
    }

    [ServerRpc]
    private void ServerFlipPlayerSprite(bool status)
    {
        RpcFlipPlayerSprite(status);
    }

    private void PlayFootStepAudio()
    {
        footstepSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
        footstepSource.Play();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            collision.rigidbody.velocity = Vector3.zero;
            collision.otherRigidbody.velocity = Vector3.zero;
        }   
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((isDashing || player.invincible) && collision.gameObject.tag == "Enemy")
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((isDashing || player.invincible) && collision.gameObject.tag == "Enemy")
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, false);
        }
    }
}