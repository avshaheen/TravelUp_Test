namespace TravelUp.Application.Models;

public class JsonResponse
{
    public bool Success { get; set; }
    public object Item { get; set; }
    public List<string> Errors { get; set; }
}
