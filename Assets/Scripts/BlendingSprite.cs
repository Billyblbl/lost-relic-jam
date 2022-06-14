using UnityEngine;
using System.Linq;

#nullable enable

[System.Serializable] public struct BlendingSprite {
	public SpriteRenderer? back;
	public SpriteRenderer? front;
	public float duration;

	public void UpdateBlend(float dt) {
		if (front == null || back == null) return;

		if (back.color.a > 0f) {
			var color = back.color;
			color.a = Mathf.Clamp(color.a - Mathf.InverseLerp(0, duration, dt), 0f, 1f);
			back.color = color;
		}

		if (front.color.a < 1f) {
			var color = front.color;
			color.a = Mathf.Clamp(color.a + Mathf.InverseLerp(0, duration, dt), 0f, 1f);
			front.color = color;
		}

	}

	public void Swap() {
		if (front == null || back == null) return;

		// Switch sorting orders
		var lvl = back.sortingOrder;
		back.sortingOrder = front.sortingOrder;
		front.sortingOrder = lvl;

		// Switch references
		var tmp = back;
		back = front;
		front = tmp;
	}

	public void SwitchTo(Sprite? sprite) {
		if (front == null || back == null) return;
		back.sprite = sprite;
		Swap();
	}
}
