using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [SerializeField] private Portal connectingPortal;
    [SerializeField] public Transform drop;
    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var o = other.gameObject;
        if (o.TryGetComponent(out Brain _))
        {
            o.transform.position = connectingPortal.drop.position;
            o.transform.rotation = connectingPortal.drop.rotation;
        }
        
    }
}
