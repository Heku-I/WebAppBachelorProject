using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAppBachelorProject.Areas.Identity.Data;

namespace WebAppBachelorProject.Models
{
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ImageId { get; set; }


        public string Description { get; set; }

        //This must be Required, but for now let's set it like this. 
        public string ImagePath { get; set; }

        //The foreign-key link between Image and ApplicationUser. 
        //One-to-Many relationship.
        public string UserId { get; set; }

        //We also need to add an attribute for evaluation. 


        //Navigation Properties: 
        public WebAppBachelorProjectUser User { get; set; }
    }
}
