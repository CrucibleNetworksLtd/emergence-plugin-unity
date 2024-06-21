namespace EmergenceSDK.Internal.UI.Screens
{
    public class CollectionScreenFilterParams
    {
        public string searchString ;
        public bool avatars;
        public bool props;
        public bool clothing;
        public bool weapons;
        public string blockchain;
        public CollectionScreenFilterParams()
        {
            searchString = "";
            avatars = true;
            props = true;
            clothing = true;
            weapons = true;
            blockchain = "ANY";
        }
    }
}