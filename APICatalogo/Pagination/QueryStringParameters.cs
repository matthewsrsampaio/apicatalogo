namespace APICatalogo.Pagination
{
    public abstract class QueryStringParameters // Obs: Uma clase abstrata não pode ser instanciada. Ela é usada com base para outras classses.
    {
        const int maxPageSize = 50;
        public int pageNumber { get; set; } = 1;
        private int _pageSize = maxPageSize;
        public int pageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
