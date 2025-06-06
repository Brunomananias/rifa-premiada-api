﻿using System.ComponentModel.DataAnnotations.Schema;

namespace API_Rifa.Models
{
    public class PixTransaction
    {
        public int Id { get; set; }

        [Column("numberSoldId")] // Nome exato da coluna
        public int? NumberSoldId { get; set; } // Propriedade pode ser null

        [Column("pix_key")]
        public string? Pix_Key { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("qr_code_url")]
        public string? QrCodeUrl { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        // Adicione estas propriedades
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; } // Nullable
        public NumberSold? NumberSold { get; set; }
    }


    public class PixTransactionAdmin
    {
        public int Id { get; set; }

        [Column("pix_key")]
        public string? Pix_Key { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("qr_code_url")]
        public string? QrCodeUrl { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Adicione estas propriedades
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; } // Nullable
        public NumberSold NumberSold { get; set; }
    }



    public class CheckoutRequest
    {
        public Customer Customer { get; set; }
        public int RaffleId { get; set; }
        public int number_sold_id { get; set; }
        public string Numbers { get; set; }
        public decimal Price { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public int TransactionId { get; set; }
        public int RaffleId { get; set; }
        public List<int> Numbers { get; set; }
    }


}
