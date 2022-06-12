using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

#endif

[System.Serializable]
public struct Cone {
	public float radius;
	public float angle;
	public bool Contains(Transform transform, Vector3 point) => Vector3.Distance(transform.position, point) < radius && Vector3.Angle(transform.forward, point - transform.position) < angle / 2f;

#if UNITY_EDITOR
	public (float, float) DrawHandle(ArcHandle arcHandle, Transform transform) {
		arcHandle.radius = radius;
		arcHandle.angle = angle / 2f;

		Vector3[] directions = {
			Vector3.right,
			Vector3.left,
			Vector3.up,
			Vector3.down
		};

		var matrices = directions
		.Select(direction => transform.rotation * Quaternion.LookRotation(Vector3.forward, direction))
		.Select(rotation => Matrix4x4.TRS(transform.position, rotation, Vector3.one));

		foreach (var matrix in matrices) using (new Handles.DrawingScope(matrix)) { arcHandle.DrawHandle(); }
		angle = arcHandle.angle * 2f;
		radius = Handles.RadiusHandle(transform.rotation, transform.position, arcHandle.radius);
		return (angle, radius);
	}

	public (float, float) DrawHandle(ArcHandle arcHandle, Vector3 position, Quaternion rotation) {
		arcHandle.radius = radius;
		arcHandle.angle = angle / 2f;

		Vector3[] directions = {
			Vector3.right,
			Vector3.left,
			Vector3.up,
			Vector3.down
		};

		var matrices = directions
		.Select(direction => rotation * Quaternion.LookRotation(Vector3.forward, direction))
		.Select(rotation => Matrix4x4.TRS(position, rotation, Vector3.one));

		foreach (var matrix in matrices) using (new Handles.DrawingScope(matrix)) { arcHandle.DrawHandle(); }
		angle = arcHandle.angle * 2f;
		radius = Handles.RadiusHandle(rotation, position, arcHandle.radius);
		return (angle, radius);
	}
#endif

}
