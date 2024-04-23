namespace AccountsTransactions_DataAccess.Enums
{
    public enum UserGender
    {
        Male = 0,
        Female = 1,
		Non = 2
    }
	public static class UserGenderHelper
	{
		public static int ToInt(this UserGender gender)
		{
			return (int)gender;
		}
		public static UserGender FromInt(int value)
		{
			return Enum.IsDefined(typeof(UserGender), value) ? (UserGender)value : UserGender.Non;
		}
	}
}