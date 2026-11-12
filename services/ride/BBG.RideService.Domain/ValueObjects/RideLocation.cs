namespace BBG.RideService.Domain.ValueObjects;

public sealed record RideLocation
{
    public string Name { get; }
    public string Address { get; }

    private RideLocation(string name, string address)
    {
        Name = name;
        Address = address;
    }

    public static RideLocation Create(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Location address is required.", nameof(address));

        return new RideLocation(name.Trim(), address.Trim());
    }
}