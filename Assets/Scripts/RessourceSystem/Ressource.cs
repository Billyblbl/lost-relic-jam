using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

[CreateAssetMenu(menuName = "lost_relic_jam/Ressource")]
public class Ressource : ScriptableObject {
	public Vector3Int	unit;
	public Segment? cableSegmentPrefab;
	public Plug? cableEndPrefab;
}
