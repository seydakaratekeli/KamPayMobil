namespace KamPay.Models;

// ===== H�ZMET PAYLA�IMI (ZAMAN BANKASI) =====
public class ServiceOffer
{
    public string ServiceId { get; set; }
    public string ProviderId { get; set; }
    public string ProviderName { get; set; }
    public ServiceCategory Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int TimeCredits { get; set; } // Saat cinsinden
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Tags { get; set; }

    public ServiceOffer()
    {
        ServiceId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        IsAvailable = true;
        Tags = new List<string>();
    }
}

public enum ServiceCategory
{
    Education = 0,      // �zel ders, ders notu
    Technical = 1,      // Bilgisayar tamiri, kod yard�m�
    Cooking = 2,        // Yemek yapma
    Childcare = 3,      // �ocuk bak�m�
    PetCare = 4,        // Evcil hayvan bak�m�
    Translation = 5,    // �eviri
    Moving = 6,         // Ta��ma yard�m�
    Other = 7           // Di�er
}

public class ServiceRequest
{
    public string RequestId { get; set; }
    public string ServiceId { get; set; }
    public string RequesterId { get; set; }
    public string RequesterName { get; set; }
    public string Message { get; set; }
    public DateTime RequestedAt { get; set; }
    public ServiceRequestStatus Status { get; set; }

    public ServiceRequest()
    {
        RequestId = Guid.NewGuid().ToString();
        RequestedAt = DateTime.UtcNow;
        Status = ServiceRequestStatus.Pending;
    }
}

public enum ServiceRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Completed = 3
}
