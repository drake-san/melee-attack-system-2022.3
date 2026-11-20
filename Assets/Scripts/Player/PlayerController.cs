using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rollSpeed;

    [Header("Ammo")] public GameObject bullet;

    public int ammoNumber;

    public bool isAttacking;

    [SerializeField] private bool isRolling;

    private Animator animator;

    private Vector2 bottomLeft;

    private Camera cam;
    private CapsuleCollider2D capsuleCollider;
    private GameManager gameManager;

    private Vector2 movement;

    private Vector2 position;

    private Rigidbody2D rb;
    private Vector2 topRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        gameManager = GameManager.instance;

        cam = Camera.main;

        isAttacking = false;
        isRolling = false;

        speed = 5.0f;
        rollSpeed = 10.0f;
        ammoNumber = 0;
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        bottomLeft = cam.ViewportToWorldPoint(Vector2.zero);
        topRight = cam.ViewportToWorldPoint(Vector2.one);

        moveAction.Enable();
        shootAction.Enable();
        exitAction.Enable();
        rollAction.Enable();
    }

    // Update is called once per frame
    private void Update()
    {
        movement = moveAction.ReadValue<Vector2>();

        animator.SetFloat("Speed", Mathf.Abs(movement.x) + Mathf.Abs(movement.y));

        if (movement.x < 0)
            transform.localScale = new Vector2(-1, 1);
        else if (movement.x > 0 && moveAction.IsPressed())
            transform.localScale = new Vector2(1, 1);

        if ((shootAction.WasPressedThisFrame() || Mouse.current.leftButton.wasPressedThisFrame) && !isAttacking &&
            !isRolling) Attack();

        if (rollAction.WasPressedThisFrame() && !isRolling && !isAttacking) Roll();

        if (exitAction.IsPressed())
            SceneManager.LoadScene(0);
    }

    private void FixedUpdate()
    {
        position = rb.position + speed * Time.fixedDeltaTime * movement;

        if (position.x < bottomLeft.x || position.x > topRight.x)
        {
        }
        else if (position.y < bottomLeft.y || position.y > topRight.y)
        {
        }
        else
        {
            rb.MovePosition(position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            if (isAttacking)
                DestroyEnemy(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            if (isAttacking)
                DestroyEnemy(collision);
    }

    private void Attack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        animator.SetBool("IsAttacking", true);
        animator.SetBool("IsRolling", false);
    }

    private void DestroyEnemy(Collision2D collision)
    {
        collision.gameObject.GetComponent<EnemyController>().enabled = false;
        collision.gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        collision.gameObject.GetComponent<Animator>().SetTrigger("Die");

        gameManager.enemyKilled++;
        gameManager.killsText.text = $"Kills: {gameManager.enemyKilled}";
    }

    private void DestroyEnemy(Collider2D collision)
    {
        collision.GetComponent<EnemyController>().enabled = false;
        collision.GetComponent<CapsuleCollider2D>().enabled = false;
        collision.GetComponent<Animator>().SetTrigger("Die");

        gameManager.enemyKilled++;
        gameManager.killsText.text = $"Kills: {gameManager.enemyKilled}";
    }

    private void Roll()
    {
        if (movement != Vector2.zero)
        {
            isRolling = true;
            animator.SetTrigger("Roll");
            animator.SetBool("IsRolling", true);
            animator.SetBool("IsAttacking", false);
            speed = rollSpeed;
        }
    }

    private void GameOver()
    {
        Destroy(gameObject);
        gameManager.enemyKilled = 0;
        gameManager.killsText.text = "";
        SceneManager.LoadScene("SelectScene");
    }

    private void StopAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    private void StopRolling()
    {
        isRolling = false;
        animator.SetBool("IsRolling", false);
        speed = 5.0f;
    }

    #region InputAction

    public InputAction moveAction;
    public InputAction shootAction;
    public InputAction exitAction;
    public InputAction rollAction;

    #endregion InputAction
}