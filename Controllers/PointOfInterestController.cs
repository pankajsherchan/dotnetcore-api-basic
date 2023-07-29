using AutoMapper;
using CityInfo.Models;
using CityInfo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.Controllers
{
    [Authorize(Policy = "MustBeFromSeattle")]
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    public class PointOfInterestController : ControllerBase
    {
         private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
         private readonly CityDataStore _cityDataStore;
        private readonly ILogger<PointOfInterestController> _logger;
        private readonly IMailService _mailService;
        public PointOfInterestController(ILogger<PointOfInterestController> logger, IMailService mailService, CityDataStore cityDataStore, ICityInfoRepository cityInfoRepository, IMapper mapper) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));;
            _cityDataStore = cityDataStore ?? throw new ArgumentNullException(nameof(cityDataStore));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId){
            // var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

            // if (!await _cityInfoRepository.CityNameMatchesCityId(cityName, cityId)) {
            //     return Forbid();
            // }
            
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists) {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                return NotFound();
            }

            var pointOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointOfInterestForCity));
        }

         [HttpGet("{pointofinterestId}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointofinterestId){
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists) return NotFound();

            var pointofinterest = _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);
            if (pointofinterest == null) return NotFound();
            
            return Ok(_mapper.Map<PointOfInterestDto>(pointofinterest));
        }

        [HttpPost]
        public async  Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest) {
            var city = _cityInfoRepository.CityExistsAsync(cityId);
            if (city == null) {
                return NotFound();
            }

          
          var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);
          

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await  _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);
            
            return CreatedAtRoute("GetPointOfInterest", new {
                cityId = cityId, 
                pointofinterestId = createdPointOfInterestToReturn.Id
            }, createdPointOfInterestToReturn
            );
        }
        
         [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult<PointOfInterestDto>> UpdatePointOfInterestAsync(int cityId, int pointofinterestId, PointOfInterestForUpdateDto pointOfInterest) {
            var city = await _cityInfoRepository.CityExistsAsync(cityId);
            
            if (!city) {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);

            if (pointOfInterestEntity == null) {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();
         
            return NoContent();
        }
        
        // [HttpPatch("{pointofinterestid}")]
        // public ActionResult<PointOfInterestDto> PartiallyUpdatePointOfInterest(int cityId, int pointofinterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument) {
        //     var city = _cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //     if (city == null) {
        //         return NotFound();
        //     }

        //     var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(poi => poi.Id == pointofinterestId);
        //     if (pointOfInterestFromStore == null) {
        //         return NotFound();
        //     }

        //     var pointOfInterestToPatch = new PointOfInterestForUpdateDto() {
        //         Name = pointOfInterestFromStore.Name, 
        //         Description = pointOfInterestFromStore.Description
        //     };

        //     patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

        //     if (!ModelState.IsValid) {
        //         return BadRequest(ModelState);
        //     }

        //     if (!TryValidateModel(pointOfInterestToPatch)) {
        //         return BadRequest(ModelState);
        //     }

        //     pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        //     pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;
         
        //     return NoContent();
        // }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId) {
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists) {
                return NotFound();
            }

            var cityEntity = await _cityInfoRepository.GetCityAsync(cityId, true);
            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterestEntity == null) {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();
            
            _mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestEntity.Name} was deleted");
            return NoContent();
        }
        
    }
}