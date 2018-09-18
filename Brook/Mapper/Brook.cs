namespace jIAnSoft.Brook.Mapper
{
    public static class Brook
    {
        public static SqlMapper Load(string db)
        {
            return new SqlMapper(db);
        }
    }
}