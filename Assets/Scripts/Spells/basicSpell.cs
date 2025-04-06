using UnityEngine;

public class basicSpell : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 25f;
 
    private Vector3 startPos;
 
    private float range = 25f;

    void Start() {
        startPos = transform.position;
    }
    
    void Update()
    {
        // move forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        CheckRange();
    }

    void CheckRange() {
        if(Vector3.Distance(startPos, transform.position) >= range) {
            Destroy(gameObject);
        }
    }

    // if spell hits something
    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag != "Player") {
            Debug.Log("Uch" + other.gameObject.name);
            Destroy(gameObject);
        }
    }
}