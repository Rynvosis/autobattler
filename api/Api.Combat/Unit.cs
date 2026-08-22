namespace Api.Combat;

public class Unit
{
    public required int Id { get; init; }
    public required int Attack { get; init; }
    public required int MaxHealth { get; init; }

    public int Health { get; set; }
    public bool Dead { get; set; }
}
