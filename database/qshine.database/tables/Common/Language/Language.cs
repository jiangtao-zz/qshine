using System;
namespace qshine.database.tables.common.language
{
	/// <summary>
	/// Supported language table.
	/// Contains information for multi-lingual support.
	/// </summary>
	public class Language : SqlDDLTable
	{
        /// <summary>
        /// Ctor:
        /// https://en.wikipedia.org/wiki/ISO_639-1
        /// https://lonewolfonline.net/list-net-culture-country-codes/
        /// https://docs.sdl.com/LiveContent/content/en-US/SDL_MediaManager_241/concept_A9F20DF9433C46FF8FED8FA11A29FAA0
        /// 
        /// </summary>
		public Language()
					: base("ln_language", "Language", "Defines supportted languages.", "comData", "comIndex")
		{
            AddPKColumn("id", System.Data.DbType.Int64,autoIncrease:false)

				.AddColumn("language", System.Data.DbType.String, 10, allowNull:false,
                comments:"Language name in English.")

				.AddColumn("iso_code", System.Data.DbType.String, 5, allowNull: false, 
                comments: "language ISO 639-1 code (2 letters)")

				.AddColumn("culture_code", System.Data.DbType.String, 10, allowNull: false, 
                comments: "RFC 4646 Culture code")

                .AddColumn("country", System.Data.DbType.Int16, 0, allowNull: false, reference:new Country().PkColumn,
                comments: "Country id");

            DataVersion = 1;
            SetData(1L, "English", "en", "en-CA", 124);
            SetData(2L, "French", "fr", "fr-CA", 124);
        }
	}
}
