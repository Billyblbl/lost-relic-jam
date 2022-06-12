using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableCable : MonoBehaviour
{
    [SerializeField] private Collider managedCollider;
    [SerializeField] public RessourcePort.RessourceType ressType;


    public bool isGrabbed = false;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGrab(GameObject grabber)
    {
        managedCollider.enabled = false;
        isGrabbed = true;
    }

    public void OnDrop(GameObject grabber)
    {
        managedCollider.enabled = true;
        isGrabbed = false;
    }
}
