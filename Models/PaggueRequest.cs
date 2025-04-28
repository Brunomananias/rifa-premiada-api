namespace API_Rifa.Models
{
    public class PaggueRequest
    {
        public string PayerName { get; set; }          // Nome do cliente
        public decimal Amount { get; set; }                // Valor do pagamento em centavos
        public int Expiration { get; set; }            // Tempo de expiração do pagamento em minutos
        public string ExternalId { get; set; }         // ID único do pagamento
        public string Description { get; set; }        // Descrição do pagamento
        public MetaData Meta { get; set; }             // Dados extras e Webhook
    }
    public class MetaData
            {
                public string ExtraData { get; set; }     // Dados extras
                public string WebhookUrl { get; set; }    // URL para o Webhook
            }
 }

