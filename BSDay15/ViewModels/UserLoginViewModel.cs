using System.ComponentModel.DataAnnotations;

namespace BSDay15.ViewModels
{
    public class UserLoginViewModel
    {
        [Display(Name ="Email")]
        [Required]
        public string UserEmail {  get; set; }
        [Display(Name ="Password")]
        [Required]
        public string UserPassword {  get; set; }
    }
}
