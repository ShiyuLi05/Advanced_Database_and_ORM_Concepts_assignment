namespace WebApplication2.Models
{
    public class Store
    {
        public Guid StoreNumber { get; set; }

        private string _streetNameAndNumber;
        public string StreetNameAndNumber
        {
            get
            {
                return _streetNameAndNumber;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                else
                {
                    _streetNameAndNumber = value;
                }
            }
        }

        public CanadianProvince Province { get; set; }

        public enum CanadianProvince
        {
            Alberta,
            BritishColumbia,
            Manitoba,
            NewBrunswick,
            NewfoundlandAndLabrador,
            NovaScotia,
            Ontario,
            PrinceEdwardIsland,
            Quebec,
            Saskatchewan,
            NorthwestTerritories,
            Nunavut,
            Yukon
        }

        public HashSet<LaptopStore> LaptopStores { get; set; }
    }
}
