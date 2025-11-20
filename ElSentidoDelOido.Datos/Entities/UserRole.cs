using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Entities
{
    public class UserRole
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool? Enabled { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
