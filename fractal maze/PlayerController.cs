using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float detectionRadius = 0.1f; // adjustable in editor
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastValidPosition;
    private FractalTreeGenerator treeGenerator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // ensure gravity = 0
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        StartCoroutine(InitializePlayerPosition());
        treeGenerator = FindObjectOfType<FractalTreeGenerator>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        movement = new Vector2(moveHorizontal, moveVertical);
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb.position + movement * speed * Time.fixedDeltaTime;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(newPosition, detectionRadius, Vector2.zero, 0, LayerMask.GetMask("Pathway", "BlockedBranch"));

        bool blocked = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("BlockedBranch"))
            {
                blocked = true;
                break;
            }
        }

        if (!blocked)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Pathway"))
                {
                    lastValidPosition = newPosition; // last pathway = last valid position
                    rb.position = newPosition;
                    break;
                }
            }
        }
        else
        {
            rb.position = lastValidPosition;
            rb.velocity = Vector2.zero; 
        }

        if (rb.position.y <= treeGenerator.GetBottomThreshold())
        {
            treeGenerator.RegenerateTree(true); // bottom condition
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BlockedBranch"))
        {
            rb.velocity = Vector2.zero; // zero
            rb.position = lastValidPosition; // GO BACK to last valid position
        }
    }

    IEnumerator InitializePlayerPosition()
    {
        yield return new WaitForSeconds(0.1f); 
        Transform startTransform = FindHighestPathway();
        if (startTransform != null)
        {
            transform.position = startTransform.position;
            lastValidPosition = startTransform.position; // find last valid position to teleport to
            Debug.Log("Player starts at: " + startTransform.position);
        }
        else
        {
            Debug.LogError("No starting pathway found!");
        }
    }

    Transform FindHighestPathway()
    {
        GameObject[] pathways = GameObject.FindGameObjectsWithTag("Pathway");
        Transform highestPathway = null;
        float highestY = float.MinValue;

        foreach (GameObject pathway in pathways)
        {
            if (pathway.transform.position.y > highestY)
            {
                highestY = pathway.transform.position.y;
                highestPathway = pathway.transform;
            }
        }

        return highestPathway;
    }

    void OnDrawGizmos()
    {
        // debugging
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
