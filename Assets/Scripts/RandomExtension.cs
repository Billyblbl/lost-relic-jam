using UnityEngine;

#nullable enable

public static class RandomExtension {
	public static bool RandomProba(float proba) => Random.Range(0f, 1f) < proba;
}
