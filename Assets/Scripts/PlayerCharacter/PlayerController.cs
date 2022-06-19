using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable

public class PlayerController : MonoBehaviour {
	[SerializeField] private float moveSpeed = 1.0f;

	public InputActionReference? move;
	public InputActionReference? grab;

	private Rigidbody? playerRigibody;
	private Grabbable? grabbedCable;
	private FixedJoint? cableJoint;

	private List<Grabbable> cablesAtRange = new List<Grabbable>();
	private List<RessourcePort> portsAtRange = new List<RessourcePort>();

	public float turnSpeed = 10f;

	private float cableStartTime = 0f;
	public float cableConnectTime = 0.5f;
	public float cableConnectSpeed = 20f;
	public float cableConnectDistance = 0.1f;
	public Vector3 cableLocalConnectionPoint = new Vector3(0, 0, -1);


	private void Awake() {
		playerRigibody = GetComponent<Rigidbody>();
		// grab!.action.performed += HandleGrabAction;
		//cableJoint = GetComponent<FixedJoint>();
		grab!.action.performed += HandleGrabAction;
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.TransformPoint(cableLocalConnectionPoint), 0.2f);
	}

	void FixedUpdate() {
		UpdatePlayerLocation();
		UpdateCableConnection();
	}

    private Vector3 cableConnectionPoint => gameObject.transform.TransformPoint(cableLocalConnectionPoint);

	private void UpdatePlayerLocation() {

		//if (grab!.action.triggered) HandleGrabAction();
		var axisValue = move!.action.ReadValue<Vector2>();

		var displacement = axisValue * moveSpeed;
		var newLoc = playerRigibody!.position + new Vector3(displacement.x, 0, displacement.y);
		var prevVel = Vector3.Scale(playerRigibody.velocity, Vector3.up);
		playerRigibody.velocity = new Vector3(displacement.x, 0, displacement.y) + prevVel;

		var dir = -Vector3.Normalize(playerRigibody.position - newLoc);

		if (axisValue.magnitude > float.Epsilon) {

			var newAngularVel = Vector3.up * Mathf.Clamp(Vector3.SignedAngle(transform.forward, dir, transform.up), -turnSpeed, turnSpeed);
			// Debug.LogFormat("New angular Velocity = {0}", newAngularVel);

			playerRigibody.angularVelocity = newAngularVel;
		} else {
			playerRigibody.angularVelocity = Vector3.zero;
		}
		// var newPLayerDir = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0.0f);
		// transform.rotation = Quaternion.LookRotation(newPLayerDir);

		Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.red);
	}

	private void UpdateCableConnection() {
		if (grabbedCable == null || cableJoint != null)
			return;

		var cableRigibody = grabbedCable.GetComponent<Rigidbody>();
		var cableDistance = Vector3.Distance(grabbedCable.transform.position, cableConnectionPoint);
		if (cableDistance < cableConnectDistance)
		{
			cableJoint = gameObject.AddComponent<FixedJoint>();
			cableJoint.connectedBody = cableRigibody;
			return;
		}

		var nextPos = Vector3.Lerp(grabbedCable.transform.position, cableConnectionPoint, (Time.time - cableStartTime) / cableConnectTime);
		var nextVel = Vector3.Normalize(nextPos - cableRigibody.position);
		cableRigibody.velocity = nextVel * cableConnectSpeed;
		
	}
	public bool TryGrabCable(Grabbable cable) {
		if (grabbedCable != null || cable.isGrabbed) return false;
		GrabCable(cable);
		return true;
	}

	void HandleEnterTrigger<T>(Collider other, List<T> container) where T : Component {
		var comp = other.GetComponent<T>();
		if (comp == null) return;
		if (!container.Contains(comp)) container.Add(comp);
	}

	void HandleExitTrigger<T>(Collider other, List<T> container) where T : Component {
		var comp = other.GetComponent<T>();
		if (comp == null) return;
		if (container.Contains(comp)) container.Remove(comp);
	}

	private void OnTriggerExit(Collider other) {
		HandleExitTrigger<Grabbable>(other, cablesAtRange);
		HandleExitTrigger<RessourcePort>(other, portsAtRange);
	}

	private void OnTriggerEnter(Collider other) {
		HandleEnterTrigger<Grabbable>(other, cablesAtRange);
		HandleEnterTrigger<RessourcePort>(other, portsAtRange);
	}

	private void GrabCable(Grabbable cable) {
		grabbedCable = cable;

		grabbedCable.OnGrab(gameObject);
		cablesAtRange.Remove(grabbedCable);
		cableStartTime = Time.time;

		grabbedCable.GetComponent<Rigidbody>().position = cableConnectionPoint;

		Debug.Log("Cable grabbed");
	}

	private bool HandlePortAction() {
		if (portsAtRange.Count == 0)
			return false;

		if (grabbedCable == null) {
			var filledPort = GetClosetFilledPortAtRange();
			if (filledPort == null) return false;
			var cable = filledPort.DisconectCable();
			if (cable == null) return false;
			GrabCable(cable);
			return true;
		}

		if (grabbedCable.ressType == null) return false;
		var port = GetClosetAvailablePortAtRangeOfType(grabbedCable.ressType);
		if (port == null)
			return false;

		var cableToConnect = grabbedCable;
		DropCable();
		port.ConnectCable(cableToConnect);
		return true;
	}

	private Grabbable? GetClosetCableAtRange() {
		if (cablesAtRange.Count == 0) return null;

		Grabbable res = cablesAtRange[0];
		float distWithRes = Vector3.Distance(transform.position, res.transform.position);

		cablesAtRange.ForEach(it => {
			var distWithIt = Vector3.Distance(transform.position, it.transform.position);
			if (distWithIt < distWithRes) {
				res = it;
				distWithRes = distWithIt;
			}
		});

		return res;
	}

	private RessourcePort? GetClosetFilledPortAtRange() {
		if (portsAtRange.Count == 0) return null;

		RessourcePort? res = null;
		float distWithRes = float.MaxValue;

		portsAtRange.ForEach(it => {
			var distWithIt = Vector3.Distance(transform.position, it.transform.position);
			if (distWithIt < distWithRes && it.IsCableConnected) {
				res = it;
				distWithRes = distWithIt;
			}
		});

		return res;
	}


	private RessourcePort? GetClosetAvailablePortAtRangeOfType(Ressource expRessType) {
		if (portsAtRange.Count == 0) return null;

		RessourcePort? res = null;
		float distWithRes = float.MaxValue;

		portsAtRange.ForEach(it => {
			var distWithIt = Vector3.Distance(transform.position, it.transform.position);
			if (distWithIt < distWithRes && it.ressType == expRessType && it.CanConnectCable) {
				res = it;
				distWithRes = distWithIt;
			}
		});

		return res;
	}

	private bool HandleCableAction() {
		if (grabbedCable != null) {
			DropCable();
			return true;
		}

		if (cablesAtRange.Count == 0)
			return false;

		return TryGrabCable(GetClosetCableAtRange()!);
	}

	private void HandleGrabAction(InputAction.CallbackContext ctx)
    {
		HandleGrabAction();
    }

	private void HandleGrabAction() {
		if (HandlePortAction()) return;
		HandleCableAction();
	}

	private void DropCable() {
		grabbedCable?.OnDrop(gameObject);
		Destroy(cableJoint);
		grabbedCable = null;
		cableJoint = null;
		Debug.Log("Cable Dropped");
	}
}
