using ProtoBuf;

namespace ItemRarity.Packets;

[ProtoContract]
public sealed class ServerConfigMessage
{
    [ProtoMember(1)]
    public string SerializedConfig = string.Empty;
}