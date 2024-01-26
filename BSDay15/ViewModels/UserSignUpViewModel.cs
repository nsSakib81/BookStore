using System.ComponentModel.DataAnnotations;

namespace BSDay15.ViewModels
{
    public class UserSignUpViewModel
    {
        [Display(Name ="Name")]
        [Required]
        public string UserName { get; set; }
        [Display(Name ="Email")]
        [Required]
        public string UserEmail {  get; set; }
        [Display(Name ="Password")]
        [Required]
        public string UserPassword { get; set; }
    }
}
