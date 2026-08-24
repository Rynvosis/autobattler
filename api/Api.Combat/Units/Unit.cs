namespace Api.Combat.Units;

public class Unit
{
    public required int Id { get; init; }
    public required Kind Kind { get; init; }
    public required int Attack { get; set; }
    public required int Health { get; set; }

    public bool Dead { get; set; }
}
