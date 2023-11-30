using System.Collections.Generic;

namespace EmergenceSDK.Types.Responses.FuturePass
{
    public class Collection
    {
        public int chainId { get; set; }
        public string chainType { get; set; }
        public string location { get; set; }
        public string name { get; set; }
    }

    public class Data
    {
        public Nfts nfts { get; set; }
    }

    public class Edge
    {
        public Node node { get; set; }
    }

    public class Metadata
    {
        public Properties properties { get; set; }
    }

    public class Nfts
    {
        public List<Edge> edges { get; set; }
    }

    public class Node
    {
        public string assetType { get; set; }
        public Collection collection { get; set; }
        public int tokenIdNumber { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Properties
    {
        public string image { get; set; }
        public string animation_url { get; set; }
        public string glb_url { get; set; }
        public string image_transparent { get; set; }
    }

    public class FVInventoryResponse
    {
        public Data data { get; set; }
    }
}
