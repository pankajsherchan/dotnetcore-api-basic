namespace CityInfo.Models
{
    /// <summary>
    /// A DTO For city without point of interest
    /// </summary>
    public class CityWithoutPointOfInterestDto
    {
        /// <summary>
        /// Id of the city
        /// </summary>
        /// <value></value>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}