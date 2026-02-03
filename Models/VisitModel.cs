using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMSService.Models
{
    public class VisitModel
    {
        [Key]
        public Guid? VisitId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? TypeId { get; set; }
        public Guid? GateId { get; set; }
        public Guid? QrId { get; set; }
        public Guid? DestinationId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        //[NotMapped]
        //public List<CapturedImageDataModel>? CapturedPhotos { get; set; }
        //[NotMapped]
        //public List<CapturedPhotosInfoModel>? CapturedPhotosInfo { get; set; }


        public bool? Status { get; set; }


        public string? Camera1Name { get; set; }
        public byte[]? Camera1Image { get; set; }

        public string? Camera2Name { get; set; }
        public byte[]? Camera2Image { get; set; }
        public string? Camera3Name { get; set; }
        public byte[]? Camera3Image { get; set; }
        public string? Camera4Name { get; set; }
        public byte[]? Camera4Image { get; set; }
        public string? Camera5Name { get; set; }
        public byte[]? Camera5Image { get; set; }
        public string? Camera6Name { get; set; }
        public byte[]? Camera6Image { get; set; }
        public string? Camera7Name { get; set; }
        public byte[]? Camera7Image { get; set; }
        public string? Camera8Name { get; set; }
        public byte[]? Camera8Image { get; set; }
        public string? Camera9Name { get; set; }
        public byte[]? Camera9Image { get; set; }
        public string? Camera10Name { get; set; }
        public byte[]? Camera10Image { get; set; }
    }
}
