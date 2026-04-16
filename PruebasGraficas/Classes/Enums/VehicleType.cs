namespace PruebasGraficas.Classes.Enums
{
    public enum VehicleType
    {
        [LocalizedDescription("VehicleType_Car", typeof(LocalResources), "vehicle-type-icon-car")]
        Car = 1,
        [LocalizedDescription("VehicleType_CarEnhanced", typeof(LocalResources), "vehicle-type-icon-car-enhanced")]
        CarEnhanced = 2,
        [LocalizedDescription("VehicleType_SmallTruck", typeof(LocalResources), "vehicle-type-icon-small-truck")]
        SmallTruck = 3,
        [LocalizedDescription("VehicleType_SmallTruckEnhanced", typeof(LocalResources), "vehicle-type-icon-small-truck-enhanced")]
        SmallTruckEnhanced = 4,
        [LocalizedDescription("VehicleType_Truck", typeof(LocalResources), "vehicle-type-icon-truck")]
        Truck = 5,
        [LocalizedDescription("VehicleType_Scooter", typeof(LocalResources), "vehicle-type-icon-scooter")]
        Scooter = 6,
        [LocalizedDescription("VehicleType_RoadBike", typeof(LocalResources), "vehicle-type-icon-road-bike")]
        RoadBike = 7,
        [LocalizedDescription("VehicleType_UrbanBike", typeof(LocalResources), "vehicle-type-icon-urban-bike")]
        UrbanBike = 8,
        [LocalizedDescription("VehicleType_MountainBike", typeof(LocalResources), "vehicle-type-icon-mountain-bike")]
        MountainBike = 9,
        [LocalizedDescription("VehicleType_OnFoot", typeof(LocalResources), "vehicle-type-icon-on-foot")]
        OnFoot = 10
    }
}
