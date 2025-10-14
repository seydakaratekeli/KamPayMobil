namespace KamPay.Models;

// Bildirim modeli
public class Notification
{
    public string NotificationId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string IconUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

 

    // �lgili veri (�r�n, mesaj vb.)
    public string RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; } // "Product", "Message", "Badge" vb.

    // Aksiyon URL'i
    public string ActionUrl { get; set; }

    public Notification()
    {
        NotificationId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public string TimeAgoText
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;

            if (diff.TotalMinutes < 1)
                return "Az �nce";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} dakika �nce";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} saat �nce";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} g�n �nce";

            return CreatedAt.ToString("dd MMM yyyy");
        }
    }
}

public enum NotificationType
{
    NewMessage = 0,      // Yeni mesaj
    ProductSold = 1,     // �r�n sat�ld�
    ProductViewed = 2,   // �r�n�n�z g�r�nt�lendi
    NewFavorite = 3,     // �r�n�n�z favorilere eklendi
    BadgeEarned = 4,     // Rozet kazand�n�z
    PointsEarned = 5,    // Puan kazand�n�z
    DonationMade = 6,    // Ba��� yap�ld�
    SystemNotice = 7,    // Sistem bildirimi
         NewOffer = 8,         // Yeni teklif/istek geldi
    OfferAccepted = 9,    // Teklifin kabul edildi
    OfferRejected = 10    // Teklifin reddedildi
}