using CityInfo.Entities;
using CityInfo.Models;

namespace CityInfo.Services
{
    public interface ICityInfoRepository
    {
         Task<IEnumerable<City>> GetCitiesAsync();
         Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
         Task<City?> GetCityAsync(int cityId, bool includePointOfInterest);

         Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
         Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);

         Task<bool> CityExistsAsync(int cityId);

         Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);

         void DeletePointOfInterest(PointOfInterest pointOfInterest);

         Task<bool> CityNameMatchesCityId(string? cityName, int cityId);

        Task<bool> SaveChangesAsync();
    }
}