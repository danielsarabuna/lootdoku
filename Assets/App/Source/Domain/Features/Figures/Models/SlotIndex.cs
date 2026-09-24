using System;

namespace App.Domain.Figures
{
    public readonly struct SlotIndex : IEquatable<SlotIndex>
    {
        public const int maxCapacity = 3;

        public int Value { get; }

        private SlotIndex(int value)
        {
            if (value is < 0 or >= maxCapacity)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Slot index must be between 0 and {maxCapacity - 1}. Received: {value}");
            Value = value;
        }

        public static bool TryCreate(int value, out SlotIndex slotIndex)
        {
            if (value >= 0 && value < maxCapacity)
            {
                slotIndex = new SlotIndex(value);
                return true;
            }

            slotIndex = default;
            return false;
        }

        public static implicit operator int(SlotIndex slotIndex) => slotIndex.Value;
        public static explicit operator SlotIndex(int value) => new(value);

        public bool Equals(SlotIndex other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SlotIndex other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(SlotIndex left, SlotIndex right) => left.Equals(right);
        public static bool operator !=(SlotIndex left, SlotIndex right) => !left.Equals(right);
    }
}