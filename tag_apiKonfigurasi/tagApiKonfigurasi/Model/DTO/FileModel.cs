namespace tagApiKonfigurasi.Model.DTO
{
    public class AccesModel
    {
        public string IdController { get; set; }
        public string IdAction { get; set; }
    }

    public class FilterMenu
    {
        public string IdAction { get; set; }
        public string NameAction { get; set; }
    }

    public class FilterMenuWeb
    {
        public string subject { get; set; }
        public string action { get; set; }

    }
}
