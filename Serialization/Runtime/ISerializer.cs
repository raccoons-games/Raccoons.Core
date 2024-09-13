namespace Raccoons.Serialization
{
    public interface ISerializer
    {
        public string Serialize(object obj);

        public T Deserialize<T>(string str);
    }
}
