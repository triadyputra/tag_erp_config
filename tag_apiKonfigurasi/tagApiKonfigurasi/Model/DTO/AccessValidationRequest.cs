namespace tagApiKonfigurasi.Model.DTO
{
    public class AccessValidationRequest
    {
        public string Token { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
