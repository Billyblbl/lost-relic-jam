
[System.Serializable] public struct Range<T> where T : System.IComparable {
	public T start;
	public T end;

	Range(T min, T max) {
		this.start = min;
		this.end = max;
	}

	bool Contains(T value) => value.CompareTo(start) > 0 && value.CompareTo(end) < 0;

	public static bool operator<(Range<T> range, T value) => range.end.CompareTo(value) < 0;
	public static bool operator>(Range<T> range, T value) => range.start.CompareTo(value) > 0;
	public static bool operator<(T value, Range<T> range) => value.CompareTo(range.end) < 0;
	public static bool operator>(T value, Range<T> range) => value.CompareTo(range.start) > 0;

}
