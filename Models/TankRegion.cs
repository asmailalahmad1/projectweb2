namespace SuqiaWaterDistribution.Models
{
    public class TankRegion
    {
        public int TankId { get; set; }
        public virtual Tank Tank { get; set; } = null!;

        public int RegionId { get; set; }
        public virtual Region Region { get; set; } = null!;
    }
}

