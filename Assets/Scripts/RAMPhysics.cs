
using UnityEngine;

public class RAMPhysics : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(
            "RAM Collision: зіткнення з " +
            collision.gameObject.name
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "InstallZone")
        {
            Debug.Log(
                "RAM Trigger: модуль увійшов у зону встановлення!"
            );
        }
    }
}
