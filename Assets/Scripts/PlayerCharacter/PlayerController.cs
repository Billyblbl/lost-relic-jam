using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[SerializeField] private float moveSpeed = 1.0f;

	public InputActionReference move;
	public InputActionReference grab;

	private Rigidbody playerRigibody;
	private Grabbable grabbedCable = null;
	private FixedJoint cableJoint;

	private List<Grabbable> cablesAtRange = new List<Grabbable>();
	private List<RessourcePort> portsAtRange = new List<RessourcePort>();

	public float turnSpeed = 10f;

	private void Awake() {
		playerRigibody = GetComponent<Rigidbody>();
		grab.action.performed += HandleGrabAction;
	}

	void Update() {
		var axisValue = move.action.ReadValue<Vector2>();

		var displacement = axisValue * moveSpeed * Time.deltaTime;
		var newLoc = playerRigibody.position + new Vector3(displacement.x, 0, displacement.y);

		var dir = -Vector3.Normalize(playerRigibody.position - newLoc);
		var newPLayerDir = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0.0f);
		transform.rotation = Quaternion.LookRotation(newPLayerDir);

		playerRigibody.MovePosition(newLoc);
		Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.red);

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
		grabbedCable.transform.position = gameObject.transform.TransformPoint(new Vector3(0, 0, -1));
		cableJoint = gameObject.AddComponent<FixedJoint>();
		cableJoint.connectedBody = cable.GetComponent<Rigidbody>();
		grabbedCable.OnGrab(gameObject);
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

		var port = GetClosetAvailablePortAtRangeOfType(grabbedCable.ressType);
		if (port == null)
			return false;

		var cableToConnect = grabbedCable;
		DropCable();
		port.ConnectCable(cableToConnect);
		return true;
	}

	private Grabbable GetClosetCableAtRange() {
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

	private RessourcePort GetClosetFilledPortAtRange() {
		if (portsAtRange.Count == 0) return null;

		RessourcePort res = null;
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


	private RessourcePort GetClosetAvailablePortAtRangeOfType(RessourcePort.RessourceType expRessType) {
		if (portsAtRange.Count == 0) return null;

		RessourcePort res = null;
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

		return TryGrabCable(GetClosetCableAtRange());
	}

	private void HandleGrabAction(InputAction.CallbackContext ctx) {
		if (HandlePortAction()) return;
		HandleCableAction();
	}

	private void DropCable() {
		grabbedCable.OnDrop(gameObject);
		Destroy(cableJoint);
		grabbedCable = null;
		Debug.Log("Cable Dropped");
	}
}
