using System;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class OptOut
    {
        public int Id { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string CampaignId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime AddDate { get; set; }
    }
}