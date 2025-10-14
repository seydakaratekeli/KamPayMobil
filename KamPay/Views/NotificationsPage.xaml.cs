// KamPay/Views/NotificationsPage.xaml.cs
using KamPay.ViewModels;

namespace KamPay.Views
{
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage(NotificationsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
/*
 * NewMessage (Yeni Mesaj):

Ne zaman olu�ur? Ba�ka bir kullan�c� sana bir �r�n hakk�nda veya do�rudan mesaj g�nderdi�inde.

�rnek: "Ali Veli, 'Ders Kitab�' �r�n�n hakk�nda bir mesaj g�nderdi."

ProductSold (�r�n Sat�ld�):

Ne zaman olu�ur? Bir al�c� ile anla��p QR kod ile teslimat� tamamlad���nda veya �r�n� "Sat�ld�" olarak i�aretledi�inde.

�rnek: "'Eski Hesap Makinesi' adl� �r�n�n sat�ld� olarak i�aretlendi."

NewFavorite (Yeni Favori):

Ne zaman olu�ur? Ba�ka bir kullan�c�, senin listeledi�in bir �r�n� favorilerine ekledi�inde.

�rnek: "Ay�e Y�lmaz, 'Kamp Sandalyesi' �r�n�n� favorilerine ekledi."

BadgeEarned (Rozet Kazan�ld�):

Ne zaman olu�ur? Belirli bir ba�ar�ya ula�t���nda (�rne�in 5. �r�n�n� listeledi�inde veya 100 puana ula�t���nda). FirebaseUserProfileService i�inde bu mant��� zaten kurmu�uz.

�rnek: "Tebrikler! 'Payla��m Kahraman�' rozetini kazand�n."

PointsEarned (Puan Kazan�ld�):

Ne zaman olu�ur? Puan kazand�racak bir eylem yapt���nda (�r�n ekleme, ba��� yapma vb.). Bu mant�k da FirebaseUserProfileService i�inde mevcut.

�rnek: "Yeni bir �r�n ekledi�in i�in +5 puan kazand�n!"

DonationMade (Ba��� Yap�ld�):

Ne zaman olu�ur? Bir �r�n�n� ba���lad���nda veya "S�rpriz Kutu"ya ekledi�inde.

�rnek: "'Okunmu� Romanlar' ba����n ihtiya� sahibine ula�t�."

SystemNotice (Sistem Bildirimi):

Ne zaman olu�ur? Uygulama genelinde bir duyuru yap�ld���nda veya hesab�nla ilgili �nemli bir g�ncelleme oldu�unda.

�rnek: "Uygulamam�zdaki yeni 'Zaman Bankas�' �zelli�ini ke�fet!" bu mekan�zmalar� da ekle
 * */