namespace WebAdvert.Web.Models.Accounts
{
    using System.ComponentModel.DataAnnotations;

    public class ConfirmEmailViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
