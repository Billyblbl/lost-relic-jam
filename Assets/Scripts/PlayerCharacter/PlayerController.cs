using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;


    // Start is called before the first frame update
    private PlayerInputAction inputAction;
    private Rigidbody playerRigibody;
    private GrabbableCable grabbedCable = null;
    private FixedJoint cableJoint;

    private List<GrabbableCable> cablesAtRange = new List<GrabbableCable>();
    private List<RessourcePort> portsAtRange = new List<RessourcePort>();

    
    private void Awake()
    {
        playerRigibody = GetComponent<Rigidbody>();

        inputAction = new PlayerInputAction();
        inputAction.Enable();
        inputAction.Player.GrabCable.performed += HandleGrabAction;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var axisValue = inputAction.Player.Move.ReadValue<Vector2>();

        var displacement = axisValue * moveSpeed * Time.deltaTime;
        var newLoc = playerRigibody.position + new Vector3(displacement.x, 0, displacement.y);

        var dir = Vector3.Normalize(playerRigibody.position - newLoc) * -1;
        var newPLayerDir = Vector3.RotateTowards(transform.forward, dir, 10 * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newPLayerDir);

        playerRigibody.MovePosition(newLoc);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.red);

        // Debug.LogFormat("axis : {0}  displacement : {1}", axisValue, displacement);

    }

    public bool TryGrabCable(GrabbableCable cable)
    {
        if (grabbedCable != null  || cable.isGrabbed)
        {
            return false;
        }
        GrabCable(cable);
        return true;
    }

    private void HandleEnterCableTrigger(Collider other)
    {

        var cable = other.GetComponent<GrabbableCable>();
        if (cable == null)        
            return;

        if (!cablesAtRange.Find(x => x.name == other.name))
            cablesAtRange.Add(cable);
}

    private void HandleEnterPortTrigger(Collider other)
    {
        var port = other.GetComponent<RessourcePort>();

        if (port == null)
            return;
        if (!portsAtRange.Find(x => x.name == other.name))
            portsAtRange.Add(port);
    }

    private void HandleExitCableTrigger(Collider other)
    {

        var cable = other.GetComponent<GrabbableCable>();
        if (cable == null)
            return;

        if (cablesAtRange.Find(x => x.name == other.name))
            cablesAtRange.Remove(cable);
    }

    private void HandleExitPortTrigger(Collider other)
    {
        var port = other.GetComponent<RessourcePort>();

        if (port == null)
            return;
        if (portsAtRange.Find(x => x.name == other.name))
            portsAtRange.Remove(port);
    }

    private void OnTriggerExit(Collider other)
    {
        HandleExitCableTrigger(other);
        HandleExitPortTrigger(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleEnterCableTrigger(other);
        HandleEnterPortTrigger(other);
    }

    private void GrabCable(GrabbableCable cable)
    {
        grabbedCable = cable;
        grabbedCable.transform.position = gameObject.transform.TransformPoint(new Vector3(0, 0, -1));
        cableJoint = gameObject.AddComponent<FixedJoint>();
        cableJoint.connectedBody = cable.GetComponent<Rigidbody>();
        grabbedCable.OnGrab(gameObject);
        Debug.Log("Cable grabbed");
    }

    private bool HandlePortAction()
    {
        if (portsAtRange.Count == 0)
            return false;

        if (grabbedCable == null)
        {
            var filledPort = GetClosetFilledPortAtRange();
            if (filledPort == null) return false;
            var cable = filledPort.DisconectCable();
            if (cable == null) return false;
            GrabCable(cable);
            return true;
        }

        var port = GetClosetAvailablePortAtRangeOfType(grabbedCable.ressType);
        if (port == null)
            return false;

        var cableToConnect = grabbedCable;
        DropCable();
        port.ConnectCable(cableToConnect);
        return true;
    }

    private GrabbableCable GetClosetCableAtRange()
    {
        if (cablesAtRange.Count == 0) return null;

        GrabbableCable res = cablesAtRange[0];
        float distWithRes = Vector3.Distance(transform.position, res.transform.position);

        cablesAtRange.ForEach(it =>
        {
            var distWithIt = Vector3.Distance(transform.position, it.transform.position);
            if (distWithIt < distWithRes)
            {
                res = it;
                distWithRes = distWithIt;
            }
        });

        return res;
    }

    private RessourcePort GetClosetFilledPortAtRange()
    {
        if (portsAtRange.Count == 0) return null;

        RessourcePort res = null;
        float distWithRes = float.MaxValue;

        portsAtRange.ForEach(it =>
        {
            var distWithIt = Vector3.Distance(transform.position, it.transform.position);
            if (distWithIt < distWithRes && it.IsCableConnected)
            {
                res = it;
                distWithRes = distWithIt;
            }
        });

        return res;
    }


    private RessourcePort GetClosetAvailablePortAtRangeOfType(RessourcePort.RessourceType expRessType)
    {
        if (portsAtRange.Count == 0) return null;

        RessourcePort res = null;
        float distWithRes = float.MaxValue;

        portsAtRange.ForEach(it =>
        {
            var distWithIt = Vector3.Distance(transform.position, it.transform.position);
            if (distWithIt < distWithRes && it.ressType == expRessType && it.CanConnectCable)
            {
                res = it;
                distWithRes = distWithIt;
            }
        });

        return res;
    }

    private bool HandleCableAction()
    {
        if (grabbedCable != null)
        {
            DropCable();
            return true;
        }

        if (cablesAtRange.Count == 0)
            return false;

        return TryGrabCable(GetClosetCableAtRange()) ;
    }

    private void HandleGrabAction(InputAction.CallbackContext ctx)
    {
        if (HandlePortAction())
        {
            return;
        }
        HandleCableAction();
    }

    private void DropCable()
    {
        grabbedCable.OnDrop(gameObject);
        Destroy(cableJoint);
        grabbedCable = null;
        Debug.Log("Cable Dropped");
    }
}
