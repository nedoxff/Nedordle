namespace Nedordle.Helpers.Types;

public struct Error
{
    public string Id;
    public string Timestamp;
    public string Message;
    public string FullException;
    public Dictionary<string, object> AdditionalInformation = new();
}