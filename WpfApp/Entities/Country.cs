using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf;

namespace WpfApp.Entities
{
    public class Country
    {

        [Key]
        [MaxLength(255)]
        public string CountryName { get; set; }
        

        public virtual ICollection<Movie> Movies { get; set; }
    }
}
