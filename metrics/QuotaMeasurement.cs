namespace metrics
{
    public struct QuotaMeasurement<T> where T : struct
    {
        public readonly T Value;
        public readonly T Limit;
        public readonly KeyValuePair<string, object?>[] Keys;

        public QuotaMeasurement(T value, T limit, KeyValuePair<string, object?>[] keys)
        {
            Value = value;
            Limit = limit;
            Keys = keys;
        }
        public bool IsZero
        {
            get
            {
                T newValue = default;
                return ValueType.Equals(Value, newValue);
            }
        }
    }
}
