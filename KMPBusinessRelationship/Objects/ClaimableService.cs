using System;

namespace KMPBusinessRelationship.Objects
{
    public class ClaimableService : Service<ClaimableService>, IEquatable<ClaimableService>
    {
        public bool Claimed { get; set; } = false;

        public override bool Equals(ClaimableService other)
        {
            if (!base.Equals(other)) return false;
            if (Claimed != other.Claimed) return false;
            return true;
        }
    }
}
