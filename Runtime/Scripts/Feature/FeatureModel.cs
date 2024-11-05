using System.Collections.Generic;
using Newtonsoft.Json;

namespace jp.go.aist3ddbclient
{
    public class Properties
    {
        [JsonProperty("file_id")]
        public int file_id { get; set; }

        [JsonProperty("minz")]
        public float minz { get; set; }

        [JsonProperty("maxz")]
        public float maxz { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("coordinates")]
//        public List<List<List<float>>> coordinates { get; set; }
        public object coordinates { get; set; } // Use object to handle both Polygon and MultiPolygon

        [JsonProperty("properties")]
        public Properties properties { get; set; }
    }

    public class FeatureProperties
    {
        [JsonProperty("reg_id")]
        public int reg_id { get; set; }

        [JsonProperty("service_name")]
        public string service_name { get; set; }

        [JsonProperty("creation_date")]
        public string creation_date { get; set; }

        [JsonProperty("creation_date_end")]
        public string creation_date_end { get; set; }

        [JsonProperty("registration_datetime")]
        public string registration_date_time { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("location")]
        public string location { get; set; }

        [JsonProperty("group")]
        public string group { get; set; }

        [JsonProperty("license")]
        public string license { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("3dtiles_url")]
        public string threeD_tiles_url { get; set; }

        [JsonProperty("downloadable")]
        public bool downloadable { get; set; }

        [JsonProperty("author")]
        public string author { get; set; }

        [JsonProperty("external_link")]
        public string external_link { get; set; }

        [JsonProperty("external_link_type")]
        public string external_link_type { get; set; }
    }

    public class Feature
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("geometries")]
        public List<Geometry> geometries { get; set; }

        [JsonProperty("properties")]
#pragma warning disable CS8632 // '#nullable' 注釈コンテキスト内のコードでのみ、Null 許容参照型の注釈を使用する必要があります。
        public FeatureProperties? properties { get; set; }
#pragma warning restore CS8632 // '#nullable' 注釈コンテキスト内のコードでのみ、Null 許容参照型の注釈を使用する必要があります。
    }

    public class FeatureCollection
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, int> properties { get; set; }

        [JsonProperty("features")]
        public List<Feature> features { get; set; }
    }

}