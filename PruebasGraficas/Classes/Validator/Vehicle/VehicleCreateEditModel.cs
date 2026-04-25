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

    public int? CapacityBufferPercent { get; set; }
    public TimeOnly DepartureTime { get; set; } = TimeOnly.FromTimeSpan(TimeSpan.FromHours(8));

    /// <summary>
    /// This property is for the Maximum Work Time hour HR input
    /// </summary>
    public int WorkTimeHours { get; set; } = 0;

    /// <summary>
    /// This property is for the Maximum Work Time hour MIN input
    /// </summary>
    public int WorkTimeMinutes { get; set; } = 0;

    /// <summary>
    /// Total amount of work time in minutes
    /// </summary>
    public int MaxWorkTimeMinutes { get; set; }

}
