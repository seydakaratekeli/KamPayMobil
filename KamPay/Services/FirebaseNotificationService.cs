// KamPay/Services/FirebaseNotificationService.cs

using CommunityToolkit.Mvvm.Messaging;
using Firebase.Database;
using Firebase.Database.Query;
using KamPay.Helpers;
using KamPay.Models;
using KamPay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KamPay.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseNotificationService()
        {
            _firebaseClient = new FirebaseClient(Constants.FirebaseRealtimeDbUrl);
        }

        private async Task CheckAndBroadcastUnreadStatus(string userId)
        {
            var result = await GetUserNotificationsAsync(userId);
            bool hasUnread = result.Success && result.Data != null && result.Data.Any(n => !n.IsRead);
            WeakReferenceMessenger.Default.Send(new UnreadGeneralNotificationStatusMessage(hasUnread));
        }

        // Yeni bir bildirim olu�turur ve Firebase'e kaydeder.
        public async Task<ServiceResult<bool>> CreateNotificationAsync(Notification notification)
        {
            try
            {
                if (notification == null || string.IsNullOrEmpty(notification.UserId))
                {
                    return ServiceResult<bool>.FailureResult("Bildirim veya kullan�c� ID'si ge�ersiz.");
                }

                await _firebaseClient
                    .Child(Constants.NotificationsCollection)
                    .Child(notification.NotificationId)
                    .PutAsync(notification);

              //  WeakReferenceMessenger.Default.Send(new UnreadGeneralNotificationStatusMessage(true));


                return ServiceResult<bool>.SuccessResult(true, "Bildirim olu�turuldu.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("Bildirim olu�turulurken hata olu�tu.", ex.Message);
            }
        }

        /// Belirli bir kullan�c�n�n t�m bildirimlerini getirir.
        public async Task<ServiceResult<List<Notification>>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                var notificationEntries = await _firebaseClient
                    .Child(Constants.NotificationsCollection)
                    .OrderBy("UserId")
                    .EqualTo(userId)
                    .OnceAsync<Notification>();

                var notifications = notificationEntries
                    .Select(n => n.Object)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                return ServiceResult<List<Notification>>.SuccessResult(notifications);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Notification>>.FailureResult("Bildirimler al�namad�.", ex.Message);
            }
        }

        /// Belirli bir bildirimi okundu olarak i�aretler.
        public async Task<ServiceResult<bool>> MarkAsReadAsync(string notificationId)
        {
            try
            {
                var notification = await _firebaseClient
                    .Child(Constants.NotificationsCollection)
                    .Child(notificationId)
                    .OnceSingleAsync<Notification>();

                if (notification == null)
                {
                    return ServiceResult<bool>.FailureResult("Bildirim bulunamad�.");
                }

                if (notification.IsRead)
                {
                    return ServiceResult<bool>.SuccessResult(true, "Bildirim zaten okunmu�.");
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _firebaseClient
                    .Child(Constants.NotificationsCollection)
                    .Child(notificationId)
                    .PutAsync(notification);

                await CheckAndBroadcastUnreadStatus(notification.UserId);

                return ServiceResult<bool>.SuccessResult(true, "Bildirim okundu olarak i�aretlendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("��lem s�ras�nda hata olu�tu.", ex.Message);
            }
        }
    }
}