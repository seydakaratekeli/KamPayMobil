using System;
using KamPay.Models;

namespace KamPay.Models
{
    // Bir teklifin veya iste�in t�m ya�am d�ng�s�n� takip eden ana model
    public class Transaction
    {
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();

        // �lgili Ana �r�n Bilgileri
        public string ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductThumbnailUrl { get; set; }
        public ProductType Type { get; set; } // Sat��, Takas, Ba���

        // Taraflar
        public string SellerId { get; set; } // �r�n� sunan ki�i
        public string SellerName { get; set; }
        public string BuyerId { get; set; }  // Teklifi yapan/iste�i g�nderen ki�i
        public string BuyerName { get; set; }

        // Durum ve Zaman Bilgileri
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Takas'a �zel alanlar
        public string? OfferedProductId { get; set; } // Takas i�in teklif edilen �r�n�n ID'si
        public string? OfferedProductTitle { get; set; }
        public string? OfferMessage { get; set; } // Teklifle birlikte g�nderilen mesaj
    }

    public enum TransactionStatus
    {
        Pending,      // Teklif yap�ld�, sat�c�n�n onay� bekliyor
        Accepted,     // Teklif kabul edildi, teslimat s�reci bekleniyor
        Rejected,     // Teklif reddedildi
        Completed,    // Teslimat tamamland� ve i�lem kapand�
        Cancelled     // Taraflardan biri iptal etti
    }
}