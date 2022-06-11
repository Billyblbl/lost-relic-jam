using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private CapsuleCollider grabRange;
    [SerializeField] private float grabCD = 0.2f; 


    // Start is called before the first frame update
    private PlayerInputAction inputAction;
    private Rigidbody playerRigibody;
    private GrabbableCable grabbedCable = null;
    private CapsuleCollider playerCollider;
    private FixedJoint cableJoint;
    private float lastGrab = 0f;

    private List<GrabbableCable> cablesAtRange = new List<GrabbableCable>();
    private List<RessourcePort> portsAtRange = new List<RessourcePort>();

    
    private void Awake()
    {
        playerRigibody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        inputAction = new PlayerInputAction();
        inputAction.Enable();
        inputAction.Player.GrabCable.performed += HandleGrabAction;
    }
    void Start()
    {
        
    }

    void TurnBaseOnDirection(Vector3 direction)
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
        lastGrab = Time.time;
        grabbedCable = cable;
        grabbedCable.transform.position = gameObject.transform.TransformPoint(new Vector3(0, 0, -1));
        cableJoint = gameObject.AddComponent<FixedJoint>();
        cableJoint.connectedBody = cable.GetComponent<Rigidbody>();
        grabbedCable.OnGrab(gameObject);
        Debug.Log("Cable grabbed");
    }

    private bool TryDropCable()
    {
        if (grabbedCable == null || grabCD + lastGrab > Time.time)
        {
            return false;
        }
        DropCable();
        return true;
    }

    private bool HandlePortAction()
    {
        if (portsAtRange.Count == 0)
            return false;

        if (grabbedCable == null)
        {
            var cable = portsAtRange[0].DisconectCable();
            if (cable != null) GrabCable(cable);
            return true;
        }

        if (!portsAtRange[0].CanConnectCable())
            return false;

        var cableToConnect = grabbedCable;
        DropCable();
        portsAtRange[0].ConnectCable(cableToConnect);
        return true;
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


        return TryGrabCable(cablesAtRange[0]); ;
    }

    private void HandleGrabAction(InputAction.CallbackContext ctx)
    {
        if (HandlePortAction()) return;
        HandleCableAction();
    }

    private void DropCable()
    {
        lastGrab = Time.time;
        grabbedCable.OnDrop(gameObject);
        Destroy(cableJoint);
        grabbedCable = null;
        Debug.Log("Cable Dropped");
    }
}
