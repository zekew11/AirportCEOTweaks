namespace AirportCEOTweaks
{
    public static class AirlineModelExtensionAccessor
    {
        public static Extend_AirlineModel GetExtend_AirlineModel(this AirlineModel airlineModel)
        {
            Singleton<ModsController>.Instance.GetExtensions(airlineModel, out Extend_AirlineModel eam);
            return eam;
        }
        public static Extend_AirlineModel GetExtend_AirlineModel(this CommercialFlightModel cfm)
        {
            return cfm.Airline.GetExtend_AirlineModel();
        }
    }
}