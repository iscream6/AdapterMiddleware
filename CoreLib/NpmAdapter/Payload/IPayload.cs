using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    public interface IPayload
    {
        void Deserialize(JToken json);

        byte[] Serialize();

        JToken ToJson();
    }
}
