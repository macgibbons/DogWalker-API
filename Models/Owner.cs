using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerAPI.Models
{
    public class Owner
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Owner name must be between 2 and 40 characters")]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int NeighborhoodId { get; set; }
        public string Phone { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public Dog dog { get; set; }
        public List<Dog> OwnersDogs { get; set; }
    }
}
