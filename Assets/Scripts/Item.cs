using UnityEngine;

public class Item : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col) =>
        Destroy(gameObject);
}
