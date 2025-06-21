namespace EHRsystem.Models.Entities
{
    public class PharmacyRecommendation
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }
        public string PharmacyName { get; set; }
        public string PharmacyLocation { get; set; }
    }
}
