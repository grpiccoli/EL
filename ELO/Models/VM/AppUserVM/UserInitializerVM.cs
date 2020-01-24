namespace ELO.Models.ViewModels
{
    public class UserInitializerVM
    {
        public string Name { get; set; }

        public string[] Roles { get; set; }

        public string[] Claims { get; set; }

        public string Email { get; set; }

        public string Key { get; set; }

        public string Image { get; set; }

        public int? Rating { get; set; }
    }
}
