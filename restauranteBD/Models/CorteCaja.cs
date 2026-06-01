using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restauranteBD.Models
{
    [Table("cortes_caja")]
    public class CorteCaja
    {
        [Key]
        [Column("id_corte")]
        public int IdCorte { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("fecha_apertura")]
        public DateTime FechaApertura { get; set; } = DateTime.UtcNow;

        [Column("fecha_cierre")]
        public DateTime? FechaCierre { get; set; }

        [Column("fondo_inicial")]
        public decimal FondoInicial { get; set; }

        // 👇 LAS 4 COLUMNAS NUEVAS PARA EL DESGLOSE PROFESIONAL 👇
        [Column("ventas_efectivo")]
        public decimal VentasEfectivo { get; set; } = 0;

        [Column("ventas_tarjeta")]
        public decimal VentasTarjeta { get; set; } = 0;

        [Column("total_gastos")]
        public decimal TotalGastos { get; set; } = 0;

        [Column("total_descuentos")]
        public decimal TotalDescuentos { get; set; } = 0;

        // 👇 TOTALES FINALES DEL ARQUEO 👇
        [Column("total_ventas_sistema")]
        public decimal TotalVentasSistema { get; set; } = 0; // Efectivo + Tarjeta - Descuentos

        [Column("total_arqueo_fisico")]
        public decimal TotalArqueoFisico { get; set; } = 0; // Lo que contó el cajero

        [Column("diferencia")]
        public decimal Diferencia { get; set; } = 0;

        [Column("estado")]
        public string Estado { get; set; } = "abierto";
    }
}