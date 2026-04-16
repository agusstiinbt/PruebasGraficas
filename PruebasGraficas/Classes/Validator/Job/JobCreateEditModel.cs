using PruebasGraficas.Classes.Models;

namespace PruebasGraficas.Classes.Validator.Job
{
    public sealed class JobCreateEditModel : BaseEntity
    {
        public const int FirstNameMaxLength = 100;
        public const int QuickDescriptionMaxLength = 10;
        public const int CommentMaxLength = 2500;
        public const int MinLabelsToPrint = 0;
        public const int MaxLabelsToPrint = 10000;
        public const int MaxInvoiceNumbers = 10;
        public const int MaxShippingBarcodes = 15;

        //public JobType Type { get; set; } = JobType.Delivery;

        #region PhoneNumber

        public string PhoneNumber { get; set; } = string.Empty;
        public CountryPhoneInfo? CountryPhoneInfo { get; set; }

        #endregion

        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }

        #region Address

        public string AddressStreet { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ProvinceState { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        #endregion

        #region Date & Status

        private DateTime? _targetDateInput;

        /// <summary>
        /// UI-bound value for MudDatePicker. Time is normalized away and request mapping converts it to DateOnly.
        /// </summary>
        public DateTime? TargetDateInput
        {
            get => _targetDateInput;
            set => _targetDateInput = value?.Date;
        }

        public DateOnly? TargetDate
        {
            get => TargetDateInput.HasValue
                ? DateOnly.FromDateTime(TargetDateInput.Value)
                : null;
            set => TargetDateInput = value?.ToDateTime(TimeOnly.MinValue);
        }

        /// <summary>
        /// UI-only toggle. When true, TargetDate is null and job is "unscheduled".
        /// </summary>
        public bool ToBeDetermined { get; set; }

        //public ConfirmationStatus ConfirmationStatus { get; set; } = ConfirmationStatus.Pending;

        #endregion

        #region Job Details

        public int? LabelsToPrint { get; set; }
        public string? QuickDescription { get; set; }
        public int? BranchId { get; set; }
        public int? WarehouseId { get; set; }
        public TimeOnly? TimeWindowStart { get; set; }
        public TimeOnly? TimeWindowEnd { get; set; }
        //public TimePreference? TimePreference { get; set; }
        public List<string> InvoiceNumbers { get; set; } = [];
        public List<string> ShippingBarcodes { get; set; } = [];

        #endregion

        #region Metrics

        public decimal? TotalValue { get; set; }
        public decimal? TotalWeightLbs { get; set; }
        public decimal? TotalVolumeFt3 { get; set; }
        public uint? TotalHandleTimeMin { get; set; }
        public uint? TotalPieceCount { get; set; }
        public uint? TotalQuantity { get; set; }
        public decimal? BalanceOwed { get; set; }

        #endregion

        public string? Comment { get; set; }

        //public void NormalizeTagCollections()
        //{
        //    InvoiceNumbers = TagCollectionHelper.Sanitize(InvoiceNumbers);
        //    ShippingBarcodes = TagCollectionHelper.Sanitize(ShippingBarcodes);
        //}
    }
}
