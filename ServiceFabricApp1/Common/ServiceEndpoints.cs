namespace Common
{
    public class ServiceEndpoints
    {
        const string FabricProtocol = "fabric:/";
        const string ApplicationName = "ServiceFabricApp1/";
        public static string MyStatefulService = $"{ FabricProtocol + ApplicationName }MyStatefulService";
        public static string Actor1 = $"{ FabricProtocol + ApplicationName }Actor1ActorService";
    }
}