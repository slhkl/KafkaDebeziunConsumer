
namespace Data.Model

{
    public class KafkaModel<T>
    {
        public T after { get; set; }
        public object patch { get; set; }
        public object filter { get; set; }
        public KafkaSourceModel source { get; set; }
        public string op { get; set; }
        public long ts_ms { get; set; }
        public object transaction { get; set; }
    }
}
