using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourcePort : MonoBehaviour
{
    private GrabbableCable connectedCable = null;
    private FixedJoint cableJoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanConnectCable()
    {
        return connectedCable == null;
    }

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

    }
}
