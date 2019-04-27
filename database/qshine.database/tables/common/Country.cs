namespace qshine.database.tables.common
{
    /// <summary>
    /// Country information
    /// </summary>
    public class Country : SqlDDLTable
    {
        /// <summary>
        /// https://en.wikipedia.org/wiki/ISO_3166-1
        /// https://en.wikipedia.org/wiki/ISO_4217
        /// </summary>
        public Country()
            : base("cm_country", "Common", "Country table.", "comData", "comIndex")
        {
            //ISO 3166-1 Number Code
            AddPKColumn("id", System.Data.DbType.Int16, autoIncrease: false)

                //ISO 3166 - 1 English short name
                .AddColumn("Name", System.Data.DbType.String, 50, allowNull: false,
                comments: "Country name.")

                //ISO 3166 - 1 Alpha-2 code
                .AddColumn("code_2", System.Data.DbType.String, 2, allowNull: false,
                comments: "two letter code.")

                //ISO 3166 - 1 Alpha-3 code
                .AddColumn("code_3", System.Data.DbType.String, 3, allowNull: false,
                comments: "three letter code.")

                //Currency code ISO 4217:
                .AddColumn("currency", System.Data.DbType.String, 10,
                comments: "Currency code.")

                .AddColumn("flag_url", System.Data.DbType.String, 256,
                comments: "Country flag icon URL");

            DataVersion = 1;

            SetData(124, "Canada", "CA", "CAN", "CAD", 
                "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d9/Flag_of_Canada_%28Pantone%29.svg/23px-Flag_of_Canada_%28Pantone%29.svg.png");

            SetData(840, "United States of America", "US", "USA", "USD",
                "https://upload.wikimedia.org/wikipedia/en/thumb/a/a4/Flag_of_the_United_States.svg/23px-Flag_of_the_United_States.svg.png");

            SetData(250, "France", "FR", "FRA", "EURO",
                "https://upload.wikimedia.org/wikipedia/en/thumb/c/c3/Flag_of_France.svg/23px-Flag_of_France.svg.png");

            SetData(276, " Germany ", "DE", "DEU", "EUR",
                "https://upload.wikimedia.org/wikipedia/en/thumb/b/ba/Flag_of_Germany.svg/23px-Flag_of_Germany.svg.png");

            SetData(156, "China", "CN", "CHN", "CNY",
                "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fa/Flag_of_the_People%27s_Republic_of_China.svg/23px-Flag_of_the_People%27s_Republic_of_China.svg.png");

            SetData(392, "Japan", "JP", "JPN", "JPY",
                "https://upload.wikimedia.org/wikipedia/en/thumb/9/9e/Flag_of_Japan.svg/23px-Flag_of_Japan.svg.png");
        }
    }
}
