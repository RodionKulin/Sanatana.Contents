namespace Sanatana.Contents.Database.EntityFrameworkCore.Context
{
    public interface IContentsDbContextFactory
    {
        ContentsDbContext GetDbContext();
    }
}