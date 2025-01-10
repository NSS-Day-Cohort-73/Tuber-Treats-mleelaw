namespace TuberTreats.Models.DTO;

public class TuberDriverDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TuberOrder> TuberDeliveries { get; set; }
}
