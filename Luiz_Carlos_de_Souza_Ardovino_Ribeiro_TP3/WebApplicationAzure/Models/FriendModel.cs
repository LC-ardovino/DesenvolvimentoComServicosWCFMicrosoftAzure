using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebApplicationAzure.Models
{
    public class FriendModel
    {
        public int Id { get; set; }
        [Display(Name = "Nome")]
        public string FirstName { get; set; }
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }
        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date), Required]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }
        [Display(Name = "Visualizacoes")]
        public int Views { get; set; }

        public string PictureUrl { get; set; }
    }
}
