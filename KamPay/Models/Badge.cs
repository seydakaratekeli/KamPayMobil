namespace KamPay.Models;

// Rozet (Badge) modeli - Oyunla�t�rma i�in
public class Badge
{
    public string BadgeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconName { get; set; }
    public BadgeCategory Category { get; set; }
    public int RequiredPoints { get; set; }
    public int RequiredCount { get; set; } // �rn: 10 �r�n sat
    public string Color { get; set; }

    public Badge()
    {
        BadgeId = Guid.NewGuid().ToString();
    }

    // Varsay�lan rozetler
    public static List<Badge> GetDefaultBadges()
    {
        return new List<Badge>
            {
                new Badge
                {
                    Name = "�lk Ad�m",
                    Description = "�lk �r�n�n� ekledin!",
                    IconName = "badge_first.png",
                    Category = BadgeCategory.Seller,
                    RequiredCount = 1,
                    Color = "#4CAF50"
                },
                new Badge
                {
                    Name = "Payla��m Kahraman�",
                    Description = "5 �r�n payla�t�n",
                    IconName = "badge_hero.png",
                    Category = BadgeCategory.Seller,
                    RequiredCount = 5,
                    Color = "#2196F3"
                },
                new Badge
                {
                    Name = "Ba��� Mele�i",
                    Description = "3 �r�n ba���lad�n",
                    IconName = "badge_angel.png",
                    Category = BadgeCategory.Donation,
                    RequiredCount = 3,
                    Color = "#FF9800"
                },
                new Badge
                {
                    Name = "Aktif Al�c�",
                    Description = "5 �r�n ald�n",
                    IconName = "badge_buyer.png",
                    Category = BadgeCategory.Buyer,
                    RequiredCount = 5,
                    Color = "#9C27B0"
                },
                new Badge
                {
                    Name = "S�per Sat�c�",
                    Description = "10 �r�n satt�n",
                    IconName = "badge_super_seller.png",
                    Category = BadgeCategory.Seller,
                    RequiredCount = 10,
                    Color = "#FF5722"
                },
                new Badge
                {
                    Name = "Kamp�s Y�ld�z�",
                    Description = "100 puana ula�t�n",
                    IconName = "badge_star.png",
                    Category = BadgeCategory.Points,
                    RequiredPoints = 100,
                    Color = "#FFC107"
                }
            };
    }
}

public enum BadgeCategory
{
    Seller = 0,    // Sat�c� rozetleri
    Buyer = 1,     // Al�c� rozetleri
    Donation = 2,  // Ba��� rozetleri
    Points = 3,    // Puan rozetleri
    Special = 4    // �zel rozetler
}

// Kullan�c� rozeti (User'�n kazand��� rozetler)
public class UserBadge
{
    public string UserBadgeId { get; set; }
    public string UserId { get; set; }
    public string BadgeId { get; set; }
    public DateTime EarnedAt { get; set; }

    // Badge bilgileri (cache i�in)
    public string BadgeName { get; set; }
    public string BadgeIcon { get; set; }
    public string BadgeColor { get; set; }

    public UserBadge()
    {
        UserBadgeId = Guid.NewGuid().ToString();
        EarnedAt = DateTime.UtcNow;
    }
}