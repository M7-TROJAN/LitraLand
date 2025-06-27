namespace LitraLand.Application.Services.Library.Subscribers;
public interface ISubscriberService
{
    Subscriber? GetById(int id);
    IQueryable<Subscriber> GetQueryable();
    IQueryable<Subscriber> GetQueryableDetails();
    Subscriber? GetSubscriberWithRentals(int id);
    Subscriber? GetSubscriberWithSubscriptions(int id);
    int GetActiveSubscribersCount();
    IEnumerable<KeyValuePairDto> GetSubscribersPerCity();
    IEnumerable<Subscriber> GetExpired(int expiredWithin);
    (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null);
    Subscriber Add(Subscriber subscriber, string imagePath, string imageName, string createdById);
    void Update(Subscriber subscriber, string updatedById);
    Subscription RenewSubscription(int id, DateTime startDate, string createdById);
    bool AllowNationalId(int id, string nationalId);
    bool AllowPhoneNumber(int id, string mobileNumber);
    bool AllowEmail(int id, string email);
}