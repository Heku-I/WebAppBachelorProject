using System.ComponentModel.DataAnnotations;

namespace WebAppBachelorProject.Models
{
    public class Image
    {
        [Key]
        public string ImageId { get; set; }


        public string Description { get; set; }

        //This must be Required, but for now let's set it like this. 
        public string ImageUrl { get; set; }

        //The foreign-key link between Image and ApplicationUser. 
        //One-to-Many relationship.
        public string UserId { get; set; }

        //We also need to add an attribute for evaluation. 


        //Navigation Properties: 
        public ApplicationUser User { get; set; }
    }
}
