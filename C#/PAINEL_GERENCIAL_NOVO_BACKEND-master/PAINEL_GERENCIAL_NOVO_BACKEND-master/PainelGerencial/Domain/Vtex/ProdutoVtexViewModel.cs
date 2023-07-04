using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Vtex
{

    public class ProdutoVtexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string LinkId { get; set; }
        public string RefId { get; set; }
        public bool? IsVisible { get; set; }
        public string? Description { get; set; }
        public string? DescriptionShort { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string KeyWords { get; set; }
        public string Title { get; set; }
        public bool? IsActive { get; set; }
        public string TaxCode { get; set; }
        public string MetaTagDescription { get; set; }
        public object SupplierId { get; set; }
        public bool ShowWithoutStock { get; set; }
        public List<int> ListStoreId { get; set; }
        public string AdWordsRemarketingCode { get; set; }
        public string LomadeeCampaignCode { get; set; }
        public int Score { get; set; }
        public int CommercialConditionId { get; set; }
    }
    public class SkuByRefIdVtexViewModel
    {
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public bool? IsActive { get; set; }
        public string Name { get; set; }
        public string RefId { get; set; }
        public double PackagedHeight { get; set; }
        public double PackagedLength { get; set; }
        public double PackagedWidth { get; set; }
        public double PackagedWeightKg { get; set; }
        public object Height { get; set; }
        public object Length { get; set; }
        public object Width { get; set; }
        public object WeightKg { get; set; }
        public double CubicWeight { get; set; }
        public bool IsKit { get; set; }
        public bool? ActivateIfPossible { get; set; }
        public DateTime CreationDate { get; set; }
        public object RewardValue { get; set; }
        public object EstimatedDateArrival { get; set; }
        public string ManufacturerCode { get; set; }
        public int? CommercialConditionId { get; set; }
        public string MeasurementUnit { get; set; }
        public double UnitMultiplier { get; set; }
        public object ModalType { get; set; }
        public bool KitItensSellApart { get; set; }
        public object Videos { get; set; }
        public int? StockKeepingUnitId { get; internal set; }
    }
}
