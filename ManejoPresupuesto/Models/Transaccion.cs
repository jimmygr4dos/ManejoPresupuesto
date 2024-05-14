using ManejoPresupuesto.Enums;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        [Display(Name = "Fecha de Transacción")]
        [DataType(DataType.Date)]
        //[DataType(DataType.DateTime)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
            //DateTime.Parse(DateTime.Now.ToString("g"));
            //DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:MM tt"));

        public decimal Monto { get; set; }

        [Display(Name = "Categoría")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoriaId { get; set; }

        [StringLength(maximumLength: 1000, ErrorMessage = "La nota no puede pasar de {1} caracteres")]
        public string Nota { get; set; }

        [Display(Name = "Cuenta")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        public int CuentaId { get; set; }

        [Display(Name = "Tipo de Operación")]

        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
        public string Cuenta { get; set; }
        public string Categoria { get; set; }
    }
}
