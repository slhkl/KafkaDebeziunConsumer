

namespace Data.Model

{
    public class KafkaSourceModel
    {

        public string version { get; set; }
        public string connector { get; set; }
        public string name { get; set; }
        public long ts_ms { get; set; }
        public string snapshot { get; set; }
        public string db { get; set; }
        public string rs { get; set; }
        public string collection { get; set; }
        public int ord { get; set; }
        public object h { get; set; }
        public object tord { get; set; }
        public string stxnid { get; set; }
    }
}
