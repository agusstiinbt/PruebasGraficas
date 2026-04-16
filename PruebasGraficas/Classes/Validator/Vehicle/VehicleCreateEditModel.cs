using PruebasGraficas.Classes.Models;

namespace PruebasGraficas.Classes.Validator.Vehicle;

public class VehicleCreateEditModel : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public VehicleType Type { get; set; } = VehicleType.Car;

    public string LicensePlate { get; set; } = string.Empty;

    public string VehicleRefId { get; set; } = string.Empty;

    public decimal? WeightCapacityLbs { get; set; }

    public decimal? VolumeCapacityFt3 { get; set; }

    public decimal? CapacityBufferPercent { get; set; }
    public TimeOnly DepartureTime { get; set; } = TimeOnly.FromTimeSpan(TimeSpan.FromHours(8));
    public int MaxWorkTimeMinutes { get; set; }

}

