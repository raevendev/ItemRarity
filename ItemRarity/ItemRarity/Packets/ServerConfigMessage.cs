using ProtoBuf;

namespace ItemRarity.Packets;

[ProtoContract]
public class ServerConfigMessage
{
    [ProtoMember(1)]
    public string SerializedConfig = string.Empty;
}