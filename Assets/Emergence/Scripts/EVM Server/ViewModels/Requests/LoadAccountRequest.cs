namespace EmergenceEVMLocalServer.ViewModels
{
    public class LoadAccountRequest
    {
        public string name { get; set; }
        public string password { get; set; }
        public string path { get; set; }
        public string nodeURL { get; set; }
    }
}
