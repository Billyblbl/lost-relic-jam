using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourcePort : MonoBehaviour
{
    public enum RessourceType { COOLANT, ENERGY, FUEL }

    private GrabbableCable connectedCable = null;
    private FixedJoint cableJoint;

    [SerializeField] public RessourceType ressType = RessourceType.ENERGY;
    [SerializeField] private float ejectForce = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, gameObject.transform.TransformPoint(new Vector3(1, 0, 0)), Color.red);
    }

    public bool IsCableConnected => connectedCable != null;

    public bool CanConnectCable => connectedCable == null;

    public void ConnectCable(GrabbableCable cable)
    {
        Debug.Log("connecting cable !"); 
        connectedCable = cable;
        connectedCable.transform.position = gameObject.transform.TransformPoint(new Vector3(1, 0, 0));
        cableJoint = gameObject.AddComponent<FixedJoint>();
        cableJoint.connectedBody = cable.GetComponent<Rigidbody>();
        connectedCable.OnGrab(gameObject);
    }

    public GrabbableCable DisconectCable()
    {
        if (connectedCable == null)
        {
            return null;
        }
        Debug.Log("disconnecting cable !");
        var disconectedCable = connectedCable;
        connectedCable.OnDrop(gameObject);
        Destroy(cableJoint);
        connectedCable = null;

        return disconectedCable;
    }

    public void EjectCable()
    {
        var cable = DisconectCable();
        var ejectDir = Vector3.Normalize(transform.position - gameObject.transform.TransformPoint(new Vector3(1, 0, 0))) * -1;
        

        cable.GetComponent<Rigidbody>().AddForce(ejectDir * ejectForce, ForceMode.Impulse);
    }
}
